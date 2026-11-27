---
id: T005
title: Emit structured request logs and metrics without sensitive data
feature: todo-list-api
phase: "Phase 2: Foundational"
story: null
repository: PrismRepo
parallel: true
depends_on: [T003]
effort: 3
status: done
---

# T005 — Emit structured request logs and metrics without sensitive data

## Objective
Every request produces a structured log entry and request count/latency metrics that support incident investigation, and no log or metric ever contains a to-do title or token contents.

## Scope
**In scope** — structured logs to stdout with correlation ID, route, status code and, on errors, the error `code`; OpenTelemetry-compatible request count and latency metrics; scrubbing so request bodies, titles and tokens never reach logs or telemetry; caller identity logged only as a correlation-safe subject identifier at debug level.
**Out of scope** — exporting to a specific monitoring backend (no deployment target in scope); feature-wide privacy/observability verification (T014).

## Behaviour & Contract
- Correlation ID present on every request log entry, including `401` and `500` responses.
- Titles are free text that may hold personal data and must never be logged (`../data-model.md` § Data classification).
- Metrics carry route and status, never titles or caller-supplied identifiers.

## Preconditions & Dependencies
- T003.

## Acceptance Criteria
- [x] A request yields one structured log entry with correlation ID, route and status code; error responses also carry the error `code`.
- [x] Request count and latency metrics are emitted per route and status.
- [x] No log entry at any level contains a token or a request body.
- [x] Caller subject appears only at debug level.

## Testing Requirements
- **Unit**: the scrubbing rule — a request carrying a title and a token yields log output containing neither.
- **Acceptance**: a log assertion on one error path (e.g. `401`) showing correlation ID, status and `code`. Frameworks: xUnit; xUnit with the ASP.NET Core in-process test host (`../research.md` R11).

## Operational Requirements
New structured logs and request metrics. Verified by the tests above and by inspecting stdout of a local run.

## Documentation Requirements
README: log fields, metrics emitted, and the guarantee that titles and tokens are never logged.

## References
`../plan.md` § Cross-cutting behaviours (Privacy, Observability), § Operations Plan; `../data-model.md` § Data classification.
