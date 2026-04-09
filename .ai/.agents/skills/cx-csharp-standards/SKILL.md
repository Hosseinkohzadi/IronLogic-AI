---
name: cx-csharp-standards
description: Comprehensive CX C# coding standards. Activate when writing, reviewing, or refactoring C# code in any CX project.
---

# CX C# Coding Standards

## CX.Core

CX.Core is a solution of multiple NuGet packages that form the backbone of all CX backend services. It is not a single library. Check it before implementing any standard .NET pattern — middleware, serialization, HTTP, logging, error handling, and Azure service connectivity are all covered.

| Package | Purpose |
|---|---|
| `CX.Core.Api` | ASP.NET Core middleware, controller support, error handling, app startup |
| `CX.Core.Data` | EF Core, SQL, repository patterns |
| `CX.Core.ServiceBus` | Azure Service Bus: listeners, senders, message handling |
| `CX.Core.Caching` | Redis distributed cache |
| `CX.Core.Cosmos` | Azure Cosmos DB |
| `CX.Core.Storage` | Azure Blob/Table storage |
| `CX.Core.Testing` | XUnit test helpers |

### Source Navigation (on demand)

CX.Core source lives at `{cx-root}/CX.Core/` where `{cx-root}` is the nearest ancestor directory containing `.ai/` (see globalRules.md — CX Monorepo Root). Read source on demand when the task involves CX.Core-specific patterns — do not load all of it upfront.

| Topic | Read |
|---|---|
| Overall package map | `{cx-root}/CX.Core/README.md` |
| Middleware, error handling, app startup | `{cx-root}/CX.Core/CX.Core.Api/README.md` |
| JSON serialization deep dive | `{cx-root}/CX.Core/Docs/JsonSerialization.md` |
| JSON standard naming | `{cx-root}/CX.Core/Docs/JsonStandardNaming.md` |
| Named HTTP clients | `{cx-root}/CX.Core/Docs/NamedHttpClients.md` |
| Configuration / options binding | `{cx-root}/CX.Core/Docs/Configuration.md` |
| Application lifecycle / startup | `{cx-root}/CX.Core/Docs/ApplicationLifecycle.md` |
| EF Core / SQL patterns | `{cx-root}/CX.Core/CX.Core.Data/README.md` |
| Azure Service Bus | `{cx-root}/CX.Core/CX.Core.ServiceBus/README.md` |
| Redis caching | `{cx-root}/CX.Core/CX.Core.Caching/README.md` |
| Cosmos DB | `{cx-root}/CX.Core/CX.Core.Cosmos/README.md` |
| Unit test helpers | `{cx-root}/CX.Core/CX.Core.Testing/README.md` |

## Async / Await
- All I/O (disk, HTTP, database) must be `async`. No synchronous I/O wrappers unless specifically required.
- No I/O in constructors — this is a design problem; discuss before proceeding.
- `CancellationToken` rules:
  - Controller endpoints: required, non-optional, last parameter. Pass through the entire call chain.
  - Public library methods: can be `= default` but must be used consistently within the app.
  - Private async methods: required (non-optional).
  - When intentionally not forwarding: pass `CancellationToken.None` or `default` explicitly.
- Never suppress CA2016 ("Forward the CancellationToken..."). Suppression is never acceptable.

## Exception Handling
- Catch only specific, intentional types (`ArgumentNullException`, `InvalidOperationException`, etc.).
- Never catch `Exception` or `SystemException` broadly — let `CXTopLevelMiddleware` handle and log it.
- Never manually log exceptions — `CXTopLevelMiddleware` provides consistent formatting.
- No `bool` success/failure returns unless method has the `Try` prefix (e.g., `TryParse`).
- Do not catch exceptions just to "wrap" or "hide" them.

