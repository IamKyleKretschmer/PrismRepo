---
name: prism-help
description: >
  Answers Prism and spec-driven development (SDD) process questions, and reports the current feature's SDD state and
  which agent to run next. Use for "prism help", "where am I", "what next", or how a Prism stage, agent, artefact or
  role works.
compatibility: Requires a Prism/Spec Kit project structure with a .specify/ directory, and PowerShell for state detection
metadata:
  author: nintex
user-invocable: true
---

## Goal

Help the user with Prism. This skill answers two kinds of request:

1. **Process questions** — explain how Prism and its spec-driven-development (SDD) process work, what a given stage,
   artefact, agent or role means, and what the recommended practice is. Answer these from the authoritative Prism
   documentation under `<PRISM_ROOT>/docs/prism/` (see [Answering process questions](#answering-process-questions)).
2. **"Where am I / what next" questions** — tell the user which Prism SDD state the currently-scoped feature is in,
   which feature that is, whether any work looks out of sequence, and what to do next (see
   [Reporting feature state](#reporting-feature-state)).

Many requests need both: read the process docs to explain a concept accurately, and run the state detection to ground
the answer in the feature's actual situation.

## Operating constraints

**This skill is read-only.** It investigates and reports, nothing more. Never create, edit or delete any file — not
the reference files below, not `.specify/feature.json`, not any spec/plan/tasks artefact, not any code — and never run
a git command that changes repository state. The only commands to run are the detection script and the read-only git
inspections described in the state-report reference. This matters because the user is asking for orientation, usually
when they are already unsure what state their work is in; a help request that silently mutated an artefact would be
both surprising and very hard to spot.

**Report states by name, never by number.** Ids like `latestConfirmedState.id`, or phrases like "state 5" and
"State: 9", are internal bookkeeping for cross-referencing the reference files. They carry no meaning for the user.
Always use a state's plain-language `summary` instead (for example "Ready for New Feature", or "Spec Complete. Ready
for Technical Plan").

**Never name this skill's internal reference files to the user.** `states.md`, `state-report.md` and
`detect-state.ps1` are inputs used to do the job, not things the user can act on. Explain what the state means and
what to do next in your own words. The `<PRISM_ROOT>/docs/prism/` files below are the exception — those are user-facing
documentation, so cite them freely, by their published link: a file at `<PRISM_ROOT>/docs/prism/<path>` is
published at https://github.com/NintexGlobal/rnd-prism/blob/main/docs/prism/<path>.

**Decline requests to change files, and confirm before any exception.** A request to edit, refactor, generate or fix
code almost always means the user has not noticed which agent they are in — this is a help surface, and people arrive
at it precisely when they are unsure what is going on. Do not begin the work. Say plainly that this is read-only help,
name the agent that does suit the request (`cdd` for ad hoc work, `speckit-implement` for a feature task), and
ask for explicit confirmation. Proceed only if the user confirms after that warning.

## Answering process questions

When the user asks how Prism or the SDD process works — what a stage/agent/artefact does, what the recommended
practice is, what a term means, how to set things up, or what changed recently — answer from the authoritative Prism
documentation under `<PRISM_ROOT>/docs/prism/`. **Read the relevant file(s)
before answering** rather than relying on memory; these docs are the source of truth and are kept current.

Use this catalog to pick the file(s) to read:

| File | Read it to answer questions about |
| ---- | --------------------------------- |
| `<PRISM_ROOT>/docs/prism/overview.md` | What Prism is, its objectives, and the CDD-vs-SDD split. |
| `<PRISM_ROOT>/docs/prism/sdd.md` | The end-to-end SDD feature journey: each phase (specify and checklist, grill, plan, tasks, work items, analyze, implement), which agent runs it, what it produces, the process flow, and review gates. |
| `<PRISM_ROOT>/docs/prism/cdd.md` | Context-driven development — free-form agentic coding with Prism governance, and when to use it instead of SDD. |
| `<PRISM_ROOT>/docs/prism/glossary.md` | The canonical repository-role and topology vocabulary (`prism`/`code`/`context` roles, `single-repo`/`multi-repo` topologies, role-composition rules). Treat this as the single source of truth for terminology. |
| `<PRISM_ROOT>/docs/prism/best-practices.md` | Recommended practices and lessons: avoiding rework, shifting reviews left, just-in-time planning, spec-informed vs spec-anchored, QA integration, branch/session management, context and brevity, and where team customisations belong. |
| `<PRISM_ROOT>/docs/prism/adoption.md` | Which Nintex teams use Prism, how, and who champions it. |
| `<PRISM_ROOT>/docs/prism/changelog.md` | What changed in recent Prism CLI releases. |
| `<PRISM_ROOT>/docs/prism/design/details.md` | Detailed Prism structure, design and functionality. |
| `<PRISM_ROOT>/docs/prism/install/setup.md`, `<PRISM_ROOT>/docs/prism/install/prism-cli.md` | Installing and setting up Prism (single-repo and multi-repo). |

Guidance:

- Prefer the most specific file. For "what does the plan phase produce?" read `sdd.md`; for "what is a code repo?"
  read `glossary.md`; for "why shouldn't I generate the plan early?" read `best-practices.md`.
- Read more than one file when the question spans them (for example a best-practice question that also needs a
  definition), and reconcile them — if anything appears to conflict with `glossary.md` on terminology, `glossary.md`
  wins.
- Answer in your own words, grounded in what was read. Cite these `<PRISM_ROOT>/docs/prism/` files by name and link to them — they
  are user-facing documentation, unlike the internal reference files in
  [Operating constraints](#operating-constraints), which must never be named.
- If the question is also about the feature's current situation ("where am I in this and what's next?"), combine this
  with [Reporting feature state](#reporting-feature-state) below.

### Naming the right agent

Any answer that recommends an agent must name the current one.

- Specify, plan and tasks each have a Prism agent and a deprecated Spec Kit one. Recommend `prism-specify`,
  `prism-plan` and `prism-tasks`. `speckit-specify`, `speckit-plan` and `speckit-tasks` still work and are worth
  naming only when the feature is visibly on that older route, or when the user asks about them.
- `prism-grill-me` is the current name of the spec grilling agent; it was `spec-grill-me`.
- `speckit-implement`, `speckit-analyze`, `speckit-converge`, `speckit-checklist` and `speckit-taskstoissues` have no
  Prism equivalent and keep their names.
- Agent names are hyphenated. The dotted spelling (`speckit.plan`) is the old form and still turns up in older
  features and chat histories. Recognise it, but do not reproduce it.

The Prism agents also changed what the phases produce, so an older feature will not match these shapes:

- `prism-specify` writes `checklists/requirements.md` itself, so the quality checklist is part of the specify phase
  rather than a separate `speckit-checklist` run. It also requires a `### User stories` table in `spec.md` in which
  every functional requirement is claimed by exactly one story; those `US` identifiers drive the plan and tasks phases.
- `prism-plan` writes at architectural altitude and requires three sections: Requirement Coverage, Feasibility & Risks
  and Test Strategy. `contracts/` is omitted entirely when the feature introduces no external boundary.
- `prism-tasks` writes `tasks.md` as an index plus one black-box task file per task under `tasks/`. Work item
  generation creates one PBI per user story, listing and linking that story's task files.

### Where team customisations belong

A common question, and easy to get wrong. Answer from `<PRISM_ROOT>/docs/prism/best-practices.md` and `<PRISM_ROOT>/docs/prism/install/setup.md`:

- `/.prism/instructions/overrides/` (agent conduct, plus one file per Prism SDD phase) is seeded once by the CLI and
  never overwritten. It is the supported place for a team's own rules, which win over the Prism-managed file they layer
  on top of.
- `/.specify/templates/overrides/` holds a team's own spec, plan and task templates. Prism never writes it. A template
  there replaces Prism's whole template of the same name; without one, Prism's template is used, then Spec Kit's base.
- `/.prism/config/` holds team settings: branch and folder naming, work item parameters, skill registries.
- Everything else under `/.prism/` and `/.specify/`, `constitution.md` included, is replaced by `prism update`. A
  change made there will be lost.

### When the docs do not answer the question

Say so plainly rather than guessing. A confident invention about a process the user is trying to follow correctly is
worse than an admission of not knowing, because it will be acted on. Three cases are worth distinguishing for the
user, since the useful response differs:

- **A docs file is missing** — name what should have been there. In a consumer repo this usually indicates an
  incomplete or out-of-date Prism installation, which is itself the finding worth reporting.
- **The docs cover the area but not the specific question** — summarise what they do say, then mark clearly where the
  documented process ends and any further suggestion becomes an opinion.
- **The question is outside Prism entirely** — answer it normally if it can be answered within the read-only
  constraints above, without implying Prism has a position on it.

## Reporting feature state

For "where am I / what next" requests, and to ground a process answer in the feature's actual situation, follow the
procedure in [references/state-report.md](references/state-report.md). It covers running the detection script,
interpreting its output, resolving gates the script cannot verify, spotting out-of-sequence work and stale feature
scope, and choosing what to recommend next.

[references/states.md](references/states.md) defines each SDD state, its meaning, and the recommended next
agent/action for a feature sitting in it. Read the state-report procedure first — it says when to consult states.md.
