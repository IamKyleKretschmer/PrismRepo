---
name: generate-context-prism
description: >
  Scaffold a lightweight, solution-level progressive-disclosure context layer
  in the repository holding the `prism` role. Applies only to the `multi-repo`
  topology (where the `prism` and `code` roles are separate repositories); it is
  unnecessary in a `single-repo` topology. It links to repos declared in
  /.prism/config/repos.yml, distinguishes `code`-role repos from read-only
  `context` repos, and defers detailed guidance to each repo's own /.context
  or declared contextPath.
---

# Skill: generate-context-prism

Create a very high level, lightweight context layer for the active Prism
solution. This is the **solution layer (L0)** above each repo's own C4
context: it orients an agent across the solution, then routes into the
relevant repo context.

> **Applicability**: This skill applies **only to the `multi-repo` topology** —
> where the repository holding the `prism` role is separate from one or more
> `code`-role repositories. In a `single-repo` topology (one repository holds
> both the `prism` and `code` roles) there is no cross-repo layer to summarise,
> so do **not** run this skill. See the
> [glossary](<PRISM_ROOT>/docs/prism/glossary.md) for role/topology definitions.

The aim is a thin map, not a second copy of each repo's documentation. Link to
each repository declared in `/.prism/config/repos.yml`, summarise its purpose
and role, and describe how the repositories relate. Detailed file shapes live
in [references/file-schema.md](./references/file-schema.md).

## Repo roles (from `/.prism/config/repos.yml`)

The two repo lists have different purposes and must be treated differently:

- **`coreSolutionRepos`** (the `code` role) — owned by this Prism instance,
  local on disk, and code **will be written** to them. Summarise each one and
  link agents into its full `/.context/` and `AGENTS.md` so deep,
  change-oriented work can continue there.
- **`contextRepos`** (the `context` role) — read-only references for
  **architectural and interface integration knowledge only**. Summarise what
  the solution consumes from them (contracts, APIs, conventions). For remote
  `context`-role repos, use only their declared `contextPath`. Never plan or
  imply code changes in a `context`-role repo.

## Principles

- **Lightweight.** This layer orients and routes; it does not duplicate repo
  detail. Prefer a short summary plus a link over restating anything.
- **Defer to repo context.** Treat each core repo's own `/.context/` and
  `AGENTS.md`, and each context repo's declared `contextPath`, as the source
  of truth. Link to them; do not copy them.
- **Evidence-backed.** Base summaries on what you read in each repo. Mark gaps
  and anything you cannot verify with `> TODO: ...`; never invent domain
  knowledge, architecture, or relationships.
- **Separate core from context.** Always make a repo's role explicit so agents
  know where they may write code and where they may only read.
- **Omit rather than pad.** A missing file is meaningful signal. Never create
  empty, speculative, or symmetry-only files.

## Workflow

1. **Read the repo manifest.** Parse `/.prism/config/repos.yml` (see
   `/.prism/config/repos.schema.json` for shape). Enumerate every
   `coreSolutionRepos` and `contextRepos` entry with its role, path/source,
   `contextPath`, and description. Also read any other repos the prompt
   references.
2. **Gather evidence per repo.** For core repos, prefer `/.context/`,
   `AGENTS.md`, package manifests, and top-level docs. For context repos, use
   their local `path` or remote `source`; if remote, stay within the declared
   `contextPath` and never pull the whole repo. Do not assume any context is
   complete or current.
3. **Synthesise the solution view.** Identify what the solution does end to
   end, which core repos own which responsibilities, which context repos supply
   which contracts/interfaces, and the key relationships and integration points
   between them.
4. **Generate or update the solution context** per the schema: a manifest, a
   solution overview, a repo map that links into each repo's `.context`, and
   (when there is evidence) cross-repo integration points. Keep every file
   short and link-led. Use TODO markers for gaps.
5. **Wire up the entry point.** Reconcile the `prism`-role repository's root
   `AGENTS.md` so its `## Repository Context` section matches the required
   text **verbatim** — see [`AGENTS.md` (repo root) — required `## Repository
   Context` section](./references/file-schema.md#agentsmd-repo-root--required--repository-context-section)
   for the canonical block and the missing/different/identical rules. Insert
   the section if absent; replace its body wholesale if it differs. Change
   nothing else in `AGENTS.md`.
6. **Report** every file created or updated, whether the `## Repository
   Context` section was inserted, replaced, or already matched, the
   core-vs-context repo breakdown, and every TODO requiring human input.

## Judgment boundaries for this skill

- Never invent domain knowledge, architecture, or cross-repo relationships —
  use TODO markers instead.
- Never duplicate a repo's detailed `.context`; link to it and summarise.
- Never plan, suggest, or imply code changes in a `contextRepos` repo.
- Never pull a whole remote context repo; require and stay within its
  `contextPath`.
- Never reword, extend, or partially merge the required `## Repository Context`
  section in the root `AGENTS.md` — it is reproduced verbatim or not at all.
- Never touch any part of the root `AGENTS.md` outside that section.
- Always confirm the repo roles and boundaries before writing any files.
- Always add this header to every generated file:
  `> Auto-generated by generate-context-prism skill. Verify before committing.`