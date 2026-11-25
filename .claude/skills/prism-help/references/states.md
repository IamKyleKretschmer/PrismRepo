# Prism Feature States

The following defines that various states in which a Prism feature might be.

**NOTE: Non-linear transitions**

The process is flexible and states are not strictly one-way. Any artefact, spec, plan, contracts, tasks or code, may
be changed at any point. When making a change, pick the agent that targets the earliest stage being modified and let it
update the downstream artefacts, then re-run the later stages. Prefer agent-driven updates over hand-editing artefacts.

**NOTE: Agent names**

The specify, plan and tasks phases each have a Prism agent (`prism-specify`, `prism-plan`, `prism-tasks`) and a
deprecated Spec Kit one (`speckit-specify`, `speckit-plan`, `speckit-tasks`). Recommend the Prism agent. Name the Spec
Kit one only when the feature is visibly on that older route, or when the user asks about it. The remaining agents
(`speckit-analyze`, `speckit-implement`, `speckit-converge`, `speckit-taskstoissues`, `speckit-checklist`) have no
Prism equivalent. Agent names are hyphenated; the dotted form (`speckit.plan`) is the old spelling and still appears in
older features and chat histories.

## 00: No Feature In Scope

No feature is currently in scope. A non-default branch is checked out, but there is no evidence that the branch is
connected to a Prism feature. This state typically represents a Context-Driven-Development branch where the developer
has chosen to handle feature development manually rather than use a Spec-Driven-Development process.

This state is not an error, but it means Prism has very limited visibility into the intent, goals, and progress of the
feature. Use agents and skills adhoc to generate and review code, or for research and troubleshooting.

## 01: Ready for New Feature

No feature is currently in scope, but the current branch is the default branch. The history is clean, and so Prism is
ready to define a new feature.

Use the `prism-specify` agent to get started on a new feature.

## 02: Spec in Progress

