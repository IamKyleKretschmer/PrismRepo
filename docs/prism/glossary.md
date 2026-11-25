# Prism Glossary — Repository Roles & Topologies

## Repository roles

A **repository role** is a named responsibility a repository fulfils within a Prism
solution. A single repository may hold more than one role, subject to the
[topologies](#topologies) below.

| Role | Meaning | Writable? | Location |
| ---- | ------- | --------- | -------- |
| `prism` | Repo holding the Prism framework, constitution, and SDD artefacts (specs, plans, tasks). | Yes | Local |
| `code` | Repos holding product source code that features are implemented against. | Yes | Local |
| `context` | Read-only repos referenced for architectural/interface context; never written to. | No | External (local path or remote source) |

## Topologies

A **topology** describes how roles are distributed across repositories in a solution.
Every solution is in exactly one topology.

| Topology | Meaning |
| -------- | ------- |
| `single-repo` | One repository holds **both** the `prism` and `code` roles. |
| `multi-repo` | The `prism` role lives in a repository separate from one or more `code` repositories. |

`context`-role repositories may exist in either topology and are always additional,
external repositories.


## Solution

A **Solution** is the complete set of repositories governed together, as declared in
[`.prism/config/repos.yml`](../../.prism/config/repos.yml), regardless of topology.

## `repos.yml` roles

The existing `.prism/config/repos.yml` properties **identify roles** (the keys are
unchanged — this is a terminology alignment, not a schema change):

| `repos.yml` property | Identifies the role | Cardinality |
| -------------------- | ------------------- | ----------- |
| `prism` | `prism` | exactly one repository |
| `coreSolutionRepos` | `code` | one or more repositories (array) |
| `contextRepos` | `context` | zero or more repositories (array) |

`single-repo` configuration: In a `single-repo` topology the one repository holds
both the `prism` and `code` roles, so it MUST be listed **both** under `prism` **and** as the
sole element of `coreSolutionRepos`. Listing it in both places makes the dual role explicit
and lets every skill/instruction resolve the `prism` role via `prism` and the `code` role via
`coreSolutionRepos` uniformly — in `single-repo` both resolve to the same repository; in
`multi-repo` they resolve to distinct repositories.

```yaml
# single-repo example — the same repository holds both roles
prism:
  name: acme-app        # holds the prism role
  path: .
coreSolutionRepos:
  - name: acme-app      # SAME repository — also holds the code role
    path: .
```

## Legacy → canonical migration

Skills, instructions, prompts and `AGENTS.md` use the canonical role/topology terms
throughout. Human-facing docs (this site's `README.md` and the pages under `docs/prism/`)
may use the plain-language shorthand "prism repo" and "code repo" for readability, and fall
back to the precise role phrasing only when explaining the role model itself (for example
when a single repository holds both the `prism` and `code` roles). The table below maps the
older, ambiguous terms to their canonical replacements.

| Legacy term | Canonical replacement |
| ----------- | --------------------- |
| "Prism repo", "prism repo", "`prism` repo" (as a fixed identity) | repository holding the `prism` role |
| "code repo", "core solution repo" | repository holding the `code` role |
| "context repo" | repository holding the `context` role |
| "Integrated", "mono-repo" | `single-repo` topology |
| "Stand alone" | `multi-repo` topology |
