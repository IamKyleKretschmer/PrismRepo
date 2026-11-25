---
name: "prism-specify"
description: "Create or update a feature specification, with user stories and requirement coverage declared in the spec itself. Prism's self-contained specify phase."
argument-hint: "Describe the feature you want to specify"
compatibility: "Requires spec-kit project structure with .specify/ directory"
metadata:
  author: "rnd-prism"
  source: "prism"
user-invocable: true
disable-model-invocation: false
---

## User Input

```text
$ARGUMENTS
```

You **MUST** consider the user input before proceeding (if not empty).

## Scope

Owns the SDD `specify` phase: produces `spec.md` plus its quality checklist, at **outcome altitude
only**, and — the reason this skill exists — makes the spec the single place where **user stories**
and **requirement coverage** are declared.

**Self-contained.** Every spec-phase rule is in this file. Do not delegate the phase to another
specify skill and do not read another skill's instructions file — none of them carry the story and
coverage rules below.

Only two external files are read at runtime:

- [prism-specify.instructions.md](/.prism/instructions/overrides/prism-specify.instructions.md)
  (step 4) — the optional team/installer override, empty by default.
- [branch-and-folder-naming.instructions.md](<PRISM_ROOT>/.prism/instructions/branch-and-folder-naming.instructions.md)
  (step 3) — Prism's shared naming rules, common to all four SDD phases. Referenced rather than
  copied so the phases cannot diverge on naming.

A spec may arrive from an older or deprecated route. The template scaffolds `### User stories`,
so the section is usually present — but present and *filled in* are different things. A table still
holding template placeholders, or rows whose `FR-<n>` column is empty, is an undeclared spec: treat
it as such, fill it in per the rules below, and say so in the completion report.

Spec Kit assets (`/.specify/`, `.claude/skills/speckit-*/`) are upstream-owned: use their scripts,
templates and hook registry, but never edit them during a run. Prism's own `spec-template.md`
already scaffolds the required section below — fill it in; change the template itself only as a
deliberate framework change, never per feature.

---

# Workflow

Applies to `/prism-specify` only.

### 1. Pre-execution hooks

