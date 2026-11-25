---
applyTo: '**/specs/**/tasks.md'
---

# Task Generation Conventions

Apply these rules when generating or updating tasks.md files.

## Feature Branch Naming

This naming rule is scoped to `/speckit-tasks` only — it is not a general branch-naming requirement for manual or other agent-created branches.

Read [branch-and-folder-naming.instructions.md](<PRISM_ROOT>/.prism/instructions/branch-and-folder-naming.instructions.md) in full and apply its "Handing a feature branch to the next phase" section, with this phase-specific guard:

`/speckit-tasks` owns the `tasks` phase's branch, but only proceed with that section's steps if the current branch is NOT the repository's default branch (see "Determining the default branch and current state" in that file). Being on the default branch here means an earlier SDD phase's branch step never ran for this feature; leave the branch as-is and warn the user rather than creating a `tasks` branch directly off the default branch.

## Deferred Declaration Rules

**Interface Declaration**: Do NOT declare interfaces (e.g., `IService`, `IRepository`) as standalone tasks until a concrete class that implements them is also being created in the same user story. Declaring an interface speculatively (e.g., in the infrastructure phase before any concrete implementation exists) is not allowed. The interface task must accompany the task for its first concrete implementation.

**DTO and Data Model Declaration**: Do NOT create DTOs, entities, value objects, or other data model classes as standalone up-front tasks. They must be created in the same task, or an immediately adjacent task in the same user story, as the logic (service, handler, endpoint, UI component, etc.) that first uses them. Do not declare them speculatively in Setup, Foundational, or another earlier phase.

**Shared Type Placement**: If a type or interface is eventually used by multiple stories, place it in the earliest user story that requires a real implementation, not in a generic up-front phase. Later stories should extend or reuse that concrete implementation rather than introducing speculative shared declarations.

## Repository Scoping Rules

- **Single-repo default**: Each user story should default to making changes in only ONE repository. This enables independent implementation, testing, and deployment.
- **Multi-repo exceptions**: Setup/Infrastructure and Polish phases may span multiple repositories. Feature user stories should only span multiple repos when technically necessary (e.g., shared interfaces).
- **Explicit declaration**: Every user story MUST include a `**Repository**:` line immediately after the user story header, stating which repository/repositories are being modified.
- **Format**: `**Repository**: ntx-tables-api` or `**Repository**: Multiple (ntx-tables-api, ntx-tables-client)`

## Operations Coverage Rules

- **REQUIRED**: Task generation MUST include operations work in `tasks.md` whenever the plan or affected repositories indicate operational impact.
- **Up-front operations**: Resource provisioning, foundational deployment wiring, shared secrets/configuration, and other blocking platform prerequisites belong in Setup or Foundational only when they must exist before any user story can run.
- **Per-story operations**: Pipeline updates, release workflow changes, monitoring additions, feature-flag rollout steps, alerting, dashboard changes, and environment-specific configuration that support a specific capability MUST be included in the same user story that introduces that capability.
- **Do not batch by default**: Do NOT defer all operations work to a single up-front or polish phase if only some user stories require it.
- **Repository declaration**: If a user story includes operations work, pipeline definitions, deployment manifests, or monitoring assets, the `**Repository**:` line must include the repo that owns those files.
- **Verification**: Operations tasks should include how completion is verified where practical, such as pipeline validation, deployment dry-run, configuration diff, or monitoring signal confirmation.

### Task Format for Operations

- Blocking setup example: `- [ ] TXXX [P] Provision required Azure resources and baseline deployment configuration in operations/provision/`
- Per-story pipeline example: `- [ ] TXXX [US2] Update deployment pipeline for [capability] in operations/pipelines/[file]`
- Per-story monitoring example: `- [ ] TXXX [US3] Add alerts and dashboards for [capability] in operations/monitor/[file]`
- Per-story release example: `- [ ] TXXX [US1] Add feature flag rollout and environment configuration for [capability]`

## Documentation Update Strategy

Documentation updates MUST be included in the same user story that introduces the feature or endpoint being documented. When generating tasks, identify the documentation work that must be done to keep docs in sync with code changes and include those tasks in the same user story. Documentation that may need to be updated should be inferred from each repo - it includes but is not limited to:

