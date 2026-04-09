---
agent: agent
description: "Run at the start of every task — checks aiWorkplans/ for in-progress work, loads .clinerules, configures context, and activates the right skill for C#, React, or pipeline work."
---
# Workflow: CX Task Startup

Trigger: At the beginning of every task, before writing any code or making any changes.

## Steps

1. If the task was tracked in `aiWorkplans/`, check for any file with `Status: IN PROGRESS` and resume from the first unchecked step (`- [ ]`) before doing anything else.
2. Read the per-project `.clinerules` file if one exists in the current project directory. This contains project-specific patterns, bans, and context that override or extend the global rules.
3. Note available files in `aiReferenceFiles/` — read them only if the task topic directly requires it (pipeline flow, routing, business rules, B2C policies, DTO fields). If `aiReferenceFiles/aiKnowledgeBase.md` exists, read it when the task involves project architecture, key integration points, or pipeline patterns — not at every task start.
4. Read `README.md` at the workspace root and at the root of the specific project/directory being modified for purpose, startup instructions, and documentation links.
5. Read relevant config files for the area being modified: `.csproj`, `package.json`, `tsconfig.json`.
6. Read and apply any linter/formatter rules in the project (`.editorconfig`, eslint config, etc.).
7. Activate the relevant skill(s) based on the task type:
   - C# code → activate skill `cx-csharp-standards`
   - React/TypeScript code → activate skill `cx-react-standards`
   - Dockerfile, `azure-pipelines*.yml`, or Helm values → activate skill `cx-azure-pipelines` (when available)

