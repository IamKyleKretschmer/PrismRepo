---

description: "Task index for the To-Do List API feature"
---

# Tasks: To-Do List API

**Input**: Design documents from `/specs/todo-list-api/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), spec-qa.md, research.md, data-model.md, contracts/, quickstart.md

Each line below links to the task's authoritative definition under `tasks/`. This file holds ordering, dependencies and strategy only.

---

## Definition of Ready

Before starting work on a work item, ensure the following criteria are met:

- [ ] Work item has been estimated
- [ ] Figma mock-up reviewed, UX finalised
- [ ] Automation test scenarios defined (consult QA as needed)
- [ ] Performance and resilience considerations documented
- [ ] Relevant tags added
- [ ] Linked to feature (if required)
- [ ] Work Type Budget has been set

---

## Definition of Done

When work on a work item is complete, ensure the following criteria are met:

- [ ] Unit tests added for new atomic functionality
- [ ] Affected automated tests flagged with "TODO-QA"
- [ ] Automation scenarios included in Dev PR review
- [ ] Automation tests cover new functionality
- [ ] Technical documentation updated
- [ ] No code quality or security vulnerabilities
- [ ] All other "done" considerations resolved

---

**Tests**: Required by the Constitution (principle 8) and by spec-qa: every FR gets at least one acceptance test; FR-2 also gets unit tests.

## Phase 1: Setup (Shared Infrastructure)

**Repository**: PrismRepo

**Purpose**: A buildable service, runnable test suites, and a CI merge gate.

- [X] T001 Scaffold the service and its test suites → [tasks/T001-scaffold-service-and-test-suites.md](tasks/T001-scaffold-service-and-test-suites.md)
- [ ] T002 [P] Add CI pipeline that gates merging on all tests → [tasks/T002-ci-pipeline-merge-gate.md](tasks/T002-ci-pipeline-merge-gate.md)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Repository**: PrismRepo

**Purpose**: Configuration, health, authentication and safe logging that every `/v1` operation relies on.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T003 Start against a configured data file with fail-fast config and health endpoint → [tasks/T003-configured-startup-and-health.md](tasks/T003-configured-startup-and-health.md)
- [X] T004 [P] Require a valid Vibranium JWT on every v1 request → [tasks/T004-authenticate-v1-requests.md](tasks/T004-authenticate-v1-requests.md)
- [X] T005 [P] Emit structured request logs and metrics without sensitive data → [tasks/T005-request-logging-and-metrics.md](tasks/T005-request-logging-and-metrics.md)

**Checkpoint**: Service starts, `/health` answers, `/v1` is authenticated and logged safely.

---

## Phase 3: User Story 1 - Add to-do items and see the full list (Priority: Must have) 🎯 MVP

**Repository**: PrismRepo

**User Story**: As an engineer (Eric), I want to add to-do items and see the full list so that I can record and review outstanding work.

**Independent Test**: On a fresh instance, list returns `[]`; create two items; list returns both oldest first; invalid titles return `400` with the right `code` and change nothing; after a restart, the list is unchanged.

**Requirements**: FR-1, FR-2, FR-3, FR-4, FR-5

- [X] T006 [US1] Create a to-do item → [tasks/T006-create-todo-item.md](tasks/T006-create-todo-item.md)
- [X] T007 [US1] List all to-do items in a stable order → [tasks/T007-list-todo-items.md](tasks/T007-list-todo-items.md)
- [X] T008 [P] [US1] Reject invalid titles with a specific error code → [tasks/T008-reject-invalid-titles.md](tasks/T008-reject-invalid-titles.md)
- [X] T009 [P] [US1] Keep created items across restarts and parallel creates → [tasks/T009-durable-concurrent-creates.md](tasks/T009-durable-concurrent-creates.md)

**Checkpoint**: User Story 1 is fully functional and testable on its own.

---

## Phase 4: User Story 2 - Mark a to-do item complete (Priority: Must have)

**Repository**: PrismRepo

**User Story**: As an engineer (Eric), I want to mark a to-do item complete so that the list shows what is done and what is still outstanding.

**Independent Test**: Create an item, complete it, list shows `isComplete: true` with `completedAt`; completing again keeps `completedAt`; unknown and malformed identifiers return `404 todo_not_found`; after a restart, completion is unchanged.

**Requirements**: FR-6, FR-7, FR-8, FR-9

- [ ] T010 [US2] Mark a to-do item complete idempotently → [tasks/T010-complete-todo-item.md](tasks/T010-complete-todo-item.md)
- [ ] T011 [P] [US2] Return not found for unknown or malformed identifiers → [tasks/T011-complete-unknown-item-not-found.md](tasks/T011-complete-unknown-item-not-found.md)
- [ ] T012 [P] [US2] Keep completion across restarts and parallel completes → [tasks/T012-durable-concurrent-completes.md](tasks/T012-durable-concurrent-completes.md)

**Checkpoint**: Both user stories work; the MVP is feature-complete.

---

## Phase 5: Polish & Cross-Cutting Concerns

**Repository**: PrismRepo

**Purpose**: Feature-wide verification of the plan's cross-cutting behaviours, documentation, repository context and the release gate.

- [ ] T013 [P] Verify authentication holds on every operation → [tasks/T013-verify-auth-on-every-operation.md](tasks/T013-verify-auth-on-every-operation.md)
- [ ] T014 [P] Verify privacy and observability across the feature → [tasks/T014-verify-privacy-and-observability.md](tasks/T014-verify-privacy-and-observability.md)
- [ ] T015 [P] Verify contract conformance, error codes and data integrity → [tasks/T015-verify-contract-and-integrity.md](tasks/T015-verify-contract-and-integrity.md)
- [ ] T016 [P] Complete the service README for operators and new callers → [tasks/T016-operations-and-api-readme.md](tasks/T016-operations-and-api-readme.md)
- [ ] T017 Validate quickstart scenarios and per-requirement test coverage → [tasks/T017-validate-quickstart-and-fr-coverage.md](tasks/T017-validate-quickstart-and-fr-coverage.md)
- [ ] T018 [P] Create the repository context for the To-Do service → [tasks/T018-create-repo-context.md](tasks/T018-create-repo-context.md)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: T001 first; T002 after T001.
- **Foundational (Phase 2)**: T003 after T001; T004 and T005 after T003. Blocks all user stories.
- **User Story 1 (Phase 3)**: T006 after T004 and T005; T007 after T006; T008 and T009 after T007.
- **User Story 2 (Phase 4)**: T010 after T007 (it needs the item store and list from US1); T011 and T012 after T010.
- **Polish (Phase 5)**: T013, T014, T015, T016 and T018 after T008, T009, T011 and T012; T017 last, after T013–T016.

### User Story Dependencies

- **US1**: no dependency on other stories.
- **US2**: depends on US1's create and list (T006, T007) — completing needs items to exist and the list to show the result. It does not depend on T008 or T009.

### Parallel Opportunities

- T002 runs alongside T003 once T001 is done.
- T004 and T005 run in parallel.
- T008 and T009 run in parallel; T010 can start alongside them once T007 is done.
- T011 and T012 run in parallel.
- T013, T014, T015, T016 and T018 run in parallel.

---

## Implementation Strategy

### MVP First

The spec makes both stories Must-have and together the MVP.

1. Phase 1 and Phase 2.
2. Phase 3 (US1) → validate its Independent Test.
3. Phase 4 (US2) → validate its Independent Test.
4. Phase 5, ending with T017 as the release gate.

### Incremental Delivery

US1 alone is a usable add-and-list service and can be demonstrated before US2 starts. US2 adds completion without changing US1's contract.

### Requirement Coverage

| Requirement | Tasks |
| ----------- | ----- |
| FR-1 | T006 |
| FR-2 | T008 |
| FR-3 | T007, T009 |
| FR-4 | T007 |
| FR-5 | T009 |
| FR-6 | T010, T012 |
| FR-7 | T010, T012 |
| FR-8 | T011 |
| FR-9 | T010, T012 |

Cross-cutting behaviours (`plan.md` § Cross-cutting behaviours) appear as acceptance criteria in T006–T012 and are verified feature-wide in T013 (security), T014 (privacy, observability) and T015 (compatibility, localisation, data integrity).
