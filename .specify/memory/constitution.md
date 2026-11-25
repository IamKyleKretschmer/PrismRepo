# Nintex Engineering Constitution

> Universal engineering principles for developing enterprise software at Nintex.

## Purpose and Scope

This constitution defines enduring principles for software development across Nintex. It applies to all products, services, clients, tools, platforms, repositories, and technologies.

The constitution is intentionally principle-led and technology-agnostic. It is not an implementation guide, architecture standard, process framework, or product-specific operating model. More specific standards may add detail for a domain, repository, product, platform, or team, but they should align with these principles.

Supporting domain standards and implementation guidance are documented in [Engineering Standards](standards/README.md).

## Core Principles

### 1. Customer Trust and Enterprise Quality

Software should be designed and built to enterprise-grade standards. Security, privacy, reliability, performance, accessibility, maintainability, and operational supportability are core product qualities, not optional enhancements.

Systems should be predictable under normal and exceptional conditions, suitable for production use, and maintainable by teams beyond the original authors.

### 2. Simplicity, Clarity, and Maintainability

Prefer the simplest design that satisfies the known requirements. Keep modules small, cohesive, and loosely coupled. Avoid unnecessary abstractions, speculative extensibility, and avoidable dependencies.

Code, contracts, documentation, and operational behaviour should be understandable by future maintainers. Complexity should be justified by clear customer, product, technical, or operational need.

### 3. Explicit Contracts and Compatibility

Communication between systems, services, modules, and clients should occur through clear, documented contracts. Public contracts should be versioned where appropriate and managed to preserve compatibility.

Breaking changes should include impact analysis, migration guidance, and a planned deprecation path.

### 4. Security, Privacy, and Compliance by Design

Security and privacy should be designed into systems from the beginning. Software should validate inputs, protect data in transit and at rest, apply least privilege, avoid leaking sensitive information, and meet applicable legal, regulatory, and contractual obligations.

Personal and sensitive data should be collected, processed, retained, logged, and deleted only according to approved requirements.

### 5. Data Integrity and Responsible State Management

Systems should preserve data correctness, consistency, and recoverability. State-changing operations should use appropriate validation, transactional behaviour, concurrency handling, idempotency, and failure recovery.

Software should avoid creating orphaned, inconsistent, or unrecoverable data.

### 6. Reliability and Operational Resilience

Software should be designed to operate reliably in production. Systems should handle expected failure modes gracefully, use appropriate timeouts and retries, degrade safely where possible, and recover without manual intervention where practical.

Operational readiness includes deployment, configuration, monitoring, alerting, rollback, support, and cost considerations.

### 7. Observability and Supportability

Production behaviour should be observable. Systems should emit useful logs, metrics, traces, health signals, and diagnostic information appropriate to their runtime context.

Telemetry should support timely investigation of incidents while excluding secrets, credentials, and sensitive personal data.

### 8. Testability and Verification

Software should be designed so important behaviour can be verified effectively. Automated tests should cover business logic, integration points, contracts, user journeys, and failure modes at the appropriate level.

Changes should be validated before release using the most relevant checks for the affected system.

### 9. Accessibility, Localisation, and Inclusive Use

User-facing software should be accessible and inclusive. Interfaces and user-visible messages should support applicable accessibility and localisation requirements.

Accessibility and localisation should be considered during design, implementation, and validation.

### 10. Sustainable Delivery and Responsible Change

Changes should be small, reviewable, and reversible where practical. Delivery practices should support safe release, rollback, operational visibility, and continuous improvement.

Teams should document important decisions, trade-offs, and exceptions so future work can build on accurate context.

## Applying the Principles

Engineering work should demonstrate how these principles have been considered at a level appropriate to the risk and impact of the change.

High-risk or broad-impact changes should include explicit consideration of customer impact, security, privacy, data integrity, compatibility, reliability, observability, test coverage, operability, cost, and rollback.

Lower-risk changes should still preserve clarity, correctness, maintainability, and production safety.

## Governance

This constitution provides high-level engineering guidance for Nintex software development. More specific product, repository, architecture, security, operational, or delivery standards may add detail, but should not conflict with these principles.

Exceptions should be documented with rationale, impact, owner, and review date.

Amendments should be reviewed by the owning engineering group and include the reason for the change and any migration or communication impact.

**Version**: 2.0.0 | **Owner**: Nintex Engineering | **Ratified**: 2026-05-01 | **Last Amended**: 2026-05-01
