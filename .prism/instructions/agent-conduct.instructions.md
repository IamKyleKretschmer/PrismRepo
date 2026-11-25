# Agent Conduct

<!-- Always loaded from [prism-agents.instructions.md](<PRISM_ROOT>/.prism/instructions/prism-agents.instructions.md). Applies to every agent in this solution, in every SDD
phase and outside SDD entirely.

Prism-owned: `prism update` overwrites this file, so do not edit it. Team and personal rules belong
in [overrides/agent-conduct.instructions.md](/.prism/instructions/overrides/agent-conduct.instructions.md), which is
seeded once and never overwritten. That file layers on top of this one, wins where the two
disagree. -->

Scope: these rules govern what the Prism process produces from now on. They are not a licence to
rewrite existing text you were not otherwise changing.

## Hard rules

These stand regardless of what the overrides file says.

- Never attribute work to Claude or AI: no Co-Authored-By: Claude trailer, no “Generated with Claude Code” footer, no mention in commit messages, PR descriptions, work items, or issue bodies.
- Never cite a ticket, work item, bug, or PR number in a code comment, commit message, or PR description. State the reason itself, not where it’s tracked.

## Talking to the user

Guidelines, not rules. Loosen them when the user asks for depth or the answer genuinely needs it.

- Lead with the answer. The first line answers the question.
- Then stop. Add a line where the user would otherwise have to ask for it.
- Facts over narrative: `file:line`, numbers, names, the exact command or error. One sentence of explanation unless asked why.
- Keep it short. Length is earned by a "why", a design question, a tradeoff, or a request for the
  long version, never by explaining what you just did.
- Skip the preamble, the restated request, and the summary of output already on screen.

This does not apply to SDD artefacts. `spec.md`, `plan.md`, task files and checklists are written
to their own phase rules, at the length those rules set.

## Writing artefacts for humans (comments, PRs, work items, Slack, docs)

- Comment only where the code can’t speak for itself: the why, the gotcha, the non-obvious constraint. Shortest wording that works, plain English. A few lines if the point needs them, never a paragraph.
- If the comment restates the line below it, delete it. Ignore the file’s existing comment density. This rule outranks matching house style.
- Before you call any code change done, re-read every comment you wrote and cut. Default is no comment. Two lines is the ceiling for an inline one. Delete anything that restates the code, repeats what a doc comment - elsewhere already says, or argues the case for a decision instead of stating it.
- XML doc comments: one summary sentence, two if the constraint needs it. No multi-paragraph <remarks>. Document a param only where the name doesn’t carry it. Never write a <summary> that just expands the method name.
- Test comments: the section markers a repo requires stay bare. No prose tail explaining why the assertion matters or what would break; the test name carries that.
- Don’t write a comment to preserve reasoning for a future reader or reviewer. If a decision needs defending, that belongs in the PR description or the spec, not the code.
- Anything with a human audience: say the whole thing, then cut the padding. As long as it needs to be, not one line longer. No restating the title, no filler headings, no sales tone.

## Git

- Don't run `git commit` or `git push` or stage changes without the user's (or override file's) approval.
- If a skill says to commit, stop and ask the user for approval.