- Code reference documentation (look for docs/)
- Backstage documentation (look for mkdocs.yml)
- README files (but don't bloat these with large conceptual docs - prefer separate files in docs/)
- End user documentation (if applicable, e.g., client library docs, API reference docs)
- Internal context documentation (/.context/)

### With Each Endpoint User Story (Incremental)

- **API reference documentation** (`/docs/api/`) should be updated WITH the user story that adds the endpoint
- The endpoint contract is already defined in `contracts/` - documentation should ship with the feature
- Aligns with Definition of Done: "Technical documentation updated"
- Enables independent deployment - if you ship an endpoint, its docs are ready
- Example: US3 adds GET /history endpoint → add `/docs/api/row-history.md` in same user story

### In Polish Phase (Batched)

- **Conceptual/overview documentation** (feature guides, tutorials)
- **mkdocs.yml navigation updates** (add all new pages at once for coherent structure)
- **/.context/ updates** (agent context, not user-facing)
- **Cross-repo documentation consistency checks**

### Task Format for Documentation

- API docs with endpoint: `- [ ] TXXX [USn] Add API documentation for [endpoint] in docs/api/[feature].md`
- mkdocs.yml in Polish: `- [ ] TXXX [P] Update mkdocs.yml navigation to include Row History documentation`
- Conceptual in Polish: `- [ ] TXXX [P] Create Row History feature overview in docs/overview/row-history.md`

## Small & Independent Work Items

Each task MUST be a small, atomic unit of work that can be completed independently:

1. **Size Principle**: Each task should be completable in a single focused session (ideally 1-4 hours of work)
2. **Independence Principle**: Each task should be mergeable/deployable without requiring other incomplete tasks
3. **Stub Strategy**: When a task depends on code that doesn't exist yet:
   - Create a **separate stub task** first only when the stub is immediately required to unblock a concrete implementation in the SAME user story
   - The dependent task then implements against the stub in that story
   - Do NOT create speculative stubs in Setup or Foundational just to prepare for later stories
   - Example: Within US1, before "Implement UserService", have "Create IUserService interface with stub implementation"
4. **Vertical Slicing**: Prefer thin vertical slices over horizontal layers
   - WRONG: "Create all models", "Create all services", "Create all endpoints"
   - CORRECT: "Create User model + UserService stub + GET /users endpoint" (minimal working feature)
5. **Clear Boundaries**: Each task should have clear entry/exit criteria
   - What files are created/modified
   - What interfaces/contracts are satisfied
   - How to verify completion (compile, test, manual check)
   - What operational outcome is enabled, if the task touches deployment, pipelines, monitoring, or runtime configuration

**Stub Task Examples**:
- `- [ ] T003 [US1] Create ITableService interface with NotImplementedException stubs in src/services/ITableService.cs`
- `- [ ] T004 [US1] Create TableService skeleton implementing ITableService with TODO markers in src/services/TableService.cs`
- `- [ ] T005 [US1] Implement GetTable method in TableService`

## Context Documentation in Polish Phase

- **REQUIRED**: The Final/Polish phase MUST include task(s) to update `/.context/` files in affected repositories
- Example: `- [ ] T099 Update /.context/ files to document new [feature name] components in [repo]/.context/`
- Update module-specific context files if relevant (discovered via manifest.yml)
- Include operational context updates when the feature changes pipelines, deployment flows, monitoring, or runtime configuration

## Stub Identification

When generating tasks, identify dependencies and create stub tasks:

1. **Cross-service dependencies**: If ServiceA calls ServiceB, create `IServiceB` stub first
2. **External API dependencies**: Create mock/stub implementations for external services
3. **Database dependencies**: Create in-memory or stub repository implementations
4. **UI dependencies on API**: Create API contract stubs that UI can develop against

Stub identification must stay within the user story that first needs the dependency unless the stub is a true cross-cutting platform prerequisite.

Mark stub tasks with `[STUB]` label for clarity:
- `- [ ] T002 [STUB] Create IFilesService interface with stub implementation`

## Acceptance Criteria

- Ensure each task has clear acceptance criteria that define what "done" looks like. These must be suitable for QA verification of each task and could include:
  - Functional requirements (e.g., "GET /users returns 200 with user list")
  - Interface contracts
  - Operational outcomes enabled (e.g., pipeline updates, monitoring signals)
  - Documentation updates 

  ## Testing requirements

- Separate to the acceptance criteria, each task should specify the testing requirements, including:
  - Unit tests to be added/updated
  - Integration tests to be added/updated
  - Manual testing steps if applicable
  - Test data setup if needed

Each category of tests should briefly include their technology stack (e.g., "Add xUnit unit tests for UserService", "Add Postman integration tests for GET /users")