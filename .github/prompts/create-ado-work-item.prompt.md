---
name: create-ado-work-item
description: Create Azure DevOps Product Backlog Items from the current feature spec and tasks
agent: create-ado-work-item
---

In Prism's skills, agents and the files they reference, `<PRISM_ROOT>` means the root of this repository. Substitute it before reading files or running commands.

Run the `/.claude/skills/create-ado-work-item` workflow to create Azure DevOps Product Backlog Items from the target feature's `spec.md` and `tasks.md`, using `/.prism/config/work-item-parameters.yml` for shared team settings.
