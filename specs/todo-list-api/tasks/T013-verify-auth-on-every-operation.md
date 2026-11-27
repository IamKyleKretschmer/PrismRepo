---
id: T013
title: Verify authentication holds on every operation
feature: todo-list-api
phase: "Phase 5: Polish & Cross-Cutting Concerns"
story: null
repository: PrismRepo
parallel: true
depends_on: [T008, T009, T011, T012]
effort: 2
status: pending
---

# T013 — Verify authentication holds on every operation

## Objective
Prove, across the finished feature, that every `/v1` operation rejects every unauthenticated case with `401 unauthenticated` and changes nothing, and that `/health` alone is open.

## Scope
**In scope** — a feature-wide check of the security cross-cutting behaviour against create, list and complete; closing any gap found.
**Out of scope** — real Vibranium issuer integration (verified in the first deployed environment, `../plan.md` § Test Strategy, Coverage gaps).

## Behaviour & Contract
- Per `../plan.md` § Cross-cutting behaviours (4 Security): Vibranium JWT on every endpoint except `/health`; identity from `sub` only; `ntx-user-id` ignored.
- Rejection cases: missing, expired, invalid signature, wrong issuer, wrong audience, `ntx-user-id` header alone (`../research.md` R9).

## Preconditions & Dependencies
- All story tasks complete (T008, T009, T011, T012).

## Acceptance Criteria
- [ ] Every rejection case returns `401 unauthenticated` on each of create, list and complete.
- [ ] After each rejected create or complete, an authenticated listing shows nothing changed.
- [ ] `/health` answers without a token and returns no item data.
- [ ] Any operation found unprotected is fixed and covered by a test.

## Testing Requirements
- **Acceptance**: the rejection matrix (case × operation) plus `/health`. Framework: xUnit with the ASP.NET Core in-process test host (`../research.md` R11).

## Operational Requirements
N/A.

## Documentation Requirements
N/A.

## References
`../plan.md` § Cross-cutting behaviours (Security); `../contracts/todo-api-v1.md` § Authorisation; `../research.md` R9; quickstart scenario 9.
