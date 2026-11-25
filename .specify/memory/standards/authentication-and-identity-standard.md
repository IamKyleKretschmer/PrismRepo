# Authentication and Identity Standard

This standard defines Nintex-wide requirements for authentication, identity propagation, and trusted user context in software systems.

## Purpose and Scope

This standard applies to all Nintex software that authenticates users, services, workloads, or external systems, or that consumes identity context from an upstream component.

This document is technology-agnostic. Product-, platform-, and implementation-specific details belong in implementation guidance.

## Core Requirements

### Approved Identity Mechanisms

Systems must use approved Nintex identity and authentication mechanisms for their runtime context.

Unapproved custom authentication flows, identity formats, or trust relationships must not be introduced without explicit review and approval.

### Trusted Identity Sources

Systems must derive identity only from trusted claims, tokens, certificates, or approved upstream identity headers.

Identity data from untrusted request inputs must not be treated as authenticated identity context.

### Consistent Identity Resolution

Where more than one identity source may be present, the precedence and fallback rules must be defined, documented, and applied consistently.

Implementations must avoid ambiguous, conflicting, or silently changing identity resolution behaviour.

### Least Privilege and Authorisation Boundaries

Authenticated identity must be evaluated only for the permissions required for the requested action.

Authentication and authorisation responsibilities must be clearly defined, including any reliance on trusted upstream components.

### Identity Propagation

When identity context is propagated across services or components, it must be transferred securely, minimally, and in a way that preserves traceability and trust boundaries.

### Auditability and Privacy

Authentication and identity handling must support operational investigation and audit needs without exposing secrets or unnecessary personal data.

Identity-related telemetry must be proportionate, protected, and aligned with privacy and compliance obligations.

## Implementation Expectations

Each implementation should document:

- the approved authentication mechanism in use
- the trusted identity sources it accepts
- identity precedence and fallback rules
- the component responsible for authorisation
- any upstream trust assumptions
- relevant logging, auditing, and privacy controls

## Related Guidance

- [Nintex Engineering Constitution](../constitution.md)
- [Authentication and Identity Implementation Guidance](authentication-and-identity-implementation-guidance.md)

**Owner**: Nintex Engineering
