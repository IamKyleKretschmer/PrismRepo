---
id: T007
title: List all to-do items in a stable order
feature: todo-list-api
phase: "Phase 3: User Story 1"
story: US1
repository: PrismRepo
parallel: false
depends_on: [T006]
effort: 3
status: done
---

# T007 — List all to-do items in a stable order

## Objective
An authenticated caller can `GET /v1/todos` and receive every item, oldest first, in an order that is identical on every listing; an empty store returns an empty array, not an error.

## Scope
**In scope** — the list operation; ordering by creation time then acceptance sequence; the empty-list result; showing each item's identifier, title, completion status and creation time (and `completedAt` when present).
**Out of scope** — filtering, searching or paging (spec Won't have); verification under parallel creates and restart (T009).

## Behaviour & Contract
- `GET /v1/todos` per `../contracts/todo-api-v1.md` § List items: `200`, body `items` array.
- Order: creation time ascending, then the sequence assigned when each create committed — strict and repeatable (`../data-model.md` invariant 5, `../research.md` R4).
- No items → `200` with `items: []` (FR-4).
- Reads a consistent snapshot; safe and side-effect free.
- Performance: a few hundred items return in under 200 ms p95 on one instance (`../plan.md` § Technical Context).

## Preconditions & Dependencies
- T006 (items can be created).

## Acceptance Criteria
- [x] A fresh instance returns `200` with an empty `items` array.
- [x] After several creates, the list returns all of them oldest first with all contract fields.
- [x] Repeated listings return the exact same order.
- [x] Listing a few hundred items completes in under 200 ms p95 locally.
- [x] Cross-cutting: list without a valid token returns `401 unauthenticated`.
- [x] Cross-cutting: titles never appear in logs for this request.

## Testing Requirements
- **Acceptance**: empty list on a fresh data file; exact order after sequential creates; repeated listings identical; unauthenticated list. Framework: xUnit with the ASP.NET Core in-process test host (`../research.md` R11).
- **Manual**: time a listing of a few hundred items against the p95 target.

## Operational Requirements
N/A.

## Documentation Requirements
README API reference: list operation, ordering rule and empty-list behaviour, with an example.

## References
FR-3, FR-4; US1. `../plan.md` § Component Interactions (US1 list); `../contracts/todo-api-v1.md` § List items; `../data-model.md` invariant 5; `../research.md` R4.
