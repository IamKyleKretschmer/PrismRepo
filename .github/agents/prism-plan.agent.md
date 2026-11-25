---
name: prism-plan
description: Create the architecture-level technical plan and design artefacts for a feature. Prism's replacement for speckit-plan.
argumentHint: Optional guidance for the planning phase
handoffs:
  - label: Create Tasks
    agent: prism-tasks
    prompt: Break the plan into tasks
  - label: Create Checklist
    agent: speckit.checklist
    prompt: Create a checklist for the following domain...
---

In Prism's skills, agents and the files they reference, `<PRISM_ROOT>` means the root of this repository. Substitute it before reading files or running commands.

Follow the agent instructions defined in [prism-plan](../../.claude/skills/prism-plan/SKILL.md)
