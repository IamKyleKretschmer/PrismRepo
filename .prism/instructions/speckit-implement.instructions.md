# Implementation Conventions

Overrides `/speckit-implement` where they disagree — in particular its assumption that the whole
task breakdown always lives inside `tasks.md`.

## Task source

`tasks.md` gives the phases, ordering, `[P]` markers and dependencies.

Each task is defined either in its own `tasks/T<NNN>-<slug>.md` file or inline in `tasks.md` —
check whether a `tasks/` directory exists beside it. Either way, read the task's full definition
before implementing, never the index line alone. Below, **task file** means whichever of the two
holds it.

## One task at a time

Work in index order, completing each before starting the next:

1. Read the task file in full, plus the `plan.md` sections and contracts it cites.
2. Set `status: in-progress` if the task file has that frontmatter.
3. Create the per-task branch if `taskBranchFormat` is configured (see below).
4. Implement, test and document until every Acceptance Criterion is met.
5. Tick the task's checkbox in `tasks.md` to `- [X]`, and set `status: done` where that
   frontmatter exists.
6. Report completion, then move on.

`[P]` tasks in the same phase may run together when they touch disjoint code. Halt and report on a
non-parallel failure. If a task is blocked, record why in the task file and raise it.

## Converting a black-box task into code

Files in `tasks/` deliberately name no file paths, classes or code, so derive the how yourself, in
this order: the target repo's `/.context/` (manifest, then the relevant context sets and module
context — patterns, file organisation, naming, dependency rules, testing conventions, build/test
commands); that repo's `/AGENTS.md`; `plan.md` and `contracts/` for structure, boundaries and
interactions; the conventions of the surrounding code; then the constitution and its standards.

Honour any constraint the task records under **Behaviour & Contract**, and follow any file paths it
does name — inline tasks usually name their own. Everything else — which files, how to decompose,
which patterns — is decided now, not in the task. If a task cannot be built without a decision the
plan should have made (missing interface, undefined contract, conflict with the architecture), stop
and raise it rather than inventing architecture inside a task.

## Scope discipline

Implement the task in front of you and nothing else. Out-of-scope work belongs to another task; if
none covers it, say so and let the user decide. Never tick an Acceptance Criterion that is not met.

## Feature branch naming

Scoped to `/speckit-implement`, which owns the `impl` phase branch in the target `code`-role
repository. Apply the "Handing a feature branch to the next phase" section of
[branch-and-folder-naming.instructions.md](<PRISM_ROOT>/.prism/instructions/branch-and-folder-naming.instructions.md), with a
topology-dependent guard that also changes where step 1 recovers `<type>`/`<feature>` from:

- **Single-repo** (one repo holds both `prism` and `code` roles): it already carries the `tasks`
  branch — proceed only if the current branch is NOT the default branch; on the default branch an
  earlier phase's branch step never ran, so leave it and warn. Recover `<type>`/`<feature>` from
  the current branch name.
- **Multi-repo**: the `code` repo is untouched by earlier phases, so this is its first branch for
  the feature — proceed only if the current branch IS that repo's default branch; otherwise warn
  and ask before switching. Use that repo's own `.prism/config/branch-naming.yml` if present, else
  `sddBranchFormat: "<type>/<feature>"`. Recover `<type>`/`<feature>` from the feature's branch or
  directory in the `prism`-role repository.

This is independent of the `prism`-role repository's branch and feature directory, which hold
`spec.md`/`plan.md`/`tasks.md` and are untouched here.

## Per-task branch naming

Before each task, apply the "Per-task branch" section of
[branch-and-folder-naming.instructions.md](<PRISM_ROOT>/.prism/instructions/branch-and-folder-naming.instructions.md) — it layers on
the phase branch and is skipped entirely when `taskBranchFormat` is blank. Derive `<task>` from the
task file's `title` (equivalently, the slug in its filename).

## Operations implementation

Implement the operations work the plan and tasks describe, not just application code. Blocking
prerequisites (provisioning, shared deployment wiring, baseline secrets/config, foundational
pipeline enablement) come first; story-specific operations (pipeline stages, deployment manifests,
feature-flag rollout, alerts, dashboards, runbooks, environment settings) ship inside the story
that needs them — never postponed to a cleanup phase when the story depends on them to be
deployable, observable or releasable. Validate changes to `operations/`, pipelines, manifests or
monitoring assets with the most focused check available (lint/validation, dry-run, config
validation).

## Update repository context documentation

After implementation, update `/.context/` in every affected repository via the
`generate-context-repo` workflow, using the feature branch's changes as evidence and reflecting the
integrated branch state rather than one branch in isolation. Include operational changes —
deployment flow, pipeline behaviour, monitoring, alerting, runtime configuration, environment
prerequisites.
