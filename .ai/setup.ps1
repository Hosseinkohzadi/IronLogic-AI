#Requires -Version 7
# AI Config Setup — Windows (PowerShell 7)
#
# Wires up Cline and GitHub Copilot to the shared AI config in .ai/.
# Safe to re-run: existing links are removed and recreated.
#
# Usage: run from the repo root
#   pwsh .ai/setup.ps1

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$RepoRoot  = (Get-Item "$PSScriptRoot\..").FullName
$AiRoot    = Join-Path $RepoRoot '.ai'
$Rules     = Join-Path $AiRoot 'clineSpecificGlobals\Rules'
$Hooks     = Join-Path $AiRoot 'clineSpecificGlobals\Hooks'
$Workflows = Join-Path $AiRoot 'clineSpecificGlobals\Workflows'
$ClaudeHooks = Join-Path $AiRoot 'claudeHooks'
$CopilotPrompts = Join-Path $env:APPDATA 'Code\User\prompts'

# ── Helpers ───────────────────────────────────────────────────────────────────
function Step([string]$Label) { Write-Host ""; Write-Host "── $Label ──" }
function Ok([string]$Name)   { Write-Host "  ✔  $Name" }
function Skip([string]$Name) { Write-Host "  –  $Name (already linked)" }
function Warn([string]$Msg)  { Write-Host "  ⚠  $Msg" -ForegroundColor Yellow }

# Hard-link DST → SRC. Skips if already linked to the same source. Replaces if stale.
function Set-HardLink([string]$Src, [string]$Dst) {
    $null = New-Item -ItemType Directory -Force -Path (Split-Path $Dst)
    if (Test-Path $Dst) {
        # fsutil hardlink list returns all paths sharing the same inode.
        # If $Dst already appears in $Src's hardlink list, they are the same file.
        $links = fsutil hardlink list $Src 2>$null
        $dstRel = $Dst -replace [regex]::Escape((Split-Path $Dst -Qualifier)), ''
        if ($links -and ($links | Where-Object { $_ -like "*$($dstRel)*" })) {
            Skip (Split-Path $Dst -Leaf)
            return
        }
        Remove-Item $Dst -Force
    }
    $null = New-Item -ItemType HardLink -Path $Dst -Target $Src
    Ok (Split-Path $Dst -Leaf)
}

# Directory symlink DST → SRC. Skips if already correct.
function Set-DirSymlink([string]$Src, [string]$Dst) {
    $null = New-Item -ItemType Directory -Force -Path (Split-Path $Dst)
    if (Test-Path $Dst -PathType Container) {
        $item = Get-Item $Dst -Force
        if ($item.LinkType -eq 'SymbolicLink' -and $item.Target -eq $Src) {
            Skip $Dst
            return
        }
        if ($item.LinkType -eq 'SymbolicLink') {
            $item.Delete()
        } else {
            Warn "$Dst exists and is not a symlink — skipping. Remove it manually and re-run."
            return
        }
    } elseif (Test-Path $Dst) {
        Remove-Item $Dst -Force
    }
    $null = New-Item -ItemType SymbolicLink -Path $Dst -Target $Src
    Ok $Dst
}

# ── 1. Cline: agents ──────────────────────────────────────────────────────────
Step 'Cline — agents'
Set-DirSymlink (Join-Path $AiRoot '.agents') (Join-Path $env:USERPROFILE '.agents')

# ── 2. Cline: rules ───────────────────────────────────────────────────────────
Step 'Cline — rules'
Set-HardLink (Join-Path $Rules 'globalRules.md')       (Join-Path $env:USERPROFILE 'Documents\Cline\Rules\globalRules.md')
Set-HardLink (Join-Path $Rules 'globalCSharpRules.md') (Join-Path $env:USERPROFILE 'Documents\Cline\Rules\globalCSharpRules.md')
Set-HardLink (Join-Path $Rules 'globalReactRules.md')  (Join-Path $env:USERPROFILE 'Documents\Cline\Rules\globalReactRules.md')

# ── 3. Cline: hooks ───────────────────────────────────────────────────────────
Step 'Cline — hooks'
Set-HardLink (Join-Path $Hooks 'PostToolUse')  (Join-Path $env:USERPROFILE 'Documents\Cline\Hooks\PostToolUse')
Set-HardLink (Join-Path $Hooks 'TaskComplete') (Join-Path $env:USERPROFILE 'Documents\Cline\Hooks\TaskComplete')

# ── 4. Cline: workflows ───────────────────────────────────────────────────────
Step 'Cline — workflows'
Get-ChildItem (Join-Path $Workflows '*.md') | ForEach-Object {
    Set-HardLink $_.FullName (Join-Path $env:USERPROFILE "Documents\Cline\Workflows\$($_.Name)")
}

# ── 5. Copilot: global hooks (all workspaces) ─────────────────────────────────
Step 'Copilot — ~/.copilot/hooks (global)'
Set-DirSymlink $ClaudeHooks (Join-Path $env:USERPROFILE '.copilot\hooks')
# hooks.json must always be up to date regardless of whether the hooks dir is a
# symlink or a real directory — hard-link it explicitly.
$null = New-Item -ItemType Directory -Force -Path (Join-Path $env:USERPROFILE '.copilot\hooks')
Set-HardLink (Join-Path $ClaudeHooks 'hooks.json') (Join-Path $env:USERPROFILE '.copilot\hooks\hooks.json')

# ── 6. Copilot: global prompts (slash commands + instruction files) ──────────
# Both .prompt.md and .instructions.md live in the same global folder.
# .instructions.md files are auto-loaded in every workspace.
# .prompt.md files register as /cx-* slash commands in Copilot Chat globally.
# Neither belongs in the repo — the global folder is user-local.
Step "Copilot — global prompts + instructions ($CopilotPrompts)"
$null = New-Item -ItemType Directory -Force -Path $CopilotPrompts
Get-ChildItem (Join-Path $Workflows '*.md') | ForEach-Object {
    $name = $_.BaseName
    Set-HardLink $_.FullName (Join-Path $CopilotPrompts "${name}.prompt.md")
}
Set-HardLink (Join-Path $Rules 'globalRules.md')       (Join-Path $CopilotPrompts 'cx-globalRules.instructions.md')
Set-HardLink (Join-Path $Rules 'globalCSharpRules.md') (Join-Path $CopilotPrompts 'cx-globalCSharpRules.instructions.md')
Set-HardLink (Join-Path $Rules 'globalReactRules.md')  (Join-Path $CopilotPrompts 'cx-globalReactRules.instructions.md')
Set-HardLink (Join-Path $AiRoot '..' 'AGENTS.md')      (Join-Path $CopilotPrompts 'cx-agents.instructions.md')

# ── Done ──────────────────────────────────────────────────────────────────────
Write-Host ""
Write-Host "✅  Setup complete. All AI config links are up to date." -ForegroundColor Green
Write-Host "    Re-run this script any time you pull new rules or switch machines."
