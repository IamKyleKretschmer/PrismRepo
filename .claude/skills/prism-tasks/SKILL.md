---
name: "prism-tasks"
description: "Break a feature plan into a task index plus one black-box task file per task. Prism's self-contained alternative to speckit-tasks."
argument-hint: "Optional task generation constraints"
compatibility: "Requires spec-kit project structure with .specify/ directory"
metadata:
  author: "rnd-prism"
  source: "derived-from-speckit-tasks"
user-invocable: true
disable-model-invocation: false
---

## User Input

```text
$ARGUMENTS
```

You **MUST** consider the user input before proceeding (if not empty).

## Scope

Owns the SDD `tasks` phase: turns `spec.md` + `plan.md` into a **task index** (`tasks.md`) plus
**one black-box task file per task** under `tasks/`.

**Self-contained.** Every task-phase rule is in this file. Do not invoke or read `/speckit-tasks`
— it produces a single monolithic `tasks.md` and is a different output shape.

Only two external files are read at runtime:

- [prism-tasks.instructions.md](/.prism/instructions/overrides/prism-tasks.instructions.md)
  (step 4) — the optional team/installer override, empty by default.
- [branch-and-folder-naming.instructions.md](<PRISM_ROOT>/.prism/instructions/branch-and-folder-naming.instructions.md)
  (step 3) — Prism's shared naming rules, common to all four SDD phases. Referenced rather than
  copied so the phases cannot diverge on branch naming.

Spec Kit assets (`/.specify/`, `.claude/skills/speckit-*/`) are upstream-owned: use their scripts,
templates and hook registry, but never edit them.

---

# Workflow

### 1. Pre-execution hooks

