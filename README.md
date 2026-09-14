# School Portal

A small slice of a school administration portal. Admin can find users, open a
user, and adjust the wallet balance on a parent account. Backend is .NET 8
with EF Core, frontend is React with TypeScript. The layout follows Clean
Architecture with the dependency arrow strictly pointing inward.

Part 2 (the wallet-mismatch investigation) and Part 3 (the frontend
architecture write-up) are in `docs/` as Word files.

## What is in the repo

```
src/
  SchoolPortal.Domain           entities, enums, domain exceptions
  SchoolPortal.Application      use case handlers, DTOs, abstractions, Result<T>
  SchoolPortal.Infrastructure   PortalDbContext, UserRepository, EF configurations, seed
  SchoolPortal.Api              controllers, contracts, exception handler, Program.cs
tests/
  SchoolPortal.UnitTests
  SchoolPortal.IntegrationTests
frontend/                       Vite + React + TypeScript + TanStack Query
docs/
  Part 2 - Production Thinking.docx
  Part 3 - Frontend Architecture.docx
```

`Api` references `Infrastructure` and `Application`. `Infrastructure` references
`Application` and `Domain`. `Application` references `Domain` only.
`Application` never imports EF Core or ASP.NET types. `Domain` has no
third-party references at all.

## Running

Requires the .NET 8 SDK and Node 20 or newer.

```
dotnet run --project src/SchoolPortal.Api
```

That starts the API on `http://localhost:5049`. On first boot it calls
`EnsureCreated` on a local SQLite file (`schoolportal.db`) and seeds eight
users. Swagger is on `/swagger`.

```
cd frontend
npm install
npm run dev
```

Vite serves on `http://localhost:5173` and proxies `/api/*` to the backend.
CORS on the API also allows the same origin explicitly, so either works.

## Tests

```
dotnet test                            # 23 tests
cd frontend && npm test                # 4 tests
```

The unit tests cover the `User` aggregate invariants and the
`AdjustWalletHandler` (top-up, overdraw, idempotent replay, and a real
optimistic-concurrency clash between two concurrent writers).

The integration tests boot the full ASP.NET pipeline with
`WebApplicationFactory` and hit real endpoints. The SQLite connection is
opened once per fixture so seed data survives across tests. Coverage
includes search, filter, missing user, top-up, overdraw, and the idempotency
contract on the POST endpoint.

The frontend tests cover the wallet adjustment form. Client-side overdraw
check, happy path with mutation invocation and idempotency-key generation,
server error propagation into the UI, and zero-amount rejection.

## The API

All responses are JSON. Errors follow RFC 7807 `application/problem+json` and
carry a `code` extension for machine-readable classification.

```
GET  /api/users?q=&role=&status=&page=1&pageSize=20
GET  /api/users/{id}
POST /api/users/{id}/wallet-adjustments
     Header: Idempotency-Key: <opaque>
     Body:   { "amount": 25.00, "reason": "Topup", "note": "optional" }
```

`amount` accepts positive or negative decimals. Amounts that would drive the
balance below zero come back as `422` with `wallet.insufficient_funds`.
Repeating a POST with the same `Idempotency-Key` returns the original
adjustment with `wasReplayed: true` and does not write a second row.

## Design decisions

The wallet is a real invariant on the `User` aggregate. `AdjustWallet` is a
method on the aggregate, not a service. The audit row (`WalletAdjustment`)
sits inside the aggregate and is only ever written by the aggregate root, in
the same `SaveChanges` call as the balance mutation. That is what stops the
balance and the audit trail from ever disagreeing.

`IUserRepository` exposes only what the use cases actually need: find, a
details projection, a paged search, and a replay lookup, plus a
`SaveChangesAsync` that forwards to the tracked context. There is no
`IRepository<T>` generic, no leaked `IQueryable<T>`, no wrapper
`IUnitOfWork` (`DbContext` already is one). Application code cannot see EF
Core.

Handlers are injected directly into the controller. No MediatR. Three
handlers, small enough that a pipeline mediator would just be ceremony. If
the surface grows and I really want behavioural pipelines (logging,
validation, retries), adding MediatR later is a one-file refactor.

Expected failures use `Result<T>`. Domain and validation problems return
`Result<T>.Failure(Error)` and the controller maps `ErrorType` to an HTTP
status. Real infrastructure exceptions (concurrency, DB) go through
`IExceptionHandler` and become `ProblemDetails`. Nothing about the
happy path throws.

Optimistic concurrency uses a `long Version` on the `User` aggregate,
incremented inside the aggregate on every adjustment and mapped as an EF
concurrency token. Two admins on the same account get one winner and a `412
Precondition Failed` for the loser. This is the concrete answer to the "the
screen does not match the database" scenario in Part 2.

Idempotency uses a required `Idempotency-Key` header. The controller
generates a UUID if the client omitted one, and `(UserId, RequestId)` has a
unique index. Retries collapse to the first row.

DTOs at every seam. Application returns DTOs, not entities. Controllers
translate DTOs into response contracts via small mapping extensions. No
entity, no EF type, no domain exception ever leaks to the wire.

SQLite is used in development and in the test fixtures. The provider is
switchable via `Persistence:Provider` config. Model configurations use
`decimal(18,2)` and portable value converters (DateTimeOffset stored as
binary) so the same model runs against SQL Server without changes.

No EF migrations in the repo. `EnsureCreated` is what the demo needs. In a
real deployment I would add proper migrations against SQL Server.

## Assumptions

- Money is `decimal` and the frontend formats as AUD purely because I had to
  pick a currency for the demo. A real product needs a Money type with
  currency and rounding rules.
- Auth is out of scope for the test. The `PerformedBy` field on the audit
  row falls back to `"portal-admin"`. In a real deployment this comes off
  the JWT.
- Seed data is a small handful of users. Real data would come from the
  ingestion path (Part 1 of the first take-home).
- Rate limiting, per-tenant scoping, and telemetry (OpenTelemetry, Serilog
  with correlation ids) are called out but not implemented within the time
  budget.

## AI tools

I used Claude Code as a pair assistant for scaffolding (project structure,
initial EF configurations, the React shell). The design decisions above, the
tests, the aggregate boundary, and the concurrency and idempotency contracts
were all decided and reviewed by hand. If it did not survive a "why is this
here" pass, it is not in the repo.
