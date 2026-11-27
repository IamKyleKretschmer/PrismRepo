---
id: T018
title: Create the repository context for the To-Do service
feature: todo-list-api
phase: "Phase 5: Polish & Cross-Cutting Concerns"
story: null
repository: PrismRepo
parallel: true
depends_on: [T008, T009, T011, T012]
effort: 3
status: pending
---

# T018 — Create the repository context for the To-Do service

## Objective
PrismRepo has a `/.context/` layer describing the To-Do service as built, so future agents and contributors source architecture, conventions and build/test commands from the repository instead of the feature's design documents.

## Scope
**In scope** — a `/.context/` with a manifest covering the service's containers and components, implementation patterns, testing conventions (xUnit unit and in-process acceptance tests, fresh data file per run), build/test/run commands, CI, configuration and operational constraints (single instance, persistent local data file); linking it from the solution-level context if one exists.
**Out of scope** — changes to the service itself.

## Behaviour & Contract
- Repository role is single-repo (`prism` + `code`, `/.prism/config/repos.yml`), so a separate solution-level context is not required.
- Constraint: built with the `generate-context-repo` skill, as Prism prescribes for every `code`-role repository; content must reflect the code as it exists, not the plan.

## Preconditions & Dependencies
- All story tasks complete (T008, T009, T011, T012), so the context describes the finished service.

## Acceptance Criteria
- [ ] `/.context/manifest.yml` exists and lists every context file.
- [ ] Testing conventions, build/test commands and the operational constraints are documented and match the repository.
- [ ] A reviewer can find from the context alone how to run the service and both test suites.
- [ ] No secrets appear.

## Testing Requirements
- **Manual**: follow the build and test commands in the context on a clean clone; they succeed.

## Operational Requirements
N/A.

## Documentation Requirements
This task is the documentation deliverable: `/.context/` for PrismRepo. (`../plan.md` § Test Strategy notes it does not exist yet.)

## References
`../plan.md` § Project Structure, § Test Strategy; `/.prism/config/repos.yml`; `../research.md` R1, R11.
