# Choosing how to install Prism

A project gets Prism in one of two ways: from the Claude plugin, or as files copied into the repository by the Prism CLI. Each project uses exactly one, chosen by the project lead.

- [Choose how to install](#choose-how-to-install), and [which route serves which agent](#which-route-serves-which-agent)
- [Set up a new project to use the Claude plugin](#set-up-a-new-project-to-use-the-claude-plugin)
- [Migrate an existing Prism project to the Claude plugin](#migrate-an-existing-prism-project-to-the-claude-plugin)
- [When to run the setup agent](#when-to-run-the-setup-agent)
- [Team rules and customisation with the Claude plugin](#team-rules-and-customisation-with-the-claude-plugin)
- [What contributors see](#what-contributors-see)
- [Appendix: reference for admins](#appendix-reference-for-admins)

## Choose how to install

| | Claude plugin | Prism CLI (file-based) |
| - | ------------- | ---------------------- |
| **How Prism arrives** | The Prism plugin for Claude Code, `prism@nintex-skills` | The `@nintex/prism` CLI copies Prism's files into the repository |
| **Agents** | Claude Code only | Claude Code and GitHub Copilot |
| **Updates** | Silent and automatic: every contributor always runs the latest release | Someone runs `npx @nintex/prism update` and commits the result |
| **Version pinning** | Not possible | The release in the repository is the one everyone runs |
| **What's in the repository** | Spec Kit's files, your team's files, a short [bootstrap](#the-bootstrap) and the plugin enablement | All of Prism's files, as well as Spec Kit's and your team's |
| **Command names** | `/prism:specify`, `/prism:plan` and so on, which can't collide with your own skills ([full list](../sdd.md)) | `/prism-specify`, `/prism-plan` and so on |
| **Changing Prism-owned files** | Not possible: change Prism's behaviour through the [customisation files](#team-rules-and-customisation-with-the-claude-plugin) | **Not supported.** Edits may not survive an update. Prism tries to warn before replacing an edited file, but can't guarantee it. Use the [customisation files](#team-rules-and-customisation-with-the-claude-plugin) instead |

Both give the same capabilities and the same artefact formats, so SDD work started with one continues with the other. Both need PowerShell 5.1 or later: Windows PowerShell (`powershell`), which comes with Windows, or PowerShell 7 (`pwsh`).

- **Choose the Claude plugin**: when everyone on the project uses Claude Code and you want Prism to stay current without anyone managing upgrades.
- **Choose the Prism CLI**: when anyone on the project uses GitHub Copilot, or the project must stay on a fixed Prism release. Install it with the [Prism CLI](prism-cli.md). The file-based install stays supported.

### Which route serves which agent

| Agent | Claude plugin | Prism CLI (file-based) |
| ----- | ------------- | ---------------------- |
| **Claude Code** | Yes: all of Prism, as `/prism:` commands, kept current automatically | Yes: all of Prism, as `/prism-` commands, at the release in the repository |
| **GitHub Copilot** | No | Yes: all of Prism, through Prism's Copilot agents and prompts in `.github/` |

- **Contributors on different agents**: a project uses one route, and only the file-based route serves Copilot, so a project where anyone uses Copilot uses the Prism CLI. Its Claude Code users get Prism from the repository too. They shouldn't also enable the Claude plugin, or Prism warns that it's [present twice](troubleshooting.md#prism-is-present-twice).
- **A fixed Prism release**: only the file-based route can hold one, so a project that must stay on a release uses the Prism CLI.

Moving a project from the Claude plugin back to a file-based install isn't covered by this guide. Ask the Prism team before trying it.

## Set up a new project to use the Claude plugin

One person does this once, and commits the result for the team.

### Before you start

The person doing the setup needs:

- **PowerShell 5.1 or later.** Every contributor needs it too.
- **Node.js, with `npx`.**
- **Access to Nintex's npm packages**, set up as in the [Prism CLI prerequisite](prism-cli.md#prerequisite). The setup agent fetches Prism's installer from there.
- **Read access to the Nintex skills marketplace**, a private GitHub repository. Every contributor needs it too.

To check each one, see [Checking the prerequisites](#checking-the-prerequisites).

### Steps

1. **Install the plugin for yourself.** In Claude Code, from the project's root folder:

   ```text
   /plugin install prism@nintex-skills
   ```

   When it asks where to install, choose this repository only (local scope). Installing for your user makes Prism warn in your other projects that still have a file-based install. From a terminal, the same is `claude plugin install prism@nintex-skills --scope local`.
2. **Start a new Claude Code session** so the plugin loads.
3. **Run the setup agent**: `/prism:setup`. It sees that Prism isn't installed yet, and goes straight ahead: it runs Prism's installer at the plugin's own release, which writes Spec Kit's files, the bootstrap, your team's starting configuration, and the setting that enables the plugin for everyone who opens the project.
4. **Do the team setup with the agent.** It carries straight on, one step at a time: choosing a single-repo or multi-repo layout, filling in `.prism/config/repos.yml`, and generating context. If the repository has no code yet, generate context once it has some. It also mentions optional settings, and that it can create your own spec, plan or tasks template from Prism's, now or later. If you stop part-way, run `/prism:setup` again to pick it up. The [team setup guide](setup.md) has the details.
5. **Review the changes**, but don't commit yet. The setup agent never commits. For what to expect, see [What a new setup adds](#what-a-new-setup-adds).
6. **We recommend starting a new session** before you commit: it loads the instructions setup wrote, and Prism checks the finished setup.
7. **Commit everything as one commit**: the setup and the team setup together. Prism needs its configuration to work properly, so the team shouldn't get the plugin without it.

## Migrate an existing Prism project to the Claude plugin

Moving replaces Prism's copied files with the plugin. Your SDD artefacts, configuration, overrides and personas stay as they are.

1. **Install the plugin for yourself**, as in [setting up a new project](#steps), after checking you have [what a new setup needs](#before-you-start): all of it applies to a move too. Then start a new session. Until the move is done, Prism warns that it's present twice: that's expected, so carry on with step 2.
2. **Run the setup agent**: `/prism:setup`. It finds the file-based install. Before it changes anything, it tells you which of Prism's copied files have been edited by hand, and which of your team's rules [point at Prism's files](#rules-that-point-at-prisms-files). Then it waits for you.
3. **Save the hand edits you want to keep**, before the go-ahead: the move replaces or removes those files, and their edits are lost. Ask the agent to show you an edit, or to copy it into the right [customisation file](#where-your-rules-go). For example, an edit to Prism's conduct rules belongs in `.prism/instructions/overrides/agent-conduct.instructions.md`.
4. **Give the go-ahead for the move**, by telling the agent to go ahead. It removes Prism's copied files, writes the [bootstrap](#the-bootstrap), and enables the plugin for the project. Your team's files, including the rules it named, stay as they are.
5. **Fix the rules it named.** After the move, the agent asks about each one separately: agreeing to the move doesn't change them. Say yes to the changes you want, or edit the rules yourself.
6. **Review and commit the changes**, as one commit. Expect a long list of deleted files: see [What a migration changes](#what-a-migration-changes).
7. **Start a new session** and carry on as before: your team setup carries over, so you can start a feature with `/prism:specify`. To check the move worked: `.prism/prism.manifest.json` now says `"route": "plugin"`, and in the new session the "present twice" warning is gone.

At the end of a move, the setup agent also checks for leftover files from the file-based install. If Prism warns about any later, ask the setup agent to list or remove them.

## When to run the setup agent

Run `/prism:setup` for:

- **First-time setup** of a project to use the Claude plugin.
- **A move** from a file-based install.
- **A Spec Kit refresh**, when Prism warns that the repository's Spec Kit files are older than the active release needs. The setup agent names both versions and any Spec Kit file you've edited, then refreshes the files. The refresh changes only Spec Kit's files and the Spec Kit version in its settings, as one change to review and commit.
- **Template overrides**, when your team wants its own spec, plan or tasks template. The setup agent lists the templates you can override and which already have an override, creates an override from Prism's current template for you to edit, or updates an existing override with the changes Prism has made since it was copied, keeping your team's edits. See [where your rules go](#where-your-rules-go).

You never need it for routine updates: new Prism releases reach every contributor on their own, with no change to the repository.

The setup agent can also list leftover files from a file-based install, or remove them.

## Team rules and customisation with the Claude plugin

### Where your rules go

These files are your team's however Prism is installed. Prism never changes them, and the plugin reads them from the repository:

| To change | Put it in |
| --------- | --------- |
| Instructions for agents in this project | the root `AGENTS.md` |
| How agents work: tone, comments, commit and PR style | `.prism/instructions/overrides/agent-conduct.instructions.md` |
| A phase's conventions | `.prism/instructions/overrides/prism-specify.instructions.md`, `prism-plan.instructions.md` or `prism-tasks.instructions.md` |
| A spec, plan or tasks template | a complete template in `.specify/templates/overrides/`, under the same name as Prism's. Ask the setup agent to create one from Prism's current template |
| Repositories, branch naming, work items | `.prism/config/` |

See [Configure the prism repo for your team](setup.md#s3-configure-the-prism-repo-for-your-team) for what each file does.

### Rules that point at Prism's files

With the Claude plugin, Prism's skills, instructions, templates, scripts and docs aren't in the repository, so a team rule that links to one, such as `.claude/skills/prism-plan/SKILL.md` or `docs/prism/sdd.md`, points at nothing. Refer to the skill by name instead, or link to the published docs. For example, "Before planning, read `.claude/skills/prism-plan/SKILL.md`" becomes "Plan features with `/prism:plan`". The setup agent names each such rule when moving a project, and can change them with your agreement. The line in `AGENTS.md` that loads `.prism/instructions/prism-agents.instructions.md` isn't one of these: that file is the [bootstrap](#the-bootstrap), and it stays.

## What contributors see

Once the setup is committed, a contributor opening the project in Claude Code is prompted to trust the folder and install the plugins it enables: Prism, and Nintex's `engineering-standards`. After that, Prism loads in every session and updates itself.

At the start of a session, Prism checks the project and the agent passes on anything wrong, once:

- **Prism isn't active**: the plugin isn't installed. Run `/plugin install prism@nintex-skills`, then start a new session.
- **Prism is present twice**, or **files from a file-based install are still in the repository**.
- **The repository's Spec Kit is older than the release needs**: [run the setup agent](#when-to-run-the-setup-agent) to refresh it.
- **PowerShell isn't installed**.

[Troubleshooting Prism's warnings](troubleshooting.md) explains each one.

## Appendix: reference for admins

### Checking the prerequisites

- **PowerShell**: `pwsh -NoProfile -Command 'Write-Output ok'` prints `ok`, or on a machine with only Windows PowerShell, `powershell -NoProfile -Command 'Write-Output ok'`. Don't rely on `pwsh --version`, which works even when PowerShell can't run anything.
- **Node.js**: `npx --version` prints a version number.
- **Nintex's npm packages**: `npm view @nintex/prism version` prints a version number.
- **The Nintex skills marketplace**, `NintexGlobal/rnd-nintex-skills`: git must be able to read it without prompting, in either of two ways. If one check passes, you're set: skip the other.
  - **SSH**: an SSH key for GitHub, loaded in your SSH agent. `git ls-remote git@github.com:NintexGlobal/rnd-nintex-skills.git HEAD` prints a line without asking for anything.
  - **HTTPS**: credentials from `gh auth login`, choosing HTTPS, then `gh auth setup-git`. `git ls-remote https://github.com/NintexGlobal/rnd-nintex-skills.git HEAD` prints a line without asking for anything. If you have no GitHub SSH key, also tell Claude Code to use HTTPS for plugins: add `export CLAUDE_CODE_PLUGIN_PREFER_HTTPS=1` to your shell profile, such as `~/.zshrc`, so it's set before Claude Code starts.

### What a new setup adds

About 65 new files:

- Spec Kit's files, under `.specify/`.
- Your team's starting files, yours to edit: `.prism/config/`, `.prism/instructions/overrides/`, `.prism/templates/overrides/`, `.specify/templates/overrides/` and `.specify/memory/personas.md`. Most are empty or examples until you fill them in.
- The [bootstrap](#the-bootstrap): `AGENTS.md`, `CLAUDE.md` and `.prism/instructions/prism-agents.instructions.md`.
- `.claude/settings.json`, which enables the Prism plugin and Nintex's `engineering-standards` plugin, from the Nintex skills marketplace.
- `.prism/prism.manifest.json`, Prism's record of what it installed.

### What a migration changes

- **Deleted**, often more than 100 files: Prism's skills, instructions, templates, scripts and docs, and `README.prism.md`. Spec Kit's other files stay, apart from its Copilot files, its records of which agents it's installed for, and its constitution-authoring files, which Prism doesn't use with the Claude plugin.
- **Changed**: `.claude/settings.json` (the plugin enabled), `.prism/prism.manifest.json` (Prism's record) and `.prism/instructions/prism-agents.instructions.md` (now the [bootstrap](#the-bootstrap)). `AGENTS.md` and `CLAUDE.md` change only if they didn't already load Prism's instructions.
- **Anything you agreed to**: hand edits copied into your customisation files, and fixed team rules.

### The bootstrap

The bootstrap is the little of Prism that stays in the repository, so agents know where Prism comes from: `.prism/instructions/prism-agents.instructions.md`, Prism's lines in `AGENTS.md` that load it, and a `CLAUDE.md` that loads `AGENTS.md`. It says which plugin the project uses, tells a contributor how to install it when it isn't active, and lists where your team's rules live. Prism writes `prism-agents.instructions.md` and may replace it, so don't edit it. The rest of `AGENTS.md` is yours: keep Prism's lines in it, and add your team's instructions around them.
