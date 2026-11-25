# SDD new feature journey

How to use spec-driven development to specify a new feature, create a technical plan, tasks and work items, and implement the tasks.

- [spec-kit detailed process](https://github.com/github/spec-kit?tab=readme-ov-file#-detailed-process)
- [spec-kit comprehensive guide](https://github.com/github/spec-kit/blob/main/spec-driven.md#streamlining-sdd-with-commands)

## Agent overview

All files are generated into the feature folder except the final output.

Each of the first three phases has a Prism-owned agent and a deprecated Spec Kit one. **Use the Prism agent.** It carries its rules internally, produces artefacts at the altitude Prism expects, and is the route the rest of this guide assumes. The Spec Kit agents still work and are listed so you can recognise them in older features and existing chat histories.

| Phase | Agent (preferred) | Deprecated alternative | Generated files |
| ----- | ----------------- | ---------------------- | --------------- |
| Specify | prism-specify | speckit-specify | `/specs/{feature-name}` feature folder, `spec.md`, and `checklists/requirements.md` |
| Plan | prism-plan | speckit-plan | `plan.md`, `research.md`, `data-model.md`, `contracts/`, `quickstart.md` |
| Tasks | prism-tasks | speckit-tasks | `tasks.md` (index) and one file per task in `tasks/` |
| Implement | speckit-implement | — | Implementation files in affected repos |

These agents have no Prism equivalent and are used alongside the phases above:

| Agent | Description |
| ----- | ----------- |
| prism-grill-me | Clarify underspecified areas from multiple role perspectives (recommended before planning). Adds `spec-{role}.md` companion files and links them from `spec.md`. |
| speckit-checklist | Generate custom quality checklists validating requirements completeness, clarity and consistency — like "unit tests for English". Only needed on the deprecated specify route; **prism-specify writes its own checklist.** |
| speckit-analyze | Cross-artefact consistency and coverage analysis. Run after the tasks phase, before implementing. |
| speckit-taskstoissues | Create GitHub issues from the task list. |
| create-ado-work-item | Create Azure DevOps work items from the task list, using the team settings in `/.prism/config/work-item-parameters.yml`. |

**Names with the Claude plugin**: When Prism comes from the Claude plugin, every agent above runs under the plugin's `prism:` prefix. Prism's own agents drop their `prism-` prefix, and the rest keep their names:

| Prism CLI (file-based) | Claude plugin |
| ---------------- | ------------ |
| `prism-specify`, `prism-plan`, `prism-tasks` | `prism:specify`, `prism:plan`, `prism:tasks` |
| `prism-grill-me`, `prism-help` | `prism:grill-me`, `prism:help` |
| `speckit-implement`, `speckit-analyze` and the other `speckit-*` agents | `prism:speckit-implement`, `prism:speckit-analyze`, and so on |
| `create-ado-work-item`, `cdd` and Prism's other skills | `prism:create-ado-work-item`, `prism:cdd`, and so on |

A team skill with the same name as one of Prism's keeps working beside it: Prism's always carries the `prism:` prefix.

## Process flow

### High level

1. **Specification phase**
   1. **PM** uses **/prism-specify** with feature brief prompt: → `spec.md`
   2. **PM, QA, UX, EM, dev, ops, AO** use **/prism-grill-me** to review spec: → `spec-qa.md`, `spec-ux.md`, `spec-ao.md`, etc
   3. Team agree on implications, changes and compromises
2. **Plan phase**
   1. **Devs** use **/prism-plan**: `spec.md` → `plan.md`, `research.md`, `contracts/`, etc.
   2. **Devs** review the plans (manual + agent review)
   3. Iterate until the plans are approved
3. **Tasks phase**
   1. **Devs** use **/prism-tasks**: plan docs → `tasks.md` index + one file per task
   2. **Devs** use **/speckit-analyze**: reviews the tasks
   3. Iterate until the task list is validated
4. **Implement phase**
   1. **Devs** use **/speckit-implement** prompted for each phase separately: task files → Code, Tests, Dev ops, NDP docs, User docs, Agent context folder, ADO work items / GitHub issues
   2. **QA** use **/qa.validation** to validate test coverage
   3. **QA** use **/prism.detailed.tests** to add visual/expensive tests
   4. Iterate until the artefacts are complete
5. **Ready for PR**

> **Unsure which stage you're at?** Use the read-only `prism-help` agent to report where the current feature sits in this journey and which agent to run next. It is a helper, not an SDD stage — see [Getting help](overview.md#getting-help).

### Invoking agents

For each development stage below:

- select the correct agent - generally `prism-{stage}`.
- select the most appropriate model (Claude Opus, etc).
- start a new chat session.
- provide feature context to the agent by ensuring a file in the current feature folder (eg /specs/004-row-history/) is open and git is in the correct feature branch.
- ensure git changeset is empty before making any changes.
- run the agent via chat or command line.
- review the generated files and modify them using the agents.

### Review process

- this is the hard part - review thoroughly! Examine everything for correctness, appropriateness, conciseness and completeness.
- make changes one at a time using a general agent - it will update all impacted artefacts. Manual modification of artefacts should only be a last resort when the agents can't do it.
- *trim out anything that you would not have added yourself* - agents are too wordy and it causes context rot!

### Detailed process

#### F1. Specification

Generate the feature specification documentation.

- **PM** uses **/prism-specify** with feature brief prompt: → git feature branch, feature folder under `/specs`, containing `spec.md` and `checklists/requirements.md`
  - Example prompt: `Add row history view to each customer table. All changes made to a table row should appear in reverse-chronological order.`
  - Include UX input such as Figma files, images or POCs if desired. Include architectural requirements if desired.
- **Review** the `spec.md` as described above. Focus on the *what* and the *why* of the feature, but not the *how* or the tech stack. Modify it as required - modifying the spec files manually is fine.
  - Pay particular attention to the **User stories** table. Every functional requirement must be claimed by exactly one story, and those `US` identifiers drive the plan's structure and the entire task phase — they are expensive to change later.
- **Grilling**: Clarify the spec from multiple roles (QA, PM, devops, architecture, etc) to identify under-specified areas by invoking **`prism-grill-me`**. It will examine issues in detail and offer solution options. This will add a `spec-{role}.md` file for each role grilled, and reference these from `spec.md`.
- **Check**: `prism-specify` writes and validates `./checklists/requirements.md` itself. On the deprecated `speckit-specify` route, invoke the **`speckit-checklist`** agent to produce it.

> Note: best practise is to sync latest from your Prism remote branch before creating the spec, then commit and push the first cut of that spec as soon as the spec branch is generated. This avoids naming conflicts from multiple spec creations.

#### F2. Technical Plan

Generate the detailed technical plans from specs.

- Invoke **`prism-plan`** - optionally also include any planning decisions that have already been made: eg:
  > /prism-plan generate the technical plan from the spec. Note that the api contract entity for a person must contain the properties {PrincipalId, Type (User, Group, Service), FirstName, LastName, Email, DisplayName}

- Output:

  | file | description |
  | ---- | ----------- |
  | research.md | Consolidated research findings resolving all "NEEDS CLARIFICATION" items with ITDs, rationale, and alternatives considered. |
  | quickstart.md | Getting started guide for developers implementing the feature. |
  | plan.md | Full technical implementation plan. |
  | data-model.md | Entity definitions extracted from the spec including fields, relationships, validation rules, and state transitions. |
  | contracts/ | API contracts (OpenAPI/GraphQL schemas) mapping user actions to endpoints using standard REST patterns. (Use Swagger Viewer extension to view) |

- The plan is written at **architectural altitude** — directory-level components, boundary contracts and data ownership, not code or file-level detail. Its `## Requirement Coverage` section is the one place that checks every requirement reaches a component, so read it first.
- Review and update the generated resources - *this is now where a lot of the technical development work is done*.

#### F3. Task list

Generates task definitions as an index plus one file per task. These should define a phased implementation including code, tests, infrastructure and documentation.

- Invoke the **`prism-tasks`** agent with a prompt like:
  > Generate the task files but do not create the Azure work items yet.
- Output is `tasks.md` — an index of phases, ordering and dependencies — plus `tasks/T001-{slug}.md` and so on, each holding the authoritative definition of one task.
- Review the task files. Note that tasks are deliberately **black-box**: they define what must become true, not how to build it. The implementer derives the how from the plan and the target repo's `/.context/` at implement time.

##### F3.1. Work items

Generates work item definitions in Azure DevOps or github to match the defined tasks.

- If you haven't yet been prompted, fill in the `azure-devops-parent-feature-id` at the top of the `spec.md`. This feature id will be used during Azure work item generation.
- For Azure DevOps work items, prompt:
  > Create the Azure DevOps work items from the tasks.
- *OR* for github issues invoke **`speckit-taskstoissues`** agent with the prompt:
  > Create the github issues from the tasks.
- Review the work items.
  - For Azure DevOps work items:
    - Azure DevOps work items should mostly match the markdown tasks.
    - Generate them using the shared `create-ado-work-item` workflow and the team settings in `/.prism/config/work-item-parameters.yml`.
    - One PBI is created per user story, listing that story's tasks; each PBI is linked to its parent feature if the feature id was set in the `azure-devops-parent-feature-id` setting at the top of the feature `spec.md` file.
  - For github issues:
    - similar - TBD.

#### F4. Implemention of code, tests, operations, docs, etc

Implement the feature one task at a time.

- Invoke the **`speckit-implement`** agent with a prompt referring to a task. eg:
  > Implement task "User Story 1: Enable/Disable History Tracking per Table"
- Review the generated code, tests, docs, architecture file updates, etc in the target repo.
- Ensure all acceptance criteria in the task file are ticked off!  Sometimes tests are left until the user approves the code, etc.

Note that when directing code implementation into a separate code repo, we find it easiest to run two IDEs. One for the Prism solution and the other for direct debugging, testing, etc of the code written to the code repo.

#### F5. Changes and fixes to feature artefacts

Any artefacts including specs, tech plans, contracts, tasks, code, etc. can be changed or fixed at any point in the process.

- Pick the agent that targets the earliest stage being modified. The process is very flexible, but as an example the **`prism-specify`** agent could be prompted:
  > Update the spec to also offer a choice of graph types to the user. Also update the technical plans and tasks to suit.
- Then after reviewing the changes use agent **`speckit-implement`** as required - eg.
  > Update the implementation of task 1 to match these changes.

  > Update the implementation of task 2 to match these changes.
- Note that this will work best if it is in the same chat window as the preceding spec and plan changes.
- Wherever possible use the agents to perform updates.

#### F6. PR review

Once the generated code, tests and docs, etc have been completed, consider asking for an agentic code review. One way to achieve this is to open the relevant task file, highlight it, and prompt such as:

    Perform a code review on all changes made in the current branch against the requirements of the highlighted task.

This assumes that the task specifies its repos.

#### Additional - Changes and fixes to process

There are three main process areas that may need improvement.

1. Context - the agents lack sufficient wide or narrow context of the codebase.
1. Guidelines - the agents lack sufficient instruction around either Prism or code-repo-specific development practises.
1. Templates - the agents produce artefacts in an undesirable format.

To fix any of these the context files listed below can be edited directly, but often it is best to invoke the most appropriate agent to make the desired change. eg prompt the `speckit-implement` agent with:

  > Update the appropriate target repo /.context/ file to improve context around Tables API NData and PData translation layers.

OR as another example prompt a general-purpose `Agent` with:

  > Ensure the task phase adds Postman integration tests in any repo where appropriate

For the Prism agents that lands in `/.prism/instructions/overrides/prism-tasks.instructions.md` — the empty-by-default customisation point that survives a `prism update`. For the deprecated Spec Kit route the equivalent is `/.prism/instructions/speckit-tasks.instructions.md`.

The point is that you don't need to know which file(s) to change - the agent can generally work it out using the existing context.

## Tips

- Don't generate the technical plan (prism-plan --> `plan.md`, etc) until you are ready to also generate code. The plan can get stale fast if there is other work going on in the code repo.
- 
