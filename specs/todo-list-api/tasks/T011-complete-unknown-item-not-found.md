---
id: T011
title: Return not found for unknown or malformed identifiers
feature: todo-list-api
phase: "Phase 4: User Story 2"
story: US2
repository: PrismRepo
parallel: true
depends_on: [T010]
effort: 2
status: pending
---

# T011 — Return not found for unknown or malformed identifiers

## Objective
Completing an item with an identifier that matches no item — well-formed but unknown, malformed, or empty — returns `404` with code `todo_not_found` naming the identifier, and changes no item.

## Scope
**In scope** — the single not-found response for unknown, malformed and empty identifiers on the complete operation.
**Out of scope** — successful completion (T010); any separate "invalid identifier" error (rejected by `../spec-qa.md` and `../research.md` R5).

## Behaviour & Contract
- `404` problem-details body, `code: todo_not_found`, `detail` names the supplied identifier (`../contracts/todo-api-v1.md` § Complete item).
- Unknown, malformed and empty identifiers are indistinguishable to the caller; the response reveals nothing about identifier format (`../research.md` R5).
- No item changes (`../data-model.md` invariant 6).
- The identifier echoed in `detail` must be safe to return (no injection into the response) and is not a title, so the "never repeat a title" rule is unaffected.

## Preconditions & Dependencies
- T010 (complete operation exists).

## Acceptance Criteria
- [ ] A well-formed unknown identifier returns `404 todo_not_found` with the identifier in `detail`.
- [ ] A malformed identifier returns the identical response shape and code.
- [ ] An empty identifier returns `404 todo_not_found`.
- [ ] After each case, listing shows every item unchanged.
- [ ] Cross-cutting: tests assert `code` and the presence of the identifier, not message wording.

## Testing Requirements
- **Acceptance**: unknown, malformed and empty identifiers, each followed by a listing proving nothing changed (`../spec-qa.md` error-assertion and malformed-identifier decisions). Framework: xUnit with the ASP.NET Core in-process test host (`../research.md` R11).

## Operational Requirements
N/A.

## Documentation Requirements
README API reference: `todo_not_found` and when it is returned.

## References
FR-8; US2. `../spec-qa.md` (malformed identifiers, error assertions); `../contracts/todo-api-v1.md` § Complete item, § Error body; `../research.md` R5.
