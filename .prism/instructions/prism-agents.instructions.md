# Prism Agent Instructions

<!-- Loaded from the root [AGENTS.md](/AGENTS.md). Universal instructions for all coding agents working in this Prism solution (GitHub Copilot, Claude Code, Spec Kit, or any other).

Prism-owned: `prism update` overwrites this file, so do not edit it. Team instructions belong in the root [AGENTS.md](/AGENTS.md), which Prism never overwrites. -->

## File paths

In Prism's skills, agents and the files they reference, `<PRISM_ROOT>` stands for where Prism's own files are. If the Prism plugin's session-start context defines it, use that. Otherwise it is the root of the repository holding the `prism` role. Substitute it before reading files or running commands. Paths without a placeholder, with or without a leading `/`, are relative to the project root.

## Agent conduct

**ALWAYS LOAD**: @agent-conduct.instructions.md, then @overrides/agent-conduct.instructions.md, which carries this team's additions and wins where the two disagree.

## Agent constitution

The overriding principles of development are listed in @../../.specify/memory/constitution.md

## Repository Context

This solution uses `/.context/` folders to provide structured architectural context for each repository. **All agents** must source context from these folders when working on tasks that affect one or more repositories.

This solution is defined by `/.prism/config/repos.yml`, which declares each repository's **role**. Roles are defined once in the canonical [glossary](<PRISM_ROOT>/docs/prism/glossary.md): the `prism` role holds the Prism framework, constitution, and SDD artefacts (this repository holds the `prism` role); the `code` role holds the product source code features are implemented against; the `context` role is a read-only external reference. In a `single-repo` topology one repository holds both the `prism` and `code` roles, so those references resolve to the same repository; in a `multi-repo` topology the `prism` role is held by a repository separate from one or more `code` repositories. Resolve any role reference to concrete repositories via the [glossary](<PRISM_ROOT>/docs/prism/glossary.md) and `repos.yml`.

1. **Start with the manifest**: Read `/.context/manifest.yml` in the root of the repository holding the `prism` role to discover the available context sets and linked repository manifests.
2. **Load repo-level context**: When a task affects specific repositories, read the `/.context/manifest.yml` in those repositories to discover available context sets and modules.
3. **Load relevant context files**: Based on the manifest, load the context files relevant to the task scope (e.g., implementation patterns, testing conventions, build/test commands, file organisation, dependency rules).
4. **Cross-repo context**: When work spans more than one repository (a `multi-repo` topology), also review `/.context/repo/dependency-rules.md` for inter-service constraints.

## Nintex skills

Nintex domain knowledge (engineering standards, platforms, products) comes from the shared [Nintex skills marketplace](https://github.com/NintexGlobal/rnd-nintex-skills), which Prism registers in `/.claude/settings.json`. Prism enables its `engineering-standards` plugin, which provides `accessibility-wcag` and `nintex-localisation` for constitution principle 9. When a task touches a Nintex platform or product, use the matching installed skill (for example the `nsp-*` skills) rather than relying on general knowledge. If a skill this process names isn't available, say so instead of guessing its guidance.

## Prism's docs

When you point a contributor to one of Prism's docs, give its published link: a file at `<PRISM_ROOT>/docs/prism/<path>` is published at https://github.com/NintexGlobal/rnd-prism/blob/main/docs/prism/<path>. Read the file from `<PRISM_ROOT>`, but never give the contributor a path inside the Prism plugin.

## Per-repo instructions

Read the /AGENTS.md files of all repos in the solution which require context or changes.

## Spec-driven development

**MANDATORY**: Before doing any work for an SDD phase (specify, plan, tasks, implement), you MUST first read that phase's linked instructions file below, in full, and apply it: it overrides the skill's own defaults (naming, frontmatter, required sections, etc.). Do this before following the skill's steps. 

| SDD Generation Phase | SDD Agent | Target Files | Instructions Link (read first, in full) |
| --------- | --------- | ------------ | ----------------- |
| Specification - Prism | prism-specify | `spec.md` | [overrides/prism-specify.instructions.md](/.prism/instructions/overrides/prism-specify.instructions.md) |
| Technical plan - Prism | prism-plan | `plan.md` | [overrides/prism-plan.instructions.md](/.prism/instructions/overrides/prism-plan.instructions.md) |
| Tasks - Prism | prism-tasks | `tasks.md` (index) + `tasks/T<NNN>-<slug>.md` | [overrides/prism-tasks.instructions.md](/.prism/instructions/overrides/prism-tasks.instructions.md) |
| Task work items | speckit-tasks | ADO work items | Use the `create-ado-work-item` skill combined with the configuration in [work-item-parameters.yml](/.prism/config/work-item-parameters.yml) |
| Implementation | speckit-implement | code files in code repos | [speckit-implement.instructions.md](<PRISM_ROOT>/.prism/instructions/speckit-implement.instructions.md) |
| (Specification - Spec Kit (deprecated)) | speckit-specify | `spec.md` | [speckit-specify.instructions.md](<PRISM_ROOT>/.prism/instructions/speckit-specify.instructions.md) |
| (Technical plan - Spec Kit (deprecated)) | speckit-plan | `plan.md` | [speckit-plan.instructions.md](<PRISM_ROOT>/.prism/instructions/speckit-plan.instructions.md) |
| (Tasks - Spec Kit (deprecated)) | speckit-tasks | `tasks.md` (all tasks inline) | [speckit-tasks.instructions.md](<PRISM_ROOT>/.prism/instructions/speckit-tasks.instructions.md) |
