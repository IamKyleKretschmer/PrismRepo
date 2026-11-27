---
id: T002
title: Add CI pipeline that gates merging on all tests
feature: todo-list-api
phase: "Phase 1: Setup"
story: null
repository: PrismRepo
parallel: true
depends_on: [T001]
effort: 3
status: pending
---

# T002 — Add CI pipeline that gates merging on all tests

## Objective
Every pull request and every push to the default branch runs restore, build, unit tests and acceptance tests in CI, and a pull request cannot merge unless that run passes.

## Scope
**In scope** — a GitHub Actions pipeline that installs its own .NET 10 SDK, builds the service and runs both test suites; publishing the deployable service package as a build output; making the pipeline a required check for merging.
**Out of scope** — deployment to any environment (`../plan.md` § Operations Plan: deployment targets are out of scope); test content (story tasks).

## Behaviour & Contract
- Triggers: every pull request and every push to the default branch.
- Fails if build, unit tests or acceptance tests fail (spec-qa release-gate decision).
- No secrets are needed or committed; acceptance tests use a test signing key that exists only in the test host (`../research.md` R11).
- Constraint: GitHub Actions, because the remote is on GitHub (`../research.md` R11).

## Preconditions & Dependencies
- T001 (buildable service and runnable suites).
- Repository admin access to make the check required.

## Acceptance Criteria
- [ ] A pull request shows the pipeline running and passing on the scaffolded code.
- [ ] A deliberately failing test on a throwaway branch makes the pipeline fail and blocks merging.
- [ ] The pipeline run produces a deployable service package artefact.
- [ ] No secret values appear in the pipeline definition or its logs.

## Testing Requirements
- **Manual**: the pass and deliberate-fail runs above, recorded on the pull request.
- **Unit / acceptance**: N/A — the pipeline runs the xUnit suites other tasks populate (`../research.md` R11).

## Operational Requirements
New CI pipeline and a branch-protection required check. Verified by the pass/fail pipeline runs above and by branch protection showing the check as required.

## Documentation Requirements
README: what CI runs and that it gates merging.

## References
`../plan.md` § Operations Plan, § Test Strategy (Release gate); `../spec-qa.md` (CI release gate); `../research.md` R11.
