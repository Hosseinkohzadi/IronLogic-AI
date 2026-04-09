#!/usr/bin/env bash
# AI Config Setup — macOS / Linux
#
# Wires up Cline and GitHub Copilot to the shared AI config in .ai/.
# Safe to re-run: existing links are removed and recreated.
#
# Usage: run from the repo root
#   bash .ai/setup.sh

set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
AI_ROOT="$REPO_ROOT/.ai"
RULES="$AI_ROOT/clineSpecificGlobals/Rules"
HOOKS="$AI_ROOT/clineSpecificGlobals/Hooks"
WORKFLOWS="$AI_ROOT/clineSpecificGlobals/Workflows"
CLAUDE_HOOKS="$AI_ROOT/claudeHooks"

# ── Platform detection ────────────────────────────────────────────────────────
OS="$(uname -s)"
if [[ "$OS" == "Darwin" ]]; then
    COPILOT_PROMPTS="$HOME/Library/Application Support/Code/User/prompts"
else
    COPILOT_PROMPTS="$HOME/.config/Code/User/prompts"
fi

# ── Helpers ───────────────────────────────────────────────────────────────────
step()    { echo ""; echo "── $1 ──"; }
ok()      { echo "  ✔  $1"; }
skip()    { echo "  –  $1 (already linked)"; }
warn()    { echo "  ⚠  $1"; }

# hard_link SRC DST
# Creates a hard link DST → SRC. Skips if already the same inode. Replaces if stale.
hard_link() {
    local src="$1" dst="$2"
    mkdir -p "$(dirname "$dst")"
    if [[ -e "$dst" ]]; then
        if [[ "$(stat -c '%i' "$src" 2>/dev/null || stat -f '%i' "$src")" == \
              "$(stat -c '%i' "$dst" 2>/dev/null || stat -f '%i' "$dst")" ]]; then
            skip "$(basename "$dst")"
            return
        fi
        rm "$dst"
    fi
    ln "$src" "$dst"
    ok "$(basename "$dst")"
}

# dir_symlink SRC DST
# Creates a directory symlink DST → SRC. Skips if already correct.
dir_symlink() {
    local src="$1" dst="$2"
    mkdir -p "$(dirname "$dst")"
    if [[ -L "$dst" ]]; then
        if [[ "$(readlink "$dst")" == "$src" ]]; then
            skip "$dst"
            return
        fi
        rm "$dst"
    elif [[ -e "$dst" ]]; then
        warn "$dst exists and is not a symlink — skipping. Remove it manually and re-run."
        return
    fi
    ln -s "$src" "$dst"
    ok "$dst"
}

# ── 1. Cline: agents ──────────────────────────────────────────────────────────
step "Cline — agents"
dir_symlink "$AI_ROOT/.agents" "$HOME/.agents"

# ── 2. Cline: rules ───────────────────────────────────────────────────────────
step "Cline — rules"
hard_link "$RULES/globalRules.md"       "$HOME/Documents/Cline/Rules/globalRules.md"
hard_link "$RULES/globalCSharpRules.md" "$HOME/Documents/Cline/Rules/globalCSharpRules.md"
hard_link "$RULES/globalReactRules.md"  "$HOME/Documents/Cline/Rules/globalReactRules.md"

# ── 3. Cline: hooks ───────────────────────────────────────────────────────────
step "Cline — hooks"
hard_link "$HOOKS/PostToolUse"  "$HOME/Documents/Cline/Hooks/PostToolUse"
hard_link "$HOOKS/TaskComplete" "$HOME/Documents/Cline/Hooks/TaskComplete"

# ── 4. Cline: workflows ───────────────────────────────────────────────────────
step "Cline — workflows"
for wf in "$WORKFLOWS"/*.md; do
    hard_link "$wf" "$HOME/Documents/Cline/Workflows/$(basename "$wf")"
done

# ── 5. Copilot: global hooks (all workspaces) ─────────────────────────────────
step "Copilot — ~/.copilot/hooks (global)"
dir_symlink "$CLAUDE_HOOKS" "$HOME/.copilot/hooks"
# hooks.json must always be up to date regardless of whether the hooks dir is a
# symlink or a real directory — hard-link it explicitly.
mkdir -p "$HOME/.copilot/hooks"
hard_link "$CLAUDE_HOOKS/hooks.json" "$HOME/.copilot/hooks/hooks.json"

# ── 6. Copilot: global prompts (slash commands + instruction files) ──────────
# Both .prompt.md and .instructions.md live in the same global folder.
# .instructions.md files are auto-loaded in every workspace.
# .prompt.md files register as /cx-* slash commands in Copilot Chat globally.
# Neither belongs in the repo — the global folder is user-local.
step "Copilot — global prompts + instructions ($COPILOT_PROMPTS)"
for wf in "$WORKFLOWS"/*.md; do
    name="$(basename "${wf%.md}")"
    hard_link "$wf" "$COPILOT_PROMPTS/${name}.prompt.md"
done
hard_link "$RULES/globalRules.md"       "$COPILOT_PROMPTS/cx-globalRules.instructions.md"
hard_link "$RULES/globalCSharpRules.md" "$COPILOT_PROMPTS/cx-globalCSharpRules.instructions.md"
hard_link "$RULES/globalReactRules.md"  "$COPILOT_PROMPTS/cx-globalReactRules.instructions.md"
hard_link "$AI_ROOT/../AGENTS.md"       "$COPILOT_PROMPTS/cx-agents.instructions.md"

# ── Done ──────────────────────────────────────────────────────────────────────
echo ""
echo "✅  Setup complete. All AI config links are up to date."
echo "    Re-run this script any time you pull new rules or switch machines."
