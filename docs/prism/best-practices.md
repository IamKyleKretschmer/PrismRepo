# Best practices for Prism

`Forget your process!`: Agentic spec-driven development is not just adding agents to help with coding - it's a completely new way of working. Here are the learnings and recommended practices from the teams that are currently using it.

- [Best practices for Prism](#best-practices-for-prism)
  - [Pitfalls](#pitfalls)
  - [Philosophies](#philosophies)
  - [Process](#process)
    - [Avoid rework](#avoid-rework)
      - [Reviews - Shifting tech reviews left](#reviews---shifting-tech-reviews-left)
      - [Don't use CDD!](#dont-use-cdd)
      - [Just-in-time planning](#just-in-time-planning)
      - [Spec-informed approach](#spec-informed-approach)
    - [Deep integration of QA](#deep-integration-of-qa)
    - [Session and branch management during implementation](#session-and-branch-management-during-implementation)
    - [PR review](#pr-review)
    - [Fixing the process itself](#fixing-the-process-itself)
  - [Configuration](#configuration)
    - [Context](#context)
    - [Brevity](#brevity)
    - [Scoping](#scoping)
    - [Linking to files](#linking-to-files)
    - [Convention over customisation](#convention-over-customisation)
    - [Multi-repo setup](#multi-repo-setup)

## Pitfalls

While basic CDD and SDD can churn out code extremely quickly, by default they are best suited to greenfield POCs. Integrating them with an existing enterprise code base and achieving quality output requires context and governance - this is what Prism is trying to achieve.

## Philosophies

- **Code as spec**: the code (and context and docs) are the final feature spec. While we do our best to perfect the spec we accept that by the time the feature is implemented the spec will be out of date.
- **Shift-left**: move as much of the design and review as early as possible.
- **Iterate in phases**: while `shift-left` implies a waterfall method, we should repeat it in agile-style phases for more complex tasks.


## Process

### Avoid rework

#### Reviews - Shifting tech reviews left

The main spec and plan reviews now happens as soon as the plan phase artefacts are generated. This is where all dev team members affected must agree on the details (eg what is being developed and where in the codebase). This is a new discipline - skipping it leads to a lot of rework and expense in wasted tokens! Getting it right streamlines the rest of the implementations.  Be clear about what is in and out of spec.

Thoroughly review outputs. Be ruthless in pruning out anything not specifically relevant to your feature. Don't gloss over it! This is a big job. Remember to use agentic review assistance via agents such as `prism-grill-me`.

Have reviews done by **all** involved parties - PM, EM, AO, UX, QA, Devs, Ops - before moving on.

#### Don't use CDD!

(unless it is really trivial) While CDD is an option, space-invaders experience is that doing anything more than absolutely trivial work is faster and cheaper when using SDD. CDD causes more chaos, churn and rework - takes longer overall and costs more in tokens. SDD can be fast-tracked to step through all stages very quickly for smaller features.

#### Just-in-time planning

In SDD, the spec can be developed ahead of time, but the plan phase should only be done when someone is ready to review and implement the feature. This detailed technical plan becomes stale quickly as the codebase changes.

#### Spec-informed approach

The space-invaders team started with a *spec-anchored* approach but moved to a *spec-informed* approach. The difference to us was that with the spec-anchored approach we tried to reverse-propagate all late changes back through all layers, back up to the spec if necessary.

With the *spec-informed* approach we use the spec, plans and tasks as initial guides, and any late changes do not propagate back. The trade-off is that the spec or technical plans may drift from the final implementation, but in practical terms it is the final outputs (code, docs, generated context) which is important for moving forward and future reference.

### Deep integration of QA

Testing is a potential bottle-neck in the SDD process if not managed correctly. Keep QA as left-shifted and integrated as possible:

- QA contribute to spec review - they should use `prism-grill-me` to create a `spec-qa.md`
- QA participate in technical plan definition - ensure user stories in the spec are covered by the generated test plans.
- QA actively integrate during development - ad-hoc testing on partially developed features.
- QA participate in the PR process - ensure that all tests required to meet the original user acceptance have been created and are passing.
- As nice as test-driven development sounds, in practice it's more efficient to let the agent finalise the code for a task first and then generate the tests to suit, rather than trying to hold the agent to strict TDD.

### Session and branch management during implementation

When implementing SDD tasks in the codebase it is best to keep the SDD agent session separate from the task coding session. Also in the case of multi-repo configuration it may be best to have two IDEs open - one for SDD and one for the code repo being developed.

Similarly managing branches needs to be done carefully in the case of Prism being installed into a mono-repo. Care must be taken to keep the bulk specify/plan branches separate from the code implementation branches.

### PR review

Once code, tests and docs for a task are complete, ask for an agentic code review before opening the PR. Open the relevant `tasks.md`, highlight the task, and prompt something like:

> Perform a code review on all changes made in the current branch against the requirements of the highlighted task.

This only works well if the task description also identifies which repo(s) it targets.

### Fixing the process itself

When the agents get something wrong, it's usually one of three things: missing **context** (repo or solution `/.context/`), missing **guidance** (Prism or repo `AGENTS.md` / instructions), or a poorly shaped **template** (`/.specify/templates/`). Don't hand-edit these files as a first resort - prompt the most relevant agent (eg `speckit.implement`, or a general-purpose agent) to make the fix, since it can usually work out which file(s) need to change without you tracking it down yourself. eg:

## Configuration

### Context

Context, especially progressive-disclosure, is crucial to accuracy, efficiency and managing the context window. Prism is oriented around the `/.context/` folders. Just make sure it is generated and referenced from the local AGENTS.md (and therefore from CLAUDE.md if relevant). The `generate-context-repo` agent will do this for you - consider telling the `generate-context-repo` skill to consider particular aspects of your repo.

### Brevity

Less is more - adding lots of agent context and instructions is highly counter-productive. All instructions must use progressive disclosure to allow the agents to only load what they need, and mandatory context and instructions should be as brief as possible.

It is a real trap to keep adding more and more useful knowledge, tools and instructions to the CLAUDE.md/AGENTS.md. These files should be limited to under 200 lines each. Beware that using @filename to reference another file in CLAUDE.md will simply paste that other file inline and pollute context.

### Scoping

Instructions and process tweaks need to be added in the correct place:

- Don't change `constitution.md` or the `standards/` beside it. They're the Nintex constitution and standards, which Prism owns: every Prism update replaces them with the current version, and with the Claude plugin they come from the plugin. Add your team's own standards through `AGENTS.md` instead.
- Don't change any files under `/.specify/` *except* the `/templates/overrides/`. They belong to Spec Kit and will be overwritten by Spec Kit updates (via Prism updates). The `/templates/overrides/` folder holds your team's own spec, plan and task templates: a template there replaces Prism's template of the same name. 
- Don't change anything under `/.prism/` *except* the `/config` files and `/instructions/overrides/`. Everything else will be overwritten by Prism updates. The files in `/instructions/overrides/` are the supported customisation point for agent conduct and for each SDD phase.
- Additional instructions applying to more that one repo should be applied to the team Prism repo `AGENTS.md`, but as progressive-disclosure, meaning that they should be separate files linked by a brief description of their intent to allow the agent to choose when to load them.
- Additional instructions applying to a single code repo should be applied to the `AGENTS.md` of that repo - again as progressive-disclosure.
- More complex capabilities should be added as local skills - consider sharing these.

### Linking to files

The links that work in markdown in VS Code don't necessarily work for agents. Agents have a different concept of the "root" folder which can be the PC root folder rather than the solution, VS Code workspace, or repo root. For this reason avoid absolute links in markdown - use relative instead. When referring to files in other repos use `.context/manifest.yml` format instead and indicate that it will be in the target code repo.

### Convention over customisation

This framework is entirely flexible. For each job a team can choose to use SDD, CDD, parts of this system or just manual development depending on the size and scope of the work. Templates and processes can be modified per team or product. Having said that, the conventions specified in Prism are the result of an evolving and working process.

### Multi-repo setup

For a `multi-repo` topology, put all of the solution's repos (the prism repo plus every code repo) under the same local root folder across every team member's machine. Prism's repo config doesn't require this, but it keeps relative paths and any shared VS Code workspace file consistent for everyone, avoiding "works on my machine" path issues.

