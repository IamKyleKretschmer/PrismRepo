# Contract: To-Do API v1 (HTTP)

| | |
| - | - |
| **Owner** | `PrismRepo`, To-Do service |
| **Direction** | Inbound. Callers are integrations and low-code solutions |
| **Base path** | `/v1` (research R10) |
| **Format** | JSON request and response bodies. Times are ISO 8601 UTC |
| **Authorisation** | Every operation needs `Authorization: Bearer <Vibranium JWT>` (research R9). A missing or invalid token gets `401` with code `unauthenticated` and changes nothing |
| **Versioning** | Initial version. Changes within v1 may only add things. A breaking change requires `/v2` |

## Item representation

Field names: `id`, `title`, `isComplete`, `createdAt`, `completedAt`. `completedAt` is present only when `isComplete` is true (FR-9).

## Operations

### Create item — `POST /v1/todos`

- **Purpose**: FR-1, FR-2.
- **Request**: `title`.
- **Success**: `201 Created`. The body is the new item, with `isComplete: false`. A `Location` header points to the item.
- **Errors**: `400` with code `title_missing` (no `title` field, or the body can't be parsed), `title_empty`, `title_blank` (whitespace only), `title_too_long` (over 200 user-perceived characters) or `title_too_large` (over 4,000 Unicode code points). `413` with code `request_too_large` when the request body is over 64 KB. No item is created.
- **Idempotency**: Not idempotent. Each request that succeeds creates a new item, and duplicate titles are allowed (spec Considerations).

### List items — `GET /v1/todos`

- **Purpose**: FR-3, FR-4, FR-5.
- **Success**: `200 OK`. The body is `items`, an array of item representations ordered by creation time and then by the order the system accepted each create. If there are no items, the array is empty (FR-4).
- **Errors**: Only `401`.
- **Idempotency**: Safe, with no side effects.

### Complete item — `POST /v1/todos/{id}/complete`

- **Purpose**: FR-6, FR-7, FR-8, FR-9.
- **Request**: No body.
- **Success**: `200 OK`. The body is the item, with `isComplete: true` and `completedAt` set to the time it was first completed.
- **Errors**: `404` with code `todo_not_found`, where `detail` names the supplied identifier. The same response covers unknown, malformed and empty identifiers (research R5). No item changes.
- **Idempotency**: Idempotent. Repeat calls and parallel calls all succeed, and `completedAt` never changes after it is first set (FR-7, research R8).

## Error body

Problem-details format (RFC 9457): `type`, `title`, `status`, `detail`, plus the stable extension `code`. The values of `code` are part of this contract. Tests and consumers check `code`, never the message wording (research R7). `detail` never repeats a title the caller supplied.

Any operation can also return `500` with code `internal_error` and a generic `detail` when an unexpected failure occurs. Nothing about the failure's cause is returned, and callers may retry operations that are idempotent.

## Operational endpoint (not part of the public contract)

`GET /health` reports whether the service is running and can reach its database. No authentication is needed, and it returns no item data.
