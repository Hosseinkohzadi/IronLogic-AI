---
applyTo: "**/*.cs,**/*.csproj"
description: "CX C# coding invariants"
---
# CX C# Rules

For full guidance when writing, reviewing, or refactoring C# code, activate skill `cx-csharp-standards`.

## Invariants — Never Do

- Never create raw `JsonSerializerOptions` or use Newtonsoft directly — always use `CXJsonSerializerOptions.Default` / `Compact` / `Legacy`.
- Never customize `JsonSerializerOptions` for internal communications — use the standard options only.
- Never call `.ToString()` on a `BinaryData` Service Bus message before deserializing.
- Never serialize to a string for HTTP — use `PostAsJsonAsync` / `ReadFromJsonAsync`.
- Never use `new HttpClient()` or `HttpClientHelper` — always use `IHttpClientFactory` with named clients.
- Never use `LogHelper.Instance` or interpolated strings in log templates.
- Never catch `Exception` or `SystemException` broadly (except in `BaseEventListener<T>` — intentional).
- Never manually log exceptions — `CXTopLevelMiddleware` handles it.
- Never suppress CA2016.
- Never use `.ToLower()` / `.ToUpper()` for string comparison — use `StringComparison.OrdinalIgnoreCase`.
- Never use both `IsNullOrWhiteSpace` and `IsNullOrEmpty` — `IsNullOrWhiteSpace` covers both.
- Never use `#region`.
- Never use primary constructors on `class` types — only acceptable on `record` types.
- Never put business logic in controllers — thin controllers only.
- Never call `.Result`, `.Wait()`, or `.GetAwaiter().GetResult()` on async methods — always `await`. Blocking on async in an ASP.NET context causes thread-pool starvation and deadlocks.
- Never expose `IQueryable<T>` outside the data layer.
- Never perform DB operations in loops — pre-fetch all required data with a single batch query (e.g. `WHERE Id IN (...)`) and operate on in-memory collections. When this pattern is detected, flag it as “N+1 detected” and propose the batch replacement.
- Never return `null` for collection return types — return empty collection.
- Never use `.IsSuccessStatusCode` on HTTP responses — use `CheckSuccessfulResponseAsync()`.

## Invariants — Never Do (Tests)

- Never use MSTest or NUnit for new test projects — xUnit only.
- Never mock `DbContext` directly — use in-memory EF context.
- Never implement an integration test without first proposing a test outline and getting user approval.
- Never use `#pragma warning disable` in test files.

## Invariants — Always Do (Tests)

- Unit tests: xUnit (`[Fact]` / `[Theory]` + `[InlineData]`) + Moq (`Mock<T>`, `.Setup()`, `.Verify()`).
- Test data: use `FakeDataGenerator` from `CX.Core.Fake` where available.
- Inject `ITestOutputHelper` via constructor for logging in tests; use `XunitTestOutputHelperShim` from `CX.Core.Testing`.
- Use `IClassFixture<T>` for shared fixture state across tests in a class.
- Integration tests: extend `IntegrationTestsBase` from `CX.Core.Testing`; use `WebApplicationFactory`.
- Arrange / Act / Assert pattern with blank line separators between each section.
- Test class naming: `{Subject}Tests`. Unit method naming: `{MethodName}_{Scenario}_{ExpectedResult}`.

## Invariants — Always Do

- `CancellationToken` as last parameter in all async methods; required on controllers, optional on public service methods, required on private methods.
- All I/O must be async. No I/O in constructors. Propagate token through full call chain.
- `IReadOnlyList<T>` or `ICollection<T>` for service method returns — never `List<T>`; avoid `IEnumerable<T>` directly (deferred execution pitfalls).
- `record` preferred for immutable DTOs and value objects.
- `[JsonPropertyName]` acceptable only to map a property when the external/legacy JSON key differs from the C# standard name (e.g., `"dejn"` → `JobNumber`). Not for changing case style.
- `StringComparer.OrdinalIgnoreCase` for `IEqualityComparer<T>` overloads (e.g., dictionary keys, `HashSet<string>`).
- File-scoped namespaces: `namespace CX.Product;`
- `ArgumentNullException.ThrowIfNull()` for null guards.
- All public members, models, DTOs, and interfaces must have XML `<summary>` doc comments.
- Implementations use `<inheritdoc/>` — do not duplicate interface documentation.
- Treat all warnings as errors regardless of `/warnaserror` setting.
- `[ApiController]` + data annotation validation — do not replicate validation manually.
- All service and repository classes must have a corresponding interface in a separate file.
- **300-line hard limit per file.** If a file approaches this ceiling it is a signal that responsibilities are mixed. Split as follows:
  - **Controllers**: extract all logic into a service — the controller must contain only route binding, model validation (via `[ApiController]`), and a single `await _service.MethodAsync(...)` call. A controller near 300 lines means multiple bounded concerns are present; split into multiple controllers by resource/feature, each backed by its own service.
  - **Services**: when a service grows large, identify distinct concerns (e.g. orchestration vs. persistence vs. mapping vs. validation) and extract each into its own focused service or helper. The orchestrating service depends on the focused ones via DI — it does not inline their logic.
  - **Repositories**: if a repository exceeds the limit, the query surface is too broad. Split by aggregate root or feature sub-domain (e.g. `IUserQueryRepository` / `IUserCommandRepository`). Never add business logic to a repository.
  - **Mapping**: never inline mapping logic in services or controllers. Extract to a static mapper class or `IMapper<TSource, TDest>` implementation.
  - **Validators**: validation logic that grows beyond simple data annotations belongs in a dedicated `{Model}Validator` class — not in the service or controller.
  - When splitting, each new file must have a single, named responsibility that you can state in one sentence. If you cannot, split further.