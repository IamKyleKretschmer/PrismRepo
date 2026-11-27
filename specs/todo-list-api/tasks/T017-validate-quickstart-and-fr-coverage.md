---
id: T017
title: Validate quickstart scenarios and per-requirement test coverage
feature: todo-list-api
phase: "Phase 5: Polish & Cross-Cutting Concerns"
story: null
repository: PrismRepo
parallel: false
depends_on: [T013, T014, T015, T016]
effort: 2
status: pending
---

# T017 — Validate quickstart scenarios and per-requirement test coverage

## Objective
The feature is confirmed done: every quickstart scenario gives its expected result against a running service, CI is green, and QA has confirmed at least one passing acceptance test for each of FR-1 to FR-9.

## Scope
**In scope** — running `../quickstart.md` scenarios 1–9 against a locally running service on a fresh data file; a traceability record mapping each FR to its passing acceptance tests; confirming the CI gate passes on the feature branch.
**Out of scope** — fixing defects found (raised back to the owning task).

## Behaviour & Contract
- Release gate per `../spec-qa.md` and `../plan.md` § Test Strategy: all unit and acceptance tests pass in CI; at least one passing acceptance test per FR.

## Preconditions & Dependencies
- T013, T014, T015, T016.

## Acceptance Criteria
- [ ] Each of quickstart scenarios 1–9 gives the expected result, recorded in the pull request.
- [ ] A traceability record lists, for each of FR-1 … FR-9, at least one passing acceptance test.
- [ ] The CI pipeline passes on the feature branch.
- [ ] QA signs off the coverage record.

## Testing Requirements
- **Manual**: the quickstart walkthrough, including the restart scenario (8) and the no-token scenario (9).
- **Acceptance**: no new tests; confirms existing xUnit acceptance coverage per FR.

## Operational Requirements
Verifies the CI gate from T002 on the complete feature. Verified by the green pipeline run.

## Documentation Requirements
N/A — `../quickstart.md` updated only if a scenario's expected result was wrong, with the change raised for review.

## References
FR-1 … FR-9; `../quickstart.md`; `../spec-qa.md` (release gate); `../plan.md` § Test Strategy.
