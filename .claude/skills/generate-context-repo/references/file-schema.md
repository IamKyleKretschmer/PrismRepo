# File schema: progressive disclosure context system

This schema defines the generated `/.context/` tree. The tree must support
progressive disclosure: repo-wide orientation first, task routing second,
module detail third, and direct links into source code last. Files map to C4
abstractions as follows:

| C4 level         | Primary file(s)                                                                          |
| ---------------- | ---------------------------------------------------------------------------------------- |
| L1 System Context | `_shared/overview.md`                                                                    |
| L2 Containers     | `_shared/module-map.md`, `manifest.yml` module entries, per-module `overview.md`         |
| L3 Components     | per-module `structure.md`, `integrations.md`, `key-patterns.md`                          |
| L4 Code           | links and path references only — never duplicated as prose                               |

## Linking to code

Context files are a map to the code, not a copy of it. Prefer links over
prose:

- Link to repo files, and to specific types, functions, methods, routes, CLI
  commands, and configuration keys, with line ranges where helpful
  (e.g. `path/to/file.ext:L120-L168`).
- Reuse those same references in `evidence:` annotations and checklist items.
- Never paste code bodies (L4); link to the definition so the code stays the
  single source of truth.

## Per-file front-matter (required on every generated context file)

Every generated Markdown file under `/.context/` must begin with a YAML
front-matter block so agents can detect scope, negative space, and
staleness without reading the body:

```yaml
---
covers: <one-line statement of what this file authoritatively covers>
does-not-cover: <one-line negative scope> — see <relative/path/to/other/file.md>
last-verified: 2026-06-15 against <commit-sha-or-branch>
source-of-truth: <code|tests|docs|schema|generated-artifact|pipeline|mixed>
confidence: <high|medium|low>
size-budget: ~200 lines
---
```

Default `size-budget` is ~200 lines per file; ~400 lines for
`patterns-checklist.md`, `playbooks.md`, and module `key-patterns.md`. When
a normal file would exceed its budget, split by `##` heading into a peer file
and update `does-not-cover` cross-references. For shared checklists and
playbooks, preserve one file per scope; move container-local detail to the
matching module file before exceeding the budget.

## Root entry point (repo_root)

The repo root `AGENTS.md` is the single entry point into `.context/`. It
carries exactly one line about the context system, and everything else is
reached through `.context/manifest.yml`. Its body is **exactly** this block,
with no additions, omissions, or reworded text:

```markdown
**Repository context**: when requiring repo context, read `.context/manifest.yml`, load the files it lists under `always-load`, then load a `shared-triggers` file or a module's context only when its hint matches the task. If your change makes anything under `.context/` inaccurate, update it as described in the `## Keeping context current` section of `.context/README.md#keeping-context-current`.
```

Reconcile `AGENTS.md` to that block:

- **Block missing**: create `AGENTS.md` if absent, otherwise add the block
  below its title (or at the top if it has none).
- **Block present but different**: replace it with the block above, verbatim.
- **Larger section from an earlier run of this skill** (mission, judgment
  boundaries, always-load links, quick-start routing, self-maintenance
  triggers): replace the whole section with the block above and list what was
  removed in the report.
- **Block already identical**: leave the file untouched.

Every other part of `AGENTS.md` belongs to the team: make no other edits.

## .context/README.md

Sections required:

1. Purpose — what the context pack is for.
2. C4 decomposition — one short paragraph naming the software system, listing
   the containers, and linking to `_shared/module-map.md`.
3. Reading order — broad-to-specific loading path. Always starts with
   `_shared/overview.md` and `_shared/patterns-checklist.md`, then follows
   `manifest.yml` load triggers to module files, then follows links into
   source.
4. Module index and task routing — link to `manifest.yml`, explain that it is
   a short index whose entries point at each module's own `manifest.yml` for
   entrypoints, owned paths, surfaces and full triggers, and explain how to
   choose relevant modules.
5. Evidence policy — how claims and commands are backed by repo files,
   including the source-of-truth and confidence policy.
6. For people — one short paragraph linking `ARCHITECTURE.md` and stating
   that it sits outside the progressive-disclosure chain.
7. Keeping context current: a `## Keeping context current` section (the root
   `AGENTS.md` links to this anchor) listing the concrete changes that require
   updating `.context/`, each naming the file(s) to update. For example: new
   container added, new variant added to a polymorphic type, build or test
   command changed, any referenced file moved or renamed.