Run the [extension hook protocol](#extension-hook-protocol) for `hooks.before_specify`. Wait for any
mandatory hook to finish before step 2. That hook creates the feature branch; note any `BRANCH_NAME`
and `FEATURE_NUM` it returns, but the branch name does not dictate the feature directory name.

### 2. Setup

- Derive a 2–4 word short name for the feature from the description: action-noun where possible,
  technical terms and acronyms preserved (`user-auth`, `oauth2-api-integration`,
  `fix-payment-timeout`).
- Resolve the feature directory and create it, per [Feature folder and branch](#feature-folder-and-branch).
- Resolve the spec template by running `<PRISM_ROOT>/.prism/scripts/powershell/resolve-template.ps1 -Name spec-template -Json`
  from the repo root, and copy the file named by `TEMPLATE` to `<feature dir>/spec.md`. This is
  `SPEC_FILE`. The script takes the team's override in `.specify/templates/overrides/` first, then
  Prism's template, then Spec Kit's base template.
- Persist the resolved directory to `.specify/feature.json` as
  `{ "feature_directory": "<resolved feature dir>" }`, so later phases locate the feature without
  relying on branch naming.

Only one feature per invocation. The spec directory and file are always created here, never by a
hook.

For single quotes in arguments, use escape syntax — `'I'\''m Groot'` — or double-quote instead.

### 3. Feature branch and folder

Apply [Feature folder and branch](#feature-folder-and-branch) from the rules below.

### 4. Load context

- The resolved `spec-template`, to learn its section order and headings.
- `.specify/memory/constitution.md` for governing principles, and
  `.specify/memory/personas.md` if the template's Personas section is in play.
- [glossary](<PRISM_ROOT>/docs/prism/glossary.md) and `/.prism/config/repos.yml` — needed only if the
  spec must name a repository; specs normally should not.
- Any `spec-*.md` companion files already in the feature folder, per
  [Companion spec files](#companion-spec-files).
- `/.prism/instructions/overrides/prism-specify.instructions.md`, if it exists and has content
  beyond its header. This is the team/installer customisation point: it layers on top of the rules
  below and wins where it conflicts. It is empty by default — skip it and move on when it is.

### 5. Write the specification

Fill the template at the altitude set out below, preserving its section order and headings.
Resolve unclear aspects with informed guesses recorded in Assumptions; reserve
`[NEEDS CLARIFICATION: question]` for choices that genuinely have no reasonable default, to a
**maximum of three**.

### 6. Declare user stories and requirement coverage

**This step is the point of the skill and must not be skipped.** Add the required
[User stories](#required-section-user-stories) section, then verify the
[coverage rule](#requirement-coverage): every functional requirement belongs to exactly one
story.

### 7. Quality checklist

Write `<feature dir>/checklists/requirements.md` per [Quality checklist](#quality-checklist) and
validate the spec against it. Fix failures and re-validate, to a maximum of three iterations; record
anything still failing in the checklist notes and warn the user. Present any remaining
`NEEDS CLARIFICATION` questions per [Clarifications](#clarifications).

### 8. Self-check, then post-execution hooks

Run the [self-check](#self-check) and fix any failure. Then — **required before reporting
completion** — run the [extension hook protocol](#extension-hook-protocol) for `hooks.after_specify`.

### 9. Completion report

Report the feature directory, `SPEC_FILE`, the branch, the story count with the FR IDs each claims,
any unclaimed requirements (there must be none), the checklist
results and the self-check results. The next phase is `/prism-grill-me`, followed by `/prism-plan`.

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

# Prism spec rules

These rules override `spec-template.md` wherever they disagree: the template supplies the section
skeleton, these rules govern what goes in it. Prism's `spec-template.md` scaffolds every required
section below, so filling them in is the normal path; add a missing one to the written spec when
working from a template that predates it. Never edit the template during a run — it is Prism's
default, changed deliberately, not per feature.

## Feature folder and branch

Apply the "Initial branch + feature folder creation" section of
[branch-and-folder-naming.instructions.md](<PRISM_ROOT>/.prism/instructions/branch-and-folder-naming.instructions.md)
in full. In summary, and deferring to that file on every detail: the convention applies only when
starting from the repository's default branch with a clean working tree; warn and ask rather than
proceeding from a non-default branch or over uncommitted changes; and resolve the feature directory
and branch name from the same token values so they stay aligned.

If the `before_specify` hook could not run — for example because the git skill blocks model
invocation — the branch will not exist. Record that in the checklist notes and tell the user which
command to run, rather than reporting the spec as fully complete.

## Altitude: outcomes only

The spec fixes **what** users need and **why**. How it is built is the plan's job and must not be
pre-empted here.

**Include**: the problem in customer terms; measurable goals; personas served; terminology;
functional requirements by MoSCoW priority, each testable and each carrying a stable `FR-<n>`
identifier; user stories per the required section below; open
questions; considerations this feature raises beyond the constitution's standing concerns;
assumptions with the default chosen; research notes.

**Exclude**: languages, frameworks, libraries, APIs, storage mechanisms, schemas, code structure,
file or directory names, repository internals. A delivery constraint stated by the requester (for
example "a single-page HTML app") may be recorded as a constraint in Context, because it bounds
scope — but it must be identified as the requester's constraint, not as a design choice made here.

**Testability**: a requirement a QA engineer could not verify without asking what was meant is not
finished. Every requirement states an observable outcome.

## Required section: User stories

Add `### User stories` under `## Functional Requirements`, immediately after the requirements list
so the `FR-<n>` identifiers exist before they are referenced.

| Column | Content |
| ------ | ------- |
| `US<n>` | Stable identifier. Assign sequentially. **Never renumber**: downstream phases key phases, task frontmatter and index markers to these. |
| Story | The intent in user terms — who wants what, and why. One sentence. |
| Requirements | The `FR-<n>` identifiers this story delivers. |
| Priority | The highest MoSCoW priority among its requirements. |

Rules:

- A story is a slice of user-visible value that can be built, demonstrated and verified on its own.
  It is not a layer, a component, or a phase of work.
- Every requirement belongs to exactly one story, whatever its priority — see
  [requirement coverage](#requirement-coverage).
- Prefer few, meaningful stories over many thin ones. If a story claims one requirement and that
  requirement is not independently demonstrable, it belongs inside a neighbouring story.
- These identifiers are the contract with `/prism-plan` and `/prism-tasks`. `/prism-plan` builds
  Component Interactions per story; `/prism-tasks` builds one phase per story, in the priority order
  recorded here.

## Requirement coverage

**Every functional requirement belongs to exactly one user story.** A requirement claimed by no
story is a spec defect — fix it before the spec is ready, rather than leaving it for the plan or
tasks phase to notice, because neither checks for it by default. A requirement claimed by two
stories is a story boundary drawn in the wrong place: move it, or merge the stories. `Won't have`
entries are out of scope and carry no `FR-<n>`, so this rule does not reach them.

A requirement that resists assignment because it constrains every story rather than forming one is
a standing concern. The constitution and the affected repositories' `/.context/` already carry
those, so leave it out of `spec.md` rather than inventing a bucket to hold it; `/prism-plan` records
the architectural choice satisfying each.

This rule is what makes coverage checkable in both directions. Every later phase verifies forwards
(a task traces to a requirement); only this rule verifies backwards (a requirement reaches a story).

## Priority and ordering

- MoSCoW priority is carried on the requirements, and each story inherits the highest priority
  among the requirements it claims.
- Story order is by that inherited priority, and is the order `/prism-tasks` uses for its phases.
  Record the stories in that order so downstream phases need no tie-breaking rule.
- An MVP is the set of stories whose requirements are all Must-have. If the Must-have set does not
  make a coherent shippable increment, say so in Context — that is a scoping problem the plan
  cannot fix.

## Clarifications

- Make informed guesses and record them in Assumptions. Reserve `[NEEDS CLARIFICATION: question]`
  for choices with no reasonable default, where multiple readings have materially different
  implications.
- Maximum three markers. Prioritise by impact: scope > security/privacy > user experience >
  technical detail.
- Present remaining questions to the user as a numbered list, each with a short context quote and
  two to four concrete options with their implications, then wait for answers and fold them in.
- Open questions that already have a working default recorded in Assumptions are **not**
  `NEEDS CLARIFICATION` markers. Record them under Open questions so the plan phase can revisit
  them with better information.

## Azure DevOps parent feature ID

The template's frontmatter carries `azure-devops-parent-feature-id`, used downstream to link work
items. Ask the user for the numeric parent Feature work item ID, or `skip`. On `skip`, leave the
placeholder and add a `[NEEDS CLARIFICATION: Azure DevOps parent Feature ID required for work item
linking]` note under Open questions.

## Dependencies on other teams

If the feature needs input, review or approval from another team, add a `## Dependencies` section
with one entry per dependency giving the team name and what is needed from them. If there are none,
state "No dependencies on other teams at this time" rather than omitting the section.

## Companion spec files

- `prism-grill-me` sessions produce per-aspect companion files beside `spec.md`
  (`spec-qa.md`, `spec-pm.md`, …). They are never merged into `spec.md`'s body.
- `spec.md` MUST carry a `## Companion Specifications` section with one row per companion file that
  exists (`Aspect | File | Summary`), linked relatively. Downstream phases read `spec.md` and rely
  on this table to discover companions — a companion with no row is invisible to them.
- On every create or update, check the feature folder for `spec-*.md` files. One without a row is a
  spec defect: add the row before treating the spec as ready.
- Never remove a row unless the file itself is gone.

## Quality checklist

Write `<feature dir>/checklists/requirements.md` with these items, and validate against it:

```markdown
## Content Quality

- [ ] No implementation details (languages, frameworks, APIs)
- [ ] Focused on user value and business needs
- [ ] Written for non-technical stakeholders
- [ ] All mandatory sections completed

## Requirement Completeness

- [ ] No [NEEDS CLARIFICATION] markers remain
- [ ] Requirements are testable and unambiguous
- [ ] Each requirement carries a stable FR identifier
- [ ] Goals are measurable and technology-agnostic
- [ ] Scope is clearly bounded
- [ ] Dependencies and assumptions identified

## Story and Coverage Completeness

- [ ] Every FR ID is claimed by exactly one user story
- [ ] No requirement is left unclaimed, and none is claimed twice
- [ ] Each user story is independently demonstrable and verifiable
- [ ] Story identifiers are sequential and none has been renumbered
- [ ] Stories are recorded in priority order

## Feature Readiness

- [ ] All functional requirements have clear acceptance conditions
- [ ] Feature meets the measurable outcomes in Goals
- [ ] No implementation details leak into the specification
```

Record validation findings in a Notes section: what passed, what was changed, and anything left
failing after three iterations.

## Self-check

Verify each and state the result; fix failures rather than reporting them.

- The User stories section is present and not empty.
- Every `FR-<n>` in the requirements list is claimed by exactly one story; **an unclaimed
  requirement is an ERROR, not a warning**.
- No story identifier is reused, skipped or renumbered; stories are in priority order.
- No story is a layer, a component, or a work phase.
- No implementation detail: no language, framework, library, API, storage mechanism, schema, or
  file or directory name.
- Every requirement states an observable outcome a tester could verify.
- At most three `NEEDS CLARIFICATION` markers, and each is genuinely undecidable.
- `## Companion Specifications` reflects every `spec-*.md` present in the folder.
- The checklist file exists, is filled in, and its findings match the spec's actual state.

## Key rules

- Absolute paths for filesystem operations; project-relative paths for references in documentation.
- ERROR on an unclaimed Must-have requirement or a missing required section.
- Never edit Spec Kit assets (`/.specify/`, `.claude/skills/speckit-*/`) or Prism's templates during
  a run — templates are defaults, changed deliberately.

---

## Done when

- [ ] `spec.md` written at outcome altitude, with User stories
- [ ] Every functional requirement claimed by exactly one story; none left unclaimed
- [ ] Quality checklist written, validated and recorded
- [ ] Self-check performed and reported
- [ ] `before_specify` and `after_specify` hooks dispatched or skipped per the hook protocol
- [ ] Completion reported with feature directory, spec path, branch, stories and coverage