## Naming
- Solutions: `CX.<Product>.sln` | Projects: `.Api` `.Web` `.Data` `.Service` `.Tests`
- Namespaces: plural noun, gerund (-ing), or adjective. Never singular nouns (conflict with class names).
- Classes and types: singular nouns. Methods: verbs.
- Enums: PascalCase members. Always include a `0`-valued member (`Unknown`, `None`). Avoid abbreviations in member names.
  - Use `[JsonPropertyName]` when the external/legacy string representation differs from the meaningful C# name.
  - Do not assign specific integer values unless necessary; document the reason if you do.
- Acronyms: 2-letter = ALL_CAPS (`IO`, `DB`). 3+ letters = PascalCase (`Api`, `Http`).
  - Exception: `DTO` remains ALL_CAPS for historical reasons.
- Closed-form compound words (do not split): `Datamart`, `Dataset`, `Workhub`, `Metadata`.
- `IOptions<T>` classes: `Options` suffix.

## Code Quality
- File-scoped namespaces: `namespace CX.Product;` (preferred; update whole solution in a dedicated PR).
- `ArgumentNullException.ThrowIfNull()` / `.ThrowIfNullOrEmpty()` for null/empty guards.
- String comparison: use `StringComparison.OrdinalIgnoreCase` for `.Equals()`/comparison overloads. Use `StringComparer.OrdinalIgnoreCase` for `IEqualityComparer` overloads. Never `.ToLower()` / `.ToUpper()`.
- `string.IsNullOrWhiteSpace()` covers the empty case — never pair it with `|| string.IsNullOrEmpty()`.
- Methods returning collection types (`List`, `Array`, `Dictionary`, `HashSet`) must never return `null`. Remove null-guard patterns on collection returns — use nullable reference types to express intent.
- Prefer C# 9+ pattern matching: `is { Count: > 0 }`, `is not { Count: > 0 }`, `is not null`.
- No copy/paste — extract to shared methods, base classes, or helpers.
- No `#region` directives. If a class needs regions to navigate, it does too much — split it.
- No primary constructors on `class` types. Only on `record`. (`IDE0290` is disabled in root `.editorconfig` — do not re-enable locally to suppress the warning.)
- Bind config to strongly-typed options classes during startup; never read from `IConfiguration` by key in application code.
- Remove unused `using` statements. Sort alphabetically with `System.*` first. Add common namespaces as global imports in `.csproj` or `Directory.Build.props`.
- Lines: break at ~130 characters. Do not align parameter lists broken across multiple lines.
- Extension methods on C# primitives (`string`, `int`, etc.): do not expose publicly in commonly-used namespaces. Make them non-extension static methods or place in a separate namespace.
- Feature flags: boolean flags use a verb (`UseConcentrixBranding`); filter-based flags append the filter type (`ShowSplashScreenPercent`).
- All compiler warnings must be fixed. Treat all warnings as errors regardless of `/warnaserror` setting. Fix warnings in any file you actively work on.
- `record` preferred for immutable DTOs and value objects.
- Service interfaces in a separate file from implementation.

## XML Documentation
- All public classes, interfaces, methods, and properties need `/// <summary>` — unless the purpose is unambiguously obvious from the name and signature.
- Private methods: add docs when logic is non-obvious or complex.
- `<param>`: only when the parameter's purpose is not clear from name and type alone.
- `<returns>`: optional if return info is already in `<summary>`.
- Remove auto-added empty tags (`<param>`, `<returns>`, `<exception>`) unless some params in the same method ARE documented (all or none, or blank for some will trigger compiler warnings).
- Implementations use `<inheritdoc/>` — never duplicate interface documentation.
- Do not overestimate how "clear" something is. Write from the perspective of a developer who is unfamiliar with the codebase.
- No `<example>` tags.

## Interfaces
- Every service and repository class must have a corresponding interface for testability.
- Interface file is separate from implementation file.
- Interface carries all documentation; implementation uses `<inheritdoc/>`.

