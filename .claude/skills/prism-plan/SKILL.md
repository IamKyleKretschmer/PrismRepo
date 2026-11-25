---
name: "prism-plan"
description: "Create the architecture-level technical plan and design artefacts for a feature. Prism's self-contained alternative to speckit-plan."
argument-hint: "Optional guidance for the planning phase"
compatibility: "Requires spec-kit project structure with .specify/ directory"
metadata:
  author: "rnd-prism"
  source: "derived-from-speckit-plan"
user-invocable: true
disable-model-invocation: false
---

## User Input

```text
$ARGUMENTS
```

You **MUST** consider the user input before proceeding (if not empty).

## Scope

Owns the SDD `plan` phase: produces `plan.md` plus the Phase 0/1 design artefacts, at
**architectural altitude only**.

**Self-contained.** Every plan-phase rule is in this file. Do not invoke or read `/speckit-plan`,
and do not read `speckit-plan.instructions.md` — that file exists only to bend the
upstream skill and duplicates the rules below.

Only two external files are read at runtime:

- [prism-plan.instructions.md](/.prism/instructions/overrides/prism-plan.instructions.md)
  (step 4) — the optional team/installer override, empty by default.
- [branch-and-folder-naming.instructions.md](<PRISM_ROOT>/.prism/instructions/branch-and-folder-naming.instructions.md)
  (step 3) — Prism's shared naming rules, common to all four SDD phases. Referenced rather than
  copied so the phases cannot diverge on branch naming.

`/speckit-plan` remains a supported alternative entry point with its own workflow and its own copy
of these rules. The two copies must stay in step: **any change to `# Prism plan rules` below must
be mirrored into `<PRISM_ROOT>/.prism/instructions/speckit-plan.instructions.md`, and vice versa.**
That is a maintenance instruction for whoever edits the rules — not a file to open while running
this skill.

Spec Kit assets (`/.specify/`, `.claude/skills/speckit-*/`) are upstream-owned: use their scripts,
templates and hook registry, but never edit them.

---

# Workflow

Applies to `/prism-plan` only. (`/speckit-plan` runs its own equivalent steps.)

### 1. Pre-execution hooks

