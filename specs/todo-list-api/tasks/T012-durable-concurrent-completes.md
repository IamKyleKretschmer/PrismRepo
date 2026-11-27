---
id: T012
title: Keep completion across restarts and parallel completes
feature: todo-list-api
phase: "Phase 4: User Story 2"
story: US2
repository: PrismRepo
parallel: true
depends_on: [T010]
effort: 2
status: pending
---

# T012 — Keep completion across restarts and parallel completes

## Objective
An item's completion and its first completion time survive a service restart, and parallel completes of one item all succeed and leave it with exactly one completion time.

## Scope
**In scope** — durability of completion across a restart on the same data file; 10 parallel completes of the same item.
**Out of scope** — durability of creates (T009); multi-instance operation (`../plan.md` § Constraints).

## Behaviour & Contract
- Every committed completion survives a restart (`../data-model.md` invariant 7).
- Parallel completes: all return `200`; the first to commit sets the completion time and every response and later listing shows that same time (`../research.md` R8; `../data-model.md` invariant 4).
- Constraint: the conditional update inside one SQLite transaction (`../research.md` R3, R8).

## Preconditions & Dependencies
- T010 (complete operation exists).

## Acceptance Criteria
- [ ] After completing some items and restarting the host on the same data file, listing shows identical completion status and `completedAt` for every item.
- [ ] 10 parallel completes of one item all return `200` with the same `completedAt`.
- [ ] Listing after the parallel run shows the item complete with that single `completedAt`.

## Testing Requirements
- **Acceptance**: restart test that creates and completes items, restarts the in-process host against the same temporary data file, then lists; 10-parallel-complete test. Both run in CI (`../spec-qa.md` restart and concurrency decisions). Framework: xUnit with the ASP.NET Core in-process test host (`../research.md` R11).

## Operational Requirements
N/A.

## Documentation Requirements
N/A.

## References
FR-6, FR-7, FR-9; US2. `../spec-qa.md` (restart, concurrency); `../plan.md` § Test Strategy (US2); `../research.md` R8; `../data-model.md` invariants 4, 7.
