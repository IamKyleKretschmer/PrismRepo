# Installing @nintex/prism CLI

**Prism** is Nintex's wrapper over **GitHub Spec Kit**, managed through the `@nintex/prism` CLI. Use the following commands to install and maintain Prism in your workspace.

Prism can also come from the Prism plugin for Claude Code instead. See [Choosing how to install Prism](routes.md) to decide which suits your project.

## Prerequisite

Your .npmrc (generally C:\users\userName\) must be configured for the @nintex scope. It should contain:

``` text
@nintex:registry=https://pkgs.dev.azure.com/nintex/_packaging/Nintex/npm/registry/
```

This can be installed using:

``` bash
npm config set @nintex:registry https://pkgs.dev.azure.com/nintex/_packaging/Nintex/npm/registry/
```

and verified with:

``` bash
npm config get @nintex:registry
```

to check for the response `https://pkgs.dev.azure.com/nintex/_packaging/Nintex/npm/registry/`

## Check latest available version

View the most recent version of `@nintex/prism` available on npm:

```bash
npm view @nintex/prism version
```

## Check current installed version

Display the currently installed Prism version and flag any managed files that may have drifted from their expected state:

```bash
npx @nintex/prism status
```

## Initial installation

Initialize Prism in your workspace for the first time:

```bash
npx @nintex/prism init
```

This sets up all necessary Prism files and structures in your repository.

## Update

Update your Prism installation to the latest version:

```bash
npx @nintex/prism update
```

This command is a no-op if you are already on the latest version.

**Editing Prism-owned files isn't supported.** An update can replace or remove any of them, and your changes may not survive. Before it overwrites or removes a Prism file you've edited, `update` names it, marked overwritten or removed, then carries on. To see them before you update, run `npx @nintex/prism status`, which lists them as modified. Prism tells an edit apart by comparing each file with what it last wrote, so it can't guarantee to catch every change. Put your team's changes in the files Prism leaves to you, listed in [Configure the prism repo for your team](setup.md#s3-configure-the-prism-repo-for-your-team).

### Force reapplication

To reapply Prism configuration even when already at the current version (for example, to restore a managed file that was accidentally changed), use the `--force` flag:

```bash
npx @nintex/prism init --force
npx @nintex/prism update --force
```

## Projects using the Prism plugin

When Prism comes from the Prism plugin for Claude Code, the repository holds only Spec Kit's files, your team's files, a short bootstrap and the plugin enablement. Everything else Prism manages comes from the plugin. Prism's setup agent normally runs these commands for you: see [setting up](routes.md#set-up-a-new-project-to-use-the-claude-plugin) and [moving a project](routes.md#migrate-an-existing-prism-project-to-the-claude-plugin).

```bash
npx @nintex/prism init --plugin
npx @nintex/prism update --plugin
```

- **`init --plugin`**: Sets up a project for the plugin, or moves a file-based project to it. Moving removes Prism's copied files, naming any you've edited first; your overrides, configuration and SDD artefacts are kept.
- **`update --plugin`**: Refreshes Spec Kit's files to the copy this release ships. It names any Spec Kit file you've edited before replacing it. If the repository's Spec Kit is newer than this release's, it changes nothing until you rerun it with `--confirm-downgrade`.
- **Spec Kit's version fields**: Whenever either route writes Spec Kit's files, it also updates the Spec Kit version in `.specify/init-options.json` and `.specify/integration.json`, and changes nothing else in them.
- **One route per project**: Plain `update` refuses to run on a project set up for the plugin. `status` shows which route a project uses.
- **Errors**: Outside a repository's root folder, or without write access, `--plugin` exits with the reason and writes nothing.
- **Changes are left uncommitted**: Review them and commit them yourself.
- **Warnings in a session**: See [Troubleshooting Prism's warnings](troubleshooting.md).

### Using Prism

[Prism overview](../overview.md)
