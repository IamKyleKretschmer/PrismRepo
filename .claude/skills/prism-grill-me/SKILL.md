---
name: "prism-grill-me"
description: "Like speckit-clarify but with additional grilling"
argument-hint: "Optional areas to clarify in the spec"
compatibility: "Requires spec-kit project structure with .specify/ directory"
metadata:
  author: "rnd-prism"
  source: "derived-from-speckit-clarify"
user-invocable: true
disable-model-invocation: true
---

## User Input

```text
$ARGUMENTS
```

You **MUST** consider the user input before proceeding (if not empty).

## Intent

Interrogate the user about ambiguities in a spec, focusing on one spec aspect at a time, and resolving dependencies between decisions before moving downstream. The goal is to produce a clear, unambiguous spec with all high-impact ambiguities resolved.

## Mandatory Additions

1. Interview depth and dependency resolution
- Interview the user deeply about every aspect of the selected spec dimension until a shared understanding is reached.
- Walk each relevant branch of the design tree, resolving dependencies between decisions one-by-one.
- Do not jump to downstream questions before resolving the prerequisite decision.

2. Start with aspect selection
- Before any clarification question, ask which spec aspect to analyze.
- Present these options to the user with an easy way to select one:
  - `PM` (product manager)
  - `UX` (user experience)
  - `QA` (test plans)
  - `architecture`
  - `implementation` (code oriented)
  - `devops` (build, CI/CD, deployment)
  - `other`
- If the user chooses `other`, ask for a short custom aspect label, then continue.

3. Strict scope control
- Focus all follow-up questions only on the selected aspect.
- Reject or defer out-of-scope questions/threads and briefly state they are outside the selected aspect.

4. Ask questions. Map this as a **design tree**: every decision branches into the decisions that hang off it. Work the tree in **rounds**. The **frontier** is every decision whose prerequisites are already settled: the questions you can ask _now_ without guessing at answers you haven't heard yet. Ask the whole frontier in one round: number each question and give your recommended answer. Then wait for the user's answers before the next round.

Format a round like so:

```
❓ **Q1** - **<question title>**: <succinct question body, might be multiple paragraphs, including multiple choices>

➡️ <your recommended answer>
---
❓ **Q2** - **<question title>**: <succinct question body, might be multiple paragraphs, including multiple choices>

➡️ <your recommended answer>
```

Before asking the user, check whether the answer can be derived confidently from: the repository `.context/` folder; the current spec; or relevant codebase evidence. If derivable, state the inferred answer with evidence and ask for confirmation instead of asking an open question. Only ask the user when evidence is insufficient or ambiguous.

In each round of questions also offer to end and save the round.

Each round the user answers reshapes the tree: settled decisions push the frontier outward and unblock questions that depended on them. Recompute the frontier and ask the next round. A question whose answer depends on another question still open in this round belongs to a _later_ round, not this one.

Finding _facts_ is your job, never the user's. When a frontier question needs a fact from the environment (filesystem, tools, etc.), dispatch a sub-agent to find it; don't ask the user for anything you could look up yourself. Don't block on it: a running exploration is an unsettled prerequisite, so only the questions downstream of it wait for the sub-agent to report; ask the rest of the frontier now. The _decisions_ are the user's: put each to them and wait.

## Execution Notes

- Update the spec in place preserving the original structure and formatting. Only do this when: there are five unsaved questions, all questions are asked, or when the user requests it. If you don't have a USER_NAME use `git config user.name` to get the name.
- Preserve the same spec writeback behavior (incremental updates, `## Clarifications`, session bullets, section updates, validation checks).
- Save all clarifications for the selected aspect in their own companion file, in the **same folder as `spec.md`** (not a subfolder): `specs\{current_spec_folder}\{source_file}-{selected_aspect}.md`, with the aspect lowercased (e.g. `QA` → `spec-qa.md`, `PM` → `spec-pm.md`, a custom `other` label → hyphenated lowercase, e.g. `spec-security-review.md`). Do not overwrite any existing content in that file; append new clarifications to the end. Save each clarifiation with this format (include the double-spaces at the end of each line to ensure proper Markdown rendering):


--- 
**Question**: <question text>  
**Decision**: <user answer or inferred answer>  
**Rationale**: <concise reasoning for the decision>  
**Author**: `<[USER_NAME]>` <date>  

## Companion Spec Linking (Mandatory)

`spec-{aspect}.md` files (e.g. `spec-qa.md`) are standalone artifacts — do **not** copy or merge their content into `spec.md`'s body sections. Instead, `spec.md` must always carry a definitive, up-to-date link to every companion file that exists alongside it, so downstream agents that only open `spec.md` (e.g. `/speckit-plan`, `/speckit-tasks`) know the companion files exist and go read them directly. This linking section is owned entirely by this skill — the spec template does not define it, so create/maintain it exactly as specified below regardless of what template the spec was generated from.

Whenever a `spec-{aspect}.md` file is created or written to in this session:

1. Ensure `spec.md` has a `## Companion Specifications` section. If missing, create it directly beneath the title/metadata header block (immediately after the `**Input**:` line if present, otherwise right after the top-level `# Feature Specification` heading and any frontmatter — before the first `##` section), using exactly this structure:

   ```markdown
   ## Companion Specifications

   | Aspect | File | Summary |
   | ------ | ---- | ------- |
   ```

   Do not invent a different heading, table shape, or location.
2. Ensure that section's table has exactly one row for this aspect's file: `Aspect | File | Summary`, where `File` is a relative markdown link (e.g. `[spec-qa.md](spec-qa.md)`) and `Summary` is a one-line description of what the file covers. Add the row if missing; update the summary if it's stale; never duplicate a row for the same file.
3. Never remove a row for a companion file unless that file has actually been deleted.

Before reporting completion, verify: every `spec-{aspect}.md` file present in the feature folder — not just the one(s) touched this session — has a matching row in `spec.md`'s `## Companion Specifications` table. If any is missing (e.g. left over from an earlier session or created by someone else), add it before finishing.
