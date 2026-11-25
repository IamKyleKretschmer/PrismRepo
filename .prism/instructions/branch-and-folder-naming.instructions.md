# Branch and Folder Naming (Shared Reference)

This file holds the branch and feature-folder naming mechanics shared by the SDD phase skills (`speckit-specify`, `speckit-plan`, `speckit-tasks`, `speckit-implement`) and the `prism-plan` / `prism-tasks` skills. It is not applied on its own - each phase's own instructions file says when to read it and layers its own phase-specific guard on top.

**Scope**: everything below applies only to branches/folders created by the SDD phase skills that reference this file. It is not a general branch-naming requirement for manual or other agent-created branches.

## Configuration

Branch and feature-directory formats are team-configured in `.prism/config/branch-naming.yml`:

- `sddBranchFormat` and `featureFolderFormat` govern the SDD artifact branches/folder (`spec.md`/`plan.md`/`tasks.md`) — see that file's comments for the full `<type>` / `<phase>` / `<feature>` token reference.
- `taskBranchFormat` governs the per-task branch `/speckit-implement` creates in the target `code`-role repository while implementing an individual task — see that file's comments for the `<type>` / `<task>` token reference.

If `.prism/config/branch-naming.yml` is missing entirely, fall back to `sddBranchFormat: "<type>/<feature>"`, `featureFolderFormat: "<feature>"`, and `taskBranchFormat: "<type>/<task>"`.

Either `sddBranchFormat` or `taskBranchFormat` may be set to an empty string to disable automatic branch creation for that scope — in that case, skills leave whatever branch is already checked out alone rather than creating or switching branches. `featureFolderFormat` has no such opt-out: the feature folder is always created.

## Determining the default branch and current state

- **Default branch**: `git symbolic-ref refs/remotes/origin/HEAD` (strip the `origin/` prefix); if that isn't set, fall back to `git ls-remote --symref origin HEAD`; if that isn't set, ask the user.
- **Current branch**: `git branch --show-current`.
- **Clean working tree**: `git status --porcelain` reports nothing.

## Initial branch + feature folder creation (used by `/speckit-specify`)

**Only apply this convention when starting from the repository's default branch with a clean working tree.**
- If `sddBranchFormat` is blank, skip the branch part of this section entirely — do not set `GIT_BRANCH_NAME`, and leave whatever branch is currently checked out as-is. Still resolve and create the feature directory as described below.
- If the current branch matches the default branch AND the working tree is clean, proceed with token resolution below.
- If the current branch does not match the default branch, do NOT override `SPECIFY_FEATURE_DIRECTORY` or `GIT_BRANCH_NAME` — warn the user that `/speckit-specify` is running from a non-default branch and ask whether to switch to the default branch first, or continue with Spec Kit's own unmodified default naming.
- If the working tree isn't clean, do NOT create the branch or feature directory yet — warn the user and ask them to commit, stash, or discard the changes first, since starting a new feature branch on top of unrelated pending changes would carry them over unintentionally.

Resolve both formats from the same token values so the feature directory and git branch stay aligned:

- **`<feature>`**: the 2-4 word short name the skill already generates for the feature in its "Generate a concise short name" step (e.g. `user-dashboard`). Never derive it from a sequential number or timestamp.
- **`<type>`**: one of `feat`, `fix`, `chore`, `docs`, `refactor`, `perf`, `test`. Default to `feat` unless the feature description clearly indicates otherwise (e.g. `fix` for a bug-driven spec).
- **`<phase>`**: always `spec` for this step.
- **Feature directory**: substitute the tokens above into `featureFolderFormat`, prefix the result with `specs/`, and set `SPECIFY_FEATURE_DIRECTORY` to it before the skill's directory-resolution step, bypassing its default `<prefix>-<short-name>` auto-numbering. Feature directories always live under `specs/` regardless of `sddBranchFormat`. Example: with the default `featureFolderFormat: "<feature>"`, this resolves to `specs/user-dashboard`.
- **Git branch**: substitute the tokens above into `sddBranchFormat` and set `GIT_BRANCH_NAME` to the result before the `before_specify` hook runs, bypassing the git extension's default numeric prefix. Example: with the default `sddBranchFormat: "<type>/<feature>"`, this resolves to `feat/user-dashboard`.
- If the resolved feature directory or branch already exists, ask the user for a different `<feature>` value rather than silently falling back to numbered or suffixed naming.

## Handing a feature branch to the next phase (used by `/speckit-plan`, `/speckit-tasks`, `/speckit-implement`)

- If `sddBranchFormat` is blank or does not include a `<phase>` token, this phase does not switch branches — continue on whatever branch the prior phase left active.
- If `sddBranchFormat` does include a `<phase>` token, this phase owns its own phase branch, subject to the calling phase's own default-branch guard (see that file):
  1. Recover `<type>` and `<feature>` from the CURRENT branch name by matching it against `sddBranchFormat`, treating the `<phase>` position as a wildcard for any of the four known values (`spec`, `plan`, `tasks`, `impl`).
  2. Re-substitute the recovered `<type>` and `<feature>` into `sddBranchFormat` with `<phase>` set to this phase's own value, to compute the target branch name.
  3. If a branch with that name already exists, switch to it (`git checkout <branch>`); otherwise create it from the current branch (`git checkout -b <branch>`), so this phase's work builds on top of the prior phase's branch history.
  4. If the current branch name can't be parsed against `sddBranchFormat` (e.g. it predates this convention, or was named manually), leave it as-is and proceed on the current branch rather than guessing.
- The feature directory (`specs/<feature>`) is unaffected by phase — never move or duplicate it.

## Per-task branch (used by `/speckit-implement`)

This is separate from, and layered on top of, the phase branch above — it applies in the target `code`-role repository where a task's implementation work actually happens.

- If `taskBranchFormat` is blank, do not create a branch per task — implement each task on whatever branch is already active (the phase branch from the section above, if any, or the branch that was already checked out).
- If `taskBranchFormat` is set, before starting work on a task:
  1. Substitute `<type>` (the same `<type>` carried over from the feature branch) and `<task>` (the kebab-case slug of the task being implemented, e.g. `add-login-endpoint`, derived from its `tasks.md` title) into `taskBranchFormat` to compute the target branch name.
  2. If a branch with that name already exists, switch to it (`git checkout <branch>`); otherwise create it from the current branch (`git checkout -b <branch>`).
