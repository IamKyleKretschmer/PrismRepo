---
id: T003
title: Start against a configured data file with fail-fast config and health endpoint
feature: todo-list-api
phase: "Phase 2: Foundational"
story: null
repository: PrismRepo
parallel: false
depends_on: [T001]
effort: 3
status: done
---

# T003 — Start against a configured data file with fail-fast config and health endpoint

## Objective
The service starts only when its required configuration is present, opens (creating on first start) its embedded data store at the configured path, and reports liveness and readiness at `GET /health`.

## Scope
**In scope** — required runtime configuration (data file path; JWT issuer, audience and signing-key source) read from the environment; refusing to start with a clear message naming each missing setting; opening/creating the embedded store at startup; `GET /health` reporting whether the service runs and can reach and write its store.
**Out of scope** — storage of to-do items (introduced with the first story that needs it, T006); token validation (T004); request logging and metrics (T005).

## Behaviour & Contract
- `/health` is an operational endpoint outside the public contract: no authentication, no item data (`../contracts/todo-api-v1.md` § Operational endpoint).
- `/health` reports unhealthy if the data file can't be reached or written (`../plan.md` § Feasibility & Risks — data file on non-persistent storage).
- Missing configuration: the process exits non-zero at startup; the message names the missing settings and never prints secret values.
- Constraint: embedded SQLite in WAL mode, one instance per data file (`../research.md` R3, R8).
- Secrets come from the environment and are never committed (`../plan.md` § Operations Plan).

## Preconditions & Dependencies
- T001.

## Acceptance Criteria
- [x] Starting with any required setting missing fails fast, naming that setting and no secret value.
- [x] Starting with valid configuration and a fresh path creates the data file and serves `/health` as healthy without a token.
- [x] `/health` reports unhealthy when the data file location is not writable.
- [x] `/health` responses contain no item data.

## Testing Requirements
- **Acceptance**: `/health` healthy on a fresh temporary data file; unhealthy when the store is unreachable; reachable without authentication. Each test run uses a fresh temporary data file (spec-qa isolation decision). Framework: xUnit with the ASP.NET Core in-process test host (`../research.md` R11; no `/.context/` yet).
- **Manual**: start with configuration missing and observe the fail-fast message.

## Operational Requirements
New required runtime configuration and a health endpoint for liveness/readiness. Verified by the fail-fast manual check and the acceptance tests above.

## Documentation Requirements
README: list each required setting (name and meaning, no values). Full operations section follows in T016.

## References
`../plan.md` § Operations Plan, § Cross-cutting behaviours (Observability); `../contracts/todo-api-v1.md` § Operational endpoint; `../research.md` R3, R8.
