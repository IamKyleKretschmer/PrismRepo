---
id: T004
title: Require a valid Vibranium JWT on every v1 request
feature: todo-list-api
phase: "Phase 2: Foundational"
story: null
repository: PrismRepo
parallel: true
depends_on: [T003]
effort: 3
status: done
---

# T004 — Require a valid Vibranium JWT on every v1 request

## Objective
Every request under `/v1` is rejected with `401` and code `unauthenticated` unless it carries a valid Vibranium JWT, and rejected requests change nothing. Errors use the contract's problem-details body.

## Scope
**In scope** — bearer-token authentication for all `/v1` routes, using the issuer, audience and signing-key settings from T003; caller identity from `sub` only; the `ntx-user-id` header ignored as an identity source; the contract's error body (RFC 9457 problem details with stable `code`) for `401` and for unhandled failures (`500`, generic body).
**Out of scope** — the `/v1` operations themselves (T006 onward); feature-wide verification of auth on every operation (T013).

## Behaviour & Contract
- Missing, expired, wrongly-signed, wrong-issuer and wrong-audience tokens → `401`, `code: unauthenticated` (`../contracts/todo-api-v1.md` § Authorisation).
- A request with only an `ntx-user-id` header and no token → `401` (`../research.md` R9).
- Any authenticated caller may use the shared list; no further authorisation (spec Assumptions).
- `/health` stays unauthenticated.
- `500` bodies are generic and never leak internals, titles or token contents (`../contracts/todo-api-v1.md` § Error body).
- Constraint: Vibranium JWT, approved profile, no header fallback (`../research.md` R9).

## Preconditions & Dependencies
- T003 (JWT configuration settings exist).

## Acceptance Criteria
- [x] Each rejection case above returns `401` with `code: unauthenticated` in a problem-details body.
- [x] A valid test token reaches `/v1` routing (an unknown `/v1` path returns `404`, not `401`).
- [x] `/health` still answers without a token.
- [x] An unhandled failure returns `500` with a generic problem-details body.

## Testing Requirements
- **Acceptance**: every rejection case, the `ntx-user-id`-only case, a valid token, and `/health` without a token. A test signing key is set only in the test host (`../plan.md` § Test Strategy). Framework: xUnit with the ASP.NET Core in-process test host (`../research.md` R11).

## Operational Requirements
Uses the JWT settings from T003; no new configuration. Real Vibranium issuer integration is verified once in the first deployed environment (`../plan.md` § Test Strategy, Coverage gaps).

## Documentation Requirements
README: how to call the API with a bearer token, and how local/test runs use a test signing key.

## References
`../plan.md` § Cross-cutting behaviours (Security); `../contracts/todo-api-v1.md` § Authorisation, § Error body; `../research.md` R7, R9.