## .context/manifest.yml (root index)

The root manifest is an **index, not a catalogue**. It must stay short enough
to read in one glance on a large monorepo: one entry per module with a
one-line summary and a one-line `load-when` hint, pointing at the module's own
`manifest.yml` for everything else. Detail that belongs to a container
(`entrypoints`, `owned-paths`, `public-surfaces`, the full `load-when` and
`skip-when` lists, `context-files`, `owners`) lives **only** in the module
manifest. Do not repeat it at the root. Every `_shared/` file appears either
under `always-load` or under `shared-triggers` with a one-line hint saying when
to load it. Budget: about 10 lines per module plus one line per shared file,
under ~120 lines total.

Required shape:

```yaml
schema: progressive-disclosure-context/v1
repo: <repo-name>
archetype: <service|web-app|library|cli|monorepo|unknown>
c4-system: <software-system-name>
last-verified: <date> against <commit-sha-or-branch>

# Root index only. Module manifests carry entrypoints, owned paths, public
# surfaces, full load-when/skip-when triggers and context-files.

always-load:
  - _shared/overview.md
  - _shared/patterns-checklist.md
  # include generated change-safety files such as anti-patterns.md,
  # playbooks.md, source-of-truth.md, change-impact.md, definition-of-done.md

shared-triggers:             # every _shared/ file not in always-load, with a one-line load hint
  _shared/<file>.md: <one line of task or file-change signals>

modules:
  - id: <container-id>
    manifest: <container-id>/manifest.yml
    container-type: <service|cli|web-app|library|job>
    summary: <one line: what the container is and does>
    load-when: <one line of task or file-change signals; the full list is in the module manifest>

non-module-paths:            # optional; paths outside any module and the shared file that covers them
  <path>/: <_shared/file.md>
```

The root manifest does **not** reference `ARCHITECTURE.md`; that document is
for people and sits outside the progressive-disclosure chain.

## .context/{module}/manifest.yml (module manifest)

Every module folder carries the full container declaration. Required shape:

```yaml
id: <container-id>
path: <source path or list of paths the container owns>
container-type: <service|cli|web-app|library|job>
c4-level: container
description: <one or two lines>
entrypoints: [<source paths agents should inspect first>]
owned-paths: [<path globs this container owns, including its test projects>]
public-surfaces: [<API routes, exports, events, config keys, images>] # optional
load-when: [<task or file-change signals that should load this module>]
skip-when: [<signals that this module is irrelevant>]
context-files: [<the .md files in this folder, in reading order>]
owners: [<team or CODEOWNERS pointer>] # optional
last-verified: <date> against <commit-sha-or-branch>
```

## .context/ARCHITECTURE.md (single page, for people)

Generate exactly one human-readable architecture summary at
`.context/ARCHITECTURE.md`. It is a reading companion for onboarding and
review, not an agent entry point: the root manifest does not reference it,
and `.context/README.md` links to it under a short "For people" heading.

Requirements:

- One page: roughly 120 to 180 lines of prose and tables; no front-matter
  block (it is not an agent context file) but state the commit and date it
  was verified against in the opening paragraph.
- Sections, in order: what the system is; the shape (one Mermaid diagram of
  containers and external systems); layers or containers as a table; the one
  or two defining design ideas; one end-to-end runtime walk-through; data;
  eventing or integration; background work (if any); security; delivery
  (build, promotion, alerting); known risks and debt; where to read more.
- Plain language a new team member can follow without opening the code.
  Name projects and a handful of key types; do not cite line numbers.
- Every statement must still be true of the code; derive it from the same
  evidence as `_shared/overview.md` and `_shared/module-map.md`, and link to
  the repo's long-form architecture docs (for example arc42 or ADRs) where
  they exist rather than duplicating them.
- Keep it current: bump the stated date and commit whenever a container is
  added or removed or a section's claim changes.

