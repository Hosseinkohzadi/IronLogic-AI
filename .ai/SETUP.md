# AI Config — Developer Setup

This folder contains shared AI assistant configuration for the CX team.
Files here are version-controlled. Updates merged to main take effect immediately for all users via the symlinks set up below.

## What's in here

```
.ai/
├── .agents/
│   └── skills/
│       ├── ado-pr-reader/            — Reads ADO PR comments and prepares a fix plan
│       ├── cx-csharp-standards/      — CX C# coding standards reference
│       └── cx-react-standards/       — CX React/TypeScript coding standards reference
├── claudeHooks/                      — Claude Code hooks (symlinked to .github/hooks/ at setup)
│   ├── hooks.json                    — Hook event bindings
│   ├── post-tool-use.js
│   ├── pre-compact.js
│   ├── session-start.js
│   └── stop.js
├── clineSpecificGlobals/
│   ├── Hooks/
│   │   ├── PostToolUse               — C# compliance check after .cs file edits
│   │   └── TaskComplete              — Reminder to update knowledge base and goals on task close
│   ├── Rules/
│   │   ├── globalRules.md            — General AI assistant rules (role, git, planning, code changes)
│   │   ├── globalCSharpRules.md      — C# invariants (never/always do)
│   │   └── globalReactRules.md       — React/TypeScript invariants
│   └── Workflows/                    — Source of truth for all workflows; hard-linked to Copilot global prompts at setup
│       ├── cx-task-startup.md
│       ├── cx-code-review.md
│       ├── cx-generate-tests.md
│       ├── cx-react-new-microfrontend.md
│       └── cx-update-pipeline.md
└── SETUP.md                          — This file
```

---

## Prerequisites