There is good evidence that the user is in the process of defining a new spec. For example, the user has checked out a
branch which is synchronized with the default branch and there are unstaged changes which include a spec.md. Or,
FEATURE_JSON.feature_directory appears to match the current branch, there is a spec.md in the branch history which is
not in the default branch, and the spec.md does not appear to be complete (for example, it still contains "NEEDS
CLARIFICATION" placeholders).

Use the `prism-specify` agent to continue working on and refining the spec.

## 03: Spec Drafted. Ready for Quality Checklist

The feature directory contains a spec.md which appears to be complete, describing the _what_ and the _why_ of the
feature with no "NEEDS CLARIFICATION" placeholders left, but `checklists/requirements.md` is missing or still has
unresolved items.

`prism-specify` writes and validates that checklist itself as part of the specify phase, so a feature sitting here
usually means the specify run did not finish. Re-run `prism-specify`. On the deprecated `speckit-specify` route the
checklist is a separate step: use `speckit-checklist` to produce it.

The checklist also covers the spec's **User stories** table and its requirement coverage: every functional requirement
must be claimed by exactly one story. Those `US` identifiers drive the plan's structure and the whole tasks phase and
are expensive to change later, so this is worth getting right before moving on.

## 04: Spec Checked. Ready for Grilling

The feature directory contains a complete spec.md and a `checklists/requirements.md` whose items are all resolved, but
there is no evidence that the spec has been stress-tested. For example, the feature directory contains no companion
spec files (`spec-{role}.md`) and spec.md's `## Companion Specifications` table is empty.

Use the `prism-grill-me` agent to clarify under-specified areas from each role in turn (PM, UX, architecture, QA,
devops). Each discipline should grill its own role: QA in particular should produce a `spec-qa.md` covering the
acceptance criteria, which the plan phase turns into its test strategy. The UX and AO may also attach their own
supporting material (Figma references, screenshots, POC code, interface or data-model requirements) and link it from
spec.md.

Companion files are never merged into spec.md. Each one needs a row in spec.md's `## Companion Specifications` table,
because that table is how the later phases discover them.

## 05: Spec Complete. Ready for Technical Plan

The feature directory contains a reviewed spec.md, its companion spec files, and a resolved
`checklists/requirements.md`. There is no plan.md. The spec has been reviewed by the PM, UX, EM and AO, and the
acceptance criteria have been reviewed by the testers.

Use the `prism-plan` agent to generate the technical plan. Note that planning is deliberately just-in-time: the
technical plan goes stale quickly as the codebase moves, so a feature may legitimately sit in this state for some time
and the plan should only be generated when a dev is ready to review and implement it.

## 06: Technical Plan in Progress

There is evidence that the plan phase has started but is not finished. For example, plan.md exists but one or more of
its companion artefacts (research.md, data-model.md, quickstart.md) is missing, plan.md still contains unresolved
placeholders or is missing one of its required sections (Requirement Coverage, Feasibility & Risks, Test Strategy), or
the plan artefacts are present as unstaged or uncommitted changes. There is no tasks.md.

Use the `prism-plan` agent to continue generating and refining the plan artefacts.

Note that `contracts/` is produced only when the feature introduces or changes an external boundary. Its absence is not
by itself evidence of an unfinished plan.

A plan produced by the deprecated `speckit-plan` route has no `## Requirement Coverage` section. If such a feature has
already moved on to tasks (a `tasks.md` with no `tasks/` directory), the section is not required and the plan is treated
as complete. If it never reached tasks, it reports here permanently; a `prism-plan` re-run adds the missing sections.

## 07: Technical Plan Complete. Ready for Tasks

plan.md and its companion artefacts are present and complete, and there is no tasks.md.

This is the main review gate. A lot of the technical development work is done here, so the whole affected dev team,
PM, EM, AO, UX, QA, devs and ops, must agree on the plan before moving on. Skipping this review is the single biggest
source of downstream rework.

The plan is written at architectural altitude: directory-level components, boundary contracts and data ownership, not
code or file-level detail. Its `## Requirement Coverage` section is the one place that checks every requirement reaches
a component, so read that first.

Once the plan is agreed, use the `prism-tasks` agent to generate the task breakdown.

## 08: Tasks in Progress

tasks.md exists but does not yet appear to be a complete, reviewed, phased breakdown of the work. For example it still
contains placeholders, the `tasks/` directory is missing or does not hold one file per index line, it is present only
as unstaged changes, or its phases do not cover the code, tests, infrastructure and documentation implied by the plan.

Use the `prism-tasks` agent to continue generating and refining the task breakdown.

## 09: Tasks Complete. Ready for Work Items

The task breakdown is complete and reviewed, but the tasks are not yet tracked. There is no evidence of Azure DevOps
work items or GitHub issues having been created from them.

`prism-tasks` writes two things: `tasks.md`, an index of phases, ordering and dependencies, and one file per task under
`tasks/` (`tasks/T001-{slug}.md`) holding that task's authoritative definition. Tasks are deliberately black-box: they
state what must become true, not how to build it, and the implementer derives the how from the plan and the target
repo's `/.context/` at implement time. A feature whose tasks.md holds the task definitions inline predates this layout
and came from `speckit-tasks`.

Use the `prism-tasks` agent to create the Azure DevOps work items. It applies the `create-ado-work-item` skill and the
team settings in `/.prism/config/work-item-parameters.yml`, creating one PBI per user story that lists and links that
story's task files. For a GitHub-hosted solution use the `speckit-taskstoissues` agent instead. Before generating Azure
DevOps work items, check that `azure-devops-parent-feature-id` is set at the top of spec.md so each PBI is linked to
its parent feature.

## 10: Work Items Created. Ready for Analysis

The work items or issues exist and match the task breakdown, but the feature's artefacts have not been checked against
each other.

Use the `speckit-analyze` agent to run a cross-artefact consistency and coverage analysis over spec.md, plan.md and the
tasks. This runs after task generation and before implementation.

## 11: Ready for Implementation

The spec, plan and tasks are complete, consistent and reviewed, the work items exist, and no task has been started:
none of the index items in tasks.md are ticked and there are no corresponding changes in the repositories holding the
`code` role.

Use the `speckit-implement` agent, prompting it with a single task at a time. Keep the implementation session separate
from the SDD session, and take care to keep the implementation branch separate from the spec and plan branches.

**Gate, ask the user.** `speckit-analyze` is read-only and leaves no file behind, so nothing on disk can prove it ran.
Ask: has the cross-artefact analysis been run over spec.md, plan.md and the tasks, and were any findings addressed?

- **Cleared** - treat this state as reached and recommend its action above (`speckit-implement`).
- **Not cleared / unsure** - stay at the previous state and explain that running `speckit-analyze` is what clears this.

## 12: Implementation in Progress

Some but not all tasks in the index are ticked off, and there are corresponding changes in the repositories holding the
`code` role.

Use the `speckit-implement` agent to implement the next task. Before considering a task done, confirm every acceptance
criterion in that task's file is genuinely met, along with its Testing, Operational and Documentation requirements.
Tests are often deferred until the code is approved and then forgotten.

## 13: Implementation Drifted

There is evidence that the implementation and the SDD artefacts no longer agree. For example the index is fully ticked
but the described behaviour is not present in the code, or substantial work has been done in the `code` repositories
outside the task list.

Use the `speckit-converge` agent to assess the codebase against the spec, plan and tasks and append any remaining
unbuilt work as new tasks, then return to state 12 to complete it. Note that Prism takes a _spec-informed_ rather than
_spec-anchored_ view: late changes are not reverse-propagated back up to the spec, so drift in the narrative of spec.md
and plan.md is expected and is not by itself a reason to enter this state.

**Gate, ask the user.** Detecting drift needs semantic comparison, which no file or git check can perform. Ask: does
the implementation still match the spec, plan and tasks, or has `speckit-converge` already been run to reconcile drift?

- **Cleared** - drift was resolved, which returns the feature to state 12; recommend state 12's action, not this one's.
- **Not cleared / unsure** - stay at the previous state and explain that running `speckit-converge` is what clears this.

## 14: Implementation Complete. Ready for PR

Every task in the index is ticked off, and the code, tests, operations artefacts and documentation for the feature
exist in the affected repositories.

Before opening the PR, ask for an agentic code review: open the task file, highlight it, and prompt for a review of all
changes made in the current branch against that task's requirements. QA should confirm that the tests covering the
original user acceptance criteria exist and pass.

## 15: Feature Merged

The feature branch has been merged into the default branch and the feature's artefacts are part of the default branch
history. Nothing remains to be done for this feature.

Return to state 01 to begin the next feature.
