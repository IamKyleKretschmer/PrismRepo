# Nintex Prism

<img src="./images/prism.png" alt="Prism" style="max-height: 300px;">

- [Install Prism into team repo](./install/prism-cli.md), or [choose between the CLI and the Claude Code plugin](./install/routes.md)
- [Prism adoption in Nintex](./adoption.md)

## Objectives

Nintex Prism is a process framework aiming to achieve agentic FDLC via:

- **Guidance**: Customisable instructions leading agentic FDLC through specs, design, code, tests and docs.
- **Governance**: Include design and implementation governance, ensuring standards and cross-cutting concerns such as logging, security, observability, accessibility, localisation, etc are maintained.
- **Context**: Provide the coding agents with a comprehensive but lightweight progressive-disclosure context of each code repo.

Additional objective:

- **Flexibility**: Each Nintex team can use parts or all of the process and adapt the process to their needs.
- **Communication**: Both within a team (PM, UX, EM, AO, devs, dev ops, QA and TC) and between affected teams.
- **Cohesion**: Ensure a cohesive solution is designed and implemented across multiple repos and work items.
- **Completeness**: Ensure a checklist of completeness applied to all output artefacts and phases.
- **Shift-left**: Minimal rework by shifting reviews earlier in the process.
- **Tooling**: Custom agents, skills, scripts, MCP servers to assist the process.
- **Velocity**: Faster specification, architecture, technical design, coding, testing, dev ops and documentation.
- **Economy**: Save tokens by providing a summarised system context.
- **History**: A durable repository of feature specification and design, including important decisions.

## Core Concepts

This is achieved using context-driven and spec-driven development:

- **CDD**: Context-driven development (CDD) directs any coding framework with governance, guidance, checklists and tools. This allows free-form agentic coding to an enterprise standard. This is vibe-coding with guardrails.
- **SDD**: Spec-drive development (SDD) builds on top of CDD to enable agentic SDLC. SDD automates the generation of specs, technical plans and work items, allowing smooth generation of a wider feature. This framework currently uses GitHub Spec Kit to run the SDD.

Prism aims to be non-prescriptive while offering a comprehensive agentic framework requiring minimal setup.

```mermaid
flowchart LR
  SDD["SDD"]
  SpecKit["Spec Kit<br/>phase skills, SDD agent instructions, checklists"]
  CDD["CDD"]
  governance["Governance<br/>(eg. constitution, per-repo requirements)"]
  guidance["Guidance<br/>(eg. per-repo instructions)"]
  tooling["Tooling<br/>(eg. skills, MCP)"]
  context["Context<br/>(system summary)"]
  SDD-->|uses processes of|SpecKit
  SDD-->|guided by|CDD
  CDD-->|governed by|governance
  CDD-->|guided by|guidance
  CDD-->|leverages|context
  CDD-->|builds with|tooling
```

### Process

- **[CDD](cdd.md)**: Context-driven development. A defined framework to apply governance, guidance, context and tooling to agentic development. Does not define process. Ideal for smaller, isolated tasks.
- **[SDD](sdd.md)**: Specification-driven development. A process framework where feature intent, validation, design, tasks and downstream implementation artefacts are driven from specifications. Best suited for larger features requiring cross-team, multi-work item or multi-repo coordination.
- **Agent Instructions**: Procedural guidance files in `.prism/instructions/` that define how agents should execute specific SDD workflows (e.g., `speckit-specify.instructions.md`, `speckit-tasks.instructions.md`, `speckit-implement.instructions.md`).
- **Checklists**: Post-stage checklists to be run during after SDD stages.

### Governance

- **Constitution**: Fundamental Nintex software-design principles that govern all framework development. Stored in `/.specify/memory/constitution.md` and defines overarching development values and constraints. This should not generally be different between teams.
- **AGENTS.md**: Root-level agent operating procedures in the prism repo that provide imperative instructions for agent processes, tooling and rules. Each code repo also has its own `AGENTS.md` for repo-specific guidance.

### Context

