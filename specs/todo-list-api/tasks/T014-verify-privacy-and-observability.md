---
id: T014
title: Verify privacy and observability across the feature
feature: todo-list-api
phase: "Phase 5: Polish & Cross-Cutting Concerns"
story: null
repository: PrismRepo
parallel: true
depends_on: [T008, T009, T011, T012]
effort: 2
status: pending
---

# T014 — Verify privacy and observability across the feature

## Objective
Prove that, across every operation and outcome, logs and telemetry carry the investigation signals the plan promises and never contain titles or token contents.

## Scope
**In scope** — a feature-wide check of the privacy and observability cross-cutting behaviours against create, list, complete and `/health`, for success, `400`, `401`, `404` and `500` outcomes; closing any gap found.
**Out of scope** — exporting telemetry to a monitoring backend (no deployment target in scope).

## Behaviour & Contract
- Per `../plan.md` § Cross-cutting behaviours (4 Privacy, 7 Observability): structured logs with correlation ID, status and error `code`; request count and latency metrics; `/health` checks the store; titles and tokens never logged; caller subject only at debug level.
- Titles may contain personal data (`../data-model.md` § Data classification).

## Preconditions & Dependencies
- All story tasks complete (T008, T009, T011, T012).

## Acceptance Criteria
- [ ] For each operation and outcome above, log output contains correlation ID and status, and error outcomes contain the error `code`.
- [ ] With debug logging on, a run of every operation using a distinctive title and token shows neither anywhere in logs or metrics.
- [ ] Request metrics exist for every `/v1` route.
- [ ] `/health` is covered by the acceptance suite.
- [ ] Any gap found is fixed and covered by a test.

## Testing Requirements
- **Unit**: log scrubbing covers every operation's request shape.
- **Acceptance**: log assertions on at least one error path per operation; `/health` healthy and unhealthy. Frameworks: xUnit; xUnit with the ASP.NET Core in-process test host (`../research.md` R11).
- **Manual**: code review of every log statement against the privacy row of the cross-cutting table.

## Operational Requirements
Confirms the logs and metrics introduced in T005 cover the finished feature. Verified by the tests and review above.

## Documentation Requirements
N/A.

## References
`../plan.md` § Cross-cutting behaviours (Privacy, Observability), § Operations Plan; `../data-model.md` § Data classification.