## .context/_shared/ — shared repo context

Generate the shared files that have evidence-backed content. `overview.md`,
`module-map.md`, `patterns-checklist.md`, and `build-and-test.md` are
normally required. Generate `dependency-rules.md`, `conventions.md`, and
`operations.md` when the repo has enough evidence to make them useful.
Generate the brownfield artefacts (`glossary.md`, `anti-patterns.md`,
`playbooks.md`, `decisions.md`, `generated-code.md`, `concept-index.md`,
`ownership.md`, `definition-of-done.md`, `source-of-truth.md`,
`invariants.md`, `contracts.md`, `change-impact.md`) when there is evidence
to populate them. Do not create placeholder files only for symmetry.

### Structural / pattern files

- `overview.md`            — **L1 System Context.** Repo purpose, the software
                              system identity, human actors, external systems
                              consumed and exposed, boundaries, non-goals.
                              Optional C4 System Context Mermaid diagram.
                              (Glossary lives in `glossary.md`, not here.)
- `module-map.md`          — **L2 Containers.** Top-level containers
                              (deployable/runnable units), responsibilities,
                              entry points, likely change locations, runtime
                              relationships. Optional C4 Container Mermaid
                              diagram.
- `patterns-checklist.md`  — **Cross-container change-review checklist** that
                              agents must consult before any change. Group by
                              `## <Category>` headings (the categories listed
                              in the skill; omit those with no evidence). Each
                              item:
                              `- [ ] <imperative rule> — <one-line rationale>
                              (evidence: <path>[:Lstart-Lend])`. Prefer
                              enforceable, tool-checked rules and name the
                              enforcing tool (lint, type-check, test). Suffix
                              unverified-but-likely items with `(?)` plus a
                              `> TODO:`. Link to `invariants.md`,
                              `contracts.md`, and `change-impact.md` instead
                              of restating their catalogues. Keep one file per
                              scope; push container-local rules to module
                              `key-patterns.md`. Size budget ~400 lines.
- `dependency-rules.md`    — Allowed dependency flow, layering rules,
                              import/call constraints, ownership boundaries
                              (the rules behind the C4 container relationships).
- `conventions.md`         — Code style, naming, testing, error handling,
                              localisation/accessibility/security conventions
                              that apply across containers.
- `build-and-test.md`      — Verified install, build, run, lint, test,
                              migration, and validation commands (what is
                              available). The mandatory subset to run before
                              claiming a change complete lives in
                              `definition-of-done.md`.
- `operations.md`          — CI/CD, deployment, environments, configuration,
                              secrets, monitoring, incidents.

### Brownfield navigation artefacts

- `glossary.md`            — Alphabetised domain terms, internal codenames,
                              acronyms. Each entry:
                              `**Term** — definition (evidence: <path>)`.
                              Call out name-vs-concept mismatches explicitly.
- `anti-patterns.md`       — Footguns, deprecated zones, "do not touch"
                              areas, known traps, look-alike-but-different
                              APIs. Same checklist shape as
                              `patterns-checklist.md` but each item framed as
                              `- [ ] DO NOT … — <rationale>; instead, <safer
                              alternative> (evidence: <path>[:Lstart-Lend])`.
- `playbooks.md`           — Recipes for the most common change types. One
                              `##` heading per playbook with fixed sub-
                              headings: *Trigger*, *Files to touch* (ordered
                              list with paths), *Patterns to satisfy*
                              (links into `patterns-checklist.md`),
                              *Verification* (exact commands),
                              *Gotchas* (links into `anti-patterns.md`).
                              Size budget ~400 lines.
- `decisions.md`           — Index of ADRs, design docs, and key commits/PRs
                              that explain non-obvious choices. Pointer-only;
                              do not duplicate decision content.
- `generated-code.md`      — Every generated path with: source of truth,
                              regen command, and a `DO NOT EDIT` marker.
- `concept-index.md`       — Reverse lookup from concept name → file path(s),
                              specifically for concepts whose names do not
                              appear in code. Omit when grep on the obvious
                              term already lands in the right files.
- `ownership.md`           — Team/owner per container or concern. Omit if
                              `CODEOWNERS` is sufficient; otherwise link to
                              it from here.
