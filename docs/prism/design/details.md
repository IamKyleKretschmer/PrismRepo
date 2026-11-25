# Prism detailed features

A detailed list of Prism structure, design and functionality. *Requires tidying!*

## Detailed features by category

## Openness

- **IDE agnostic**: Designed to not rely on any specific IDE - achieved by defining workspace with [repos.yml](../../../.prism/config/repos.yml) rather than a VSCode workspace. `♾️open`
- **Coding assistant agnostic***: It at least works with both Claude Code and Copilot - not tested with other assistants! Achieved by using abstracted [AGENTS.md](../../../AGENTS.md) and skills folder. `♾️open`
- **OS agnostic**: Works with Windows and Mac. Achieved by pre-installing (haven't tried Linux but would probably work) `♾️open`
  
### Agent guidance

### Context driven development (CDD)

- The solution configuration binds the prism, code and context repos so CDD always operates with full repo context.
- `/cdd` agent for ad-hoc agentic coding without full SDD ceremony.
- CDD enforces the same constitution, AGENTS.md rules and repo context as SDD - vibe-coding to enterprise standard.
- Per-repo `AGENTS.md` ensures that direct agentic edits in code repos still respect Prism solution rules even when working outside the SDD flow.

#### Context

- **Solution**: The Prism repo plus the repositories declared in `/.prism/config/repos.yml`, form the governed repo set (both development and context) for a team or feature area.
- **Repo context**: Progressive-disclosure guidance stored in each code repository's `/.context/` directory to help agents follow local architecture, conventions and build/test practices without overwhelming agent context window.
- **Context generation**: Agents `generate-context-repo` and `generate-context-prism` build the repo context. `💲cost`, `✨quality`
- **Context maintenance**: Prism agents/skills are told to modify repo context in sync with code changes. Implemented in [speckit-implement.instructions.md](../../../.prism/instructions/speckit-implement.instructions.md) `✨quality`

#### Cost

- TBD! (constitution.md?) **Brevity**: Tell agents to be succinct in their output. Output tokens are expensive. `💲cost`
  - CRITICAL: Be exceptionally succinct. Never use conversational filler, pleasantries, or meta-commentary (e.g., "Sure, I can help with that"). `💲cost`
  - Do not output full files or large blocks of code unless explicitly asked. Only output the exact lines or diffs that changed.
  - Avoid explaining *how* your code works or summarizing your actions unless a specific explanation is requested.
  - Prioritize token efficiency for both thought process and final response output.

#### Nintex-specific

- **Rosetta for translations**: The `nintex-localisation` skill (from the Nintex skills marketplace, `engineering-standards` plugin) encourages using Rosetta for localisation translations.

#### Testing

- TBD! **Acceptance criteria**: Prism SDD encourages acceptance criteria to be added and numbered in a specification artefact.
- TBD! **Track test plans**: Prism SDD process creates detailed test plans linked to the spec acceptance criteria. This then propagates down to linked tasks and generated tests.
- TBD! **Deferred tests**: Agent instructions to remind user to create tests if deferred.

### Spec driven development (SDD)

- **Cross-functional development**: The Prism SDD process places a strong emphasis on involving all product roles at both design and review stages of a feature. eg PM, UX, AO, QA, DevOps, Devs, (also add TC???) `👥collab`, `🌐 completeness`
- **Phased development**: Inherited from Spec Kit, the Prism SDD process places a strong emphasis on very thoroughly reviewing each stage of development (spec, plan, tasks) before continuing.
- **Grilling**: The concept behind [grill me](https://github.com/mattpocock/skills/blob/main/skills/productivity/grill-me/SKILL.md) has been incorporated into the prism agent [prism-grill-me](../../../.claude/skills/prism-grill-me/SKILL.md). This provides deep investigation into different aspects of a spec. `🌐 completeness`
- TBD! **Checklists**: Post-stage checklists are performed to ensure artefacts are generated correctly.
- **Terminology**: The custom [spec-template.md](../../../.specify/templates/spec-template.md) ensures a terminology section in spec to assist understanding and code cleanliness.
- **Phased development**: minimises rework waste `💲cost`
- **Spec-informed**: preferring a process that works forward-only through the phases rather than reverse-propagating lower-level changes back to the spec saves time and tokens. The generated code, docs and context are the definitive documentation. `💲cost`
- TBD! **Defer test generation**: As nice as test driven development (TDD) would be, it is a lot more efficient to finalise the code then generate tests to suit. `💲cost`

#### Stages

- Spec Kit templates (`spec-template.md`, `plan-template.md`, `tasks-template.md`, `checklist-template.md`) standardise artefact shape.
- Solution-level template overrides under `/.specify/templates/overrides/` allow team-specific tailoring without forking Spec Kit.

#### Tracking

- **Work item creation**: The `create-ado-work-item` skill combined with the [work-item-parameters.yml](../../../.prism/config/work-item-parameters.yml) allows automatic generation of ADO work items with team-specific parameters.
- TBD! **Work item tracking**: Ensure created work items are tracked in task listing.

#### Defined process

- **Work items**: Define [create-ado-work-item](../../../.claude/skills/create-ado-work-item/) skill and specify its use in [AGENTS.md](../../../AGENTS.md). (TBD! Include templates for different work item types - eg bugs, etc)

#### Recommended process

These are recommended practice, not defined by the Prism framework:

- **SDD flow**: Prefer spec-informed (updates to code can diverge from initial spec) over tight spec <-> code synchronisation. The reasoning is that the spec is only used to generate the code, docs, tests and context which are the canonical source of truth. `💲cost`, `⏩speed`
- **Feature scope**: The SDD process works best at a feature level, not epic or work item.
- Shifting tech review left: main review happens at end of plan phase, before implementation, to minimise rework.

#### Alterations to Spec Kit

- Avoid generating all feature entities and interfaces as the Phase 0 task of a new feature (default Spec Kit behavoiur) [link](../../../.prism/instructions/speckit-tasks.instructions.md)
- TBD! (do in constitution?) Default behavoiur of speckit.specify is to include measurable metrics as acceptance criteria - eg "80% users to adopt feature within 1 month". These have been removed.

## Decision Record

Q: **Keep work items in sync**: Do we get the agents to propagate any spec changes to the ADO feature. The feature will then sync with AHA through its own mechanism. Ask whether this is important or not.  
A: Nope - one-way process now. ADO work items are primarily for work item tracking, not the canonical definition of the work.

### Governance

- All governance instructions and checklists are designed with progressive disclosure principles, ensuring a minimum use of the agent context window.
- `Constitution` in `/.specify/memory/constitution.md` encodes non-negotiable Nintex software-design principles applied to all phases.
- Agent operating procedures in the root `AGENTS.md` reference imperative rules for tooling and processes. File-scoped `.instructions.md` files with `applyTo` patterns target rules to the right files for GitHub Copilot (SDD artefacts, code, tests, etc.).
- Per-repo AGENTS.md files cascade rules into each code repo (architectural pattern, coding conventions, etc.).

### Repo and solution context

- All context has been designed as progressive disclosure, ensuring a minimum use of the agent context window.
- Per code repo
  - `generate-context-repo` skill scaffolds a `/.context/` folder per code repo with manifest-driven progressive disclosure. It includes:
    - `manifest.yml` per code repo maps modules, verification commands and dependency rules so agents load only what they need.
    - `dependency-rules.md` captures inter-service constraints for `multi-repo` features.
  - `AGENTS.md` mandates updating `/.context/` it whenever module boundaries, build commands or conventions change - keeping context live, not stale.
  - `AGENTS.md` mandates code repo context is consumed by all agents working in the repo, improving understanding and saving tokens.
- prism repo
  - `generate-context-prism` skill produces a cross-repo solution summary in the prism repo. Only needed in a `multi-repo` topology.
  - `AGENTS.md` mandates the prism repo's context is consumed by all agents working in the CDD and SDD flows, improving understanding and saving tokens.

### AI coding assistant agnosticism

- Overarching effort to work the same way with either Claude Code or GitHub Copilot AI coding assitants.
- Single canonical `AGENTS.md` is delegated to from both GitHub Copilot (via `.github/copilot-instructions.md`) and Claude Code (via `CLAUDE.md`).
- Stub redirect files in `.github/instructions/` point to canonical `/.prism/instructions/` files to avoid drift between assistants.
- Skills authored once under `.claude/skills/` with redirects from `.github/skills/`, `.github/agents/` and `.github/prompts/`.
- Shared `.vscode/mcp.json` used by both Copilot and Claude Code instead of duplicate MCP configs.
- Spec Kit agents installed identically across assistant surfaces.
- Convention table in this README documents the canonical-source-and-redirect pattern for future contributors.

### Agent context discovery

This is the discovery tree that drives the Prism framework. It describes the chain of references from one file to others that links the agent context.
Each discovery tree shows the file purpose on the left and the file name and location on the right (matching). The scope indicates who owns and is responsible for the content of that file and the purpose of that file.

**File owner**: USR:User, PSM:Prism, SPK:Spec Kit  
**File purpose**: GOV:Governance, CTX:Context, ART:Artefact, INS:Instruction, CFG:Config, SCR:Script, TPL:Template

Note that files owned by the user can be modified per team to suit their own needs for the purpose of that file.

#### Universal Prism agent context

This context is loaded for every agent operation performed from the Prism repo. `CLAUDE.md` and `.github/copilot-instructions.md` both delegate to the root `AGENTS.md`, which pulls in conduct, constitution, repo context and agent-specific instructions, etc. 

```text
Concept                                     Scope    File/Section
-------                                     -----    ------------
Claude Code instructions                    USR·GOV  CLAUDE.md (root)
Github Copilot instructions                 USR·GOV  copilot-instructions.md (.github)
└─ Universal agent instructions             USR·GOV  └─ AGENTS.md (root)
   ├─ Agent conduct                         PSM·GOV     ├─ agent-conduct.instructions.md (.prism/instructions/)
   │  └─ team additions, seed-once          USR·GOV     │  └─ agent-conduct.instructions.md (.prism/instructions/overrides/)
   ├─ Agent constitution                    PSM·GOV     ├─ constitution.md (.specify/memory/)
   ├─ Repository glossary                   PSM·CTX     ├─ glossary.md (docs/prism/)
   ├─ Prism repos listing                   USR·CTX     ├─ repos.yml (.prism/config/)
   │  ├─ define code repos                  USR·CTX     │  ├─ code-repos (coreSolutionRepos:)
   │  │  ├─ code repos rules via            USR·GOV     │  │  ├─ code-repos AGENTS.md 
   │  │  ├─ context via                     USR·CTX     │  │  ├─ code-repos .context/ 
   │  │  └─ governs edits to                USR·ART     │  │  └─ code-repos source code 
   │  └─ define context repos               USR·CTX     │  ├─ context-repos (contextRepos:)
   │     ├─ code repos rules via            USR·GOV     │  │  ├─ context-repos AGENTS.md 
   │     ├─ context via                     USR·CTX     │  │  ├─ context-repos .context/ 
   │     └─ governs edits to                USR·ART     │  │  └─ context-repos source code 
   ├─ Prism repo context (multi-repo only)  USR·CTX     ├─ manifest.yml (.context/)
   └─ SDD table, read on demand             PSM·INS     └─ per-phase instructions files (### Spec-driven development section)
      ├╌ prism-specify                      USR·INS        ├╌ prism-specify.instructions.md (.prism/instructions/overrides/)
      ├╌ prism-plan                         USR·INS        ├╌ prism-plan.instructions.md (.prism/instructions/overrides/)
      ├╌ prism-tasks                        USR·INS        ├╌ prism-tasks.instructions.md (.prism/instructions/overrides/)
      └╌ speckit-implement                  PSM·INS        └╌ speckit-implement.instructions.md (.prism/instructions/)
```

#### Sample agent context for the `/prism-plan` skill

```text
Concept                               Scope    File/Section
-------                               -----    ------------
Universal agent instructions          USR·GOV  AGENTS.md (root, full discovery in the section above)
└─ AGENTS.md row for this phase       USR·INS  └─ prism-plan.instructions.md (.prism/instructions/overrides/)
   └─ /prism-plan skill               PSM·INS     └─ SKILL.md (.claude/skills/prism-plan/)
      ├─ Pre-Exec Check, reads        SPK·CFG        ├─ extensions.yml (.specify/)
      │  └─ matches                   SPK·CFG        │  └─ hooks.before_plan: git.commit (optional)
      │     └─ invokes                SPK·INS        │     └─ SKILL.md (.claude/skills/speckit-git-commit/)
      │        └─ runs                SPK·SCR        │        └─ auto-commit.{sh,ps1} (.specify/extensions/git/scripts/)
      ├─ Setup, runs                  SPK·SCR        ├─ setup-plan.{sh,ps1} (.specify/scripts/)
      │  ├─ resolves, priority 1      USR·TPL        │  ├─ plan-template.md (.specify/templates/overrides/)
      │  ├╌ else falls back to        SPK·TPL        │  ├╌ plan-template.md (.specify/templates/)
      │  └─ copies template to        USR·ART        │  └─ plan.md (specs/[feature]/)
      ├─ Feature branch naming        PSM·INS        ├─ branch-and-folder-naming.instructions.md (.prism/instructions/)
      │  └─ team-configured formats   USR·CFG        │  └─ branch-naming.yml (.prism/config/)
      ├─ Load context, reads          USR·ART        ├─ spec.md (specs/[feature]/, from /prism-specify)
      ├─ Load context, reads          PSM·GOV        ├─ constitution.md (.specify/memory/)
      ├─ Execute workflow, generates  USR·ART        ├─ plan.md, research.md, data-model.md, contracts/, quickstart.md (specs/[feature]/)
      └─ Post-Exec Check, re-reads    SPK·CFG        └─ extensions.yml (.specify/)
         └─ matches                   SPK·CFG           └─ hooks.after_plan: git.commit + agent-context.update
            ├─ invokes                SPK·INS              ├─ SKILL.md (.claude/skills/speckit-git-commit/)
            └─ invokes                SPK·INS              └─ SKILL.md (.claude/skills/speckit-agent-context-update/)
               └─ runs                SPK·SCR                 └─ update-agent-context.{sh,ps1} (.specify/extensions/agent-context/scripts/)
                  └╌ writes block     SPK·CTX                    └╌ CLAUDE.md (root, per agent-context-config.yml)
```

### Tooling and integrations

- Some Nintex-wide skills defined in the Prism framework, but these may move to a global Nintex skills repo.
- `code`-role-repository-specific skills defined per `code`-role repository.
- MCP server registry in `/.vscode/mcp.json` for Azure, ADO, Pylance and other capabilities.
- ADO integration via `create-ado-work-item` skill plus team config in `/.prism/config/work-item-parameters.yml`.
- PowerShell scripts under `/.specify/scripts/powershell/` for feature scaffolding, prerequisites checks, plan setup and agent context updates.
- Spec Kit workflows in `/.specify/workflows/` orchestrate multi-step SDD operations with a workflow registry.
- Git helper agents (`speckit.git.feature`, `speckit.git.initialize`, `speckit.git.remote`, `speckit.git.validate`, `speckit.git.commit`) standardise branch and commit conventions.

### Process and team enablement

- `overview.md`, `install/setup.md`, `SDD.md`, `CDD.md` provide entry points for setup, large-feature flow and ad-hoc flow respectively.
