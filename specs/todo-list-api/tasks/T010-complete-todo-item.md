---
id: T010
title: Mark a to-do item complete idempotently
feature: todo-list-api
phase: "Phase 4: User Story 2"
story: US2
repository: PrismRepo
parallel: false
depends_on: [T007]
effort: 3
status: pending
---

# T010 — Mark a to-do item complete idempotently

## Objective
An authenticated caller can `POST /v1/todos/{id}/complete` on an existing item and gets it back complete with the time it was first completed; repeating the call succeeds and changes nothing; the list shows the item as complete with `completedAt`.

## Scope
**In scope** — the complete operation for existing items; recording the first completion time; idempotent repeat completes; showing `isComplete: true` and `completedAt` for completed items in both the complete response and the list.
**Out of scope** — unknown, malformed or empty identifiers (T011); restart and parallel-complete guarantees (T012); un-completing (spec Won't have).

## Behaviour & Contract
- `POST /v1/todos/{id}/complete` per `../contracts/todo-api-v1.md` § Complete item: no body, `200` with the item in its current state.
- Completion time is set only if not already set, in one conditional update inside one transaction — first writer wins (`../research.md` R8; `../data-model.md` invariant 4).
- Completion status is derived from the presence of completion time, never stored separately (`../data-model.md`).
- `completedAt` is present only when `isComplete` is true; UTC ISO 8601 (FR-9).
- A repeat complete returns `200` with the item unchanged — same `completedAt`, title and `createdAt` (FR-7).
- Storage failure rolls back and returns the generic `500`; callers may safely retry.

## Preconditions & Dependencies
- T007 (items can be created and listed). US2 builds on the item store and representation introduced in US1.

## Acceptance Criteria
- [ ] Completing an incomplete item returns `200` with `isComplete: true` and a UTC `completedAt`.
- [ ] Listing afterwards shows that item complete with the same `completedAt`; other items are unchanged.
- [ ] Completing it again later returns `200` with `completedAt` unchanged and no other field changed.
- [ ] Incomplete items in the list have no `completedAt`.
- [ ] Cross-cutting: complete without a valid token returns `401 unauthenticated` and changes nothing.
- [ ] Cross-cutting: titles never appear in logs for this request.

## Testing Requirements
- **Acceptance**: complete then list; repeat complete keeps `completedAt` (`../spec-qa.md` repeat-complete decision); `completedAt` absent on incomplete items; unauthenticated complete. Framework: xUnit with the ASP.NET Core in-process test host (`../research.md` R11).

## Operational Requirements
N/A.

## Documentation Requirements
README API reference: complete operation, idempotency, and the `completedAt` field, with an example.

## References
FR-6, FR-7, FR-9; US2. `../plan.md` § Component Interactions (US2); `../contracts/todo-api-v1.md` § Complete item, § Item representation; `../data-model.md` § Lifecycle, invariant 4; `../research.md` R8.
