---
name: cdd
description: >
  Follow prism and repository-specific development guidelines from AGENTS.md.
  Use for context-driven development (CDD) within a Prism solution.
argumentHint: >
  Describe the development task or feature you're working on (e.g., "implement user authentication",
  "refactor payment service", "add unit tests for OrderService").
---

# Skill: cdd (Nintex Context-Driven Development)

Use this skill when working on code changes in a repository that follows context-driven development
but is NOT using spec-driven development (SDD) workflows. The aim is to ensure that all code changes adhere to the architectural patterns, coding conventions, testing requirements, and documentation standards defined in the applicable `AGENTS.md` file (the `prism`-role repository's and/or the `code`-role repository's) and any referenced context files.

## Purpose

This skill ensures the agent:
1. Reads and follows all instructions in the solution's `/AGENTS.md` file and the repository's `/AGENTS.md` file
2. Applies repository-specific architectural patterns, coding conventions, and standards
3. Adheres to testing, documentation, and workflow requirements
4. Maintains consistency with existing codebase practices

## When to use

Invoke this skill when:
- Starting work on a new feature or bug fix
- Refactoring existing code
- Adding or updating tests
- Creating or updating documentation
- Making changes that affect architectural patterns
- The user explicitly asks for "context-driven development" or references AGENTS.md

## Required sources

1. **MUST READ**: `/AGENTS.md` in the root of **the repository that defines this solution** (the one holding the `prism` role) — not the AGENTS.md of any other repository visible in the VS Code workspace or any core solution or context repos defined in the solution.
2. **SHOULD READ**: Any context files referenced in that AGENTS.md that may be relevant to the task (e.g., `.context/` directories, constitution.md, other AGENTS.md of repos in the same Prism solution, Prism's instructions under `<PRISM_ROOT>/.prism/instructions/`, and the team's under `/.prism/instructions/overrides/`).

## Workflow

### 1. Load repository context

1. Identify the repository holding the `prism` role: the project root, or in a multi-root VS Code workspace the folder with `/.prism/config/repos.yml`. This will be termed the `solution` repo. Don't infer it from where this skill file is: it may run from a plugin outside the repository.
2. Read `/AGENTS.md` from the root of the **solution** repo — in a multi-root VS Code workspace this may differ from the repository where code changes are being made; always resolve the path relative to the solution repo's root
3. Identify and load any referenced context files (e.g., `constitution.md`, `.context/manifest.yml`, module-specific context) from that same repository, Prism's instructions under `<PRISM_ROOT>/.prism/instructions/` and the team's under `/.prism/instructions/overrides/`, as relevant to the task, any cross-referenced AGENTS.md files from other affected repositories and any other files mentioned in AGENTS.md that provide relevant context for the task at hand.
4. Note architectural patterns, coding standards, and conventions to follow.

### 2. Apply development guidelines

1. Follow the architectural patterns specified. This may include layering, modularization, design patterns, dependency rules, etc.
2. Apply coding conventions as specified (naming, formatting, language-specific guidelines)
3. Respect file organization and directory structure conventions
4. Apply localization/internationalization requirements, accessibility standards, and other relevant guidelines as specified. For user-facing changes use the `nintex-localisation` and `accessibility-wcag` skills from the Nintex skills marketplace (`engineering-standards` plugin), and any other installed Nintex skill that covers the platform or product being changed (for example the `nsp-*` skills for the Nintex Shared Platform)

### 3. Implement with quality standards

1. Write code following the identified patterns and conventions
2. Create appropriate tests as specified (unit, integration, E2E)
3. Use the correct testing frameworks and patterns
4. Follow mocking and assertion library preferences
5. Ensure tests follow repository-specific organization patterns

### 4. Document appropriately

1. Follow documentation standards specified in AGENTS.md
2. Update or create documentation as required by the repository guidelines
3. Use the correct documentation format (ADR, Backstage markdown, etc.)
4. Include diagrams in the specified format (PlantUML, Mermaid, etc.)
5. Follow markdown linting rules if specified

### 5. Validate and verify

1. Check for compile/lint errors using get_errors tool
2. Run appropriate tests to verify changes
3. Ensure changes align with architectural principles
4. Verify adherence to all guidelines from AGENTS.md

## Execution rules

- **MANDATORY**: Always read AGENTS.md in this solution FIRST before making any code changes.
- **MANDATORY**: Follow ALL instructions and conventions specified in that AGENTS.md.
- Read the AGENTS.md of any other repositories in the Prism solution if your task affects those repositories.
- If an AGENTS.md file references additional context or documentation, read those files. 
- If guidelines conflict, ask the user for clarification
- If AGENTS.md is missing or unclear, inform the user and ask for guidance
- Do not assume conventions - always check AGENTS.md for repository-specific requirements
- Update context files (e.g., `.context/`) if changes affect architectural patterns or conventions

## Example invocations

- "cdd: implement user authentication"
- "Follow context-driven development to refactor the payment service"
- "Apply CDD approach to add unit tests for OrderService"
- "Use cdd workflow for this bug fix"

## Notes

- This skill is complementary to spec-driven development (SDD) workflows but should not be used simultaneously
- For repositories that use both approaches, determine which workflow is appropriate for the specific task
- Always verify the repository has an AGENTS.md file before proceeding
- Keep AGENTS.md updated as development practices evolve