Run the [extension hook protocol](#extension-hook-protocol) for `hooks.before_tasks`. Wait for any
mandatory hook to finish before step 2.

### 2. Setup

Run `.specify/scripts/powershell/setup-tasks.ps1 -Json` from the repo root and parse
`FEATURE_DIR` (absolute) and `AVAILABLE_DOCS`. The script errors if `plan.md` or `spec.md` is
missing — do not proceed without both. Ignore its `TASKS_TEMPLATE` values, which skip Prism's
template: run `<PRISM_ROOT>/.prism/scripts/powershell/resolve-template.ps1 -Name tasks-template -Json` and read the file named
by `TEMPLATE` (the team's override in `.specify/templates/overrides/`, then Prism's, then Spec
Kit's).

Use the template only for the index scaffolding named in [Index file](#tasksmd--index-file);
its sample task lists are illustrative and must be replaced, not filled in.

For single quotes in arguments, use escape syntax — `'I'\''m Groot'` — or double-quote instead.

### 3. Feature branch

Apply [Feature branch](#feature-branch) from the rules below.

### 4. Load context

From `FEATURE_DIR`:

- **Required**: `plan.md` (architecture, repositories, interfaces, Requirement Coverage, Test
  Strategy, Cross-cutting behaviours) and `spec.md` (user stories with priorities, FR/NFR IDs).
  Use its `US<n>` unchanged; if it declares none, use the provisional set from `plan.md` and repeat
  the spec defect in the completion report.
- **If present**: `research.md`, `data-model.md`, `contracts/`, `quickstart.md`, `spec-qa.md`.
- `.specify/memory/constitution.md` for governance constraints.
- `/.prism/config/repos.yml` + [glossary](<PRISM_ROOT>/docs/prism/glossary.md) — every task names a
  real repository.
- Affected repositories' `/.context/manifest.yml` — testing conventions and frameworks come from
  there, not from invention.
- `/.prism/instructions/overrides/prism-tasks.instructions.md`, if it exists and has content beyond
  its header. This is the team/installer customisation point: it layers on top of the rules below
  and wins where it conflicts. It is empty by default — skip it and move on when it is.

Not every feature has every document. Generate from what exists.

### 5. Plan the breakdown

Before writing anything, decide the full set of tasks:

- One phase per user story, in spec priority order, plus Setup, Foundational and Polish.
- Work through `plan.md` § Requirement Coverage and decide, for every `FR-<n>` in it, which task
  delivers it — see [requirement coverage](#requirement-coverage).
- Apply [task sizing](#task-sizing-and-independence), [deferred declaration](#deferred-declaration),
  [repository scoping](#repository-scoping), [operations coverage](#operations-coverage) and
  [documentation](#documentation).
- Assign sequential IDs in execution order and resolve `depends_on` between them.
- Put Foundational work there only if it genuinely blocks every story; an empty Foundational phase
  is a correct outcome, not a gap to fill.

### 6. Write the task files

Create `FEATURE_DIR/tasks/` and write one file per task per
[task file format](#task-file-format), honouring the [black-box rule](#the-black-box-rule).
Write these **before** the index, so no index line can point at a missing file.

### 7. Write the index

Write `FEATURE_DIR/tasks.md` per [Index file](#tasksmd--index-file).

### 8. Self-check, then post-execution hooks

Run the [self-check](#self-check) and fix any failure. Then — **required before reporting
completion** — run the [extension hook protocol](#extension-hook-protocol) for `hooks.after_tasks`.

### 9. Completion report

Report: the index path and `tasks/` directory; total task count and count per user story;
repositories touched; parallel opportunities; the independent test criterion per story; the
suggested MVP scope (usually User Story 1); and the self-check results.

---

## Extension hook protocol

Read `.specify/extensions.yml` at the repo root. If it is absent, unparseable, or registers no
hooks under the key, skip silently. Otherwise, for each entry under that key:

- Skip it if `enabled` is explicitly `false` (absent `enabled` means enabled).
- Do **not** interpret `condition` expressions. No `condition`, or null/empty → executable. A
  non-empty `condition` → skip, leaving evaluation to the HookExecutor.
- Build the command name by replacing dots with hyphens (`speckit.git.commit` →
  `/speckit-git-commit`).

Emit, per executable hook:

**Optional** (`optional: true`):

```
## Extension Hooks

**Optional Hook**: {extension}
Command: `/{command}`
Description: {description}

Prompt: {prompt}
To execute: `/{command}`
```

**Mandatory** (`optional: false`) — you MUST emit `EXECUTE_COMMAND:` for each:

```
## Extension Hooks

**Automatic Hook**: {extension}
Executing: `/{command}`
EXECUTE_COMMAND: {command}
```

After emitting a mandatory block you MUST actually invoke that hook and wait for it to finish
before continuing — emitting the block alone does not run it. Invoke it the way this agent runs
commands (which may differ from the literal `{command}` id, e.g. `/skill:speckit-...`).

---

# Prism task rules

## Feature branch

Scoped to the tasks phase only; not a general branch-naming requirement. Apply the "Handing a
feature branch to the next phase" section of
[branch-and-folder-naming.instructions.md](<PRISM_ROOT>/.prism/instructions/branch-and-folder-naming.instructions.md)
in full — but only if the current branch is NOT the repository's default branch. Being on the
default branch means an earlier phase's branch step never ran: leave the branch as-is and warn the
user.

## Output layout

```text
specs/<feature>/tasks.md              # INDEX: phases, ordering, dependencies, strategy
specs/<feature>/tasks/T001-<slug>.md  # one file per task — the authoritative definition
```

- One task per file, named `T<NNN>-<kebab-slug>.md` (slug ≈ 3–6 words from the title).
- IDs sequential in execution order, ≥ 3 digits, never reused or renumbered; removals leave gaps.
- `tasks.md` holds no task content beyond the one-line title.

## `tasks.md` — index file

The index is the compatibility surface for `/speckit-analyze`, `/speckit-converge`,
`/speckit-taskstoissues`, the `create-ado-work-item` skill and
`validate-spec-propagation.ps1`. It MUST contain, in order:

1. Frontmatter `description` and the `# Tasks: [FEATURE NAME]` heading.
2. The `**Input**` and `**Prerequisites**` block.
3. `## Definition of Ready` and `## Definition of Done`, from the tasks template.
4. One `## Phase N: …` section per phase, each carrying a `**Repository**:` line and containing
   only index lines.
5. `## Dependencies & Execution Order`, parallel opportunities, and `## Implementation Strategy`.

### Index line format (required)

```text
- [ ] T001 [P?] [STUB?] [US<n>?] <Task title> → [tasks/T001-<slug>.md](tasks/T001-<slug>.md)
```

`[P]` only when parallelisable; `[US<n>]` on user-story tasks only (not Setup, Foundational or
Polish). The link is the task's "file path" — do not name source files here. ID, title, story and
`[P]` must match the task file exactly.

## Task file format

Keep each under ~120 lines; longer means it is describing implementation.

```markdown
---
id: T014                      # matches filename
title: Add note history endpoint
feature: 009-notes-history
phase: "Phase 5: User Story 2"
story: US2                    # or null
repository: ntx-notes-api     # exactly one, declared in repos.yml
parallel: true                # true only when the index line has [P]
depends_on: [T009]            # earlier task IDs, or []
effort: 3                     # Fibonacci agent estimate, used by the ADO/issue skills
status: pending               # pending | in-progress | blocked | done
---

# T014 — Add note history endpoint

## Objective
One or two sentences: the observable outcome. WHAT changes for a consumer, never HOW.

## Scope
**In scope** — the behaviour this task must deliver.
**Out of scope** — adjacent behaviour deferred, naming the task that covers it.

## Behaviour & Contract
Externally observable behaviour: boundary touched, inputs/outputs by meaning, error and failure
semantics, authorisation, idempotency, performance/accessibility bars. Cite `../contracts/*` and
`../plan.md` sections rather than restating them. Record any genuinely mandated implementation
choice (fixed library, wire format, algorithm) here as a constraint with its reason.

## Preconditions & Dependencies
What must already exist — prior task IDs, resources, flags, config.

## Acceptance Criteria
- [ ] QA-verifiable statements of done: functional outcome, contract conformance, operational
      outcome enabled, documentation updated. Checkable by someone who did not implement it.

## Testing Requirements
Per level (unit / integration / manual / test data): what behaviour must be covered and the
framework from the target repo's `/.context/`. Derived from `plan.md` § Test Strategy and any
`spec-qa.md`, narrowed to this task. No test method names or test file paths.

## Operational Requirements
Pipeline, deployment, monitoring, alerting, feature-flag or runtime-config changes, and how each is
verified. `N/A` when there is no operational impact.

## Documentation Requirements
What must be updated — API reference, README, mkdocs nav, `/.context/` — or `N/A`.

## References
Spec FR IDs and user story; `../plan.md` sections; `../contracts/*`.
```

## The black-box rule

A task defines **what must become true**, not how. The implementer derives the how at implement
time from the target repo's `/AGENTS.md`, its `/.context/`, and the architecture in `plan.md` —
guidance that is current then, unlike anything frozen into a task file.

A task file MUST NOT contain: source file paths or file-by-file instructions; class, method or
internal type names (published contract names may be cited via `contracts/`); code, pseudo-code,
algorithms, SQL/DDL or config contents; design-pattern or DI prescriptions; step-by-step recipes.

## Task sizing and independence

- Completable in one focused session (~1–4 hours), roughly one coherent change.
- Mergeable/deployable without other incomplete tasks.
- Vertical slices, not horizontal layers — one capability made observably to work, never
  "create all models" / "create all services".
- Explicit entry (Preconditions) and exit (Acceptance Criteria) criteria.
- **Stubs**: only when immediately needed to unblock a concrete implementation in the SAME story —
  never speculatively in Setup or Foundational. Mark `[STUB]` in the index; its objective is a
  callable surface that fails predictably, not finished behaviour.

## Deferred declaration

Do not create interfaces, DTOs, entities or value objects as standalone up-front tasks. They ship
with the task — or an adjacent task in the same story — that first uses them. A type needed by
several stories belongs to the earliest story needing a real implementation; later stories reuse
or extend it rather than adding speculative shared declarations.

## Repository scoping

Each user story defaults to ONE repository; Setup and Polish may span several, and a story spans
several only when technically necessary (e.g. shared contracts). Every task file names exactly one
`repository:` — work spanning two repositories is two tasks. Each phase heading in `tasks.md` keeps
its `**Repository**:` line.

## Requirement coverage

`plan.md` § Requirement Coverage lists every `FR-<n>` and what covers it. Read it: nothing else in
this phase notices a requirement that never reaches a task, because the self-check otherwise only
runs forwards.

Each requirement covered by a story reaches at least one task in that story's phase. Each marked
`Cross-cutting` becomes an obligation in the Acceptance Criteria of every task it constrains **and**
gets a Polish verification task — criteria alone never prove the whole holds together, a
verification task alone finds problems only after every story is built. Each marked `Not planned` is
left alone and named in the completion report. Anything else reaching no task is a defect, and an
unclaimed **Must-have** is an ERROR. If `plan.md` has no such section, build the mapping from
`spec.md` and report the plan as missing it.

## Operations coverage

Include operations work whenever the plan or affected repos show operational impact. Provisioning,
shared deployment wiring and baseline secrets go up front only if they block every story;
pipeline, release, monitoring, alerting, dashboard, feature-flag and environment-config work goes
in the story that introduces the capability. Do not batch it all into one phase. Operations tasks
state how completion is verified (pipeline validation, dry-run, config diff, monitoring signal) in
their Acceptance Criteria. Define the operational outcome, not the pipeline YAML.

## Documentation

Documentation ships with the story that introduces what it documents — as a Documentation
Requirements entry, or its own task when substantial. API reference goes with the endpoint's story.
Conceptual docs, mkdocs nav, `/.context/` updates and cross-repo consistency checks batch into
Polish. Infer targets per repo (`docs/`, `mkdocs.yml`, READMEs, end-user docs, `/.context/`).
Polish MUST include a `/.context/` update task for each affected repository.

## Appending tasks later

Anything adding tasks to an existing feature — `/speckit-converge`'s `## Phase N: Convergence`, or
a manual addition — creates the task file first, then the index line. An index line without a file,
or a file not in the index, is a defect.

## Self-check

Verify and state the result; fix failures rather than reporting them. `tasks/` holds exactly one
correctly named file per task, 1:1 with the index; ID, title, story and `[P]` agree between them;
no task definitions in `tasks.md`; no source paths, class names or code in any task file;
every task has one valid `repository:` and non-empty
Acceptance Criteria and Testing Requirements; `depends_on` resolves to earlier existing tasks;
Polish covers `/.context/` per repository.

Traceability runs both ways: every task traces to a spec requirement or plan decision; every
`FR-<n>` in `plan.md` § Requirement Coverage reaches a task, is marked `Not planned`, or is raised,
and **an unclaimed Must-have is an ERROR, not a warning**; every user story has a phase; every
cross-cutting requirement appears both in the acceptance criteria of the tasks it constrains and in
a Polish verification task.

---

## Done when

- [ ] `tasks/` written with one black-box file per task, and `tasks.md` written as the index
- [ ] Every user story, plus Setup/Foundational/Polish, represented in phase order
- [ ] Every `FR-<n>` in the plan's Requirement Coverage reaches a task or is a recorded deferral
- [ ] Self-check performed and reported
- [ ] `before_tasks` and `after_tasks` hooks dispatched or skipped per the hook protocol
- [ ] Completion reported with counts, repositories, MVP scope, deferrals and self-check results
