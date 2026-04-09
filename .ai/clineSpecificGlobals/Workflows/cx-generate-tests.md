---
agent: agent
description: "Generate or migrate unit tests — supports xUnit (C#) and Jest (React). Handles MSTest/NUnit migration, locates test projects, and writes tests to CX standards."
---
# cx-generate-tests

Generate or migrate unit tests. Run when new code needs coverage or existing tests need to be brought to standard.

> **Note for AI agents:** This workflow uses **Plan mode** and **Act mode** terminology. In agent/edit mode, follow **Act mode** instructions — apply changes directly. In read-only or proposal-only mode, follow **Plan mode** instructions — propose changes without applying them.

---

## Step 1 — Identify scope

Determine what needs tests from task context or user request. Ask if ambiguous.

Do not generate tests for:
- Pure refactors where all existing tests already pass and coverage is not reduced.
- Trivial wiring (e.g., DI registration, pass-through properties, auto-generated scaffolding).

Focus on: new public methods, new components, new sagas, new API functions, new business logic.

For each item in scope, capture the **business intent** — tests must state in plain language *why* the behavior matters, not just what it does. This intent drives test names, `<summary>` docs, and first-line comments in `it()` blocks.

---

## Step 2 — Locate test project / folder

- **C#**: `{ProjectName}.Tests/` alongside source. If missing, confirm with user before creating.
- **React**: `ClientApp/` inside the UI project. Confirm path with user if it cannot be inferred.

---

## Step 3 — C# framework check

Read one existing test file. Branch:

| Found | Action |
|---|---|
| xUnit | Skip to step 5 |
| MSTest / NUnit | Step 4 |
| None | Write xUnit from scratch, skip step 4 |

**.NET 10+ projects** (`<TargetFramework>net10.0</TargetFramework>` or higher): `CX.Core` is the base library and dictates the test package ecosystem. Do **not** hardcode versions from memory. Instead:
1. Read the source project's `.csproj` and find the `CX.Core` (or `CX.Core.*`) `<PackageReference>` version — this is the authoritative version signal.
2. Open the test project's `.csproj` (if it exists) and verify `CX.Core.Testing` matches that same major version. All companion packages (`Microsoft.NET.Test.Sdk`, `xunit`) must align with the `CX.Core.Testing` version present.
3. If no test project exists yet, ask the user to confirm the `CX.Core` version before creating the project and adding packages.

Note: newer xUnit major versions may have different annotation names from older ones — verify attribute names against the version present in the project before generating test classes.

---

## Step 4 — Migration assessment (C# only)

Map attributes: `[TestMethod]`→`[Fact]`, `[DataTestMethod]+[DataRow]`→`[Theory]+[InlineData]`, `[TestInitialize]`→constructor, `[TestCleanup]`→`IDisposable.Dispose()`. NUnit: `[Test]`→`[Fact]`, `[TestCase]`→`[Theory]+[InlineData]`, `[SetUp]`→constructor, `[TearDown]`→`IDisposable.Dispose()`.

Flag **unsafe** (suggest only — no auto-migrate) if: in-memory DB with complex multi-test state, multi-level fixture inheritance, or no direct xUnit equivalent. Present plan to user; wait for per-file confirmation.

---

## Step 5 — React setup check

1. Read `jest.config.ts` — note `moduleNameMapper`, `moduleDirectories`, `setupFilesAfterEnv`, custom `test-utils` render wrapper.
2. Read `tsconfig.test.json` — verify `"noImplicitAny": false` in `compilerOptions`. If missing, flag and suggest adding it (tests need `any`/`unknown`/`null` freedom).
3. Read `package.json` — identify test script name. Always run via `npm run <script>` — never `jest` directly.

---

## Step 6 — Write tests

Spawn subagents for parallel test writing where multiple independent files are involved (≤ 5 at a time). Each prompt must be fully self-contained with file path, source content, and the rules below.

### C# unit tests

- xUnit: `[Fact]` / `[Theory]`+`[InlineData]`. Moq: `Mock<T>`, `.Setup().Returns()/.ReturnsAsync()`, `.Verify()`, `It.IsAny<T>()`.
- Test data: `FakeDataGenerator` from `CX.Core.Fake`. EF: in-memory context — never mock `DbContext`.
- Inject `ITestOutputHelper` via constructor; use `XunitTestOutputHelperShim` from `CX.Core.Testing`.
- `IClassFixture<T>` for shared state. Cleanup via `IDisposable.Dispose()`.
- No `#pragma warning disable`. Class: `{Subject}Tests`. Method: `{MethodName}_{Scenario}_{ExpectedResult}`.
- XML `<summary>` on every method — intent in plain language; method name alone is not sufficient.
- Single-flow: Arrange / Act / Assert with blank line separators.
- Multi-step flow: `// Step N —` inline comments instead of Arrange/Act/Assert.

```csharp
/// <summary>
/// Verifies GetUserAsync returns a mapped DTO when the user exists in the repository.
/// </summary>
[Fact]
public async Task GetUserAsync_UserExists_ReturnsDto()
{
    // Arrange
    var userId = FakeDataGenerator.NewGuid();
    _repoMock.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
             .ReturnsAsync(new User { Id = userId });

    // Act
    var result = await _sut.GetUserAsync(userId, CancellationToken.None);

    // Assert
    Assert.NotNull(result);
    Assert.Equal(userId, result.Id);
}
```

### C# integration tests

Always propose first: present class name, method names, and what is being tested. Get user approval before writing. Once approved: extend `IntegrationTestsBase` from `CX.Core.Testing`, use `WebApplicationFactory`, inject `ITestOutputHelper`.

### React tests

- `@testing-library/react` + `@testing-library/user-event`. Use custom `test-utils` wrapper if present — never raw `render`.
- `jest.mock()` for all external modules. `beforeEach(() => jest.clearAllMocks())` + `afterEach(() => cleanup())`.
- `any`/`unknown`/`null` permitted (covered by tsconfig.test.json). File: `{componentName}.test.tsx`, co-located.
- `describe` / `it` / `expect`. First line inside `it`: plain-language comment of intention.
- Single-flow: Arrange / Act / Assert. Multi-step: `// Step N —` comments.

```tsx
it('disables submit button on initial render', async () => {
  // Tests that submit is disabled before any input is provided.

  // Arrange
  renderComponent();

  // Act + Assert
  expect(screen.getByRole('button', { name: /submit/i })).toBeDisabled();
});
```

---

## Step 7 — Run and verify

**Act mode**: `npm run <test-script>` / `dotnet test <path>`. Fix failures before completing.
**Plan mode**: report the exact commands to run.