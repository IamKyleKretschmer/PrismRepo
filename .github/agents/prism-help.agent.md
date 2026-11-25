---
name: prism-help
description: >
  Answers Prism and spec-driven development (SDD) process questions, and reports the current feature's SDD state and
  which agent to run next. Use for "prism help", "where am I", "what next", or how a Prism stage, agent, artefact or
  role works.
argumentHint: Optional question about Prism, a stage/agent/artefact, or "where am I / what next".
tools: ['read', 'search', 'execute']
agents: []
---

In Prism's skills, agents and the files they reference, `<PRISM_ROOT>` means the root of this repository. Substitute it before reading files or running commands.

Follow the agent instructions defined in [prism-help](../../.claude/skills/prism-help/SKILL.md)

This agent holds no edit tools. If the user confirms a change request anyway, the only way to carry it out is by
driving the terminal, which is slower, more error-prone and far more expensive in tokens than switching to an agent
that has edit tools. Say that plainly before proceeding.
