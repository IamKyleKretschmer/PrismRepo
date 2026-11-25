# Implementation Plan: To-Do List API

**Branch**: `feat/todo-list-api` | **Date**: 2026-11-25 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/todo-list-api/spec.md`, with companion [spec-qa.md](spec-qa.md)

## Summary

This plan builds a new, single-instance HTTP service that lets authenticated callers create to-do items (FR-1, FR-2), list them in a stable order (FR-3, FR-4), and mark them complete idempotently (FR-6 to FR-9). Data survives restarts (FR-5, FR-6). The service is built in C# on .NET 10, keeps its data in an embedded SQLite file, and authenticates every call with a Vibranium JWT. The external surface is one versioned contract, [contracts/todo-api-v1.md](contracts/todo-api-v1.md).

## Technical Context

**Language/Version**: C# on .NET 10 LTS (research R2)
**Primary Dependencies**: ASP.NET Core minimal APIs, the ASP.NET Core JWT bearer authentication handler, and an SQLite data provider
**Storage**: Embedded SQLite file in WAL mode, owned by the service (research R3)
**Target Platform**: Any platform .NET 10 supports. The service runs as a single instance with persistent local storage for the data file
**Project Type**: Web service, with HTTP API only and no UI
**Quality-attribute targets**:
- *Integrity*: no partial or lost writes. Every state change is one database transaction (research R3, R8).
- *Concurrency*: 50 parallel creates give 50 distinct items, and parallel completes of one item give one completion time (spec-qa). Met by the transactional sequence and the conditional complete (research R4, R8).
- *Performance*: listing a few hundred items returns in under 200 ms p95 on one instance. Met by an embedded store with no network hop and lists of a bounded size (spec Assumptions).
- *Security*: every operation requires authentication (research R9).

**Constraints**: One instance per data file. Scaling out requires a different store (see Feasibility & Risks).
**Operations Impact**: There is a new CI pipeline, the service needs runtime JWT configuration, a persistent location for the data file, and a health endpoint. See Operations Plan.

## Constitution Check

*Before Phase 0: **PASS**. After Phase 1 design: **PASS** (re-checked against the research, data model and contract; nothing changed).*

| # | Principle | Result | Evidence |
| - | --------- | ------ | -------- |
| 1 | Customer trust & enterprise quality | Pass | Quality targets above. Error contract designed for diagnosing problems |
| 2 | Simplicity | Pass | Three operations, one service, embedded store, no speculative extension points |
| 3 | Explicit contracts | Pass | `/v1` contract with stable error codes and additive-only changes (research R7, R10) |
| 4 | Security & privacy | Pass | Approved Vibranium JWT profile with no header fallback (research R9). Titles kept out of logs |
| 5 | Data integrity | Pass | Transactional writes, idempotent complete, invariants in [data-model.md](data-model.md) |
| 6 | Reliability | Pass with caveat | Single instance. Recovery after restart is guaranteed by the durable store, but availability depends on that one instance (Feasibility & Risks) |
| 7 | Observability | Pass | Structured logs, request metrics, health endpoint (Operations Plan) |
| 8 | Testability | Pass | Test Strategy, derived from spec-qa |
| 9 | Accessibility & localisation | Pass | No UI. Stable error codes let consumers localise messages |
| 10 | Sustainable delivery | Pass | New service with no existing consumers. Rolling back means redeploying the previous build |

No violations, so Complexity Tracking is empty.

## Requirement Coverage

| Requirement | Covered by | Owning component | Reason / location |
| ----------- | ---------- | ---------------- | ----------------- |
| FR-1 | US1 | PrismRepo: `src/TodoApi/Api/`, `src/TodoApi/Items/`, `src/TodoApi/Persistence/` | |
| FR-2 | US1 | PrismRepo: `src/TodoApi/Items/` | |
| FR-3 | US1 | PrismRepo: `src/TodoApi/Persistence/` | |
| FR-4 | US1 | PrismRepo: `src/TodoApi/Api/` | |
| FR-5 | US1 | PrismRepo: `src/TodoApi/Persistence/` | |
| FR-6 | US2 | PrismRepo: `src/TodoApi/Api/`, `src/TodoApi/Persistence/` | |
| FR-7 | US2 | PrismRepo: `src/TodoApi/Persistence/` | |
| FR-8 | US2 | PrismRepo: `src/TodoApi/Api/` | |
| FR-9 | US2 | PrismRepo: `src/TodoApi/Items/`, `src/TodoApi/Persistence/` | |

## Feasibility & Risks

**Verdict**: Feasible with caveats. The scope is small and well understood. The caveats concern the environment and scale, not the design.

- **Assumptions**:
  - *A Vibranium issuer, audience and signing keys are available for the deployment environment.* If they are not, the service can't authenticate anyone outside tests, and the work is blocked until they are provided.
  - *One instance is enough* (spec Assumptions). If more instances are needed, SQLite must be replaced with a shared database. The data model and contract stay the same, and only `Persistence/` changes.
  - *Lists stay at hundreds of items.* At much larger sizes, the full list without paging would breach the performance target, and paging would have to be added to v1.
- **Risks**:
  - *The .NET 10 SDK is not installed on the development machine* (only older runtimes are). Likelihood: certain. Impact: low. Mitigation: install the SDK before implementation starts. CI installs its own.
  - *The data file is placed on non-persistent or network storage.* Likelihood: medium. Impact: high (data loss breaks FR-5). Mitigation: the data file location is required configuration, the Operations Plan documents that it must be persistent local storage, and the health check fails if the file can't be written.
  - *Grapheme counting differs between consumers and the service.* Likelihood: low. Impact: low. Mitigation: the contract documents the rule, and boundary tests cover emoji and combined accents.
- **Open questions**:
  - The Azure DevOps parent Feature ID is still missing (spec Open questions). It doesn't block the plan, but it is needed before work items are created.
  - Who provisions the Vibranium configuration for each environment? The answer changes only when the first deployment can happen, not the design.
- **Blast radius**: None to existing systems. This is a new, standalone service with no consumers yet.

## Project Structure

### Documentation (this feature)

```text
specs/todo-list-api/
├── spec.md / spec-qa.md
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
└── contracts/
```

### Affected repositories & components

| Repository | Role | Components / modules touched | New or changed |
| ---------- | ---- | ---------------------------- | -------------- |
| PrismRepo | prism + code (single-repo) | `src/TodoApi/Api/`: the HTTP boundary, authentication and error mapping | New |
| PrismRepo | prism + code | `src/TodoApi/Items/`: item rules (title validation, completion semantics) | New |
| PrismRepo | prism + code | `src/TodoApi/Persistence/`: the embedded store, transactions, ordering | New |
| PrismRepo | prism + code | `tests/TodoApi.UnitTests/`, `tests/TodoApi.AcceptanceTests/` | New |
| PrismRepo | prism + code | `.github/workflows/`: CI pipeline | New |

**Structure Decision**: One deployable service under `src/TodoApi/`, split into three folders: HTTP boundary, item rules and persistence. The item rules don't depend on HTTP or storage, so they can be unit tested on their own. Tests sit in a separate `tests/` tree, one project per test level.

**Operational Ownership**: PrismRepo owns the pipeline, runtime configuration and health endpoint.

## Boundaries & Contracts

| Boundary | Owner (repo) | Consumers | Purpose | Compatibility impact |
| -------- | ------------ | --------- | ------- | -------------------- |
| To-Do API v1 (HTTP) | PrismRepo | Integrations (Eric), low-code solutions (Carmine) | Create, list and complete to-do items | New. Changes within v1 may only add things |

The details are in [contracts/todo-api-v1.md](contracts/todo-api-v1.md).

## Component Interactions

### US1 — Add to-do items and see the full list

- **Flow (create)**: `Api/` authenticates the caller → `Items/` validates the title (FR-2) → `Persistence/` inserts the item and assigns the creation time and sequence in one transaction → `Api/` returns `201`.
- **Flow (list)**: `Api/` authenticates → `Persistence/` reads every item ordered by creation time, then sequence → `Api/` returns `200`, with an empty array if there are none.
- **Sync/async**: Everything happens synchronously within the request.
- **Consistency & transaction boundaries**: One transaction per create. A list reads a consistent snapshot. Nothing is eventually consistent.
- **Failure handling**: A validation failure returns `400` before storage is touched. A storage failure rolls the transaction back and returns `500` with a generic problem body, and the error is logged. The service never retries on the caller's behalf, because create is not idempotent.

### US2 — Mark a to-do item complete

- **Flow**: `Api/` authenticates → `Persistence/` sets the completion time only if none is set, and returns the item in its current state, all in one transaction → `Api/` returns `200`, or `404` if no item matched (FR-8).
- **Sync/async**: Synchronous.
- **Consistency & transaction boundaries**: A single conditional update. When completes run in parallel, the first one to commit sets the time and the rest see it (FR-7, FR-9).
- **Failure handling**: Unknown, malformed or empty identifiers are all handled the same way and get `404`. On a storage failure the transaction rolls back and the service returns `500`. Callers can safely retry, because complete is idempotent.

### Cross-cutting behaviours

| Principle | What it constrains | Architectural choice satisfying it | How it is verified |
| --------- | ------------------ | ---------------------------------- | ------------------ |
| 4 Security (auth standard) | Every operation | Vibranium JWT on every endpoint except `/health`. Identity from `sub` only, with `ntx-user-id` ignored (research R9) | Acceptance tests: missing, expired and invalid tokens get `401`, and the `ntx-user-id` header alone is rejected |
| 4 Privacy | Logs and telemetry | Titles and token contents are never logged. Caller identity is logged only as a correlation-safe subject identifier, and only at debug level | Unit test of the log scrubbing, plus code review against this row |
| 5 Data integrity | Every write | One transaction per state change. Invariants in [data-model.md](data-model.md) | The "error changes nothing" checks from spec-qa, plus restart tests |
| 7 Observability | Every request | Structured request logs with correlation ID, status code and error `code`. Request count and latency metrics. `/health` checks the database is reachable | Acceptance test of `/health`, and a log assertion on one error path |
| 9 Localisation | Error responses | A stable `code` on every error, so consumers localise from it (research R7) | Contract tests check `code` values |
| 3 Compatibility | Contract | `/v1` prefix. Changes within v1 may only add things | Contract tests fail on removed or renamed fields |

## Data Ownership & Invariants

| Data | System of record | Read by | Lifecycle | Invariants |
| ---- | ---------------- | ------- | --------- | ---------- |
| To-do item | PrismRepo: `src/TodoApi/Persistence/` (embedded file) | API callers via v1 | Created and completed. Never edited or deleted | See [data-model.md](data-model.md), invariants 1–7 |

## Operations Plan

- **Resource / infrastructure changes**: A persistent, local writable location for the data file in each environment. No other infrastructure is needed.
- **Deployment / pipeline changes**: A new GitHub Actions workflow that runs on every PR and push to `main`: restore, build, unit tests, acceptance tests. Merging is blocked unless it passes (the spec-qa release gate). Deployment targets are out of scope for this feature. The build output is a deployable service package.
- **Observability / supportability**: Structured logs to stdout, OpenTelemetry-compatible request metrics, and `/health` for liveness and readiness. A short README section covers configuration and the restart and backup behaviour of the data file.
- **Rollout / configuration**: Required configuration is the data file path and the JWT issuer, audience and signing key source. The service refuses to start if any of these is missing. Secrets come from the environment and are never committed to the repository. There are no feature flags, because the service is new. Rolling back means redeploying the previous build against the same data file. The schema is created when the service first starts, and v1 needs no migrations.
- **Operational verification**: Running the CI pipeline proves the gate works. Starting the service with configuration missing proves it fails fast. The acceptance suite runs the `/health` check.

## Test Strategy

Derived from [spec-qa.md](spec-qa.md).

| Level | What it proves here | Repository & suite | Framework |
| ----- | ------------------- | ------------------ | --------- |
| Unit | Title rules and their boundaries (FR-2), plus the log scrubbing | PrismRepo: `tests/TodoApi.UnitTests/` | xUnit |
| Acceptance (black-box, in-process host) | Every FR through the real HTTP contract, including restart, concurrency, ordering, error codes and auth | PrismRepo: `tests/TodoApi.AcceptanceTests/` | xUnit with the ASP.NET Core test host |

There is no `/.context/` in this repository yet, so the frameworks above come from the stack decision (research R11) rather than from repository context.

- **US1**: create returns a complete item. All FR-2 boundary cases are rejected or accepted as spec-qa sets out, with the right `code`, and nothing changes after a rejection. An empty list gives an empty array. The order is exact, including after 50 parallel creates. Items survive a restart.
- **US2**: completing shows the item as complete. A repeat complete keeps `completedAt`. 10 parallel completes give one completion time. Unknown and malformed identifiers both give `todo_not_found`, with the identifier named. Completion survives a restart.
- **Cross-cutting**: the auth rejection cases and `/health` from the table above.
- **Test data & environments**: Each acceptance test run uses a fresh temporary data file, so it starts empty (spec-qa isolation decision). A test JWT signing key is set only in the test host. Restart tests stop and restart the host against the same file.
- **Release gate**: CI must pass before merging. QA confirms at least one passing acceptance test for every FR before the feature is done (spec-qa).
- **Coverage gaps**: No load or soak testing beyond the concurrency checks, because the expected list sizes are small. Real Vibranium issuer integration is not covered in CI, because CI uses a test key. It is verified once in the first deployed environment.

## Complexity Tracking

No violations.