- [Cline](https://marketplace.visualstudio.com/items?itemName=saoudrizwan.claude-dev) installed in VS Code
- This repo cloned locally
- Run the setup from the **repo root** (`/path/to/cx/`)

---

## Quick Start

Two scripts handle everything. Run once after cloning, and again whenever you pull new rules or change machines.

**macOS / Linux:**
```zsh
bash .ai/setup.sh
```

**Windows (PowerShell 7):**
```powershell
pwsh .ai/setup.ps1
```

Both scripts are **idempotent** — already-correct links are skipped, stale links are replaced. No manual cleanup needed before re-running.

---

> **macOS iCloud Note:** `~/Documents/` is managed by iCloud Drive on most Macs. iCloud silently replaces directory symlinks with real directories. Use **hard links** for individual files inside `~/Documents/Cline/`. Directory symlinks are safe everywhere else (outside iCloud).

## macOS Setup (zsh / bash)

Run once from the repo root after cloning:

```zsh
REPO_ROOT="$(pwd)"

# 1. Symlink agents directory into Cline's global agents location
# (dir symlink — ~/.agents is outside iCloud)
ln -s "$REPO_ROOT/.ai/.agents" "$HOME/.agents"

# 2. Hard-link rule files into Cline's global rules directory
mkdir -p "$HOME/Documents/Cline/Rules"
ln "$REPO_ROOT/.ai/clineSpecificGlobals/Rules/globalRules.md"       "$HOME/Documents/Cline/Rules/globalRules.md"
ln "$REPO_ROOT/.ai/clineSpecificGlobals/Rules/globalCSharpRules.md" "$HOME/Documents/Cline/Rules/globalCSharpRules.md"
ln "$REPO_ROOT/.ai/clineSpecificGlobals/Rules/globalReactRules.md"  "$HOME/Documents/Cline/Rules/globalReactRules.md"

# 3. Hard-link hooks into Cline's global hooks directory
mkdir -p "$HOME/Documents/Cline/Hooks"
ln "$REPO_ROOT/.ai/clineSpecificGlobals/Hooks/PostToolUse"  "$HOME/Documents/Cline/Hooks/PostToolUse"
ln "$REPO_ROOT/.ai/clineSpecificGlobals/Hooks/TaskComplete" "$HOME/Documents/Cline/Hooks/TaskComplete"

# 4. Hard-link workflows into Cline's global workflows directory
mkdir -p "$HOME/Documents/Cline/Workflows"
ln "$REPO_ROOT/.ai/clineSpecificGlobals/Workflows/cx-task-startup.md"            "$HOME/Documents/Cline/Workflows/cx-task-startup.md"
ln "$REPO_ROOT/.ai/clineSpecificGlobals/Workflows/cx-code-review.md"             "$HOME/Documents/Cline/Workflows/cx-code-review.md"
ln "$REPO_ROOT/.ai/clineSpecificGlobals/Workflows/cx-generate-tests.md"          "$HOME/Documents/Cline/Workflows/cx-generate-tests.md"
ln "$REPO_ROOT/.ai/clineSpecificGlobals/Workflows/cx-react-new-microfrontend.md" "$HOME/Documents/Cline/Workflows/cx-react-new-microfrontend.md"
ln "$REPO_ROOT/.ai/clineSpecificGlobals/Workflows/cx-update-pipeline.md"         "$HOME/Documents/Cline/Workflows/cx-update-pipeline.md"

# 5. Symlink Copilot global hooks — loaded for any workspace, including sub-folders
# (dir symlink — ~/.copilot/ is outside iCloud)
ln -s "$REPO_ROOT/.ai/claudeHooks" "$HOME/.copilot/hooks"

# 6. Hard-link workflow files as .prompt.md into Copilot's global prompts folder
# .prompt.md registers slash commands; .instructions.md files are auto-loaded.
# Both go into the same global folder — nothing touches the repo directory.
mkdir -p "$HOME/Library/Application Support/Code/User/prompts"
ln "$REPO_ROOT/.ai/clineSpecificGlobals/Workflows/cx-task-startup.md"            "$HOME/Library/Application Support/Code/User/prompts/cx-task-startup.prompt.md"
ln "$REPO_ROOT/.ai/clineSpecificGlobals/Workflows/cx-code-review.md"             "$HOME/Library/Application Support/Code/User/prompts/cx-code-review.prompt.md"
ln "$REPO_ROOT/.ai/clineSpecificGlobals/Workflows/cx-generate-tests.md"          "$HOME/Library/Application Support/Code/User/prompts/cx-generate-tests.prompt.md"
ln "$REPO_ROOT/.ai/clineSpecificGlobals/Workflows/cx-react-new-microfrontend.md" "$HOME/Library/Application Support/Code/User/prompts/cx-react-new-microfrontend.prompt.md"
ln "$REPO_ROOT/.ai/clineSpecificGlobals/Workflows/cx-update-pipeline.md"         "$HOME/Library/Application Support/Code/User/prompts/cx-update-pipeline.prompt.md"

# 7. Hard-link rule files as .instructions.md into the same global prompts folder
ln "$REPO_ROOT/.ai/clineSpecificGlobals/Rules/globalRules.md"       "$HOME/Library/Application Support/Code/User/prompts/cx-globalRules.instructions.md"
ln "$REPO_ROOT/.ai/clineSpecificGlobals/Rules/globalCSharpRules.md" "$HOME/Library/Application Support/Code/User/prompts/cx-globalCSharpRules.instructions.md"
ln "$REPO_ROOT/.ai/clineSpecificGlobals/Rules/globalReactRules.md"  "$HOME/Library/Application Support/Code/User/prompts/cx-globalReactRules.instructions.md"
ln "$REPO_ROOT/AGENTS.md"                                           "$HOME/Library/Application Support/Code/User/prompts/cx-agents.instructions.md"
```

> **Why hard links for `~/Documents/` and `~/Library/`?** Hard links share the same inode — edits to either path modify the same file. iCloud preserves hard links but silently converts directory symlinks into real folders.
>
> **Why `~/.copilot/hooks`?** This is Copilot's global hooks location — loaded for every workspace regardless of how VS Code is opened, including sub-folders like `CX.Hydra/`.
>
> **Why global prompts folder for `.prompt.md` and `.instructions.md`?** Nothing AI-config-related belongs in the repo. The global `~/Library/Application Support/Code/User/prompts/` folder is user-local and never committed. `.prompt.md` files register as `/cx-*` slash commands; `.instructions.md` files are auto-loaded as context in every workspace.

### Verify

```zsh
ls -lai "$HOME/Documents/Cline/Rules/"
ls -lai "$HOME/Documents/Cline/Hooks/"
ls -lai "$HOME/Documents/Cline/Workflows/"
ls -la "$HOME/.copilot/hooks"        # should show -> .ai/claudeHooks
ls -lai "$HOME/Library/Application Support/Code/User/prompts/" | grep cx-
```

Rule/workflow files should show link count `2`. Symlinks show the `->` target.

---

## Windows Setup (PowerShell)

> **Windows OneDrive Note:** `~/Documents/` on Windows is typically managed by OneDrive. Use **hard links** for individual files inside `Documents\Cline\`. Directory symlinks are safe for in-repo folders (`.github/`) and `~/.agents` (outside OneDrive). Hard links do **not** require Administrator privileges or Developer Mode.
>
> **Prerequisite:** The repo and your `Documents` folder must be on the same drive (typically both on `C:`).

Run once from the repo root after cloning:

```powershell
$RepoRoot = (Get-Location).Path

# 1. Symlink agents directory into Cline's global agents location
# (dir symlink — ~/.agents is outside OneDrive)
New-Item -ItemType SymbolicLink -Force -Path "$env:USERPROFILE\.agents" -Target "$RepoRoot\.ai\.agents"

# 2. Hard-link rule files into Cline's global rules directory
New-Item -ItemType Directory -Force -Path "$env:USERPROFILE\Documents\Cline\Rules"
New-Item -ItemType HardLink -Path "$env:USERPROFILE\Documents\Cline\Rules\globalRules.md"       -Target "$RepoRoot\.ai\clineSpecificGlobals\Rules\globalRules.md"
New-Item -ItemType HardLink -Path "$env:USERPROFILE\Documents\Cline\Rules\globalCSharpRules.md" -Target "$RepoRoot\.ai\clineSpecificGlobals\Rules\globalCSharpRules.md"
New-Item -ItemType HardLink -Path "$env:USERPROFILE\Documents\Cline\Rules\globalReactRules.md"  -Target "$RepoRoot\.ai\clineSpecificGlobals\Rules\globalReactRules.md"

# 3. Hard-link hooks into Cline's global hooks directory
New-Item -ItemType Directory -Force -Path "$env:USERPROFILE\Documents\Cline\Hooks"
New-Item -ItemType HardLink -Path "$env:USERPROFILE\Documents\Cline\Hooks\PostToolUse"  -Target "$RepoRoot\.ai\clineSpecificGlobals\Hooks\PostToolUse"
New-Item -ItemType HardLink -Path "$env:USERPROFILE\Documents\Cline\Hooks\TaskComplete" -Target "$RepoRoot\.ai\clineSpecificGlobals\Hooks\TaskComplete"

# 4. Hard-link workflows into Cline's global workflows directory
New-Item -ItemType Directory -Force -Path "$env:USERPROFILE\Documents\Cline\Workflows"
New-Item -ItemType HardLink -Path "$env:USERPROFILE\Documents\Cline\Workflows\cx-task-startup.md"            -Target "$RepoRoot\.ai\clineSpecificGlobals\Workflows\cx-task-startup.md"
New-Item -ItemType HardLink -Path "$env:USERPROFILE\Documents\Cline\Workflows\cx-code-review.md"             -Target "$RepoRoot\.ai\clineSpecificGlobals\Workflows\cx-code-review.md"
New-Item -ItemType HardLink -Path "$env:USERPROFILE\Documents\Cline\Workflows\cx-generate-tests.md"          -Target "$RepoRoot\.ai\clineSpecificGlobals\Workflows\cx-generate-tests.md"
New-Item -ItemType HardLink -Path "$env:USERPROFILE\Documents\Cline\Workflows\cx-react-new-microfrontend.md" -Target "$RepoRoot\.ai\clineSpecificGlobals\Workflows\cx-react-new-microfrontend.md"
New-Item -ItemType HardLink -Path "$env:USERPROFILE\Documents\Cline\Workflows\cx-update-pipeline.md"         -Target "$RepoRoot\.ai\clineSpecificGlobals\Workflows\cx-update-pipeline.md"

# 5. Symlink Copilot global hooks — loaded for any workspace, including sub-folders
# (dir symlink — ~/.copilot/ is outside OneDrive)
New-Item -ItemType SymbolicLink -Force -Path "$env:USERPROFILE\.copilot\hooks" -Target "$RepoRoot\.ai\claudeHooks"

# 6. Hard-link workflow files as .prompt.md into Copilot's global prompts folder
# .prompt.md registers slash commands; nothing touches the repo directory.
$CopilotPrompts = "$env:APPDATA\Code\User\prompts"
New-Item -ItemType Directory -Force -Path $CopilotPrompts
New-Item -ItemType HardLink -Path "$CopilotPrompts\cx-task-startup.prompt.md"            -Target "$RepoRoot\.ai\clineSpecificGlobals\Workflows\cx-task-startup.md"
New-Item -ItemType HardLink -Path "$CopilotPrompts\cx-code-review.prompt.md"             -Target "$RepoRoot\.ai\clineSpecificGlobals\Workflows\cx-code-review.md"
New-Item -ItemType HardLink -Path "$CopilotPrompts\cx-generate-tests.prompt.md"          -Target "$RepoRoot\.ai\clineSpecificGlobals\Workflows\cx-generate-tests.md"
New-Item -ItemType HardLink -Path "$CopilotPrompts\cx-react-new-microfrontend.prompt.md" -Target "$RepoRoot\.ai\clineSpecificGlobals\Workflows\cx-react-new-microfrontend.md"
New-Item -ItemType HardLink -Path "$CopilotPrompts\cx-update-pipeline.prompt.md"         -Target "$RepoRoot\.ai\clineSpecificGlobals\Workflows\cx-update-pipeline.md"

# 7. Hard-link rule files as .instructions.md into the same global prompts folder
New-Item -ItemType HardLink -Path "$CopilotPrompts\cx-globalRules.instructions.md"       -Target "$RepoRoot\.ai\clineSpecificGlobals\Rules\globalRules.md"
New-Item -ItemType HardLink -Path "$CopilotPrompts\cx-globalCSharpRules.instructions.md" -Target "$RepoRoot\.ai\clineSpecificGlobals\Rules\globalCSharpRules.md"
New-Item -ItemType HardLink -Path "$CopilotPrompts\cx-globalReactRules.instructions.md"  -Target "$RepoRoot\.ai\clineSpecificGlobals\Rules\globalReactRules.md"
New-Item -ItemType HardLink -Path "$CopilotPrompts\cx-agents.instructions.md"            -Target "$RepoRoot\AGENTS.md"
```

> **Why hard links for `Documents/` and Copilot prompts?** Hard links share the same inode — edits to either path modify the same file. OneDrive preserves hard links.
>
> **Why `~/.copilot/hooks`?** This is Copilot's global hooks location — loaded for every workspace regardless of how VS Code is opened, including sub-folders like `CX.Hydra/`.
>
> **Why global prompts folder for `.prompt.md` and `.instructions.md`?** Nothing AI-config-related belongs in the repo. The global `%APPDATA%\Code\User\prompts\` folder is user-local and never committed. `.prompt.md` files register as `/cx-*` slash commands; `.instructions.md` files are auto-loaded as context in every workspace.

### Verify

```powershell
Get-Item "$env:USERPROFILE\.agents"           | Select-Object Name, LinkType, Target
Get-Item "$env:USERPROFILE\.copilot\hooks"    | Select-Object Name, LinkType, Target
Get-ChildItem "$env:USERPROFILE\Documents\Cline\Rules\"    | Select-Object Name, LinkCount
Get-ChildItem "$env:USERPROFILE\Documents\Cline\Hooks\"    | Select-Object Name, LinkCount
Get-ChildItem "$env:USERPROFILE\Documents\Cline\Workflows\" | Select-Object Name, LinkCount
Get-ChildItem "$env:APPDATA\Code\User\prompts\" | Where-Object Name -like 'cx-*'
```

Rule/workflow files show `LinkCount` of `2`. Symlinks show `LinkType` of `SymbolicLink`.

---

## How Updates Work

1. `git pull` on any branch that updates files in `.ai/` → your symlinks immediately reflect the new content — no re-setup needed.
2. If you edit a file via Cline (which edits through the symlink) → the change lands in `.ai/` → shows up in `git status` → can be submitted as a PR for team review.

---

## Opting Out of a Specific Rule or Hook

To disable a single item without breaking the rest, remove just the hard link:

```zsh
# macOS — remove just that hard link (source file in .ai/ is untouched)
rm "$HOME/Documents/Cline/Rules/globalCSharpRules.md"
```

```powershell
# Windows — remove just that hard link (source file in .ai/ is untouched)
Remove-Item "$env:USERPROFILE\Documents\Cline\Rules\globalCSharpRules.md"
```

Re-run the relevant `ln` / `New-Item` line to re-enable.

---

## Re-setup After Cloning to a New Path

Symlinks use absolute paths resolved at setup time. If you re-clone the repo to a different location, remove the existing symlinks and re-run the setup script from the new repo root.

---

## VS Code / GitHub Copilot Setup

GitHub Copilot discovers AI customization files (rules, prompts, AGENTS.md) from the workspace root. Each developer needs to add these settings to their **VS Code user settings** once — this is required regardless of whether you open the full monorepo root or a project subfolder (e.g. `CX.Hydra/`).

> **Why user settings?** `.vscode/settings.json` in the monorepo root only applies when you open the repo root as your workspace folder. It does not apply when opening a sub-folder directly. User settings ensure Copilot always loads the `.ai/` rules.

### One-time user settings setup

Add these settings to your VS Code user settings once:

1. Open user settings JSON: `⌘,` (macOS) or `Ctrl+,` (Windows), then click the `{}` icon top-right.
2. Add:

```json
{
  "chat.useCustomizationsInParentRepositories": true,
  "chat.useNestedAgentsMdFiles": true,
  "chat.instructionsFilesLocations": {
    ".github/instructions": true,
    ".ai/clineSpecificGlobals/Rules": true
  }
}
```

This enables VS Code to walk up the folder tree to the monorepo root and discover `AGENTS.md`, rule files, and prompt files automatically.

### What these settings do

| Setting | Effect |
|---|---|
| `chat.useCustomizationsInParentRepositories` | Discovers root AGENTS.md, rules, and prompts when a subfolder is opened |
| `chat.useNestedAgentsMdFiles` | Loads per-project AGENTS.md files (e.g. `CX.Core/AGENTS.md`) when working in subdirectories |
| `chat.instructionsFilesLocations` | Registers `.ai/clineSpecificGlobals/Rules/` as a source for `.md` instruction files alongside `.github/instructions/` |