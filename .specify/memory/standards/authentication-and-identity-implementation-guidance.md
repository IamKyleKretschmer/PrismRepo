# Authentication and Identity Implementation Guidance

This document provides implementation-level guidance that supports the [Authentication and Identity Standard](authentication-and-identity-standard.md).

## Purpose and Scope

Use this guidance for concrete authentication integrations, identity claim handling, header trust rules, and fallback behaviour.

This is the correct place for platform-specific or shared-stack rules that are too detailed for the constitution or the standard.

## Approved Implementation Profile: Vibranium JWT

Use Nintex Vibranium JWT authentication where this profile is the approved mechanism for the runtime environment.

Resolve the user identifier from the `sub` claim when it is present.

If the `sub` claim is not present and the request comes through an approved trusted path, the `ntx-user-id` header may be used as the fallback source of the user identifier.

The `ntx-user-id` header must not be trusted when it originates from an untrusted client or bypasses the approved trust boundary.

## Implementation Notes

Implementations using this profile should document:

- where the trust boundary is enforced
- which components are permitted to set or forward trusted identity headers
- how conflicting claim and header values are handled
- how identity values are logged, masked, or excluded from telemetry
- how authorisation decisions are separated from authentication

## When to Add More Guidance

Add additional implementation profiles when Nintex approves other authentication mechanisms, trust models, or deployment patterns.

Keep implementation guidance specific enough to be actionable, but avoid duplicating shared library code or repository-specific setup instructions.

## Related Guidance

- [Authentication and Identity Standard](authentication-and-identity-standard.md)
- [Nintex Engineering Constitution](../constitution.md)

**Owner**: Nintex Engineering
