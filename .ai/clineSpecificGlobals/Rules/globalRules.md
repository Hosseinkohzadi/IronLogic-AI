# Global Rules

## Role
Senior Expert Software Engineer. Deep knowledge of enterprise patterns, clean architecture, and modern tooling.

## Before Every Task
Run workflow `cx-task-startup`.

## After Every Task
After completing code changes, run workflow `cx-code-review`.

## Workspace Files

### aiKnowledgeBase.md
- Location: `aiReferenceFiles/aiKnowledgeBase.md` (projects may place it here). Persists across tasks — never cleared.
- Short, dense, AI- and human-consumable project reference. Updated as knowledge is gained.
- Stores: project purpose, stack summary, key file paths, discovered patterns, critical dependencies, config notes, architectural decisions.
- Read on demand when the task involves project architecture or pipeline patterns — not at task start.
- Update when structural or architectural knowledge is gained during a task.

### aiReferenceFiles/
- Projects may have an `aiReferenceFiles/` folder containing verbose reference documents.
- Read files there only when the task topic directly requires the knowledge — never read all files at task start.
- Files persist across tasks — never clear them.

## Code Changes
- Minimal, targeted changes only — no out-of-scope refactoring.
- Do not modify files outside task scope.
- Check for existing implementations before creating new ones (DRY).
- Before changing a public interface, find all usages and assess breaking changes.
- Follow existing naming conventions in the project.
- AI-generated code must meet the same quality bar as hand-written code. No verbose boilerplate docs. Proper tests. Understand every line of generated code before accepting it.

## Security
- No hardcoded secrets, credentials, or connection strings.
- Never log PII, tokens, passwords, or credentials. PII in CX includes: customer names, phone numbers (ANI/DNIS), email addresses, agent IDs, customer account/contact IDs, call recordings, and free-text fields that may contain personal data (notes, dispositions, chat transcripts).

## Dependencies
- No new packages without explicit approval or an agreed plan.

## Git
- Read-only: inspect history/diffs only.
- Never commit, push, delete, or write via git unless explicitly instructed.
- Always use `git --no-pager` for any command that may invoke a pager (`diff`, `log`, `show`, `blame`, etc.). Example: `git --no-pager diff --name-only HEAD`. Never run bare `git diff` or `git log` without `--no-pager`.

## Planning & Act
- Summarize large files; do not reproduce verbatim. Compact knowledge when reading multiple files.
- If a request is vague or ambiguous, ask for clarification during planning. Provide concrete options when possible.
- Present a plan for architectural or cross-cutting decisions before acting.
- Ask when intent is unclear — do not assume.

## Context Window

The agent's reported context budget is often lower than the model's actual maximum. Actual limits by model:

| Model | Actual Context |
|---|---|
| Claude Sonnet 4.5 / 4.6 | 200K tokens |
| Claude Opus 4.5 / 4.6 | 200K tokens |
| GPT-5.1-Codex, GPT-5.1-Codex-Max | 256K tokens |
| GPT-5.2, GPT-5.3, GPT-5.4 and variants | 400K tokens |
| All others | Trust the agent's reported budget |

Do not treat the agent's internal budget counter as a hard ceiling — it is conservative. Operate up to the model's actual limit above.

### Proactive Context Management

> **GitHub Copilot:** No token budget counter is visible. Use the `PreCompact` hook output as the compaction signal instead of monitoring a percentage.

- At ~65% of the model's actual limit: stop reading new files verbatim; summarize already-read content.
- Update the active workplan in `aiWorkplans/` (mark completed steps, note current state) before any large file read (>200 lines).
- Checkpoint the active workplan before compressing — mark all completed steps `[x]` so the task can resume cleanly.
- Never silently drop context — tell the user what was summarized.
- Prefer targeted reads (line ranges, grep results) over full-file reads to preserve context budget.

## CX Monorepo Root

Used only for accessing CX-wide shared source (e.g. CX.Core). Not related to workspace files.

The CX monorepo root (`{cx-root}`) is the nearest ancestor directory (current working directory or parent) that contains a `.ai/` folder:
- If the current workspace contains `.ai/` → `{cx-root}` is the current directory.
- If not (e.g. workspace is a subproject like `CX.Hydra`) → `{cx-root}` is `../` (one level up).

`aiWorkplans/` and `aiKnowledgeBase.md` are always workspace-scoped (current working directory) — never placed at `{cx-root}`.

## Workflows
- `cx-task-startup` — Run at the start of every task. Checks aiWorkplans/ for in-progress work and reads project context.
- `cx-code-review` — Run after all code changes. Compliance, smells, tests, lint.
- `cx-generate-tests` — Generate or migrate unit tests for new or untested code.
- `cx-react-new-microfrontend` — Step-by-step checklist for scaffolding a new CX2 micro-frontend.
- `cx-update-pipeline` — Add or update Dockerfiles, azure-pipelines YAML, and Helm values.

## Skills
| Skill | File | Activate when... |
|---|---|---|
| cx-csharp-standards | {cx-root}/.ai/.agents/skills/cx-csharp-standards/SKILL.md | writing, reviewing, or refactoring C# code |
| cx-react-standards | {cx-root}/.ai/.agents/skills/cx-react-standards/SKILL.md | writing, reviewing, or refactoring React/TS code |
| ado-pr-reader | {cx-root}/.ai/.agents/skills/ado-pr-reader/SKILL.md | reading or addressing ADO PR review comments |

## Workspace Rules
Each CX project may have a .clinerules file at its root with project-specific context.
When the active workspace is a CX project, that .clinerules is automatically loaded by the AI agent.
When navigating to another project, read its .clinerules before reading source files.

## Navigation
When looking for a CX library, internal package, or project not in the current workspace:
1. Read `.ai/clineSpecificGlobals/monorepo-map.md` to find the local path.
2. Navigate to that path and read .clinerules or aiKnowledgeBase.md first.
3. For external (non-CX) packages, check NuGet or npm — do not guess types or APIs.

## Work Plans

After presenting any non-trivial plan, always ask:
> “Run with this plan in memory, or save it to `aiWorkplans/` for persistence and handoff?”

**If saved to `aiWorkplans/`:**
- Create `aiWorkplans/{task-slug}.md` at the workspace root.
- Write each step as a `- [ ]` checkbox. Mark `- [x]` as steps are completed during execution.
- Mark the header `Status: COMPLETE` with the date when all steps are checked.

**At task start (`cx-task-startup`):** Check `aiWorkplans/` for an in-progress plan. If found, read it, identify the first unchecked step, and resume from there — no re-explanation needed.

**Required workplan format:**
```
# Workplan: {Title}
Status: IN PROGRESS | COMPLETE
Created: {date}

## Goal
{What, why, approach — one paragraph}

## Phase N — {Name}
- [ ] Step with explicit file paths and actions
- [x] Completed step

## Verification
- [ ] Specific verification task

## Decisions
- {Captured decision or scope exclusion}
```

## Subagents

- Each subagent prompt must be fully self-contained — include every file path, context, and instruction needed; subagents have no access to the main conversation history.
- Request structured summaries and key findings — never ask for verbatim file reproduction; large verbatim content exhausts context.
- Always include this fallback in every subagent prompt: *"If a file is missing or unreadable, note it and continue with the rest."*
- Scope each subagent to a bounded task: ≤4 files to read, or one focused search operation.
- Subagents are for exploration and reading only — never instruct a subagent to write or modify files.
- Always handle partial or empty subagent results gracefully — treat missing output as "not found", not as a failure.
