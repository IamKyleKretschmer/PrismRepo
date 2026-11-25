# Data Model: To-Do List API

Phase 1 output for [plan.md](plan.md). This is the logical model only. The physical schema is decided at implementation time.

## Entity: To-do item

| Attribute | Meaning | Source |
| --------- | ------- | ------ |
| identifier | Opaque, unique, assigned by the system, never changes or gets reused | FR-1, research R5 |
| title | Text exactly as the caller supplied it | FR-1, FR-2 |
| creation time | UTC instant at which the system accepted the create | FR-1, FR-3 |
| sequence | Increasing number assigned when the create commits, used only for ordering and never exposed | FR-3, research R4 |
| completion time | UTC instant the item was first marked complete. If it is absent, the item is incomplete | FR-6, FR-9 |

Completion status is **derived** from completion time: complete if and only if a completion time is present. It is never stored separately, so the two can't disagree.

**System of record**: the To-Do service's embedded database (research R3). No other component writes to it.

**Relationships**: none. There is one shared list, with no owners, lists or tags (spec Won't have).

## Lifecycle

```text
 create (FR-1) ──► Incomplete ──complete (FR-6)──► Complete ──complete again (FR-7)──► Complete (unchanged)
```

There is no deletion, editing or un-completing (spec Won't have). Items are kept for the life of the database.

## Invariants

1. Identifiers are unique and never change or get reused.
2. Every item's title meets the FR-2 rules at all times. A rejected create leaves no item behind.
3. Creation time and sequence are set once, at creation, and never change.
4. Completion time is set at most once. Once set, it never changes (FR-7, FR-9).
5. List order is creation time, then sequence. It is strict and the same on every listing (FR-3).
6. A failed or rejected operation changes no item (FR-2, FR-8, constitution principle 5).
7. Every committed state change survives a restart (FR-5, FR-6).

## Data classification

Titles are free text and may contain personal data. They are not written to logs or telemetry (see the plan's cross-cutting behaviours). Caller identity (`sub`) is not stored with items.
