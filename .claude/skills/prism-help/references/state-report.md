# Reporting feature state

Work through these steps to detect where the scoped feature sits in the Prism SDD journey and recommend the next
action. [states.md](states.md) is the companion reference: it defines every state, what it means, the recommended next
agent for a feature sitting in it, and — for states that cannot be verified mechanically — the question to put to the
user.

Two of the skill's operating constraints are easy to lose sight of part-way through this procedure, so restate them
here: refer to a state by its `summary` text and never by its number, and never name this file, `states.md` or
`detect-state.ps1` to the user.

## 1. Run the detection script

Run `detect-state.ps1` (a sibling of this file) with PowerShell and parse its JSON output:

```
pwsh -NoProfile -File "<this-skill-folder>/references/detect-state.ps1"
```

On a machine with only Windows PowerShell, run `powershell -NoProfile -File "<same path>"` instead. The script works
under both.

It returns:

| Field | Meaning |
| ----- | ------- |
| `featureDirectory` | Repo-relative path to the resolved feature directory, or `null` if none could be resolved. |
| `latestConfirmedState` | `{ id, summary }` — the furthest state reached by an unbroken run of confirmed evidence. Authoritative. |
| `latestUnconfirmedState` | `{ id, summary }`, or `null` — the furthest later state showing some evidence despite the confirmed run having broken. |
| `unconfirmedStates` | Array of `{ id, summary }` — every later state showing evidence. |
| `blockedByUnconfirmableState` | `{ id, summary }`, or `null` — a gate the script cannot verify that is capping the confirmed run. See step 2. |

Make the **first** call with no parameters, so the script's own resolution is what gets reported by default.

### Probing a specific directory

The script resolves the feature from `.specify/feature.json` first, falling back to matching the branch name against
`specs/`. That fallback covers the branch shapes `.prism/config/branch-naming.yml` produces by default, but a team
using a custom format may only resolve through the pin.

The script also accepts `-FeatureDirectory <path>`, which scans that directory directly and bypasses its own
`.specify/feature.json` / branch-name resolution:

```
pwsh -NoProfile -File "<this-skill-folder>/references/detect-state.ps1" -FeatureDirectory specs/some-other-feature
```

Re-run with this parameter to resolve a specific doubt, not to routinely double-check a clean result:

- **A suspected stale `.specify/feature.json` pin** (see step 6) — if the branch name suggests a different feature
  than the unparametrized call resolved, probe the directory under `specs/` matching the branch name.
- **Multiple candidate feature directories** — if git history shows more than one feature directory touched on this
  branch, probe each in turn to build up a state per feature rather than guessing from a single resolved directory.

### If the script cannot run

If PowerShell is unavailable or the user declines to run the script, fall back to judging the state by inspecting
files the same way the script would, using it as a guide. This loses the script's determinism, so use it only when
there is genuinely no result to work from — never in preference to a result the script returned. Tell the user the
report is a manual estimate when relying on this path.

## 2. Resolve an unverifiable gate

Some transitions depend on judgement the script cannot observe — a read-only agent that leaves no artefact behind, or
a semantic comparison between code and spec. When the confirmed run stops at one of these and no later state confirms
as a proxy for it, the script names it in `blockedByUnconfirmableState`.

If `blockedByUnconfirmableState` is `null`, skip this step.

If it is non-null, look that state up in states.md and put its **Gate** question to the user in the chat, phrased in
your own words. Then follow that state's own **Cleared** / **Not cleared** guidance. This differs per state and is
worth reading rather than assuming: clearing one gate advances the feature to that state, while clearing another
returns it to an earlier state instead.

Ask only about the gate the script reports. Gates further along the journey are not yet in question.

## 3. Report the confirmed state

Treat `latestConfirmedState` — or the effective state from step 2, if a gate was asked about and answered — as where
the feature actually is. Look it up in states.md and summarise its meaning in plain language.

## 4. Report the feature in scope

If `latestConfirmedState.id` is 2 or higher, report `featureDirectory` prominently near the top of the response, for
example bolded and ahead of the state summary. The user is the only one who can spot that the wrong feature is in
scope, and a mismatched feature directory usually means a stale `.specify/feature.json` pin — which makes the rest of
the report unreliable rather than merely imprecise.

## 5. Check for out-of-sequence work

If `latestUnconfirmedState` is `null`, the picture is clean. Say so plainly and skip the hedging.

If it is non-null, some evidence of work exists beyond the confirmed state. Report that clearly, then form a hedged
guess about what happened by comparing `unconfirmedStates` against the full run of ids between
`latestConfirmedState.id + 1` and `latestUnconfirmedState.id`:

- **Most of that range present**, with only one or two ids missing from an otherwise long dense run — more likely a
  **step was skipped**.
- **Sparse relative to the range**, with few entries and large gaps — more likely **work began out of sequence**,
  whether someone jumped the gun on a later stage or skipped ahead deliberately.

Phrase this as a guess ("it looks like…", "my best guess is…"), never as fact, and name the furthest state showing
evidence along with the states in between that appear to be missing.

## 6. Sanity-check code changes against the scoped feature

If any evidence points at implementation having started — the "Implementation in Progress" state or later appearing in
either `latestConfirmedState` or `unconfirmedStates` — do not take it at face value. Inspect what actually changed,
read-only:

```
git -C <repo root> log --oneline <default-branch>..HEAD
git -C <repo root> diff --stat <default-branch>...HEAD
```

Bound the output of both (for example by piping through `head`), since a long-running branch can produce far more
than is needed to judge whether the changes relate to the feature.

Compare the changed paths and commit messages against the scoped feature's own `spec.md` (title, problem statement,
functional requirements), and the branch name against the feature name. If they do not obviously relate — the changes
touch an unrelated subsystem, the commit messages describe different work, or the branch name has no apparent relation
to the feature — this looks like context-driven development against a stale `.specify/feature.json` pin or a corrupted
scope. Say so instead of confidently reporting the implementation states as accurate for this feature. If the branch
name points at a different, plausible feature directory under `specs/`, probe it per step 1 before reporting.

## 7. Always suggest a next step

Pick the branch matching what the previous steps found:

- **Suspected stale scope** (step 6) — lead with that concern rather than a confident next step. Suggest verifying or
  fixing `.specify/feature.json`, or confirming the ad hoc CDD was intentional, before acting on any state-based
  recommendation.
- **Clean** (`latestUnconfirmedState` is `null`, and steps 2 and 6 raised nothing) — suggest the action listed under
  `latestConfirmedState`'s own heading in states.md.
- **Guessing a skipped step** — suggest the action under `latestUnconfirmedState`'s heading, and separately note how
  to go back and complete each skipped state.
- **Guessing out-of-sequence work** — suggest the action under `latestConfirmedState`'s heading as in the clean case,
  but flag explicitly that it may not fit the work already done, and ask the user to verify before proceeding.
