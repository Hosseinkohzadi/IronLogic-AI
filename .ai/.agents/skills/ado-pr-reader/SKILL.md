---
name: ado-pr-reader
description: Read and analyze Azure DevOps pull request review comments. Activate when the user asks to read, review, list, or address comments/feedback on a pull request in Azure DevOps (ADO). Retrieves reviewer threads via the ADO REST API using az CLI Bearer token (preferred) or git credential PAT (fallback) — no credential is ever hardcoded.
---

# Azure DevOps PR Reader

Retrieves all reviewer comment threads for an Azure DevOps pull request and outputs them in a structured, actionable format.

## Security — Credential Handling

**Never hardcode, echo, or log any credential.** Always retrieve at runtime. Credentials are stored only in shell variables and used only in the `Authorization` header.

Prefer `az` CLI Bearer token (works with GCM/Entra OAuth setups). Fall back to `git credential fill` PAT only if `az` is unavailable or not logged in.

```bash
# Prefer az CLI Bearer token; fall back to git credential PAT
TOKEN=$(az account get-access-token \
  --resource 499b84ac-1321-427f-aa17-267ca6975798 \
  --query accessToken -o tsv 2>/dev/null)

if [ -n "$TOKEN" ]; then
  AUTH_HEADER="Authorization: Bearer $TOKEN"
else
  PAT=$(printf 'protocol=https\nhost=dev.azure.com\n\n' \
    | git credential fill 2>/dev/null \
    | grep '^password=' | cut -d= -f2-)
  if [ -z "$PAT" ]; then
    echo "ERROR: No credential found. Run 'az login' or configure git credential store for dev.azure.com."
    exit 1
  fi
  AUTH_HEADER="Authorization: Basic $(printf ':%s' "$PAT" | base64 | tr -d '\n')"
fi
# $TOKEN, $PAT, and $AUTH_HEADER are never echoed or logged
```

## Prerequisites

- `az` CLI installed and logged in (`az login`) — preferred auth method
- `git` with credential helper configured (macOS keychain by default) — fallback auth
- `curl` available on PATH
- `jq` available on PATH
- ADO organization: `convergys-cx`, project: `CX`, repository: `CX`

## Step 1 — Resolve the PR number

The user may provide:
- A full PR URL: `https://dev.azure.com/convergys-cx/CX/_git/cx/pullrequest/21340` → extract the trailing number
- A bare PR number: `21340`

Store as `PR_ID`.

## Step 2 — Fetch PR threads

Resolve `$AUTH_HEADER` using the credential handling block from the Security section above, then:

```bash
curl -s \
  -H "$AUTH_HEADER" \
  "https://dev.azure.com/convergys-cx/CX/_apis/git/repositories/CX/pullRequests/${PR_ID}/threads?api-version=7.1"
```

Before piping through `jq`, check that the response contains threads:

```bash
RESPONSE=$(curl -s -H "$AUTH_HEADER" "https://...")
THREAD_COUNT=$(echo "$RESPONSE" | jq '.value | length')
if [ "$THREAD_COUNT" -eq 0 ]; then
  echo "No reviewer comment threads found on PR #${PR_ID}."
  exit 0
fi
```

Pipe through `jq` to extract structured data:

```bash
... | jq '[
  .value[]
  | select(.isDeleted != true)
  | {
      id: .id,
      file: (.threadContext?.filePath // null),
      line: (.threadContext?.rightFileStart?.line // null),
      comments: [
        .comments[]
        | select(.commentType != "system")
        | { author: .author.displayName, content: .content }
      ]
    }
  | select(.comments | length > 0)
]'
```

## Step 3 — Interpret the output

Each thread object contains:
- `id` — thread ID (for reference)
- `file` — repository-relative file path (e.g. `/CX.Phanes/CX.Phanes.Common/BaseEventListener.cs`), or `null` for PR-level comments
- `line` — line number in the right (new) file
- `comments` — array of `{ author, content }` objects; the first comment is the original, subsequent ones are replies

**Severity convention** used in this repo:
| Emoji | Meaning |
|-------|---------|
| 🔴 | Blocker — must fix before merge |
| 🟡 | Warning — should fix, user should review if pertinent to fix, if not stories have to be generated after review |
| 🟢 | Suggestion — nice to have |

Parse the severity by matching the **leading emoji only** — check that `content` starts with the exact severity emoji. Do not match emojis that appear mid-text. In jq:

```jq
if (.comments[0].content | startswith("\ud83d\udd34")) then "blocker"
elif (.comments[0].content | startswith("\ud83d\udfe1")) then "warning"
elif (.comments[0].content | startswith("\ud83d\udfe2")) then "suggestion"
else "none" end
```

## Step 4 — Produce an action plan

Group threads by file and severity. For each thread, produce a structured entry:

1. **Severity** — 🔴 Blocker / 🟡 Warning / 🟢 Suggestion / ⬜ None
2. **Location** — file path + line number (or "PR-level" if `file` is null)
3. **Comment** — full original comment text (reviewer name, content)
4. **Context** — fetch the code at `line` using `read_file` or `git diff` to understand what the reviewer is pointing at
5. **Proposed fix** — specific change required; for 🔴/🟡 describe exact code to change; for 🟢 describe the improvement

Present the plan with Blocker / Warning / Suggestion sections. Ask for approval before making any code changes.

## Error Handling

| Symptom | Cause | Fix |
|---------|-------|-----|
| Empty `$TOKEN` and empty `$PAT` | Not logged in to az CLI and no git credential stored | Run `az login`, then retry |
| HTTP 401 with Bearer token | az session expired | Run `az login` to refresh |
| HTTP 401 with Basic token | PAT expired or wrong scope | Regenerate PAT with Code (Read) scope and store via `git credential approve` |
| HTTP 404 | Wrong org/project/repo or PR ID | Verify the URL components |
| `jq` parse error | API returned HTML (redirect to sign-in) | Auth failed — check token/PAT |

## Notes

- `az repos pr thread list` does **not** exist in the currently installed `az devops` extension version — always use the `curl` + REST API approach above.
- The ADO REST API path uses the **repository name** (`CX`), not the project name, in the `repositories` segment.
- `api-version=7.1` is current as of 2026. Bump if you receive version deprecation warnings.