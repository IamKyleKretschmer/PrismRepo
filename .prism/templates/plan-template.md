# Implementation Plan: [FEATURE]

**Branch**: `[type/feature-name]` | **Date**: [DATE] | **Spec**: [link]
**Input**: Feature specification from `/specs/[feature-name]/spec.md`

<!--
  ARCHITECTURE ALTITUDE. This plan fixes what is expensive to change later. Implementation
  detail is derived at implement time from the target repository's /.context/ — do not freeze
  it here. No code, pseudo-code, SQL/DDL or config contents; no class, method or internal type
  names; no file-level trees or per-file responsibilities; no field types, nullability or
  indexes; no T### task identifiers. Cite spec FR IDs instead of restating spec text.
  Budget: this file <= ~400 lines. Over budget means implementation detail crept in — cut it.
  Delete every instructional comment and unused placeholder before delivering the plan.
-->

## Summary

[Primary requirement (cite FR IDs) + the technical approach in a few sentences]

## Technical Context

<!--
  REFERENCE: /.context/manifest.yml in each affected repository for existing patterns,
  technical standards and integration guidelines. Mark genuine unknowns NEEDS CLARIFICATION —
  Phase 0 research must resolve every one before Phase 1 design.
-->

**Language/Version**: [e.g., Python 3.11, C# 12 or NEEDS CLARIFICATION]
**Primary Dependencies**: [e.g., FastAPI, ASP.NET Core or NEEDS CLARIFICATION]
**Storage**: [if applicable, e.g., PostgreSQL, blob storage or N/A]
**Target Platform**: [e.g., Linux server, iOS 15+ or NEEDS CLARIFICATION]
**Project Type**: [e.g., web service/library/cli/mobile-app or NEEDS CLARIFICATION]
**Quality-attribute targets**: [performance, availability, scale, security or compliance targets,
and for each the architectural choice that meets it]
**Constraints**: [domain-specific, e.g., <200ms p95, offline-capable, data residency]
**Operations Impact**: [provisioning, deployment, pipelines, monitoring, alerting, feature flags,
runtime configuration, or N/A]

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design and record the result.*

[[Gates](../memory/constitution.md#development-workflow--quality-gates) determined based on constitution file]

## Requirement Coverage

<!--
  ACTION REQUIRED: one row per FR-<n> in spec.md — no requirement may be absent. Every other
  phase verifies forwards (a task traces to a requirement); only this table verifies backwards
  (a requirement reaches a component). An unclaimed MUST-HAVE is an ERROR: assign it, record it
  as Cross-cutting, or raise it with the user — never leave it out. "Not planned" needs a
  reason; silence is not deferral. "Won't have" entries carry no FR-<n> and are not listed.
  /prism-tasks reads this table, so keep identifiers exactly as spec.md writes them.
-->

| Requirement | Covered by | Owning component | Reason / location |
| ----------- | ---------- | ---------------- | ----------------- |
| FR-[n] | [US<n> \| Cross-cutting \| Not planned] | [repo — directory-level component] | [reason if Not planned; location if Cross-cutting] |

## Feasibility & Risks

**Verdict**: [Feasible | Feasible with caveats | Not feasible as specified] — [justification]

<!-- A non-Feasible verdict must be raised with the user, not planned around. -->

- **Assumptions**: [each assumption + what breaks if it is wrong]
- **Risks**: [risk — likelihood/impact — mitigation]
- **Open questions**: [question — what a spike would settle — what a different answer would change]
- **Blast radius**: [what else is affected if this goes wrong]

## Project Structure

### Documentation (this feature)

```text
specs/[feature-name]/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output (omit if the feature adds no external boundary)
└── tasks.md             # Tasks phase output — NOT created by the plan phase
```

### Affected repositories & components

<!--
  ACTION REQUIRED: Resolve every repository by role via <PRISM_ROOT>/docs/prism/glossary.md and
  /.prism/config/repos.yml before naming it. Components/modules at DIRECTORY granularity only —
  the tree stops at directories. Do not list files, classes or per-file responsibilities.
-->

| Repository | Role | Components / modules touched | New or changed |
| ---------- | ---- | ---------------------------- | -------------- |
| [repo] | [prism \| code \| context] | [directory-level component] | [New / Changed] |

**Structure Decision**: [The layout chosen and why, referencing the real directories above]

**Operational Ownership**: [Which repository owns deployment, pipeline, monitoring and runtime
configuration changes for this feature]

## Boundaries & Contracts

<!--
  One entry per boundary the feature introduces or changes — HTTP endpoint, event, public
  library surface, CLI command, UI contract. Detail belongs in contracts/; summarise here.
  Omit the section if the feature adds no external boundary.
-->

| Boundary | Owner (repo) | Consumers | Purpose | Compatibility impact |
| -------- | ------------ | --------- | ------- | -------------------- |
| [name] | [repo] | [callers] | [what it is for] | [none / additive / breaking] |

For each boundary, record in `contracts/`: operations, semantics, errors, idempotency,
authorisation and versioning impact.

## Component Interactions

<!-- One subsection per user story. Say how components cooperate, not how they are coded. -->

### [US1 — story name]

- **Flow**: [which component calls which, in order]
- **Sync/async**: [per hop]
- **Consistency & transaction boundaries**: [where they fall, and what is eventually consistent]
- **Failure handling**: [what happens when each hop fails — retry, compensate, surface]

### Cross-cutting behaviours

<!--
  Constitution principles that constrain every story rather than forming one, so have no home in
  the per-story subsections above. Source them from the constitution, the affected repositories'
  /.context/, and the spec's Considerations. One row per principle this feature actually engages.
-->

| Principle | What it constrains | Architectural choice satisfying it | How it is verified |
| --------- | ------------------ | ---------------------------------- | ------------------ |
| [constitution principle] | [which components/flows] | [the choice] | [verification] |

## Data Ownership & Invariants

<!-- Entities, system of record and lifecycle. No types, nullability, indexes or DDL — those
     do not belong in the plan, and the physical model does not belong in data-model.md either. -->

| Data | System of record | Read by | Lifecycle | Invariants |
| ---- | ---------------- | ------- | --------- | ---------- |
| [entity] | [repo/component] | [consumers] | [created/updated/retained/deleted] | [what must always hold] |

## Operations Plan

- **Resource / infrastructure changes**: [required before rollout, or N/A]
- **Deployment / pipeline changes**: [setup-phase work plus per-story pipeline or release changes]
- **Observability / supportability**: [logging, metrics, tracing, dashboards, alerts, runbooks]
- **Rollout / configuration**: [feature flags, secrets, environment variables, migration
  sequencing, rollback]
- **Operational verification**: [how the above is validated — pipeline validation, dry-run,
  deployment check, alert test]

## Test Strategy

<!-- Strategy only. No individual test cases — those belong in task files. If spec.md lists a
     spec-qa.md companion, derive this from it; if one exists in the feature folder but is
     unlisted, incorporate it anyway or flag the missing link. -->

| Level | What it proves here | Repository & suite | Framework (from /.context/) |
| ----- | ------------------- | ------------------ | --------------------------- |
| [unit/integration/contract/e2e] | [what this level establishes] | [repo — suite] | [framework] |

- **Per user story**: [one line each on what verification must demonstrate]
- **Test data & environments**: [significant needs]
- **Coverage gaps**: [gap — justification]

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
| --------- | ---------- | ------------------------------------ |
| [e.g., 4th project] | [current need] | [why 3 projects insufficient] |
