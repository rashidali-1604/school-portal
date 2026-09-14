# Part 3. Frontend Architecture

If I had to shape a React codebase to carry the school portal, a family-facing
parent portal, and internal operational tooling in the same organisation, this
is roughly how I would set it up. The important thing is that no one app owns
the whole story; the shared bits sit in the middle and the apps are thin.

## Repo shape

A monorepo with npm or pnpm workspaces is enough for the first year. Nx and
Turborepo become useful when the number of apps or the CI matrix grows, but
paying that cost early usually slows a small team down.

```
apps/
  school-portal/       admin app (what Part 1 covers)
  parent-portal/       family-facing app
  ops-console/         internal tooling
packages/
  ui/                  design system components
  api-client/          typed clients generated from the backend OpenAPI
  domain/              small pure types and helpers (Money, dates, enums)
  auth/                sign-in flows, token handling, role guards
  config/              tsconfig, eslint, prettier, vitest bases
  test-utils/          RTL render helpers, MSW handlers, fixtures
```

Every app is deployable on its own. Nothing app-specific ever leaks into
`packages/`; if it only makes sense for one app, it stays in that app.

## Inside each app

I keep a feature-sliced layout because it scales much better than
"components/ hooks/ pages/" folders once the app has more than a handful of
screens.

```
src/
  app/               router, providers, layouts
  features/
    users/
      api/           thin functions that call the api-client
      hooks/         useUsers, useUser, useAdjustWallet
      components/    UserTable, WalletAdjustmentForm
      pages/         UsersPage, UserDetailsPage
      types.ts
  shared/
    hooks/
    utils/
  styles/
```

The rule I would enforce (and lint) is: features do not import from other
features. When two features need the same code, it moves up to `shared/`
inside the app, or eventually to `packages/`. This is what stops the graph
turning into a ball of mud six months in.

## State management

Server state and UI state are two different problems and I would treat them
that way.

For server state I would use TanStack Query everywhere. Every list, every
detail, every mutation. Query keys follow a `[feature, resource, params]`
shape, and mutations invalidate the relevant keys on success. This is what
prevents the class of bug covered in Part 2.

For local UI state (form fields, modal open/close, wizard step) I use plain
`useState` or `useReducer`. Form state does not belong in a global store.

For genuinely cross-cutting app state (current tenant, active school,
theme), a small React Context is normally enough. If a page's state clearly
belongs in the URL (filters, pagination, active tab), it goes into
`useSearchParams`, so links are shareable and the back button behaves.

I would not reach for Redux unless there is a specific reason.

## API communication

The backend publishes OpenAPI. `packages/api-client` is generated from it in
CI, so request and response types can never drift out of sync with the
server. A single `httpClient` layer handles the base URL, auth header,
retry policy, and error normalisation. Every non-2xx response becomes a
typed `ApiError` with `status`, `code`, and `detail` (matching the
ProblemDetails the API returns).

Every mutating call carries a client-generated `Idempotency-Key`. Feature
hooks wrap the raw client with the right query keys and invalidations, so
page components never touch `fetch` directly.

## Components

`packages/ui` owns the primitives: `Button`, `TextField`, `Select`,
`Table`, `Modal`, `Toast`, `Card`, `Layout`, and so on. Styled once, tested
once, documented in Storybook. Accessibility is baked into the primitives
(correct roles, focus rings, labels, keyboard behaviour), so consumers get
it for free.

Feature components are dumb about routing. They take props and callbacks.
The `pages/` layer is where router hooks and query hooks meet the
components.

## Testing

Unit and component tests use Vitest and React Testing Library. One test
file next to each component under `__tests__`. Component tests use MSW from
`test-utils` to intercept network calls, so the real query hooks are
exercised and cache invalidation is actually tested rather than mocked
away.

For end-to-end, Playwright with two smoke tests per app on the golden path.
Not a replacement for component tests, more of a safety net against
integration regressions between the shell, the router, and the API.

Contract testing is handled by the OpenAPI pipeline. If the client no
longer compiles, the server broke the contract.

## Maintainability

A few things I would put in on day one because they get much more painful
to add later:

- Strict TypeScript. `noUncheckedIndexedAccess`, `noImplicitAny`,
  `noFallthroughCasesInSwitch`. `any` does not survive review.
- Shared ESLint and Prettier configs in `packages/config`. One place to
  change the rules.
- Vite bundle budgets per app. Alert if the main chunk crosses a threshold
  so nobody accidentally imports a 500 KB icon set.
- Feature flags on anything risky. Rolling back a broken screen is a config
  change, not a redeploy.
- One error boundary per route with Sentry reporting the current user,
  tenant, and route. A crash in one feature does not blank the whole app.
- CI runs typecheck, unit tests, and build for every app on every PR. Slow
  jobs (e2e, visual regression) run on the merge queue.

That covers what a small team could ship in the first two or three months
and still keep adding portals to over the next year.
