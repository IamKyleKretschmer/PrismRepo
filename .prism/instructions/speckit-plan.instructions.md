---
applyTo: '**/specs/**/plan.md'
---

# Plan Conventions for `/speckit-plan`

Applies when the plan phase is run with the upstream `/speckit-plan` skill, which cannot be edited.
This file is the only lever over its behaviour, so it states Prism's plan rules in full and
overrides that skill's defaults and `plan-template.md` wherever they disagree. The template gives
the section skeleton; this file governs the altitude of what goes in it.

`/prism-plan` is the Prism-owned alternative and does **not** read this file — it carries the same
rules internally. The two are kept deliberately in step: **any change here must be mirrored in the
`# Prism plan rules` section of the
`/prism-plan` skill, and vice versa.**

## Feature branch naming

Scoped to `/speckit-plan` only. Apply the "Handing a feature branch to the next phase" section of
[branch-and-folder-naming.instructions.md](<PRISM_ROOT>/.prism/instructions/branch-and-folder-naming.instructions.md) in full — but
only if the current branch is NOT the repository's default branch. Being on the default branch
means the specify phase's branch step never ran: leave the branch as-is and warn the user.

## User stories: consume, never invent

Stories belong to the spec: `/prism-specify` writes `### User stories` with stable `US<n>`, the
`FR-<n>` each claims, and a priority. Use those identifiers unchanged — the tasks phase builds one
phase per story and keys task frontmatter to them.

If `spec.md` declares none, do **not** invent them silently: derive a set from the Must-have
requirements grouped by user intent — not by component or layer — record it in `plan.md` as
provisional with the `FR-<n>` each claims, and report the spec defect, noting that the tasks phase
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

**Include**: affected repositories and which owns each component (resolve via
[glossary](<PRISM_ROOT>/docs/prism/glossary.md) + `repos.yml`); components/modules at **directory**
granularity; each boundary's contract (operations, semantics, errors, idempotency, compatibility);
component interactions per user story (sync/async, consistency and transaction boundaries, failure
handling); data ownership and invariants; quality-attribute targets and the choice meeting each;
operations architecture; alternatives rejected and why (in `research.md`).

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

## Required sections (add to the template)

**`## Requirement Coverage`** (after Constitution Check): one row per `FR-<n>` in `spec.md` giving
the requirement, what covers it (the user story, `Cross-cutting`, or `Not planned`), the owning
component at directory granularity, and a reason for `Not planned` or a location for
`Cross-cutting`. No requirement may be absent — every other phase verifies forwards (a task traces
to a requirement), and only this verifies backwards (a requirement reaches a component). An
unclaimed **Must-have** is an ERROR: assign it, record it as cross-cutting, or raise it, never leave
it out. `Not planned` needs a reason; silence is not deferral. `Won't have` entries carry no
`FR-<n>` and are not listed. The tasks phase reads this table, so keep identifiers exactly as
`spec.md` writes them.

**`## Feasibility & Risks`** (after Requirement Coverage): verdict — `Feasible` /
`Feasible with caveats` / `Not feasible as specified` — with justification; assumptions and what
breaks if each is wrong; risks with likelihood/impact and mitigation; open questions needing a
spike, and what a different answer would change; blast radius. A non-`Feasible` verdict must be
raised with the user, not planned around.

**`## Test Strategy`**: levels in play and what each proves here; which repository and suite hosts
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
- **`contracts/`** — one file per boundary: operation, purpose, direction, payload field names,
  error semantics, idempotency, authorisation, versioning impact. Signatures allowed,
  implementations not. Omit entirely if the feature adds no external boundary.
- **`quickstart.md`** — prerequisites, proof scenarios, expected outcomes. Reference the other
  artefacts rather than restating them.

## Self-check before reporting completion

Verify and state the result; fix failures rather than reporting them. No implementation inside
fenced blocks (trees, signatures and diagrams only); source tree stops at directories; no
class/method names; no physical schema in `data-model.md`; no `T###` identifiers; all three required
sections present and complete; every repository resolved via `repos.yml`; budgets met; no fact
restated in a second section where a citation would do.

Traceability runs both ways: every user story traces to a component and an interface or interaction;
every `FR-<n>` in `spec.md` appears in Requirement Coverage, and **an unclaimed Must-have is an
ERROR, not a warning**; story identifiers match `spec.md`, or the derivation is recorded and
reported; every cross-cutting requirement has an architectural statement, not a passing mention.
