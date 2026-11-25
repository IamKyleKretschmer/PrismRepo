# Context-Driven Development (CDD) new feature journey

How to perform context-driven development - this is free-form coding in your target repo, but with the benefits of the Prism framework. See [CDD overview](./overview.md#core-concepts)

This is intended for smaller jobs that do not require significant design, UX or architecture effort and do not span multiple repos or work items.

This is NOT intended for normal feature development. Use SDD instead.

## Usage

Just use the `/cdd` agent or skill in your prompt (`/prism:cdd` with the Claude plugin; see [command names](./sdd.md)). Ensure the target repo (from those in the Prism solution) is defined. eg

``` text
/cdd Add a select-all option to the list of selector checkboxes in the tables listing page #Tables.tsx in the ntx-tables-client repo.
```

## Work tracking

All work should be done via a work item for tracking, and code changes will be recorded in both code comments and any NDP, technical or user documentation updated by the CDD agent.

## Setup

For multi-repo work, the `/cdd` agent relies on the solution repos setup defined in [setup.md](./install/setup.md).

