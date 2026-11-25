# Troubleshooting Prism's warnings

When Prism comes from the Prism plugin for Claude Code, it checks the project at the start of each session. Claude Code shows each warning as the session starts, and the agent repeats it at the top of its first reply, marked ⚠ **Prism warning:**, then carries on with what you asked. None of these warnings stops a session.

## Which Prism is running

At the start of each session, Prism tells the agent which copy is active; run `/prism:about` to see it. It's one of these:

- **"Active Prism release: 1.15.0, installed from the nintex-skills catalogue as prism@nintex-skills (user scope)."** A published release, installed by Claude Code. The release and the catalogue come from Claude Code's own record of what it installed, not from the plugin.
- **"Active Prism: … a local copy from …, not a release installed from a catalogue."** A copy Claude Code runs from a folder, such as a maintainer's build loaded with `--plugin-dir` or from a folder catalogue. The build number shows how recent it is.
- **"Where it came from is unknown."** Prism couldn't read Claude Code's record of installed plugins, or couldn't run its checks without PowerShell. Prism still works; only the report is missing.

## Prism is present twice

Only one copy of Prism can run at a time, and it may not be the one you expect. The warning names every copy it found and the settings that enable each one.

- **This project has its own copy of Prism, and the plugin is also enabled.** The project has a file-based install, so Prism's files are in the repository, but the plugin is enabled too, often in your user settings. Choose one:
  - To keep the project's copy, disable the plugin for this project: add `"prism@nintex-skills": false` (or the id the warning names) to `enabledPlugins` in `.claude/settings.local.json`, or uninstall the plugin with `/plugin`.
  - To use the plugin, ask Prism's setup agent to [migrate the project to the Claude plugin](routes.md#migrate-an-existing-prism-project-to-the-claude-plugin). While you're moving it, this warning is expected.
- **Prism is enabled twice.** Two plugins named `prism` are enabled, for example the published one and a copy from another catalogue. Keep `prism@nintex-skills`, and disable the other in `.claude/settings.local.json` or uninstall it with `/plugin`.
- **A copy loaded for this session only.** The warning says this for a copy started with `--plugin-dir`, which is how maintainers try an unreleased build. Start Claude Code without `--plugin-dir` to use only the installed copy.

## Files from a file-based install are still in the repository

The project uses the Prism plugin, but some of Prism's copied files are still in the repository, for example under `.claude/skills/` or `docs/prism/`. The warning names the first ten. The plugin provides them now, so they can be deleted. Ask Prism's setup agent for the full list, or to remove them. Your own files, such as overrides, configuration and SDD artefacts, are never on this list.

## Prism isn't active

The project is set up for the Prism plugin, but the plugin didn't load, so Prism's skills, constitution and conduct rules aren't in the session. Run `/plugin install prism@nintex-skills` in Claude Code, or accept Claude Code's prompt to install it after trusting the folder, then start a new session.

## The repository's Spec Kit is older than this release needs

With the Claude plugin, Spec Kit's files stay in the repository, and a new Prism release can need newer ones. The warning names the repository's Spec Kit version and the oldest version this release works with. Until the files are refreshed, the SDD phases may run inefficiently or unpredictably. Run Prism's setup agent, `/prism:setup`, to refresh them: it names any Spec Kit file you've edited before replacing it, and leaves the change for you to review and commit. See [When to run the setup agent](routes.md#when-to-run-the-setup-agent).

A repository that's only behind the Spec Kit the release ships, but not older than the oldest it works with, gets no warning.

## Prism couldn't find a valid Spec Kit install

The project has no `.specify/init-options.json`, or it records no Spec Kit version, so Prism can't tell which Spec Kit the project has. You'll see this in a new project before its setup: run Prism's setup agent, `/prism:setup`, to set the project up. In a project that's already set up, the setup agent can refresh Spec Kit's files; review its change before committing.

## This session runs an unreleased Prism build

The session runs a copy of Prism from a folder rather than a release installed from a catalogue: a maintainer's build, loaded with `--plugin-dir` or from a folder catalogue. It may differ from the release everyone else gets. The warning names the build's version, build number and folder. If you didn't mean to try a build, start Claude Code without `--plugin-dir`, or uninstall the folder catalogue's copy with `/plugin`.

## PowerShell isn't installed

Prism needs PowerShell 5.1 or later: Windows PowerShell (`powershell`), which comes with Windows, or PowerShell 7 (`pwsh`). Without it, Prism can't check the project's setup and its SDD phases won't run. Install PowerShell, then start a new session.

## PowerShell won't run

PowerShell is installed, but it crashes before it can run anything, so Prism can't check the project and its SDD phases may fail. To confirm, run the command the warning names, such as `pwsh -NoProfile -Command 'Write-Output ok'`: a working PowerShell prints `ok`, and a broken one prints "An error has occurred that was not properly handled".

The usual cause is a corrupt startup cache, which PowerShell rebuilds on its own. Delete the files named `StartupProfileData-*` in PowerShell's cache folder, then start a new session:

- macOS and Linux: `~/.cache/powershell/`
- Windows: `%LOCALAPPDATA%\Microsoft\PowerShell\`

If PowerShell still crashes, reinstall it.
