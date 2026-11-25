# QA Clarifications: To-Do List API

Companion to [spec.md](spec.md). Records the QA (test plan) decisions made in `prism-grill-me` sessions.

## Session 2026-11-25

--- 
**Question**: Which test levels must cover the requirements?  
**Decision**: Every FR gets at least one automated black-box acceptance test that calls the running API the way a real caller would. Unit tests also cover the FR-2 title rules in detail.  
**Rationale**: Acceptance tests prove the behaviour callers see. Unit tests cover the many title edge cases cheaply.  
**Author**: `Kyle Kretschmer` 2026-11-25  

--- 
**Question**: Which title boundary cases (FR-2) must be tested, and are leading and trailing spaces kept or trimmed?  
**Decision**: Rejected: no title, empty title, title of only spaces, tabs or newlines, and 201 characters. Accepted: 1 character, 200 characters, and 200 characters that include emoji and accented letters (counted as user-perceived characters). Non-English text must come back unchanged. Leading and trailing spaces are kept as supplied, and the length check counts them.  
**Rationale**: Covers both sides of every rule in FR-2, and keeps the spec's "stored exactly as supplied" rule.  
**Author**: `Kyle Kretschmer` 2026-11-25  

--- 
**Question**: How is "survives a restart" (FR-5, FR-6) verified?  
**Decision**: An automated test creates and completes items, restarts the service in the test environment, then lists them and checks that each item and its completion status are unchanged. It runs in CI.  
**Rationale**: Losing data on restart is easy to miss and serious for callers, so it must be checked automatically.  
**Author**: `Kyle Kretschmer` 2026-11-25  

--- 
**Question**: Should the concurrent-use rules be tested automatically, and at what load?  
**Decision**: Yes. One test sends 50 creates in parallel and checks that 50 items come back, each with a different identifier. Another sends 10 parallel completes of the same item; all must succeed, and the item must end up complete with one completion time.  
**Rationale**: Turns the Considerations rules for concurrent use into checks that can pass or fail.  
**Author**: `Kyle Kretschmer` 2026-11-25  

--- 
**Question**: What should error tests (FR-2, FR-8) check: a stable error code, or the exact message wording?  
**Decision**: Tests check a stable machine-readable error code that differs per rule (for example "title missing" versus "title too long"), plus the presence of a readable message. For FR-8 they also check that the message names the identifier. Every error test then lists the items to confirm nothing changed.  
**Rationale**: Exact-wording checks break whenever the text changes or is translated (constitution principle 9). Listing afterwards proves the "MUST NOT change" part of each requirement.  
**Author**: `Kyle Kretschmer` 2026-11-25  

--- 
**Question**: How are tests isolated, given there is one shared list?  
**Decision**: Each acceptance test run starts against a fresh, empty instance.  
**Rationale**: FR-4 (empty list) and the "oldest first" ordering in FR-3 can only be checked exactly from a known starting state.  
**Author**: `Kyle Kretschmer` 2026-11-25  

--- 
**Question**: Does marking an item complete again change its completion time (FR-7, FR-9)?  
**Decision**: No. A test completes an item, completes it again later, and checks that the completion time is unchanged.  
**Rationale**: FR-9 records when the item was *first* marked complete, and this matches the concurrency decision that a completed item has one completion time.  
**Author**: `Kyle Kretschmer` 2026-11-25  

--- 
**Question**: How are items with the same creation time ordered (FR-3)?  
**Decision**: Equal creation times are ordered by when the system accepted each create. The order is strict and stays the same across repeated listings, and tests check the exact order.  
**Rationale**: The 50 parallel creates may share a creation time. Without a tie-break, both callers and tests would see an unpredictable order.  
**Author**: `Kyle Kretschmer` 2026-11-25  

--- 
**Question**: What do callers get when they try to complete an item using a malformed or empty identifier (FR-8)?  
**Decision**: The same "not found" error as an unknown identifier. Tests cover both a well-formed unknown identifier and a malformed one.  
**Rationale**: A caller can't act differently in the two cases, it keeps the contract simple, and it reveals nothing about the identifier format.  
**Author**: `Kyle Kretschmer` 2026-11-25  

--- 
**Question**: When does the test suite block a change?  
**Decision**: All acceptance and unit tests must pass in CI before a change can be merged. Before the feature is called done, QA confirms there is at least one passing acceptance test for every FR (FR-1 to FR-9).  
**Rationale**: Prevents tests from being deferred and forgotten, and gives every requirement verified coverage.  
**Author**: `Kyle Kretschmer` 2026-11-25  
