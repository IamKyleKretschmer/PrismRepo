---
id: T006
title: Create a to-do item
feature: todo-list-api
phase: "Phase 3: User Story 1"
story: US1
repository: PrismRepo
parallel: false
depends_on: [T004, T005]
effort: 3
status: pending
---

# T006 — Create a to-do item

## Objective
An authenticated caller can `POST /v1/todos` with a valid title and gets back the new item — unique identifier, the title exactly as sent, `isComplete: false` and a UTC `createdAt` — and the item is stored durably.

## Scope
**In scope** — the create operation for valid titles; durable storage of the to-do item (identifier, title, creation time, the internal ordering sequence, and room for a completion time); the item representation in `../contracts/todo-api-v1.md` § Item representation; a `Location` header pointing at the item.
**Out of scope** — rejecting invalid titles (T008); listing (T007); restart and parallel-create guarantees beyond a single create (T009); completing (T010).

## Behaviour & Contract
- `POST /v1/todos` per `../contracts/todo-api-v1.md` § Create item: `201 Created`, body is the new item, `Location` header set.
- Identifier: opaque, unique, system-assigned, never reused; not a sequential integer (`../research.md` R5).
- Title stored and returned byte-for-byte as supplied, including leading/trailing whitespace and any Unicode (spec Considerations, `../research.md` R6).
- Creation time and sequence assigned inside one transaction when the create commits (`../research.md` R3, R4; `../data-model.md` invariants 1, 3).
- Not idempotent: each successful request creates a new item; duplicate titles are allowed.
- Storage failure rolls back and returns the generic `500` from T004; the service does not retry.
- Constraint: embedded SQLite, one transaction per state change (`../research.md` R3).

## Preconditions & Dependencies
- T004 (authentication and error body), T005 (logging with scrubbing).

## Acceptance Criteria
- [ ] A valid create returns `201` with `id`, `title`, `isComplete: false` and a UTC ISO 8601 `createdAt`, and no `completedAt`.
- [ ] Non-English text, emoji and leading/trailing spaces come back unchanged.
- [ ] Two creates with the same title produce two items with different identifiers.
- [ ] Cross-cutting: create without a valid token returns `401 unauthenticated` and creates nothing.
- [ ] Cross-cutting: the title never appears in logs for this request.
- [ ] Cross-cutting: response fields match the v1 contract names exactly.

## Testing Requirements
- **Acceptance**: successful create and its full response shape and `Location`; title round-trip for non-English, emoji and padded titles; duplicate titles; unauthenticated create. Each run starts on a fresh temporary data file. Framework: xUnit with the ASP.NET Core in-process test host (`../research.md` R11).

## Operational Requirements
N/A — uses the data file, logging and metrics already wired in T003 and T005.

## Documentation Requirements
README API reference: create operation, request, response and an example call.

## References
FR-1; US1. `../plan.md` § Component Interactions (US1 create), § Requirement Coverage; `../contracts/todo-api-v1.md` § Create item; `../data-model.md`; `../research.md` R3–R5.
