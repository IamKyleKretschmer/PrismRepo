---
id: T001
title: Scaffold the service and its test suites
feature: todo-list-api
phase: "Phase 1: Setup"
story: null
repository: PrismRepo
parallel: false
depends_on: []
effort: 2
status: pending
---

# T001 — Scaffold the service and its test suites

## Objective
An empty, runnable To-Do service and two empty test suites (unit, acceptance) exist in the repository and build and run with one command each, so every later task has a home.

## Scope
**In scope** — a buildable service that starts and serves nothing yet; a unit test suite and an acceptance test suite that run; the solution layout described in `../plan.md` § Project Structure (HTTP boundary, item rules and persistence kept separate so item rules can be unit tested alone); ignore rules for build output and local data files.
**Out of scope** — CI (T002), configuration and health (T003), authentication (T004), any `/v1` operation (T006 onward).

## Behaviour & Contract
No consumer-visible behaviour. Mandated constraints:
- C# on .NET 10 LTS with ASP.NET Core minimal APIs — `../research.md` R2 (.NET 8 is out of support at release).
- Test framework per `../research.md` R11.
- Item rules must not depend on HTTP or storage (`../plan.md` Structure Decision).

## Preconditions & Dependencies
- .NET 10 SDK installed on the development machine (`../plan.md` § Feasibility & Risks — only older runtimes are installed today).

## Acceptance Criteria
- [ ] A clean clone builds the service and both test suites with no errors.
- [ ] Both test suites run from the command line and report success.
- [ ] The service starts locally and stops cleanly.
- [ ] The layout matches `../plan.md` § Project Structure; item rules have no reference to the HTTP or storage layers.
- [ ] Build output and local data files are ignored by git.

## Testing Requirements
- **Unit / acceptance**: each suite runs and is discoverable by the test runner. xUnit for unit tests; xUnit with the ASP.NET Core in-process test host for acceptance tests (`../plan.md` § Test Strategy, `../research.md` R11 — the repository has no `/.context/` yet).
- **Manual**: build and run from a clean clone.

## Operational Requirements
N/A — CI arrives in T002.

## Documentation Requirements
A short README section: prerequisites (.NET 10 SDK) and how to build, run and test. Configuration details follow in T016.

## References
`../plan.md` § Technical Context, § Project Structure; `../research.md` R2, R11.
