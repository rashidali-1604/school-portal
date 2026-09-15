# School Portal

A small backend + web app for a school administration portal. Admin can search
users, open a user, and adjust the wallet balance on a parent account. The
backend is .NET 8 with EF Core and the frontend is React + TypeScript. The
layout follows Clean Architecture with the dependency arrow pointing inward.

The two write-ups asked for in the test (Part 2 and Part 3) live in `docs/`
as Word files.

## Layout

```
src/
  SchoolPortal.Domain           entities, enums, domain exceptions
  SchoolPortal.Application      services, repositories, DTOs, wrappers, Result<T>
  SchoolPortal.Infrastructure   AppDbContext, EF configurations, generic + user repositories
  SchoolPortal.Api              controllers, viewmodels, exception handler, Program.cs
tests/
  SchoolPortal.UnitTests
  SchoolPortal.IntegrationTests
frontend/                       Vite + React + TypeScript + TanStack Query
docs/
  Part 2 - Production Thinking.docx
  Part 3 - Frontend Architecture.docx
```

`Api` references `Infrastructure` and `Application`. `Infrastructure`
references `Application` and `Domain`. `Application` references `Domain` only.
`Application` never imports EF Core or ASP.NET. `Domain` has no third-party
references at all.

## Running

Prerequisites: .NET 8 SDK, Node 20 or newer.

```
dotnet run --project src/SchoolPortal.Api
```

The API starts on `http://localhost:5049`. On first boot it calls
`EnsureCreated` on a local SQLite file (`schoolportal.db`) and seeds eight
users. Swagger is on `/swagger`.

```
cd frontend
npm install
npm run dev
```

Vite serves on `http://localhost:5173` and proxies `/api/*` to the backend.
CORS on the API also allows the same origin explicitly.

## Tests

```
dotnet test                            # 25 tests
cd frontend && npm test                # 4 tests
```

The unit tests cover the `User` aggregate invariants and every path through
`UserService.AdjustWalletAsync`: top-up, overdraw, idempotent replay, zero
amount, missing key, unknown user, and a real optimistic-concurrency clash
between two writers.

The integration tests boot the full ASP.NET pipeline with
`WebApplicationFactory` and hit real endpoints. The SQLite connection is
opened once per fixture so seed data survives across tests. Coverage
includes search, filter, missing user, top-up, overdraw, and the idempotency
contract on the POST endpoint.

The frontend tests cover the wallet adjustment form. Client-side overdraw
check, happy path with mutation invocation and idempotency-key generation,
server error propagation into the UI, and zero-amount rejection.

## The API

All responses are JSON. Errors follow RFC 7807 (`application/problem+json`)
and carry a `code` extension for machine classification.

```
GET  /api/users?q=&role=&status=&page=1&pageSize=20
GET  /api/users/{id}
POST /api/users/{id}/wallet-adjustments
     Header: Idempotency-Key: <opaque>
     Body:   { "amount": 25.00, "reason": "Topup", "note": "optional" }
```

`amount` takes positive or negative decimals. Amounts that would push the
balance below zero come back as 422 with `wallet.insufficient_funds`.
Repeating a POST with the same `Idempotency-Key` returns the original
adjustment with `wasReplayed: true` and does not write a second row.

## How the layers fit together

The `User` aggregate is what owns wallet correctness. `AdjustWallet` is a
method on `User`, not a service. The audit row (`WalletAdjustment`) sits
inside the aggregate and is only written by the aggregate root, in the
same `SaveChangesAsync` call as the balance mutation. That is what keeps
the running balance and the audit trail from disagreeing.

The Application layer follows the shape of the previous take-home. A
generic `IRepository<T>` and `IService<T>` provide the shared CRUD surface,
and `IUserRepository : IRepository<User>` plus `UserService : Service<User>,
IUserService` add the user-specific queries (search projection, details
projection, replay lookup) and the wallet operation. All write paths go
through `SaveAllAsync`, which returns `ITransactionResult` so callers can
report a save failure without swallowing the exception detail.

The controller does not do CQRS. There is one `UsersController` and one
`IUserService`. The three endpoints (search, get, adjust) map one-to-one
onto three service methods. Search inputs come in as
`UserSearchParametersViewModel` and get mapped to the service's
`UserSearchCriteria`; wallet adjustments come in as
`WalletAdjustmentViewModel` and get mapped to `WalletAdjustmentRequest`.

Two things carry over from the design that are worth calling out because
they are the direct answers to Part 2:

- Optimistic concurrency uses a `long Version` on the `User` aggregate,
  incremented inside the aggregate on each adjustment and mapped as an EF
  concurrency token. Two admins on the same account: one winner, a
  `DbUpdateConcurrencyException` for the loser, which the global handler
  turns into a 412.
- Idempotency uses a required `Idempotency-Key` header. `(UserId,
  RequestId)` has a unique index, and the service checks for an existing
  adjustment before touching the user. Retries collapse to the first row.

Expected failures use a small `Result<T>` type. Domain and validation
problems become `Result.Failure(Error)` and the controller maps `ErrorType`
to an HTTP status. Real infrastructure exceptions (concurrency, DB) still
propagate to the exception handler and become `ProblemDetails`.

## Assumptions

- Money is `decimal` and the frontend formats as AUD purely because I had
  to pick a currency for the demo. A production version needs a Money
  type with currency and rounding rules.
- Auth is out of scope. `PerformedBy` on the audit row falls back to
  `"portal-admin"`. In a real deployment this comes off the JWT.
- Seed data is a handful of users. Real data would come from the
  ingestion path from the previous take-home.
- Rate limiting, per-tenant scoping, and structured telemetry (Serilog +
  correlation ids, OpenTelemetry) are called out but not implemented
  inside the time budget.

## AI tools

I used Claude Code as a pair assistant for scaffolding project structure,
the initial EF configurations, and the React shell. Anything design-shaped
(the aggregate boundary, the concurrency token, the idempotency contract,
the service structure) I decided on my own and reviewed line by line. The
generic repository and service pattern here is deliberately the same shape
as my previous take-home so the two projects read as coming from the same
codebase.
