---
agent: agent
description: "Holistic end-of-task code review — runs tests, checks CX compliance rules for C#/React/CSS/pipelines, cleans up pragmas, and produces a structured findings summary."
---
# cx-code-review

Trigger: After all code changes are complete for a task. Also available on-demand.

> **Note for AI agents:** This workflow uses **Plan mode** and **Act mode** terminology. In GitHub Copilot **Agent mode**, follow **Act mode** instructions — apply changes directly. In GitHub Copilot **Plan mode** (or any read-only/proposal mode), follow **Plan mode** instructions — propose changes without applying them.

Holistic end-of-task review. Run after all code changes are complete.

---

## Step 1 — Scope check

Collect changed files: `.cs`, `.csproj`, `.ts`, `.tsx`, `.js`, `.jsx`, `.css`, `.scss`, `.less`, `.sass`, `.html`, `.htm`, `.cshtml`, `.json` — skip `.d.ts`, `.test.*`, `.spec.*`.

- **≤ 15 files**: full review — run all steps.
- **> 15 files**: large-change mode — run tests (step 2), sample 5 representative files for steps 3–4, skip pragma cleanup (step 5), note in the summary that a full review is recommended as a separate task.

---

## Step 2 — Run existing tests

**Plan mode**: read `package.json` / locate `{ProjectName}.Tests/` — report exact commands only, do not execute.
**Act mode**: use declared scripts only — `npm run <test-script>` / `dotnet test <path>`. Never invoke `jest`, `tsc`, or `eslint` directly.

On failure: fix production code that caused the regression. Do not create or modify test files. Re-run until green. Surface to user if fix is ambiguous.

---

## Step 3 — Compliance + code smell review

Spawn subagents in parallel (≤ 5 at a time, 1 file per subagent). Each prompt must be fully self-contained and include the file path, its full content, and the instruction below. If a file is missing or unreadable, note it and continue. Request a structured findings list — not a verbatim copy of the file.

**C# files**: check all invariants in `globalCSharpRules.md` (XML docs, CancellationToken, exception handling, JSON serialization, Service Bus, HttpClient, `#region`, `JsonPropertyName`, `IsNullOrWhiteSpace`, null collections, primary constructors, return types, controller thickness, null guards, logging, string comparison, compiler warnings).

**React/TS files**: check all invariants in `globalReactRules.md` (bare `any`/ignore pragmas, side effects in components, API patterns, hook naming, pure utils, presentational isolation, smart component purity, CSS scoping, JSDoc props).

**CSS/SCSS files** (`.css`, `.scss`, `.less`, `.sass`):
- No bare HTML tag selectors (e.g., `div {}`, `span {}`) — scope to component class prefix.
- No `!important` without an inline comment explaining why.
- No hardcoded hex/rgb/hsl color values — use CSS custom properties (`var(--cx-…)`) or theme tokens.
- No hardcoded px spacing/sizing values that should map to a design token.
- `.module.*` files: no `:global` selectors without documented justification.
- No duplicate property declarations within the same selector block.
- No vendor-prefixed properties that are now standard (remove stale `-webkit-`/`-moz-` prefixes).

**Legacy HTML files** (`.html`, `.htm`, `.cshtml`):
- No inline `style` attributes.
- No deprecated presentational attributes (`bgcolor`, `align`, `border` on non-table elements, `cellpadding`, `cellspacing`).
- No commented-out HTML blocks.
- `.cshtml` Razor views: thin templates only — no business logic; delegate to controller/service.

**JSON config files** (`.json` — modified files only):
- No hardcoded secrets, connection strings, tokens, or passwords.
- `package.json`: no devDependency accidentally listed under `dependencies`; no new packages added without approval.
- `tsconfig.json`: no weakening of strict mode settings — `strict`, `noImplicitAny`, `strictNullChecks` must not be disabled.
- `appsettings*.json` / `appsettings.*.json`: no credentials, tokens, or PII in any value.

**`.csproj` project files** (`.csproj` — modified files only):
- No `<PackageReference>` with wildcard or floating versions (`*`, `[1.0,)`) — pin exact versions.
- No hardcoded secrets, tokens, or environment-specific values in `<DefineConstants>` or `<RuntimeHostConfigurationOption>`.
- `<TreatWarningsAsErrors>` / `<WarningsAsErrors>` not removed or weakened.
- `<TargetFramework>` not downgraded — must match the rest of the project's .NET version.

**Legacy inline script extraction** (`.cshtml` only — modified files only):

Count `.cshtml` files in the changed set that contain at least one `<script>` block with logic.

- **> 3 such files**: large legacy change — flag all occurrences in the summary, do not attempt extraction. Recommend a dedicated legacy cleanup task.
- **≤ 3 such files**:
  - **Plan mode**: flag each occurrence explicitly to the user —
    > ⚠️ Legacy violation: `{file}` contains an inline `<script>` block. This must be extracted to a separate `.js` file. Review with the developer before any automated change is applied.
    Do not extract — surface for discussion only.
  - **Act mode**: before touching each file, present the file name and the inline script content to the user and ask for explicit confirmation: *"This file contains an inline `<script>` block. Extract to a separate `.js` file?"* Proceed only if confirmed. Do not batch-confirm across multiple files.

**All files** — code smell scan:
- Unused imports, dead variables, unreachable code.
- Commented-out code blocks anywhere in the file.
- Magic strings or numbers without named constants.
- Methods or functions exceeding ~40 lines.
- Obvious duplicate logic within the file.

Batch remaining files in groups of 5 until all are reviewed. Collect and deduplicate all findings.

---

## Step 4 — Pragma + stale-comment cleanup *(full review mode only)*

For each changed file, if the file contains any of the following, remove them:

- `// ReSharper disable` / `// ReSharper restore` without a tracked issue reference.
- `#pragma warning disable` / `#pragma warning restore` without a tracked issue reference.
- Contiguous blocks of commented-out code.
- Stale `// TODO:` / `// HACK:` / `// FIXME:` with no linked issue.

**GitHub Copilot / Act mode**: apply removals directly.
**Plan mode**: present each as a diff suggestion.

---

## Step 5 — Summary

| File | Tests | Compliance | Smells | Status |
|------|-------|------------|--------|--------|
| `…` | pass / fail / n/a | count | count | ✅ / ⚠️ / ❌ |

If large-change mode: note files not reviewed and recommend a dedicated review task.

---

## Step 6 — Fix violations

**Act mode**: apply all fixes. Run `npm run <lint-script>` and `npm run <type-check-script>` per `package.json`. Re-run tests to confirm green. If a fix has multiple valid approaches, present the concrete options and wait for user selection — do not speculatively choose one.
**Plan mode**: present fixes as numbered code blocks. Warnings, type errors, and lint errors are always written directly — never left as suggestions regardless of mode.

---

## Step 7 — Handoff

Present a completion summary to the user:

> **Code review complete.**
> - Tests: [passed / no test project found / commands reported for manual run]
> - Compliance issues found and resolved: [count]
> - Smells resolved: [count]
> - Warnings/errors fixed: [count]

Then assess test coverage for new code. If new public methods, components, services, or React components were added with no corresponding test coverage visible in the changed files, ask explicitly:

> New code was introduced in this task with no test coverage. Would you like me to run the `cx-generate-tests` workflow now to add unit tests?

Wait for the user's response. Do not start `cx-generate-tests` automatically. If the user confirms, run the workflow. If they decline or do not respond, close the review.
