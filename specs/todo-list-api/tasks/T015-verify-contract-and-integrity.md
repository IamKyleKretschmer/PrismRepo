---
id: T015
title: Verify contract conformance, error codes and data integrity
feature: todo-list-api
phase: "Phase 5: Polish & Cross-Cutting Concerns"
story: null
repository: PrismRepo
parallel: true
depends_on: [T008, T009, T011, T012]
effort: 2
status: pending
---

# T015 — Verify contract conformance, error codes and data integrity

## Objective
Prove that every operation matches the v1 contract field for field, every error carries its documented stable `code`, and no rejected or failed request changes any item.

## Scope
**In scope** — a feature-wide check of the compatibility, localisation and data-integrity cross-cutting behaviours; contract tests that fail if a field or `code` is removed or renamed; closing any gap found.
**Out of scope** — new contract additions.

## Behaviour & Contract
- Per `../plan.md` § Cross-cutting behaviours: 3 Compatibility (`/v1` prefix, additive-only changes), 9 Localisation (stable `code` on every error, consumers localise from it), 5 Data integrity (one transaction per state change; errors change nothing).
- Contract source: `../contracts/todo-api-v1.md` (item fields, status codes, error codes `title_missing`, `title_empty`, `title_blank`, `title_too_long`, `todo_not_found`, `unauthenticated`; RFC 9457 body).
- Invariants: `../data-model.md` 1–7.

## Preconditions & Dependencies
- All story tasks complete (T008, T009, T011, T012).

## Acceptance Criteria
- [ ] Contract tests assert the exact field set and types for the item representation on create, list and complete.
- [ ] Every documented error `code` is produced by at least one test, with a problem-details body containing `type`, `title`, `status`, `detail` and `code`.
- [ ] Renaming or removing any contract field or `code` makes a test fail.
- [ ] Every error path (`400`, `401`, `404`) is followed by a listing proving nothing changed.
- [ ] No error `detail` repeats a caller-supplied title.

## Testing Requirements
- **Acceptance**: contract-shape tests per operation; one test per error `code`; "error changes nothing" checks. Framework: xUnit with the ASP.NET Core in-process test host (`../research.md` R11).

## Operational Requirements
N/A.

## Documentation Requirements
N/A.

## References
`../plan.md` § Cross-cutting behaviours (Compatibility, Localisation, Data integrity); `../contracts/todo-api-v1.md`; `../data-model.md` § Invariants; `../research.md` R7, R10; `../spec-qa.md` (error assertions).
