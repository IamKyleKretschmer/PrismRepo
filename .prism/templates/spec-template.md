---
azure-devops-parent-feature-id: [PARENT_FEATURE_ID]
---

# Feature Specification: [FEATURE NAME]

**Feature Branch**: `[###-feature-name]`  
**Created**: [DATE]  
**Status**: Draft  
**Input**: User description: "$ARGUMENTS"

<!--
  OUTCOME ALTITUDE. This specification fixes WHAT users need and WHY. How it is built belongs to
  plan.md and must not be pre-empted here: no languages, frameworks, libraries, APIs, storage
  mechanisms, schemas, or file and directory names. A delivery constraint stated by the requester
  (for example "a single-page HTML app") may be recorded in Context as the requester's constraint,
  never as a design choice made here.

  Every requirement states an observable outcome a tester could verify without asking what was
  meant, and carries a stable FR identifier. Delete every instructional comment and unused
  placeholder before delivering the specification.
-->

## Overview

### Problem to solve

<!-- INSTRUCTION: In this section, clearly articulate the problem that this feature is intended to solve. Use customer-centric language to describe the issue. -->
<!-- TODO: Replace with problem statement. What is the limitation the customer is wanting to overcome? Example: When (situation), I want to (motivation), so I can (expected outcome). -->

### Goals

<!-- TODO: Replace with success goals and how each is measured. How will we know when we have reached a successful outcome? Keep each measure technology-agnostic and observable — telemetry, customer feedback, financial, time-to-complete. -->

| Goal | How success is measured |
| ---- | ----------------------- |
|      |                         |

### Context

<!-- TODO: Replace with solution context. Is there more detail about the solution, the approach? Something forcing this feature? Internal or external factors? Record any delivery constraint the requester has already fixed, and attribute it to them. -->

### Personas

<!-- INSTRUCTION: Reference the persona catalogue at `.specify/memory/personas.md` for full persona details. List which personas are relevant to this feature and briefly explain how the feature serves them. -->
<!-- TODO: Replace with the relevant personas (e.g., Carmine, Eric, Lara, Ursula) and describe how this feature addresses their needs. -->

### Terminology

<!-- TODO: Define any feature-specific terminology, acronyms, or domain language needed to understand this specification. Omit this section if no special terms are required. -->

| Term | Definition |
| ---- | ---------- |
|      |            |

## Functional Requirements

### Requirements

<!--
  INSTRUCTION: Group by MoSCoW priority. Every requirement carries a stable `FR-<n>` identifier,
  assigned sequentially and never renumbered — user stories, the plan and every task reference
  them. State an observable outcome ("System MUST …", "Users MUST be able to …"), not a mechanism.
  Won't-have entries record what is deliberately out of scope; they carry no FR identifier because
  they are not requirements.
-->

**Must have**

- **FR-1**: <!-- TODO: e.g. "System MUST retain the note across a browser restart without an explicit save action." -->

**Should have**

- **FR-2**: <!-- TODO: Replace with should-have requirements, or state that there are none. -->

**Won't have**

- <!-- TODO: Replace with what is deliberately out of scope, and why. No FR identifier. -->

<!--
  Reserve `[NEEDS CLARIFICATION: question]` for choices with no reasonable default, where the
  readings have materially different implications. Maximum three per specification; everything
  else becomes an informed guess recorded under Assumptions. Example:
  - **FR-n**: System MUST retain user data for [NEEDS CLARIFICATION: retention period not specified]
-->

### User stories

<!--
  INSTRUCTION: A story is a slice of user-visible value that can be built, demonstrated and
  verified on its own — never a layer, a component, or a phase of work. Prefer few meaningful
  stories over many thin ones: if a story claims a single requirement that is not independently
  demonstrable, it belongs inside a neighbouring story.

  `US<n>` identifiers are the contract with the later phases — the plan builds one Component
  Interactions subsection per story, and the tasks phase builds one phase per story in the order
  recorded here. Assign sequentially, list in priority order, and never renumber.

  Each story inherits the highest MoSCoW priority among the requirements it claims. The MVP is the
  set of stories whose requirements are all Must-have; if that set is not a coherent shippable
  increment, say so in Context — the plan cannot fix a scoping problem.

  COVERAGE: every `FR-<n>` above belongs to exactly one story. A requirement claimed by no story is
  a specification defect; one claimed by two is a story boundary drawn in the wrong place. Standing
  concerns that constrain every feature rather than this one — accessibility, localisation,
  privacy, security, performance, observability — are not requirements here: they live in the
  constitution and the affected repositories' `/.context/`, and the plan states how the design
  satisfies them.
-->

| US | Story | Requirements | Priority |
| -- | ----- | ------------ | -------- |
| US1 | <!-- As a [persona], I want [outcome] so that [value]. --> | FR-1 | Must have |

### Open questions

<!-- TODO: Replace with open questions and outstanding items. What is not resolved or outstanding? A question that already has a working default recorded under Assumptions belongs here, not in a NEEDS CLARIFICATION marker. -->

### Considerations

<!-- TODO: Replace with what this feature raises beyond the constitution's standing principles: UX/UI considerations, and boundary behaviour (empty state, maximum size, concurrent use, loss of connectivity, permission denied). -->

### Assumptions

<!-- TODO: Replace with feature assumptions. Have some assumptions been made due to missing information or data? Record the default chosen, not just the uncertainty. -->

### Investigations / Research / Notes

<!-- TODO: Replace with research findings and notes. Include any research, notes or other details learned from customer input. Customer context and details can help an engineering team understand requirements better. -->

## Dependencies

<!-- TODO: One entry per dependency on another team — what is needed from them, and by when. If there are none, state "No dependencies on other teams at this time" rather than deleting the section. -->

| Team | What is needed | Needed by |
| ---- | -------------- | --------- |
|      |                |           |

## Companion Specifications

<!-- INSTRUCTION: `prism-grill-me` sessions produce per-aspect companion files beside this one (`spec-qa.md`, `spec-pm.md`, …). They are never merged into this file's body, and a companion with no row here is invisible to the plan and tasks phases. Add a row for every `spec-*.md` in the feature folder; remove a row only when the file itself is gone. -->

| Aspect | File | Summary |
| ------ | ---- | ------- |
|        |      |         |