Run the [extension hook protocol](#extension-hook-protocol) for `hooks.before_plan`. Wait for any
mandatory hook to finish before step 2.

### 2. Setup

Run `.specify/scripts/powershell/setup-plan.ps1 -Json` from the repo root and parse the JSON for
`FEATURE_SPEC`, `IMPL_PLAN`, `SPECS_DIR` and `BRANCH`. The script copies Spec Kit's plan template
to `IMPL_PLAN`. Then run `<PRISM_ROOT>/.prism/scripts/powershell/resolve-template.ps1 -Name plan-template -ReplaceSpecKitCopy
"<IMPL_PLAN>" -Json`: while `IMPL_PLAN` is still an untouched copy of Spec Kit's base template, it
replaces it with the plan template that applies (the team's override in
`.specify/templates/overrides/`, then Prism's, then Spec Kit's). The result is the starting
skeleton.

For single quotes in arguments, use escape syntax — `'I'\''m Groot'` — or double-quote instead.

### 3. Feature branch

Apply [Feature branch](#feature-branch) from the rules below.

### 4. Load context

- `FEATURE_SPEC` (`spec.md`) and the `IMPL_PLAN` skeleton just copied.
- `.specify/memory/constitution.md` and the standards it references.
- [glossary](<PRISM_ROOT>/docs/prism/glossary.md) and `/.prism/config/repos.yml` — resolve every
  repository by role before naming it.
- Affected repositories' `/.context/manifest.yml` and the context sets relevant to the feature.
- `/.prism/instructions/overrides/prism-plan.instructions.md`, if it exists and has content
  beyond its header. This is the team/installer customisation point: it layers on top of the rules
  below and wins where it conflicts. It is empty by default — skip it and move on when it is.

### 5. Fill the plan

Work through the `IMPL_PLAN` skeleton at the altitude and in the
[style](#writing-style-state-it-once) set out below:

- **User stories** — establish these first from `spec.md`; every later section is organised around
  them ([rules](#user-stories-consume-never-invent)).
- **Technical Context** — mark genuine unknowns `NEEDS CLARIFICATION`.
- **Constitution Check** — assess against the constitution. ERROR on an unjustified gate violation;
  justify any violation in Complexity Tracking or change the design.
- **Component Interactions** — one subsection per user story, plus
  [Cross-cutting behaviours](#cross-cutting-behaviours) for what constrains every story.
- **Operations Plan** — deployment, configuration, rollout and observability shape of the feature.
- Add all three [required sections](#required-section-requirement-coverage).

### 6. Phase 0 — research

Resolve every `NEEDS CLARIFICATION`, dependency choice and integration pattern, dispatching
research tasks in parallel where independent. Consolidate into `research.md` per
[artefact altitude](#artefact-altitude).

**Output**: `research.md`, no `NEEDS CLARIFICATION` remaining.

### 7. Phase 1 — design & contracts

Prerequisite: `research.md` complete. Produce `data-model.md`, `contracts/` and `quickstart.md` per
[artefact altitude](#artefact-altitude). Re-run the Constitution Check after design and record the
result.

### 8. Self-check, then post-execution hooks

Run the [self-check](#self-check) and fix any failure. Then — **required before reporting
completion** — run the [extension hook protocol](#extension-hook-protocol) for `hooks.after_plan`.

### 9. Completion report, then stop for approval

Report the branch, the `IMPL_PLAN` path, the artefacts generated, the feasibility verdict and the
self-check results.

**The plan is a decision document: stop here and obtain the user's approval.** Once approved, the next phase is `/prism-tasks`.

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

# Prism plan rules

These rules override `plan-template.md` wherever they disagree: the template supplies the section
skeleton, these rules govern the altitude of what goes in it. Mirrored in
`speckit-plan.instructions.md` for the `/speckit-plan` route — keep the two in step.

## Feature branch

Scoped to the plan phase only; not a general branch-naming requirement. Apply the "Handing a
feature branch to the next phase" section of
[branch-and-folder-naming.instructions.md](<PRISM_ROOT>/.prism/instructions/branch-and-folder-naming.instructions.md)
in full — but only if the current branch is NOT the repository's default branch. Being on the
default branch means the specify phase's branch step never ran for this feature: leave the branch
as-is and warn the user rather than branching off the default.

## User stories: consume, never invent

Stories belong to the spec: `/prism-specify` writes `### User stories` with stable `US<n>`, the
`FR-<n>` each claims, and a priority. Use those identifiers unchanged — `/prism-tasks` builds one
phase per story and keys task frontmatter to them.

If `spec.md` declares none, do **not** invent them silently: derive a set from the Must-have
requirements grouped by user intent — not by component or layer — record it in `plan.md` as
provisional with the `FR-<n>` each claims, and report the spec defect, noting that `/prism-tasks`
will build its entire phase structure on it. Derived stories are a stopgap; the fix is to add the
section to `spec.md`.

**"Declares none" includes a section left as the template wrote it** — rows still holding
placeholder or comment text, or an empty `FR-<n>` column. A scaffolded table is not a declaration:
check the rows carry real content before trusting them, because an unfilled table fails silently
where a missing section fails loudly.

## Cross-cutting behaviours

Some constitution principles constrain every story rather than forming one, so the per-story
subsections have no room for them and they get lost between spec and tasks. Add a
`### Cross-cutting behaviours` subsection to Component Interactions, one row per principle this
feature actually engages: what it constrains, the architectural choice satisfying it, how it is
verified. Source them from the constitution, the affected repositories' `/.context/`, and the
spec's Considerations. The plan is the first artefact to state them (`spec.md` carries no
cross-cutting section, since a requirement there belongs to exactly one story), and scattering one
through task acceptance criteria does not discharge it.

## Altitude: architecture only

The plan fixes what is expensive to change later. Implementation detail is derived at implement
time from the target repository's `/.context/` — do not freeze it here.

**Include**: affected repositories and which owns each component (resolve via the glossary and
`/.prism/config/repos.yml`); components/modules at **directory** granularity; each boundary's
contract (operations, semantics, errors, idempotency, compatibility); component interactions per
user story (sync/async, consistency and transaction boundaries, failure handling); data ownership
and invariants; quality-attribute targets and the choice meeting each; operations architecture;
alternatives rejected and why (in `research.md`).

**Exclude**: code, pseudo-code, SQL/DDL, config file contents; class, method or internal type
names; file-level trees or per-file responsibilities; field types, nullability, indexes; `T###`
tasks or build sequencing; restated spec text (cite FR IDs); library usage tutorials.

**Budget**: `plan.md` ≤ ~400 lines; `data-model.md` and each contract ≤ ~150; `quickstart.md`
≤ ~100. Over budget means implementation detail crept in — cut it, don't compress it.

## Writing style: state it once

A plan is read end to end by a human deciding whether to trust it. Write for that reader: short
declarative sentences; decisions stated as decisions, with the reason attached; no "this section
describes…" preamble and no closing summary of the section above. Use a table when the same few
fields repeat across many items, prose otherwise; nest bullets one level at most.

State each fact once, in the section that owns it, and cite it elsewhere instead of restating it —
requirements in `spec.md` (cite `FR-<n>`), rationale and rejected alternatives in `research.md`,
entities and invariants in `data-model.md`, boundary semantics in `contracts/`; within `plan.md`,
repositories and component ownership in the structure section, each quality target beside the
choice meeting it, every risk in Feasibility & Risks. A sentence that could be deleted without
losing a decision was padding.

## Required section: Requirement Coverage

Add `## Requirement Coverage` after Constitution Check: one row per `FR-<n>` in `spec.md` giving
the requirement, what covers it (the user story, `Cross-cutting`, or `Not planned`), the owning
component at directory granularity, and a reason for `Not planned` or a location for
`Cross-cutting`. No requirement may be absent — every other phase verifies forwards (a task traces
to a requirement), and only this verifies backwards (a requirement reaches a component). An
unclaimed **Must-have** is an ERROR: assign it, record it as cross-cutting, or raise it with the
user, never leave it out. `Not planned` needs a reason; silence is not deferral. `Won't have`
entries carry no `FR-<n>` and are not listed. `/prism-tasks` reads this table, so keep identifiers
exactly as `spec.md` writes them.

## Required section: Feasibility & Risks

Add `## Feasibility & Risks` after Requirement Coverage. Verdict — `Feasible` /
`Feasible with caveats` / `Not feasible as specified` — with justification; assumptions and what
breaks if each is wrong; risks with likelihood/impact and mitigation; open questions needing a
spike, and what a different answer would change; blast radius. A non-`Feasible` verdict must be
raised with the user, not planned around.

## Required section: Test Strategy

Add `## Test Strategy`. Levels in play and what each proves here; which repository and suite hosts
each; frameworks per level, taken from the repos' `/.context/`; one line per user story on what
verification must demonstrate; significant test data/environment needs; coverage gaps with
justification. No individual test cases — those belong in task files. If `spec.md` lists a
`spec-qa.md` companion, derive the strategy from it; if one exists in the feature folder but is
unlisted, incorporate it anyway or flag the missing link rather than ignoring it.

## Artefact altitude

- **`research.md`** — Decision / Rationale / Alternatives rejected. Resolve every
  `NEEDS CLARIFICATION`. No samples or setup steps.
- **`data-model.md`** — entities, relationships, ownership (system of record), lifecycle,
  invariants. No types, nullability, indexes or DDL.
- **`contracts/`** — one file per boundary the feature introduces or changes: operation, purpose,
  direction, payload field names, error semantics, idempotency, authorisation, versioning impact.
  Signatures allowed, implementations not. Covers whatever the project exposes — HTTP endpoints,
  events, public library surface, CLI commands, UI contracts. Omit the directory entirely if the
  feature adds no external boundary.
- **`quickstart.md`** — prerequisites, the scenarios that prove the feature works end to end, and
  expected outcomes. Reference the other artefacts rather than restating them.

## Self-check

Verify each and state the result; fix failures rather than reporting them. No implementation inside
fenced blocks (trees, signatures and diagrams only); source tree stops at directories; no
class/method names; no physical schema in `data-model.md`; no `T###` identifiers; all three required
sections present and complete; every repository resolved via `repos.yml`; budgets met; no fact
restated in a second section where a citation would do.

Traceability runs both ways: every user story traces to a component and an interface or interaction;
every `FR-<n>` in `spec.md` appears in Requirement Coverage, and **an unclaimed Must-have is an
ERROR, not a warning**; story identifiers match `spec.md`, or the derivation is recorded and
reported; every cross-cutting requirement has an architectural statement, not a passing mention.

## Key rules

- Absolute paths for filesystem operations; project-relative paths for references in documentation.
- ERROR on gate failures, unresolved clarifications, or an unclaimed Must-have requirement.
- Never edit Spec Kit assets (`/.specify/`, `.claude/skills/speckit-*/`).

---

## Done when

- [ ] `plan.md` filled at architectural altitude, with Requirement Coverage, Feasibility & Risks
      and Test Strategy
- [ ] Every `FR-<n>` covered, no unclaimed Must-have; stories from `spec.md` or recorded as derived
- [ ] Phase 0 and Phase 1 artefacts generated; Constitution Check re-run post-design
- [ ] Self-check performed and reported
- [ ] `before_plan` and `after_plan` hooks dispatched or skipped per the hook protocol
- [ ] Completion reported with branch, plan path, artefacts and verdict
- [ ] Stopped for user approval before the tasks phase
