# Nintex Prism Setup

- [Nintex Prism Setup](#nintex-prism-setup)
  - [S1. Multi-repo setup](#s1-multi-repo-setup)
    - [S1.1. Create new team prism repo](#s11-create-new-team-prism-repo)
    - [S1.2. Configure repo manifest](#s12-configure-repo-manifest)
    - [S1.3. Solution repos context](#s13-solution-repos-context)
    - [S1.4. Solution context](#s14-solution-context)
  - [S2. Single-repo setup](#s2-single-repo-setup)
  - [S3. Configure the prism repo for your team](#s3-configure-the-prism-repo-for-your-team)
  - [S4. Add Extensions and MCP (optional)](#s4-add-extensions-and-mcp-optional)
  - [S5. Commit](#s5-commit)
  - [Notes](#notes)

Nintex Prism is an enterprise design and development framework using [Spec Kit](https://github.com/github/spec-kit) as a SDD framework. It works with any git provider, any Agent Assistant (eg Claude Code or GitHub Copllot), any OS (eg PC or Mac) and any IDE of choice.

Prism reaches a project from the `@nintex/prism` CLI or from the Prism plugin for Claude Code: see [Choosing how to install Prism](routes.md). The setup below applies to both. With the Claude plugin, run the commands under the plugin's names, such as `/prism:generate-context-repo`.

Prism can be configured for a `multi-repo` or a `single-repo` topology (see the [glossary](../glossary.md) for the role/topology model):

**[Stand alone](#s1-multi-repo-setup)**: A team can create a stand-alone `{your-system}-prism` repository (the prism repo) which then governs development of a suite of the team's code repos.

- keeps the Prism framework separate from the team's code repos.
- features span multiple repositories.
- adds another team repository.

**[Single-repo](#s2-single-repo-setup)**: A team installs Prism directly into their existing repository, so one repository holds both the `prism` and `code` roles.

- adds Prism folders and framework into the existing repository.
- does not require additional repositories.

## S1. Multi-repo setup

In this topology the prism repo is separate from the team's code repos and is customised for the team. It is then configured with links to the team's existing code repos and will guide development within that set of repositories. eg:

``` text
space-invaders-prism                  (prism repo)  
├─ ntx-tables-api                     (code repo)  
├─ ntx-tables-orchestration-service   (code repo)  
├─ ntx-tables-client                  (code repo)  
└─ ntx-files                          (code repo)
```

The basic setup steps are:

1. Create a team-customised prism repo for your team (`{your-system}-prism`).
1. Configure that repository with links to the other team's code repos.
1. *(optional) Create a VS Code workspace including all of these repositories.*
1. Generate the context for each repository.

Everything in this setup only needs to be done by one team member to be pushed for the rest of the team.

### S1.1. Create new team prism repo

Create a new empty repo, e.g. `team-name-prism` (follow the [NDP repo creation process](https://ndp.platform.nintex.io/create/templates/default/new-github-repo)), clone it locally and install Prism into it, [with the Claude plugin or the Prism CLI](routes.md).

### S1.2. Configure repo manifest

A dedicated prism repo requires context of its target code repos. Fill in `/.prism/config/repos.yml`, which Prism's installer creates with blank entries, using `/.prism/config/repos.yml.sample` as an example and `/.prism/config/repos.schema.json` for the rules: list the prism repo under `prism`, the code repos under `coreSolutionRepos`, and optionally context repos under `contextRepos` (leave it as `[]` if there are none). Note that this does require each team member to have the same relative locations of local repos, so a policy of putting all local repos under the same root is highly recommended!

> Note that for convenience the space-invaders team also configure a VS Code workspace containing all of these repositories - the prism repo and the code repos. This is optional but makes everything more visible. The VS Code workspace file is saved and used as the top level entry point in VS Code for all team members. Then work can be done in the prism repo but the affected code repos and their git status are all visible in one place.

### S1.3. Solution repos context

Context setup is required for each code repo - these provide essential context for design and implementation in those repositories. These additions have no impact on existing code or functionality.

Run the agent/skill `/generate-context-repo` on **every code repo**. In a `multi-repo` topology that is each code repo (the dedicated prism repo is not a code repo, so it is not a target here). It will add a `/.context/` folder containing a progressive-disclosure technical documentation tree to assist coding agents. This defines the architecture, build instructions, dependencies, structure, etc of each module in a repo. The agent will also add or update an `/AGENTS.md` file in the code repo to ensure coding agents use and update this context when making changes to the repo - ensuring the context stays up to date. eg prompt

```text
/generate-context-repo Generate agent context for the {your-code-repo} repo.
```

Also run that skill against each context repo if possible unless they already have a progressive disclosure context. This will require opening pull requests against other teams' repos, but these are harmless addition of context.

### S1.4. Solution context

Once context has been created for each code repo, invoke the `generate-context-prism` skill to create global solution context in the prism repo. This will create a `/.context/` folder in the prism repo with a high-level outline of the solution. **This step applies only to the `multi-repo` topology** — in a `single-repo` topology there is only one repository and no cross-repo map to build, so `/generate-context-prism` is not needed. eg prompt with:

```text
/generate-context-prism Generate agent context for the {your-system}-prism repo.
```

## S2. Single-repo setup

Install Prism into your repository, [with the Claude plugin or the Prism CLI](routes.md). It will only have visibility of other repositories when they are explicitely referenced in a prompt.

Configure `/.prism/config/repos.yml` so the one repository is listed **both** under `prism` (the `prism` role) **and** as the sole element of `coreSolutionRepos` (the `code` role):

```yaml
prism:
  name: acme-app        # holds the prism role
  path: .
coreSolutionRepos:
  - name: acme-app      # SAME repository — also holds the code role
    path: .
contextRepos: []        # or your context repos, if any
```

Then run `/generate-context-repo` once against the one repository (it holds the `code` role). You do **not** run `/generate-context-prism` in a `single-repo` topology, because there is no separate set of repositories to map. See the [glossary](../glossary.md) for the role/topology model.

## S3. Configure the prism repo for your team

- **Agent conduct and SDD phase conventions**: (Optional) [/.prism/instructions/overrides/](/.prism/instructions/overrides/) holds files seeded once by the CLI and never overwritten, so they are safe to edit: `agent-conduct.instructions.md` for your team's rules on how agents work (tone, comments, commit and PR style), and `prism-specify.instructions.md`, `prism-plan.instructions.md` and `prism-tasks.instructions.md` for your conventions in each SDD phase. Each layers on top of the Prism-managed rules it names and wins where the two disagree. All four ship empty: leave them as-is if you have no local conventions.
- **Branch and feature-folder naming**: (Optional) [/.prism/config/branch-naming.yml](/.prism/config/branch-naming.yml) is seeded once by the CLI and never overwritten again, so it's safe to edit. It controls the branch/folder names `speckit-specify`, `speckit-plan`, `speckit-tasks`, and `speckit-implement` create — `sddBranchFormat` and `featureFolderFormat` for the SDD artifact branch/feature folder, `taskBranchFormat` for the per-task branch created in the target code repo while implementing — using `<type>` / `<phase>` / `<feature>` / `<task>` tokens (see the comments in that file). Blank out `sddBranchFormat` or `taskBranchFormat` if your team doesn't want branches created automatically for that scope.
- **ADO work item configuration**: (If you will be using Prism to generate ADO work items) In your team's prism repo update [/.prism/config/work-item-parameters.yml](/.prism/config/work-item-parameters.yml) with your team's Azure DevOps work item values. The `create-ado-work-item` skill reads this file when generating Azure DevOps work items.
- **Nintex skills**: Prism's installer (`prism init` and `prism update`, or the setup agent with the Claude plugin) registers the shared [Nintex skills marketplace](https://github.com/NintexGlobal/rnd-nintex-skills) in [/.claude/settings.json](/.claude/settings.json) and enables its `engineering-standards` plugin (the `accessibility-wcag` and `nintex-localisation` skills that Prism's CDD and SDD steps rely on). Add any other plugins your team needs to `enabledPlugins` in that file, for example `"nsp@nintex-skills": true` for services on the Nintex Shared Platform. Prism only adds missing entries, so your choices (including setting a plugin to `false`) are kept. See the [skills catalog](https://github.com/NintexGlobal/rnd-nintex-skills/blob/main/docs/skills-catalog.md).
- **Artefact templates**: (Optional) To use your own spec, plan or tasks template, put a complete template in `/.specify/templates/overrides/` under the same name, such as `plan-template.md`. Prism's SDD phases use the first template they find: your override, then Prism's own template, then Spec Kit's base template in `/.specify/templates/`. An override replaces the whole template, so start from a copy of Prism's. Prism adds only a README to this folder, and never overwrites your templates. Don't edit Spec Kit's base templates: Spec Kit refreshes replace them.

## S4. Add Extensions and MCP (optional)

These are useful for SDD and will be needed by all team members.

- **Swagger viewer** - view Swagger yaml files generated by SDD
- **Azure MCP Server** - required to generate Azure DevOps work items:
  - Install `Azure MCP Server` extension into VS code.
  - Install `azure-devops-mcp-server` using `npm install -g azure-devops-mcp-server`

## S5. Commit

Check it all in to make the Prism solution available to the team.  

The only action required for each team member is to ensure they have any desired extensions and MCP servers installed, and that the Nintex skills can be fetched. With the Claude plugin, Prism itself comes from the same marketplace:

- **Claude Code**: open the repo and accept the folder trust prompt; the Nintex skills marketplace and its enabled plugins load automatically. The marketplace is a private GitHub repository, so git must be able to read it without prompting: either an SSH key loaded in your agent, or HTTPS credentials via `gh auth login` and `gh auth setup-git` (set `CLAUDE_CODE_PLUGIN_PREFER_HTTPS=1` if you have no GitHub SSH key).
- **GitHub Copilot** doesn't read Claude Code marketplaces. Install the same skills with `npx @nintex/skills --skill accessibility-wcag nintex-localisation --agent github-copilot` (see the [skills repo README](https://github.com/NintexGlobal/rnd-nintex-skills#install)).

## Notes

Ensure all agentic instructions are added at the correct level. Most agentic instructions are team-specific and should be added in the `AGENTS.md` of each team code repo. If any changes are seen to be truly Nintex-wide contact the Prism team to make a global change.
