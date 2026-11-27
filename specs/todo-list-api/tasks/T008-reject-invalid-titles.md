---
id: T008
title: Reject invalid titles with a specific error code
feature: todo-list-api
phase: "Phase 3: User Story 1"
story: US1
repository: PrismRepo
parallel: true
depends_on: [T007]
effort: 3
status: pending
---

# T008 — Reject invalid titles with a specific error code

## Objective
A create request whose title is missing, empty, whitespace-only, longer than 200 user-perceived characters or larger than 4,000 Unicode code points is rejected with `400` and a code naming the broken rule, and a create request body over 64 KB is rejected with `413`. In every case, no item is created.

## Scope
**In scope** — the five title rules and their error codes; unparseable bodies treated as a missing title; boundary behaviour for 1, 200 and 201 characters including emoji and combined accents; the 4,000-code-point title ceiling; the 64 KB request body limit on create.
**Out of scope** — editing titles (spec Won't have); valid creates (T006).

## Behaviour & Contract
- `400` problem-details body with `code` per `../contracts/todo-api-v1.md` § Create item: `title_missing` (no field, or body can't be parsed), `title_empty`, `title_blank` (any Unicode whitespace only), `title_too_long` (over 200 grapheme clusters), `title_too_large` (over 4,000 Unicode code points).
- `413` problem-details body with `code: request_too_large` for a create body over 64 KB. The limit is enforced before the body is read in full, so an oversized payload is never buffered or parsed (`../research.md` R6).
- Length counts grapheme clusters and includes leading/trailing whitespace; titles are never trimmed (`../research.md` R6, spec Assumptions).
- The code-point ceiling is checked alongside the grapheme limit, so a title of 200 or fewer graphemes built from stacked combining marks is still rejected once it exceeds 4,000 code points. When a title breaks both rules, the result must be deterministic and documented (e.g. `title_too_large` takes precedence).
- `detail` explains the rule in plain English and never repeats the supplied title (`../contracts/todo-api-v1.md` § Error body).
- Validation happens before storage is touched; a rejection changes nothing (`../data-model.md` invariants 2, 6).
- Constraint: title rules live with the item rules, independent of HTTP and storage, so they can be unit tested alone (`../plan.md` Structure Decision).

## Preconditions & Dependencies
- T007 (listing, used to prove nothing changed).

## Acceptance Criteria
- [ ] Each rejected case in `../spec-qa.md` (no title, empty, spaces only, tabs only, newlines only, 201 characters) returns `400` with the matching `code` and a readable `detail`.
- [ ] Each accepted boundary (1 character, 200 characters, 200 characters mixing emoji and accented letters, 200 characters with leading/trailing spaces) returns `201`.
- [ ] A title of 4,000 code points that is within 200 graphemes returns `201`; a title of 4,001 code points that is within 200 graphemes (stacked combining marks) returns `400 title_too_large`.
- [ ] 200 of the most complex emoji (multi-person ZWJ sequences with skin-tone modifiers) are accepted.
- [ ] A create body just under 64 KB is processed normally (accepted or rejected by the title rules); a body over 64 KB returns `413 request_too_large` with a problem-details body.
- [ ] After every rejection, listing shows no new item.
- [ ] Cross-cutting: `detail` and logs never contain the supplied title.
- [ ] Cross-cutting: tests assert `code`, never message wording.

## Testing Requirements
- **Unit**: every FR-2 rule on both sides of each boundary, including grapheme counting for emoji and combining accents, Unicode whitespace variants, both sides of the 4,000-code-point ceiling, and the precedence rule when a title breaks both length rules. Framework: xUnit.
- **Acceptance**: each rejected case returns the right `code` and leaves the list unchanged; accepted boundaries create items; an oversized body returns `413 request_too_large`. Framework: xUnit with the ASP.NET Core in-process test host (`../research.md` R11).

## Operational Requirements
N/A.

## Documentation Requirements
README API reference: the title rules, how length is counted, the code-point ceiling, the 64 KB body limit, and each error `code`.

## References
FR-2; US1. `../spec-qa.md` (title boundary cases, error assertions); `../contracts/todo-api-v1.md` § Create item, § Error body; `../research.md` R6, R7; `../data-model.md` invariants 2, 6.
