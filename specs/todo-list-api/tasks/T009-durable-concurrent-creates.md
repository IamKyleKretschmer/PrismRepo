---
id: T009
title: Keep created items across restarts and parallel creates
feature: todo-list-api
phase: "Phase 3: User Story 1"
story: US1
repository: PrismRepo
parallel: true
depends_on: [T007]
effort: 3
status: pending
---

# T009 — Keep created items across restarts and parallel creates

## Objective
Items created before a restart are listed unchanged after it, and parallel creates each produce a distinct item that appears in a strict, repeatable order.

## Scope
**In scope** — durability of created items across a service restart on the same data file; correctness under 50 parallel creates, including items that share a creation time.
**Out of scope** — durability of completion (T012); multi-instance operation (`../plan.md` § Constraints — one instance per data file).

## Behaviour & Contract
- Every committed create survives a restart (`../data-model.md` invariant 7).
- 50 parallel creates yield 50 items with 50 different identifiers (spec Considerations, `../spec-qa.md`).
- Items sharing a creation time are ordered by the order the system accepted them; the order is identical across repeated listings and across a restart (FR-3, `../research.md` R4).
- Constraint: SQLite in WAL mode, one transaction per create (`../research.md` R3, R8).

## Preconditions & Dependencies
- T007 (create and list exist).

## Acceptance Criteria
- [ ] After creating items and restarting the host on the same data file, listing returns the same items with identical fields and order.
- [ ] 50 parallel creates all return `201` with distinct identifiers, and listing returns exactly 50 items.
- [ ] The listing order after the parallel creates is strictly ordered by creation time then acceptance order and identical across repeated listings.
- [ ] Cross-cutting: no partial items exist after the parallel run (every listed item has all fields).

## Testing Requirements
- **Acceptance**: restart test that stops and restarts the in-process host against the same temporary data file; 50-parallel-create test checking count, uniqueness and exact repeatable order. Both run in CI. Framework: xUnit with the ASP.NET Core in-process test host (`../research.md` R11).

## Operational Requirements
N/A — the persistent data file requirement is documented in T016.

## Documentation Requirements
N/A.

## References
FR-3, FR-5; US1. `../spec-qa.md` (restart, concurrency, ordering ties); `../plan.md` § Test Strategy (US1); `../research.md` R3, R4, R8; `../data-model.md` invariants 5, 7.
