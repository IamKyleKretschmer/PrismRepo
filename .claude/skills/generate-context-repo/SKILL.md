---
name: generate-context-repo
description: >
  Scaffold a progressive disclosure agent context system for an existing
  repository holding the `code` role, using a C4-aligned decomposition (System
  Context → Container → Component → Code) and an evidence-backed patterns
  checklist. Use when asked to set up agent context or create the /.context/
  directory structure for a `code`-role repository. Run it on every `code`-role
  repository in the solution; in a `single-repo` topology that is the one
  repository holding both the `prism` and `code` roles. See the glossary for
  role/topology definitions.
---

# Skill: generate-context-repo

Scaffold a practical progressive-disclosure context system for a chosen
repository. The generated tree must let later coding agents understand the
repo broadly, then drill into the files, types, methods, patterns, and
verification steps needed to design and implement changes safely.
Decomposition follows the [C4 model](https://c4model.com/) so the context
layers match how architects and developers already reason about the code:

- **L1 – System Context** → `_shared/overview.md` (the software system, its
  human actors, external systems it depends on or is consumed by, repo
  boundary, non-goals).
- **L2 – Containers** → `_shared/module-map.md`, a one-line entry per module
  in the root `manifest.yml`, and the full declaration in each module's own
  `manifest.yml` (deployable or independently runnable units: services, CLIs,
  web apps, jobs, shared libraries published as packages). Each container
  normally becomes one module under `.context/{module}/`.
- **L3 – Components** → per-module `structure.md`, `integrations.md`, and
  `key-patterns.md` (internal components and their responsibilities, entry
  points, and collaborations).
- **L4 – Code** → never duplicated as prose. Link to canonical source files
  (cite line ranges where helpful) so agents read the code itself.

The repo root `AGENTS.md` is the canonical entry point: one fixed line pointing
at `.context/manifest.yml`, which routes agents to everything else. This skill defines the *workflow*; the detailed
file shapes, required front-matter, and per-file contents live in
[references/file-schema.md](./references/file-schema.md).

## Principles

- **Evidence-backed.** Every statement cites the repo files, types, methods,
  or config that justify it. Mark anything you cannot verify with
  `> TODO: ...`; never invent architecture, patterns, or domain knowledge.
- **Link, don't restate.** Context files are a map to the code, not a copy.
  Link directly to repo files, types, functions, methods, routes, and config
  keys (with line ranges where helpful) so agents jump to the source of
  truth. Never reproduce code (L4) as prose.
- **Declare negative space.** Each file states what it does *not* cover and
  links to the file that does, so agents load the minimum needed.
- **Omit rather than pad.** A missing file is meaningful signal. Never create
  empty, speculative, or symmetry-only files.
- **Keep files small and routed.** Respect the schema's size budgets; push
  detail down a level before a file grows unwieldy.
- **Progress from broad to specific.** Default reading starts at the repo
  overview and mandatory shared files, then follows manifest load triggers to
  modules, then follows file/type/method links into source.
- **Keep the root manifest an index.** The root `manifest.yml` carries one
  summary line and one `load-when` hint per module and points at the module
  manifest for everything else. Container detail (entrypoints, owned paths,
  surfaces, full triggers) lives only in `.context/{module}/manifest.yml`, so
  the root stays readable however many containers a monorepo has.
- **Write one page for people.** `.context/ARCHITECTURE.md` is a single
  human-readable architecture summary for onboarding and review. It is not an
  agent entry point and the root manifest does not reference it.

## Workflow

1. **Confirm scope.** Ask which repo to target if unspecified; confirm the
   repo, the C4 "software system" it represents, and its purpose before
   proceeding.
2. **Inspect existing context.** Read existing agent instructions and any
   `/.context/` files already present.
3. **Inventory evidence.** Read repo structure, manifests, build/test config,
   source entry points, docs, pipelines, tests, and infra/deployment files.
4. **Decompose with C4.**
   - L1: the software system, its human/role actors, and external systems it
     depends on or serves.
   - L2: containers (deployable or independently runnable units) — each
     normally becomes one module under `.context/{module}/`.
   - L3: the major components inside each container and their entry points.
   - L4: never documented as prose; link to the code instead.
5. **Extract the patterns checklist.** Scan each container for repeatable
   rules any change must respect, across the categories in [Pattern categories
   to scan](#pattern-categories-to-scan). Capture each as an evidence-backed
   checklist item.
6. **Identify brownfield artefacts.** Decide which navigation files the
   evidence warrants (the file schema defines each one's shape and purpose):
   `glossary`, `anti-patterns`, `playbooks`, `decisions`, `generated-code`,
   `concept-index`, `ownership`, `definition-of-done`, `source-of-truth`,
   `invariants`, `contracts`, and `change-impact`.
7. **Propose and confirm.** Present the container boundaries, the
   patterns-checklist categories, and the artefacts you will generate
   (including the source-of-truth hierarchy and any high-risk impact areas).
   **Stop for confirmation.**
8. **Generate context files** per the schema: required front-matter, links to
  the relevant repo files/types/methods, optional Mermaid C4 diagrams, and
  evidence references. If a source branch is specified, use its commit
  history and diffs; otherwise use the current checked-out branch.
9. **Author the patterns checklist** at `_shared/patterns-checklist.md`, plus
   module `key-patterns.md` for container-local rules. The checklist states
   rules and links to `invariants.md`, `contracts.md`, and `change-impact.md`
   for the underlying catalogues rather than restating them.
10. **Author the confirmed brownfield artefacts**, each backed by evidence;
    omit any without evidence.
11. **Author the manifests.** Write each module's `manifest.yml` with the full
    container declaration (entrypoints, owned paths, public surfaces, full
    `load-when` and `skip-when`, `context-files`). Then write the root
    `manifest.yml` as a short index: per module only `id`, `manifest`,
    `container-type`, a one-line `summary` and a one-line `load-when` hint.
    Never copy module detail into the root; if the root exceeds roughly 120
    lines, move detail down.
12. **Author `ARCHITECTURE.md`.** Write the single-page, human-readable
    architecture summary at `.context/ARCHITECTURE.md` per the schema, from
    the same evidence as `_shared/overview.md` and `_shared/module-map.md`.
    Link it from `.context/README.md` ("For people") and the repo `README.md`;
    do not reference it from the root manifest.
13. **Wire up the entry point.** Reconcile the root `AGENTS.md` to the fixed
    pointer block in the schema's [root entry point](./references/file-schema.md#root-entry-point-repo_root)
    section. It points at `.context/manifest.yml` and leaves all further
    loading to the manifest. Make no other edits to `AGENTS.md`.
14. **Validate the manifests.** Check, and fix if needed, that every
    `_shared/` file is listed in the root `manifest.yml` under either
    `always-load` or `shared-triggers` (with a one-line hint), that the root has a
    one-line `load-when` hint per module, and that every module
    `manifest.yml` has `load-when` and `skip-when` triggers.
15. **Report** created/updated files, the C4 decomposition, the
    patterns-checklist categories populated, artefacts generated versus
    deliberately skipped, evidence gaps, TODOs, and recommended next
    maintenance checks.

## Pattern categories to scan

Scan only the categories with evidence; record each finding as a
patterns-checklist item. Where a category has a dedicated catalogue file
(`invariants`, `contracts`, `change-impact`), the checklist links to it rather
than duplicating its contents.

- architectural / layering (allowed call directions, ports & adapters,
  hexagonal or onion boundaries)
- module & dependency rules (what may import what; public vs. internal API)
- **extending polymorphic / sum / strategy types** — adding a variant to a
  discriminated union, enum, sealed/abstract hierarchy, plugin registry,
  factory, or strategy touches every exhaustive switch/match, registry entry,
  serializer/deserializer, validation, persisted enum/migration, public API
  contract, and test matrix together
- data flow & state (request lifecycle, transactions, caching, idempotency)
- error handling & result/exception conventions
- logging, telemetry, tracing, correlation IDs
- security (authn/authz, input validation, secret handling, OWASP-relevant
  mitigations)
- configuration & feature flags
- concurrency, async, cancellation
- persistence & migrations
- behavioral / domain invariants (business rules, state-machine rules,
  impossible states, cross-field validation)
- API contracts, versioning, backwards compatibility
- eventing, messaging, queues, topics, retries, dead-letter handling
- testing pyramid, test placement, fixtures, mocking conventions
- localisation / i18n / accessibility (where applicable)
- performance budgets, pagination, batching
- code style, naming, file layout
- build, lint, format, type-check gates
- CI/CD, deployment, environment promotion
- deprecation & migration windows (parallel-running, sunset dates, shims)

## Judgment boundaries for this skill

- Never generate commands you cannot verify from toolchain config.
- Never invent domain knowledge, architecture, patterns, footguns, glossary
  entries, playbooks, or decisions — use TODO markers instead.
- Never document L4 (code) as prose; link to source instead.
- Never create a brownfield artefact with no evidence; an empty or
  speculative file actively misleads agents.
- Never flatten all details into shared files; route container-local detail to
  the relevant module context.
- Never put container detail (entrypoints, owned paths, surfaces, full
  trigger lists) in the root `manifest.yml`; it is an index that points at
  module manifests.
- Never skip `ARCHITECTURE.md`, and never turn it into an agent file: no
  front-matter, no line-number citations, no reference from the root manifest.
- Always confirm the target repo, the C4 container boundaries, the
  patterns-checklist categories, and the brownfield artefacts to be generated
  before writing any files.

