# Research: To-Do List API

Phase 0 output for [plan.md](plan.md). One entry per decision: what was decided, why, and which alternatives were rejected.

## R1. Repository topology

- **Decision**: Single-repo. `PrismRepo` (this repository) holds both the `prism` and `code` roles, and `/.prism/config/repos.yml` records it under both.
- **Rationale**: Chosen by the requester. A new, standalone service has no existing code repository to join.
- **Alternatives rejected**: A separate code repository. It adds cross-repo coordination for no benefit at this size.

## R2. Language and runtime

- **Decision**: C# on .NET 10 (the current LTS), using ASP.NET Core minimal APIs.
- **Rationale**: The requester chose C#/ASP.NET Core. .NET 10 is the supported LTS line, while .NET 8 reaches end of support in November 2026. Minimal APIs suit three operations with no MVC overhead.
- **Alternatives rejected**: .NET 8, which is out of support at release. Controller-based MVC, which is more structure than three operations need.

## R3. Persistence

- **Decision**: An embedded SQLite database file owned by the service, written in WAL mode. Every state change happens inside a single transaction.
- **Rationale**: Chosen by the requester. It meets FR-5 and FR-6 (data survives a restart) without separate infrastructure. It is transactional, which supports constitution principle 5 and the concurrency rules. It easily handles lists of hundreds of items (spec Assumptions).
- **Alternatives rejected**: A managed database, which brings operational cost that is only justified once there are multiple instances (see the R8 risk). A JSON file, which would need hand-rolled locking and atomic replacement, so data integrity is weaker.

## R4. Ordering and tie-break (FR-3)

- **Decision**: Each item gets a monotonically increasing sequence number when its create commits. The list is ordered by creation time, then by sequence number.
- **Rationale**: Gives a strict, stable order even when parallel creates share a timestamp (QA decision on ordering ties). Because the sequence is assigned inside the create transaction, it reflects the order the system accepted the creates.
- **Alternatives rejected**: Time-ordered identifiers on their own, which can tie or skew within the same millisecond. Leaving tie order unspecified, which the QA decision rejected.

## R5. Identifiers (FR-1, FR-8)

- **Decision**: The system assigns opaque, unique string identifiers that never change and are never reused. Callers must treat them as opaque. Any identifier that doesn't resolve to an item, including a malformed or empty one, gets the same `todo_not_found` response.
- **Rationale**: Meets the spec's identifier assumption and the QA decision on malformed identifiers. Callers learn nothing about the identifier format.
- **Alternatives rejected**: Sequential integers, which expose the item count and invite enumeration. A separate "invalid identifier" error, which the QA decision rejected.

## R6. Title validation (FR-2)

- **Decision**: A title is rejected when it is missing, empty, made only of whitespace (any Unicode whitespace), or longer than 200 user-perceived characters (grapheme clusters). Length includes leading and trailing whitespace, and accepted titles are stored byte-for-byte as supplied. Each rule has its own stable error code.
- **Rationale**: Applies the spec's title Assumptions and the QA boundary decisions. Counting grapheme clusters means emoji and combined accented letters count as one character each.
- **Alternatives rejected**: Counting UTF-16 code units, which miscounts emoji. Trimming titles, which contradicts "stored exactly as supplied".

## R7. Error format

- **Decision**: Errors use the standard HTTP problem-details format (RFC 9457) with a stable machine-readable `code` extension and a human-readable `detail`. Messages are in English. Consumers localise based on `code`.
- **Rationale**: A widely supported standard. The stable codes meet the QA error-assertion decision and constitution principle 9.
- **Alternatives rejected**: A custom error envelope, which is non-standard and more to document. Message text alone, which can't be reliably asserted in tests or translated.

## R8. Concurrency and deployment shape

- **Decision**: One service instance owns the database file. Completing an item is a conditional update in one transaction that records the completion time only if none is set. That makes it idempotent and first-writer-wins.
- **Rationale**: Meets FR-7, FR-9 and the concurrency rules without distributed coordination.
- **Alternatives rejected**: Several instances sharing the file, which is unsafe on network storage. Locking inside the application, which the database transaction makes redundant.

## R9. Authentication and authorisation

- **Decision**: Every operation needs a valid Vibranium JWT (the approved profile in the authentication implementation guidance). The caller's identity comes from the `sub` claim. The `ntx-user-id` header fallback is **not** accepted, because this service sits behind no approved trusted path. Any authenticated caller may use the shared list (spec Assumptions). The issuer, audience and signing keys come from runtime configuration.
- **Rationale**: The requester chose Vibranium JWT, and it is the only approved mechanism the standard documents. Turning off the header fallback removes an identity source that could be spoofed.
- **Alternatives rejected**: No authentication, which breaks the standard. Accepting `ntx-user-id`, which is only allowed through an approved trusted path and none exists here.

## R10. Versioning

- **Decision**: All operations sit under a `/v1` path prefix. Changes within v1 may only add things.
- **Rationale**: Constitution principle 3 (explicit, versioned contracts), at the lowest cost.
- **Alternatives rejected**: Header-based versioning, which is harder for low-code consumers such as Carmine.

## R11. Test stack and CI

- **Decision**: xUnit for both unit and acceptance tests. Acceptance tests run the real service in-process (the ASP.NET Core test host) against a temporary SQLite file and a test JWT signing key. GitHub Actions is the CI pipeline, because the repository's remote is on GitHub.
- **Rationale**: These are the standard .NET test tools. In-process hosting can restart the service against the same file, which covers the restart persistence tests. Each test run gets a fresh temporary file, which gives the isolation the QA decision asks for.
- **Alternatives rejected**: Testing against a deployed environment, which is slower and can't guarantee an empty starting list.
