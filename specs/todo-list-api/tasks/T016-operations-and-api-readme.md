---
id: T016
title: Complete the service README for operators and new callers
feature: todo-list-api
phase: "Phase 5: Polish & Cross-Cutting Concerns"
story: null
repository: PrismRepo
parallel: true
depends_on: [T008, T009, T011, T012]
effort: 2
status: pending
---

# T016 — Complete the service README for operators and new callers

## Objective
A developer new to the service can run it and complete create → list → complete in under 5 minutes using only the README, and an operator knows how to configure, run, back up and roll back it safely.

## Scope
**In scope** — consolidating the README sections added by earlier tasks into one coherent document: getting started, API reference with examples and every error `code`, required configuration, the persistent-local-storage requirement for the data file, restart and backup behaviour, single-instance constraint, health, logs and metrics, CI, and rollback (redeploy previous build against the same data file).
**Out of scope** — `/.context/` (T018); deployment runbooks for specific environments (no deployment target in scope).

## Behaviour & Contract
- Content must agree with `../contracts/todo-api-v1.md` and `../plan.md` § Operations Plan; where they conflict, the contract wins and the conflict is raised.
- The data file must be on persistent local storage, never network or ephemeral storage (`../plan.md` § Feasibility & Risks).
- No secret values appear in documentation.

## Preconditions & Dependencies
- All story tasks complete (T008, T009, T011, T012).

## Acceptance Criteria
- [ ] Someone who did not build the service follows the README alone and completes create → list → complete in under 5 minutes (spec Goals).
- [ ] Every required setting, every operation and every error `code` in the contract is documented.
- [ ] The persistence, single-instance, backup and rollback guidance from the Operations Plan is present.
- [ ] No secrets appear.

## Testing Requirements
- **Manual**: a timed walkthrough by someone who did not implement the feature, recorded in the pull request.

## Operational Requirements
Documents the operational surface; no change to it.

## Documentation Requirements
This task is the documentation deliverable: the repository README.

## References
Spec § Goals ("easy to adopt"); `../plan.md` § Operations Plan, § Feasibility & Risks; `../contracts/todo-api-v1.md`; `../quickstart.md`.
