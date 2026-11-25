---
applyTo: '**/specs/**/spec.md'
---

# Specification Conventions

Apply these rules when creating or updating spec.md files.

## Dependencies on other teams

- The spec template scaffolds a `## Dependencies` section as a table — fill that table in rather than replacing it with another format:

```
| Team | What is needed | Needed by |
| ---- | -------------- | --------- |
```
- One row per dependency on another team: the team name, a brief description of what is needed from them (e.g., "Review and approve the proposed architecture", "Provide input on API design", "Coordinate on release timelines"), and the date or milestone it is needed by.
- If there are no dependencies on other teams, state "No dependencies on other teams at this time" rather than deleting the section.

## YAML Frontmatter — Azure DevOps Parent Feature ID

The spec template includes a YAML frontmatter block with an `azure-devops-parent-feature-id` property. This ID links the spec to its parent Feature work item in Azure DevOps and is used by downstream agents (e.g., task generation, work item creation) to create and link Product Backlog Items.

- Ask the user: "What is the Azure DevOps parent Feature ID for this spec? (Enter the numeric work item ID, or 'skip' to fill in later)"
- If the user provides a numeric ID, replace `[PARENT_FEATURE_ID]` in the frontmatter with that number.
- If the user says "skip" or provides no ID, leave the placeholder as-is and add a `[NEEDS CLARIFICATION: Azure DevOps parent Feature ID required for work item linking]` note in the spec's Open Questions section.

## Feature Folder and Branch Naming

Read [branch-and-folder-naming.instructions.md](<PRISM_ROOT>/.prism/instructions/branch-and-folder-naming.instructions.md) in full and apply its "Initial branch + feature folder creation" section — that is the step `/speckit-specify` performs when creating a new feature.

## Companion Spec Files (prism-grill-me)

- `prism-grill-me` sessions produce a standalone, per-aspect companion file in the **same folder** as `spec.md`: `{source_file}-{aspect}.md` (e.g. `spec-qa.md` for QA/test-plan clarifications, `spec-pm.md` for product-manager clarifications). These files are never merged into `spec.md`'s body — they stay separate.
- `spec.md` MUST always carry a `## Companion Specifications` section with one table row per companion file that exists alongside it (`Aspect | File | Summary`), linking to it with a relative markdown link. Downstream commands (`/speckit-plan`, task generation) read `spec.md` by default and rely on this table to discover that companion files exist — a companion file with no row is effectively invisible to them. The spec template scaffolds this section, so it is normally already present; add it if you are working from a spec that predates it, and see the `prism-grill-me` skill's `Companion Spec Linking` execution notes for the exact structure if it needs repairing manually.
- When creating, reviewing, or updating a spec, check the feature folder for any `spec-*.md` companion files. If one exists but has no matching row in `## Companion Specifications`, treat this as a spec defect and add the row before considering the spec ready for planning.
- Do not remove a companion file's row unless the file itself has been deleted.