## API & Controllers
- Controllers must be thin — no business logic. Extract to injectable service/manager/model classes.
- `[ApiController]` auto-validates model annotations — do not replicate validation manually in controllers.
- Validation belongs on the model via data annotations (`[Required]`, `[MaxLength]`, `[Range]`), `IValidatableObject`, or custom validation attributes.
- Return `ProblemDetails` with sanitized messages. Never expose stack traces or PII.
- Meaningful user-actionable error messages are permitted — keep them factual, safe, internal-detail-free.
- Do not throw custom exceptions for validation that data annotations already handle.

## Data Layer
- No DB operations in loops. Batch with `Where(...).Contains(...)` then one `SaveChangesAsync()`.
- Collect all `_dbContext.Add()`/`Update()` calls first, then call `SaveChangesAsync()` once.
- Never expose `IQueryable<T>` outside the data layer.
- Service methods return `IReadOnlyList<T>` or `ICollection<T>`, not `List<T>`; avoid `IEnumerable<T>` directly (deferred execution pitfalls).

## JSON & Serialization
- Use `CXJsonSerializerOptions` — never create raw options from scratch.
  - `Default`: standard for all API and internal communication. Based on `JsonSerializerDefaults.Web` + enums as strings + NaN allowed + case-insensitive deserialization.
  - `Compact`: same as `Default` with whitespace minimized. Use for storage (Cosmos, Redis) when readability isn't needed.
  - `Legacy`: PascalCase. Only for communicating with pre-.NET Core systems.
- For internal service-to-service communication: `Default` or `Compact` only. Never customize options for this use case.
- For Controller I/O: never customize `JsonSerializerOptions`. Never use `[JsonPropertyName]`, `[JsonConverter]`, etc. to change the casing/mode of controller input/output.
- `[JsonPropertyName]` IS acceptable to rename a property when the external/legacy name differs from the C# standard name (e.g., `dejn` → `JobNumber`). NOT for changing case style.
- Custom `JsonSerializerOptions` (for external systems only): must be a singleton. Copy from `Default` as base: `new JsonSerializerOptions(CXJsonSerializerOptions.Default) { ... }`. Must be documented explaining why it differs. CA1869 fires for local instances — treat as error.
- Use `CX.Core` compatibility shims for legacy Newtonsoft APIs. Never add Newtonsoft directly.

## HTTP Serialization (streams only)
- **HTTP out**: `client.PostAsJsonAsync(uri, obj, CXJsonSerializerOptions.Default, cancellationToken)`
- **HTTP in**: `response.Content.ReadFromJsonAsync<T>(CXJsonSerializerOptions.Default, cancellationToken)`
- Never serialize to a string then pass to `StringContent`. Never read response to string then deserialize.
- `CheckSuccessfulResponseAsync()` on all HTTP responses. Do not use `IsSuccessStatusCode` or throw custom exceptions.

## Service Bus Serialization
- **Deserialization**: `message.Body.ToObjectFromJson<T>(CXJsonSerializerOptions.Default)` — never call `.ToString()` on `BinaryData` first (doubles memory).

## Cosmos Serialization
- Deserialize directly from stream: `await JsonSerializer.DeserializeAsync<T>(msg.Content, CXJsonSerializerOptions.Default, cancellationToken)` — never convert stream to string.

## HTTP Client & Logging
- `IHttpClientFactory` with named clients only. Never `new HttpClient()`. Never use `HttpClientHelper`.
- `ILogger<T>` injected in constructor as `private readonly`. Never use `LogHelper.Instance`.
- Semantic logging only: `"Processing user {UserGuid}"`, `logger.LogInformation(...)`. Never C# interpolated strings in log templates.
- Never log PII, tokens, passwords, or credentials.

## AI-Generated Code
- AI-generated code must meet the exact same standards as hand-written code.
- No overly-verbose AI documentation — factual, minimal, purposeful.
- Proper unit tests required — no filler tests that don't assert meaningful behavior.
- You must be able to understand and explain all generated code before accepting it.