---
name: prism-specify
description: Create a feature specification with user stories and requirement coverage declared in the spec. Prism's replacement for speckit-specify.
argumentHint: Describe the feature you want to specify
handoffs:
  - label: Clarify Spec
    agent: prism-grill-me
    prompt: Clarify the specification
    send: true
  - label: Create Plan
    agent: prism-plan
    prompt: Create the technical plan for this feature
    send: true
---

In Prism's skills, agents and the files they reference, `<PRISM_ROOT>` means the root of this repository. Substitute it before reading files or running commands.

Follow the agent instructions defined in [prism-specify](../../.claude/skills/prism-specify/SKILL.md)
