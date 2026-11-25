---
name: prism-tasks
description: Break a feature plan into a task index plus one black-box task file per task. Prism's self-contained alternative to speckit-tasks.
argumentHint: Optional task generation constraints
handoffs:
  - label: Create Work Items
    agent: create-ado-work-item
    prompt: Create Azure DevOps work items for these tasks
  - label: Implement Tasks
    agent: speckit.implement
    prompt: Implement the tasks, one at a time
  - label: Create Checklist
    agent: speckit.checklist
    prompt: Create a checklist for the following domain...
---

In Prism's skills, agents and the files they reference, `<PRISM_ROOT>` means the root of this repository. Substitute it before reading files or running commands.

Follow the agent instructions defined in [prism-tasks](../../.claude/skills/prism-tasks/SKILL.md)
