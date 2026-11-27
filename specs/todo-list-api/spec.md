---
azure-devops-parent-feature-id: [PARENT_FEATURE_ID]
---

# Feature Specification: To-Do List API

**Feature Branch**: `feat/todo-list-api`  
**Created**: 2026-11-25  
**Status**: Draft  
**Input**: User description: "Add a simple to do list API with create list and complete endpoints"

## Overview

### Problem to solve

When I am building a tool or integration that needs to track simple pieces of outstanding work, I want to record to-do items, see them all, and mark them done through a programmatic interface, so that I can keep track of what is outstanding without building and maintaining my own tracking store.

### Goals

| Goal | How success is measured |
| ---- | ----------------------- |
| Callers can manage a to-do list end to end | A caller can create an item, see it listed as incomplete, mark it complete and see it listed as complete, with every acceptance condition in this spec verified by test |
| The interface is easy to adopt | A developer new to the interface completes the create → list → complete flow in under 5 minutes using only its published documentation |
| Callers get clear feedback on mistakes | 100% of rejected requests (invalid title, unknown item) return an error that states the reason, and change no data |

### Context

The requester has fixed the delivery form: this is an **API** (a programmatic interface for other software to call), offering three operations: **create** an item, **list** items, and **complete** an item. There is no user interface in scope.

The feature is deliberately small. Both user stories below are Must-have and together form the MVP: a list that can be added to, read, and progressed.

### Personas

- **Eric — Engineer**: the primary consumer. Eric builds integrations and needs a predictable interface with clear, diagnosable errors rather than opaque failures.
- **Carmine — App & Asset Creator**: a secondary consumer who may wire the interface into low-code solutions, and benefits from simple operations and plain error messages.

### Terminology

| Term | Definition |
| ---- | ---------- |
| To-do item | A single piece of work to be done, consisting of a title and a completion status. |
| Title | The short text describing the to-do item. |
| Complete | The status of an item that has been marked as done. A newly created item is incomplete. |
| Caller | The software (or developer) using the API. |

## Functional Requirements

### Requirements

**Must have**

- **FR-1**: Callers MUST be able to create a to-do item by supplying a title. The response MUST include the new item's unique identifier, its title, an incomplete status and its creation time.
- **FR-2**: The system MUST reject a create request whose title is missing, empty, whitespace-only, longer than 200 characters, or larger than 4,000 Unicode code points, returning an error that states which rule was broken, and MUST NOT create an item. The system MUST also reject any create request whose body is larger than 64 KB, without creating an item.
- **FR-3**: Callers MUST be able to retrieve all to-do items. Each item MUST show its identifier, title, completion status and creation time, and items MUST be ordered oldest first by creation time. Items with the same creation time MUST be ordered by when the system accepted each create, so the order is the same on every listing.
- **FR-4**: When no to-do items exist, retrieving the list MUST return an empty list, not an error.
- **FR-5**: Created items MUST still be returned by the list after the service restarts.
- **FR-6**: Callers MUST be able to mark an existing to-do item complete by its identifier. After that, the list MUST show the item as complete, including after the service restarts.
- **FR-7**: Marking an already-complete item complete MUST succeed and leave the item complete and otherwise unchanged.
- **FR-8**: Marking an item complete using an identifier that does not exist MUST return a "not found" error that names the identifier, and MUST NOT change any item. A malformed or empty identifier MUST get the same "not found" error.

**Should have**

- **FR-9**: A completed item SHOULD record the time it was first marked complete, and the list SHOULD show that time for completed items.

**Won't have**

- Editing an item's title. Out of scope to keep the feature simple.
- Deleting items. Out of scope for this iteration.
- Marking a completed item as incomplete again. Out of scope for this iteration.
- Due dates, priorities, tags, or multiple separate lists. Out of scope for a "simple" list.
- Filtering, searching or paging the list. Not needed at the expected list sizes (see Assumptions).
- Per-user lists and sharing. Not in scope (see Assumptions).
- Any user interface.

### User stories

| US | Story | Requirements | Priority |
| -- | ----- | ------------ | -------- |
| US1 | As an engineer (Eric), I want to add to-do items and see the full list so that I can record and review outstanding work. | FR-1, FR-2, FR-3, FR-4, FR-5 | Must have |
| US2 | As an engineer (Eric), I want to mark a to-do item complete so that the list shows what is done and what is still outstanding. | FR-6, FR-7, FR-8, FR-9 | Must have |

### Open questions

- Azure DevOps parent Feature ID for linking work items has not been provided yet. [NEEDS CLARIFICATION: Azure DevOps parent Feature ID required for work item linking]
- Should lists be separated per user or per caller? The working default is one shared list (see Assumptions). Revisit if more than one consumer is expected.
- Is a 200-character title limit right for expected use? The working default is 200 characters.

### Considerations

- **Empty state**: an empty list is a normal, successful result (FR-4).
- **Repeated requests**: completing an item twice is harmless (FR-7). Repeating a create request creates a second item with the same title; duplicate titles are allowed.
- **Concurrent use**: two callers creating items at the same moment MUST both get distinct items; two callers completing the same item at the same moment MUST both see it complete.
- **Error clarity**: errors state the reason in plain language so that a caller can fix the request without further help (Eric's pain point with opaque errors).
- **Title content**: titles are stored and returned exactly as supplied, apart from the length and blank checks in FR-2. They may contain any characters, including non-English text.

### Assumptions

- **Single shared list**: all callers see and change one list. There are no per-user lists or ownership.
- **Access control**: who may call the API follows the organisation's standing security standards. The plan phase decides how, and this spec adds no feature-specific permission rules.
- **List size**: lists are expected to stay small (hundreds of items, not millions), so returning the whole list in one response is acceptable.
- **Title limit**: 200 characters, counted as user-perceived characters. Leading and trailing spaces are kept and count towards the limit.
- **Title size ceiling**: a title may also hold at most 4,000 Unicode code points. A single user-perceived character can be built from an unlimited number of code points (for example, stacked combining marks), so the 200-character limit alone does not bound a title's size. The ceiling is high enough that 200 of the most complex emoji (about 15 code points each) are still accepted.
- **Request size**: a create request body may be at most 64 KB. This comfortably fits the largest valid title.
- **Timestamps**: creation and completion times are reported in UTC.
- **Identifiers**: identifiers are assigned by the system, are unique, and never change or get reused.

### Investigations / Research / Notes

- The scope is taken directly from the requester's one-line description. No customer research was supplied.

## Clarifications

### Session 2026-11-25 (QA)

- Six test-plan decisions are recorded in [spec-qa.md](spec-qa.md). The only change to this spec: leading and trailing spaces in a title count towards the 200-character limit (Assumptions).
- Four more test-plan decisions are recorded in [spec-qa.md](spec-qa.md). Changes to this spec: FR-3 now sets the order of items with the same creation time, and FR-8 now gives malformed or empty identifiers the same "not found" error.

### Session 2026-11-27 (cross-artifact analysis)

- The 200-character limit counts user-perceived characters, which on their own do not bound a title's size. FR-2 now also rejects titles larger than 4,000 Unicode code points and create requests larger than 64 KB (Assumptions: Title size ceiling, Request size).

## Dependencies

No dependencies on other teams at this time.

## Companion Specifications

| Aspect | File | Summary |
| ------ | ---- | ------- |
| QA | [spec-qa.md](spec-qa.md) | Test levels, title boundary cases, restart and concurrency tests, error-code checks, test isolation, ordering ties, malformed identifiers and the CI release gate |
