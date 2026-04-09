#!/usr/bin/env node

// PostToolUse Hook — GitHub Copilot
// Migrated from .ai/clineSpecificGlobals/Hooks/PostToolUse
//
// Fires after every tool use. Injects a compliance checklist into Copilot context
// after writing or modifying a .cs, .ts/.tsx/.js/.jsx, .css/.scss, Dockerfile, or pipeline YAML file.
//
// Copilot input:  { tool_name, tool_input: { filePath, replacements }, tool_response, ... }
// Copilot output: { hookSpecificOutput: { hookEventName, additionalContext } }
//
// Tool name mapping from Cline → Copilot:
//   write_to_file    → create_file
//   replace_in_file  → replace_string_in_file | multi_replace_string_in_file

const EDIT_TOOLS = new Set([
    "create_file",
    "replace_string_in_file",
    "multi_replace_string_in_file",
]);

const chunks = [];
process.stdin.on("data", (d) => chunks.push(d));
process.stdin.on("end", () => {
    try {
        const input = JSON.parse(Buffer.concat(chunks).toString());
        const toolName = input.tool_name ?? "";

        if (!EDIT_TOOLS.has(toolName)) {
            process.stdout.write(JSON.stringify({}));
            return;
        }

        // Skip if the tool call itself failed
        if (input.tool_response?.isError === true) {
            process.stdout.write(JSON.stringify({}));
            return;
        }

        // Collect unique file paths — handles single-file and multi-replace tools
        const filePaths = [];
        if (input.tool_input?.filePath) {
            filePaths.push(input.tool_input.filePath);
        }
        if (Array.isArray(input.tool_input?.replacements)) {
            for (const r of input.tool_input.replacements) {
                if (r.filePath && !filePaths.includes(r.filePath)) {
                    filePaths.push(r.filePath);
                }
            }
        }

        if (filePaths.length === 0) {
            process.stdout.write(JSON.stringify({}));
            return;
        }

        const messages = [];

        for (const rawFilePath of filePaths) {
            // Normalize path separators for cross-platform regex matching
            const filePath = rawFilePath.replace(/\\/g, "/");

            // React / TypeScript
            if (
                /\.(tsx?|jsx?)$/.test(filePath) &&
                !filePath.endsWith(".d.ts") &&
                !/\.(test|spec)\.(tsx?|jsx?)$/.test(filePath)
            ) {
                const isApiOrDto =
                    /\/src\/api\//.test(filePath) ||
                    /(Dto|DTO|Request|Response)/.test(filePath);

                const items = [
                    "[React/TS Compliance Check] Standards for " +
                        filePath +
                        ":",
                    "(1) No bare `any`, `@ts-ignore`, or `@ts-expect-error` without documented justification?",
                    "(2) No async work or side effects in components/handlers — all side effects go in sagas?",
                    "(3) API calls through `src/api/` using `request` from `@cx/login` — no raw fetch/axios?",
                    "(4) Hook names start with `use` followed by a capital letter?",
                    "(5) Utils are pure functions — no `useState`, `useEffect`, or React hooks inside utils?",
                    "(6) Presentational components have no store, saga, or `src/api/` imports?",
                    ...(/\/components\/[^/]+\/index\.tsx?$/.test(filePath)
                        ? [
                              "(7) Smart component (index.tsx) — only minimal wrapper markup? No business logic in JSX? No direct store/saga imports in the presentational layer?",
                          ]
                        : []),
                    "(8) All CSS scoped — no bare HTML tag selectors, no global style leaks?",
                    "(9) All props interfaces/types have JSDoc `/** */` documentation on every property?",
                    "(10) Run `npm run lint` (ESLint fix + `tsc --noEmit`)?",
                    "(11) Run `npm run test` after any logic change — no regressions?",
                ];

                if (isApiOrDto) {
                    items.push(
                        "--- C# API Contract (API/DTO file detected) ---",
                        "(C1) camelCase props for Default/Compact endpoints (`CXJsonSerializerOptions.Default` / `Compact`), PascalCase for Legacy — documented on the interface?",
                        "(C2) Nullable C# fields: `T | null` not `T | undefined` — arrays never `T[] | null`?",
                        "(C3) Enums: PascalCase string literals matching C# member names — includes zero-member (Unknown/None) first?",
                        "(C4) DateTime fields typed as `string` — not `Date`?",
                        "(C5) DTO name mirrors C# (DTO suffix ALL_CAPS, closed-form compounds match)?",
                    );
                }

                messages.push(items.join("\n"));
            }

            // C#
            if (filePath.endsWith(".cs")) {
                messages.push(
                    [
                        "[C# Compliance Check] Standards for " + filePath + ":",
                        "(1) XML <summary> on all new public members?",
                        "(2) CancellationToken in all async methods?",
                        "(3) No broad Exception catch (except BaseEventListener — intentional)?",
                        "(4) CXJsonSerializerOptions.Default or Compact (camelCase) for standard endpoints; Legacy (PascalCase) for legacy endpoints — no raw/custom JsonSerializerOptions for internal comms?",
                        "(5) BinaryData.ToObjectFromJson<T>() for Service Bus — never .ToString() first?",
                        "(6) IHttpClientFactory only — no new HttpClient(), no HttpClientHelper?",
                        "(7) No #region directives?",
                        "(8) No [JsonPropertyName] to change casing style — only to map a differing external JSON key?",
                        "(9) No null checks on collection returns — collections never return null?",
                        "(10) ArgumentNullException.ThrowIfNull() for null guards — no manual null checks?",
                        "(11) StringComparison.OrdinalIgnoreCase — no .ToLower()/.ToUpper() for string comparison?",
                        "(12) IReadOnlyList<T> or ICollection<T> for returns — never List<T> or bare IEnumerable<T>?",
                        "(13) record for new immutable DTOs/value objects?",
                        "(14) Fix all compiler warnings introduced by this change.",
                    ].join("\n"),
                );
            }

            // .csproj — focused project-file checks (different from .cs source checks)
            if (filePath.endsWith(".csproj")) {
                messages.push(
                    [
                        "[.csproj Compliance Check] Standards for " +
                            filePath +
                            ":",
                        "(1) No <PackageReference> with wildcard or floating versions (e.g. *, [1.0,)) — pin exact versions?",
                        "(2) No hardcoded secrets, tokens, or environment-specific values in <DefineConstants> or <RuntimeHostConfigurationOption>?",
                        "(3) <TreatWarningsAsErrors> or <WarningsAsErrors> not removed or weakened?",
                        "(4) <TargetFramework> not downgraded — matches the rest of the project's net version?",
                    ].join("\n"),
                );
            }

            // Dockerfile / Pipeline / Helm
            if (
                /(Dockerfile|azure-pipelines.*\.yml|helm\/values.*\.yaml)/.test(
                    filePath,
                )
            ) {
                messages.push(
                    [
                        "[Dockerfile/Pipeline Compliance Check] Standards for " +
                            filePath +
                            ":",
                        "(1) Build stage: FROM concentrixcx.azurecr.io/cx/dotnet-sdk — not the public mcr.microsoft.com image?",
                        "(2) Runtime stage: FROM concentrixcx.azurecr.io/cx/dotnet-aspnet — not the public mcr.microsoft.com image?",
                        "(3) --warnaserror present in BUILD_ARGS?",
                        "(4) Tests run in the build stage before dotnet build?",
                        "(5) No hardcoded secrets or credentials — secrets via --mount=type=secret only?",
                        "(6) Pipeline uses base.v3.yaml — not base.v2.yaml or v1?",
                        "(7) New service uses 10.0 tags — existing service matches its current version?",
                        "(8) No 6.0 tags on new or modified services?",
                        "(9) Helm values files exist for all environments (values.yaml, Development, UAT, Production)?",
                        "(10) Trigger paths.include scoped to only this project's directories?",
                    ].join("\n"),
                );
            }

            // CSS / SCSS / Less / Sass
            if (/\.(css|scss|less|sass)$/.test(filePath)) {
                messages.push(
                    [
                        "[CSS/SCSS Compliance Check] Standards for " +
                            filePath +
                            ":",
                        "(1) No bare HTML tag selectors (e.g. div {}, span {}) — scope to component class prefix?",
                        "(2) No `!important` without an inline comment explaining why?",
                        "(3) No hardcoded hex/rgb/hsl color values — use CSS custom properties (var(--cx-…)) or theme tokens?",
                        "(4) No hardcoded px spacing/sizing values that should map to a design token?",
                        "(5) .module.* files: no :global selectors without documented justification?",
                        "(6) No duplicate property declarations in the same selector block?",
                        "(7) No vendor-prefixed properties now covered by standards (remove stale -webkit-/-moz-)?",
                    ].join("\n"),
                );
            }
        }

        if (messages.length === 0) {
            process.stdout.write(JSON.stringify({}));
            return;
        }

        process.stdout.write(
            JSON.stringify({
                hookSpecificOutput: {
                    hookEventName: "PostToolUse",
                    additionalContext: messages.join("\n\n"),
                },
            }),
        );
    } catch {
        process.stdout.write(JSON.stringify({}));
    }
});
