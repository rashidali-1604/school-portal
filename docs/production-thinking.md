# Part 2. Production Thinking

Scenario: an administrator adjusts a parent's wallet balance and reports that
the number on their screen does not match what is stored in the database.

## What could cause this

The most common answer, before anything else, is that the screen is stale.
The mutation went through fine, but the details view or the list is still
showing whatever was fetched a minute ago. If the client is not invalidating
its cache after the mutation, the two values just drift.

A close second is two admins editing the same wallet at once. Both browsers
load a balance of 100. Admin A adds 20 and saves. Admin B is looking at
100 as their baseline, adds 30, and saves a moment later. Without an
optimistic-concurrency check, Admin B's write silently overwrites Admin A's,
and the database now shows 130 when it should show 150.

Retries are another one. If the network flakes and the request is retried,
and there is no idempotency key, the server may write two rows and the
balance will be higher than the UI thinks.

There are a few less common ones I would still keep on the list. Reads and
writes could be pointed at different databases (a reporting mirror or a
snapshotted preview). A 4xx response could have been treated as success by a
broken client error handler. The UI could be rounding a value for display
that is stored at more precision. A background job (a refund, a purchase,
a chargeback) may have fired between the admin's save and the next refresh,
in which case nothing is actually broken, the admin just did not see it.

And, once in a while, it turns out to be the wrong user record entirely,
because two accounts have similar names and the URL got mixed up.

## How I would investigate

I would start by asking the admin exactly which screen shows what, when they
last refreshed, and whether they had multiple tabs open. Half the "bugs" go
away at that point.

If there is a real discrepancy, the audit trail is where I go next. In this
codebase every adjustment writes a `WalletAdjustment` row with `Amount`,
`BalanceAfter`, `OccurredAt`, `PerformedBy`, and `RequestId`. Ordering those
by time and running a sum should reconstruct exactly what
`Users.WalletBalance` should be. If those two numbers do not agree, either
someone wrote to the balance without going through the aggregate (bad), or
an adjustment landed twice (also bad, but caught by the unique index on
`(UserId, RequestId)`).

Then I would pull API logs for that user in the relevant window. A silently
swallowed 409, or a 5xx that the client did not surface, is often the culprit.
If it looks fine on the server, I open the browser Network tab and the
React Query devtools. If the response body says 175 but the cached copy
says 100, the bug is in cache invalidation, not in the database.

If concurrency is the suspect, the fastest reproduction is opening two
browser tabs, loading the same user, and submitting adjustments a second
apart. If one of them silently overwrites the other, that confirms the
optimistic check is not doing its job.

## How I would prevent it

Most of the prevention work is already in the code:

- The balance and the audit row are updated inside the same
  `SaveChangesAsync` call, on the same aggregate. Nothing else in the codebase
  can touch `Users.WalletBalance` without going through `User.AdjustWallet`.
- The `User` aggregate carries a `Version` concurrency token. Two admins
  saving against the same version get one winner and a 412 for the other,
  which the UI can turn into a "please refresh" prompt.
- The POST endpoint requires an `Idempotency-Key` header. The client
  generates a UUID per attempt; retries and double-clicks collapse to the
  first row and return `wasReplayed: true`.
- On the client, every successful mutation invalidates the affected user's
  query key. The screen re-fetches, so it never shows a value that predates
  the write.

Outside the code, I would want a reconciliation dashboard that sums
`WalletAdjustments.Amount` per user daily and alerts if it drifts from
`Users.WalletBalance`. Any drift means the invariant has been broken
somewhere, and I want to hear about it before an admin does.

Two final things worth saying. Money should stay as decimal end to end and
only be formatted at render time; do not parse to float anywhere in the
middle. And for anything the admin is actually going to act on, keep the
read and write connections pointed at the same primary. Replica lag on a
wallet endpoint is not a bug I want to debug at 11pm.
