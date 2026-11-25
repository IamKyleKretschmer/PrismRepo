# Quickstart: To-Do List API

End-to-end scenarios that show the feature works. The operations and error codes are defined in [contracts/todo-api-v1.md](contracts/todo-api-v1.md).

## Prerequisites

- .NET 10 SDK installed.
- A Vibranium JWT accepted by the service's configured issuer and audience. For local and test runs, use a test signing key set in the service's local configuration (research R9, R11).
- An empty data file. A fresh path gives an empty list.

## Scenarios

| # | Do | Expect | Proves |
| - | -- | ------ | ------ |
| 1 | List items on a fresh instance | `200` with an empty `items` array | FR-4 |
| 2 | Create item "Buy milk" | `201`, a new `id`, `isComplete: false`, `createdAt` set | FR-1 |
| 3 | Create an item with a blank title, then one with a 201-character title | `400` with `title_blank`, then `400` with `title_too_long`. Listing still shows one item | FR-2 |
| 4 | Create "Call Sam", then list | Both items listed, "Buy milk" first | FR-3 |
| 5 | Complete "Buy milk", then list | That item shows `isComplete: true` with `completedAt` | FR-6, FR-9 |
| 6 | Complete "Buy milk" again | `200`, with `completedAt` unchanged | FR-7 |
| 7 | Complete `does-not-exist` | `404` with `todo_not_found`, and the message names the identifier | FR-8 |
| 8 | Restart the service on the same data file, then list | Both items, with completion status unchanged | FR-5, FR-6 |
| 9 | List without a token | `401` with `unauthenticated` | Cross-cutting: security |

The feature works end to end when every scenario gives the expected result.