- **Solution**: The full set of repositories declared in `/.prism/config/repos.yml` — the prism repo plus the code and context repos — forming the governed repo set (both development and context) for a team or feature area. See the [glossary](./glossary.md) for the canonical role and topology definitions.
- **Repo context**: Progressive-disclosure guidance stored in each code repo's `/.context/` directory to help agents follow local architecture, conventions and build/test practices without overwhelming initial context.

### Tooling

- **Agents**: Both Spec Kit and custom Prism agents are used to guide design and implementation workflows.
- **Skills**: Packaged workflows with specialised domain knowledge (e.g., `generate-context-repo`, `create-ado-work-item`) that extend agent capabilities.
- **MCP Servers**: Model Context Protocol server definitions in `/.vscode/mcp.json` that provide additional tooling and integrations.
- **`prism-help`**: A read-only helper agent that answers Prism/SDD process questions and reports where the current feature is in the SDD journey and what to do next. See [Getting help](#getting-help).

## Setup process

Prism CLI [installation](./install/prism-cli.md) and [setup](./install/setup.md)

## Getting help

Not sure how Prism works, or where you are in a feature? Use the read-only `prism-help` agent (`/prism:help` with the Claude plugin, whose command names are listed in the [SDD guide](./sdd.md)). It handles two kinds of request:

- **Process questions** — how Prism and its SDD process work, what a given stage, artefact, agent or role means, and the recommended practice, answered from these `docs/prism/` pages.
- **"Where am I / what next"** — which SDD stage the currently-scoped feature is in, whether any work looks out of sequence, and which agent to run next.

It never changes files or repository state, so it is safe to invoke at any point. It is a helper, not part of the CDD or SDD flow.

## Prism Structure

### Dependency map of Prism solution

Tooling is defined at multiple levels:

- {prism} - setup defined in the prism repo.
- {per code repo} - setup defined inside the code repos of a team's solution. In a `single-repo` topology the same repository holds both the `prism` and `code` roles. See the [glossary](./glossary.md) for role/topology definitions.

> *Note the orange lines indicating the context-driven development (CDD) path.*

```mermaid
flowchart TD
subgraph "colours legend"
    direction LR
    prism(("prism repo")):::prism
    repo(("code repo")):::repo
    A[ ]
    B[ ]
    %% Make Nodea A,B invisible
    style A fill:none,stroke:none,width:0px,height:0px
    style B fill:none,stroke:none,width:0px,height:0px
    C[ ]
    D[ ] 
    %% Make Node C,D invisible
    style C fill:none,stroke:none,width:0px,height:0px
    style D fill:none,stroke:none,width:0px,height:0px
    A -->|CDD<br/>pathway| B
    C -->|SDD<br/>pathway| D

end

    User -->|CDD <br/>invocation| Assistant
    Assistant --> |"Execute /cdd agent<br/>to directly generate<br/>code using CDD<br/>(still uses Coding<br/> Assitant Guidance)"| ArtefactsRepo

    User((User))
    Assistant{{"AI Coding Assistant<br/>eg. Claude Code<br/>(Using selected LLM model)"}}
    Skills["Skills<br/>/.claude/skills"]:::prism
    MCP["MCP Servers<br/>/.vscode/mcp.json"]:::prism

    User -->|SDD</br>invocation| Assistant
    Assistant --->|executes<br/>Speckit agent<br/>eg speckit.implement| Framework
    
    Framework <-->|generates<br/>and reads| ArtefactsSdd
    Framework ---->|generates| ArtefactsRepo

    Assistant --->|"reads"| AgentsPrism
    Framework -->|uses artefact <br/>templates| TemplatesDefault
    Assistant -->|invokes| Skills
    Assistant -->|invokes| MCP

subgraph "Coding Assistant Context"
    AgentsPrism["/AGENTS.md"]:::prism
    AGENTSrepo[["/AGENTS.md<br/>{per code repo}"]]:::repo
    Instructions["Agent Instructions for Prism framework<br/>/.prism/instructions/*.instructions.md"]:::prism
    SolutionCtx["Solution Context<br/>(prism repo)<br/>/.context/...<br/>*(this is generated per solution)*"]:::prism
    TeamConfig[Team config<br/>/config/*]:::prism
    AgentsPrism -->|invokes| Instructions
    AgentsPrism -->|references| AGENTSrepo
    AgentsPrism --->|retrieves<br/>context from| SolutionCtx
    SolutionCtx -->|references| RepoCtx
    AgentsPrism --->|references| TeamConfig
    subgraph CodeRepos["Code repos"]
      AGENTSrepo -->|"references (this also assists with agentic code changes made directly in the repo"| RepoCtx
    RepoCtx[["Repo Context<br/>{per code repo}<br/>/.context/..."]]:::repo
    end
    style CodeRepos stroke:#666
end

subgraph "Spec Kit"
    Framework(("SDD Framework<br/>(Spec Kit agents, skills, scripts)")):::prism
    TemplatesDefault["Default Spec Kit<br/>templates<br/>/.specify/templates/...<br/>"]:::prism
    TemplatesOverrides["User-overridden<br/>Spec Kit templates<br/>/.specify/templates/overrides/<br/>{solution}"]:::prism
    ArtefactsSdd["SDD Artefacts<br/>(Specs, plans, <br/>tasks, etc)"]:::prism

    TemplatesDefault -->|overridden by| TemplatesOverrides
end

    Framework -->|governed by| Constitution
    Constitution["Constitution<br/>/.specify/memory/<br/>constitution.md<br/>"]:::prism
    AgentsPrism -->|governed by| Constitution

    ArtefactsRepo[["Code repo Artefacts<br/>(Code, ops, tests, docs, etc)"]]:::repo

    linkStyle 0 stroke:orange,stroke-width:2px,stroke-dasharray:10,10
    linkStyle 1 stroke:green,stroke-width:2px,stroke-dasharray:10,10
    linkStyle 2 stroke:orange,stroke-width:2px,stroke-dasharray:10,10
    linkStyle 3 stroke:orange,stroke-width:2px,stroke-dasharray:10,10
    linkStyle 4 stroke:green,stroke-width:2px,stroke-dasharray:10,10
    linkStyle 5 stroke:green,stroke-width:2px,stroke-dasharray:10,10
    linkStyle 6 stroke:green,stroke-width:2px,stroke-dasharray:10,10
    linkStyle 7 stroke:green,stroke-width:2px,stroke-dasharray:10,10

    classDef prism fill:#338
    classDef wkspc fill:#77b
    classDef repo fill:#484
```

### Prism repo components

| Component | Source | Location | Notes |
| --- | --- | --- | --- |
| Constitution | prism | /.specify/memory/ | Fundamental Nintex software-design principles. Generally do not modify. |
| Solution context | user | {sdd repo}/.context/ | Very high level multi-repo architecture. Generate with `/generate-context-prism` after generating code repo contexts (multi-repo topology only). |
| Agent operating procedures | prism | /AGENTS.md, /.prism/instructions/ | Imperative instructions for agent processes, tooling and rules. |
| Agent conduct & phase overrides | user | /.prism/instructions/overrides/ | Team additions to agent conduct and to each SDD phase. Seeded once, never overwritten. |
| Team configuration | user | /.prism/config/ | Team-specific settings. Configure after Prism installation. |
| Skills | prism | /.claude/skills | SDD and CDD skills |
| MCP Servers | prism | /.vscode/mcp.json | MCP server definitions. |
| AI coding assistant | user | /.claude or /.github, etc | Claude Code, GitHub Copilot, or your approved favourite. |
| Spec Kit templates | prism | /.specify/templates/overrides/... | Templates for SDD artefacts - for specs, plans, etc. Override per solution. |
| SDD Framework | Spec Kit (via prism) | /.specify | Spec kit SDD workflows, processes, templates and scripts. Do not modify. |
| SDD artefacts | user (via prism) | /specs/{feature} | Per-feature specs, plans, tasks and contracts. |

### Code repo components

| Component type | Config Owner | Folder | Notes |
| --- | --- | --- | --- |
| Code repo context | Dev team | /.context/ | Progressive disclosure per code repo of architecture, patterns and practices, dependencies and build instructions, etc. Generate with `/generate-context-repo`. |
| Code repo agent operating procedures | Dev team | /AGENTS.md (or similar) | Imperative instructions per code repo for agent processes, providing repo rules, architecture, tooling and processes. Anything that is not universal to all Nintex repos or universal to the Prism process should be referenced from this file. |