- `definition-of-done.md`  — The exact commands an agent must run and pass
                              before claiming a change complete, plus the
                              context-maintenance steps (which files to
                              update, when to bump `last-verified`).
- `source-of-truth.md`     — Precedence order for conflicting evidence and
                              how to set `confidence`. Example concerns:
                              generated clients vs hand-written code, schemas
                              vs docs, tests vs comments, production config
                              vs sample config. Use TODOs for unresolved
                              conflicts.
- `invariants.md`          — Behavioral, domain, data integrity, security,
                              and state-machine rules that must remain true.
                              Each item names owning source paths, tests, and
                              verification commands when available.
- `contracts.md`           — Public compatibility surfaces: APIs, events,
                              persisted formats, CLI arguments, configuration
                              keys, package exports, extension points, and
                              consumer-facing behavior. Include consumers
                              when evident.
- `change-impact.md`       — Risk and blast-radius map. Required item shape:
                              `If you touch <path/concept>, also inspect
                              <paths/concepts> and run <commands> (evidence:
                              <path>[:Lstart-Lend])`.

## .context/{module}/ — per confirmed container (L2)

Each module folder represents one L2 container. Generate only files with
content not covered by `_shared/`:

- `manifest.yml`     — always generate. Declares the container type, entry
                       points, `load-when`, and `skip-when` triggers.
- `overview.md`      — always generate. Container purpose, runtime shape,
                       external interfaces, and the L3 components it contains.
                       Optional C4 Container or Component Mermaid diagram.
- `structure.md`     — always generate. **L3 Component view.** Components
                       inside the container, their responsibilities, entry
                       points, and collaborations. Link to source paths;
                       do not duplicate code (L4).
- `key-patterns.md`  — generate when the container has repeatable
                       container-local patterns. Same checklist shape as
                       `_shared/patterns-checklist.md`. Do not duplicate
                       cross-container rules already in the shared checklist.
- `anti-patterns.md` — generate when the container has container-specific
                       footguns not covered by `_shared/anti-patterns.md`.
- `playbooks.md`     — generate when the container has container-specific
                       recipes not covered by `_shared/playbooks.md`.
- `integrations.md`  — generate when the container depends on APIs, events,
                       services, generated clients, packages, or other
                       containers.
- `build-test.md`    — generate when the container has container-specific
                       verification commands.
- `invariants.md`    — generate when the container has local behavioral,
                       domain, data-integrity, security, or state-machine
                       invariants not covered by `_shared/invariants.md`.
- `contracts.md`     — generate when the container exposes local public
                       surfaces not covered by `_shared/contracts.md`.
- `change-impact.md` — generate when the container has local risk or
                       blast-radius relationships not covered by
                       `_shared/change-impact.md`.
- `operations.md`    — generate when the container has container-specific
                       operational knowledge.
- `conventions.md`   — omit if `_shared/conventions.md` fully applies.

## Diagrams

Diagrams are optional. When used, they must be Mermaid fenced blocks placed
inside the relevant file (no separate diagram files):

- `_shared/overview.md` may contain a C4 System Context diagram.
- `_shared/module-map.md` may contain a C4 Container diagram.
- A module's `overview.md` or `structure.md` may contain a C4 Component
  diagram for that container.

Skip a diagram rather than fabricate one. Diagrams must agree with the prose
and the evidence references.

## Omission rule

A missing file is meaningful signal. Do not create empty files for
consistency. If a container has no unique conventions, omit `conventions.md`.
If a category has no evidence, omit the section from the patterns checklist.
If there are no domain terms, footguns, recurring change recipes, ADRs,
generated files, hidden concepts, or ownership rules, omit the matching
brownfield artefact entirely.

## Freshness

The `last-verified` value in front-matter is authoritative. An agent that
modifies code referenced by a context file must:

1. Update the file's content if the referenced behaviour changed.
2. Bump `last-verified` to the current date and commit SHA.

A file whose `last-verified` is older than the most recent commit touching
its referenced paths is presumed stale and must be re-validated before use.

## TODO marker format

Where human input is needed:

> TODO: [what is needed and why]