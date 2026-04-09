# CX Monorepo — AI Agent Configuration

All AI agent rules, skills, hooks, and workflows live in `.ai/`. This is the canonical configuration file — every rule referenced below is defined here.

## Always Load

Read these files at the start of every task:

| File | When |
|---|---|
| `.ai/clineSpecificGlobals/Rules/globalRules.md` | Always |
| `.ai/clineSpecificGlobals/Rules/globalCSharpRules.md` | C# tasks (`.cs`, `.csproj`) |
| `.ai/clineSpecificGlobals/Rules/globalReactRules.md` | React/TypeScript tasks (`.ts`, `.tsx`, `.js`, `.jsx`) |

## Per-Project Context

Every CX project subdirectory may have a `.clinerules` file at its root with project-specific context. Read it before any code task in that project. Some projects also have an `AGENTS.md` that points here.

Lookup chain when navigating to a project:
1. `{project}/.clinerules`
2. `{project}/aiReferenceFiles/aiKnowledgeBase.md`
3. `{project}/README.md`

## CX Monorepo Root (`{cx-root}`)

The nearest ancestor directory that contains a `.ai/` folder.
- If the current workspace contains `.ai/` → `{cx-root}` is the current directory.
- If not (e.g. workspace is `CX.Hydra/`) → `{cx-root}` is `../` (one level up).

`aiWorkplans/` and `aiKnowledgeBase.md` are always workspace-scoped — never placed at `{cx-root}`.

## Skills Registry

| Skill | File | Activate when... |
|---|---|---|
| `cx-csharp-standards` | `.ai/.agents/skills/cx-csharp-standards/SKILL.md` | writing, reviewing, or refactoring C# code |
| `cx-react-standards` | `.ai/.agents/skills/cx-react-standards/SKILL.md` | writing, reviewing, or refactoring React/TypeScript code |
| `ado-pr-reader` | `.ai/.agents/skills/ado-pr-reader/SKILL.md` | reading or addressing ADO pull request review comments |

## Workflows

| Workflow | File | Notes |
|---|---|---|
| `cx-task-startup` | `.ai/clineSpecificGlobals/Workflows/cx-task-startup.md` | Run at the start of every task |
| `cx-code-review` | `.ai/clineSpecificGlobals/Workflows/cx-code-review.md` | Run after all code changes |
| `cx-generate-tests` | `.ai/clineSpecificGlobals/Workflows/cx-generate-tests.md` | Generate or migrate unit tests |
| `cx-react-new-microfrontend` | `.ai/clineSpecificGlobals/Workflows/cx-react-new-microfrontend.md` | Scaffold a new CX2 micro-frontend |
| `cx-update-pipeline` | `.ai/clineSpecificGlobals/Workflows/cx-update-pipeline.md` | Add or update Dockerfiles and Azure pipelines |

VS Code / GitHub Copilot users can also invoke these via `/prompt` (the workflows are registered as global slash commands).

## Navigation

When looking for a CX library, internal package, or project:
1. Read `.ai/clineSpecificGlobals/monorepo-map.md` to find the local path.
2. Navigate to that path and read `.clinerules` or `aiKnowledgeBase.md` first.
3. For external packages, check NuGet or npm — do not guess types or APIs.

## Before Every Task

Run workflow `cx-task-startup`.

## After Every Task

After completing code changes, run workflow `cx-code-review`.
