# IronLogic AI Project Standards

## 1. Language & Translation
- **Strict English Policy:** All code, variable names, method names, and inline comments must be in English.
- **Persian to English:** Any Persian (Farsi) comments found must be translated to professional English (e.g., replace `// چک کردن مسیر` with `// Ensure the path is correct`).
- **UI Strings:** All hardcoded strings in the API or Angular dashboard should be in English.

## 2. Documentation (XML Docs)
- **Mandatory XML Tags:** Every public class, interface, and method in `IronLogic.Domain`, `Application`, `Infrastructure`, and `Api` must have `<summary>` XML documentation.
- **Parameter Docs:** Use `<param name="...">` and `<returns>` tags for complex business logic, especially in `WorkoutParserService` and `PersonalRecordService`.
- **Inheritance:** Use `/// <inheritdoc />` for implementations to avoid duplicating interface documentation.

## 3. Technical Preferences & Architecture
- **Architecture:** Strictly follow **Clean Architecture** principles.
- **C# Features:** - Use **Primary Constructors** exclusively for `record` types; do not use them for `class` types.
    - Use **File-scoped namespaces** and **Global usings**.
- **Angular:** - Use **Signals** and **Standalone Components** for the frontend.
    - **Component Priority:** Always prioritize using existing internal components (e.g., Custom Grid, Card, Input wrappers) over creating new ones.
    - **Route Logic:** When managing UI state via routes (e.g., `hideSidebar`), check the entire route hierarchy. If any route in the chain has `hideSidebar: true`, the sidebar must be hidden (Hierarchical Override).
- **Performance:** For string cleanup or normalization, prefer modern **.NET Regex** over manual `while` or `for` loops.

## 4. Error Handling & Validation
- **Result Pattern:** Use a functional `Result<T>` pattern for service responses instead of throwing custom exceptions for expected business failures.
- **Thin Controllers:** Controllers must contain only route binding and model validation; extract all business logic to services.
- **Validation:** Use Data Annotations (`[Required]`, `[Range]`, etc.) in DTOs for automatic validation via `[ApiController]`.

## 5. Data Layer & Persistence
- **Batch Operations:** Never perform DB operations in loops. Use batch queries (e.g., `WHERE Id IN (...)`) to avoid N+1 issues.
- **Return Types:** Service methods should return `IReadOnlyList<T>` or `ICollection<T>`—never return `null` for collection types; return an empty collection instead.
- **Async/Await:** All I/O must be async. Propagate `CancellationToken` as the last parameter through the entire call chain.

## 6. Testing Standards (xUnit)
- **Framework:** Use **xUnit** with **Moq** for unit tests.
- **Naming Convention:** Test methods must follow the pattern: `{MethodName}_{Scenario}_{ExpectedResult}`.
- **Mocking:** - Use an in-memory EF context for database testing—never mock `DbContext` directly.
    - **Mock Lifecycle:** Use the constructor to initialize mocks and the SUT (System Under Test) to ensure a clean state for every `[Fact]` or `[Theory]`.
- **Assertions:** Use explicit assertions. Each test should verify a single behavior.

## 7. Code Quality, Security & Constraints
- **300-Line Limit:** Maintain a hard limit of **300 lines per file**. If a file exceeds this, split it by concern.
- **No Regions:** Use of `#region` is strictly prohibited.
- **Security:** Never log PII, tokens, or credentials. Use KeyVault or secure configuration for secrets.
- **Logging:** Use structured logging with message templates; avoid string interpolation in log calls.
- **Data Integrity:** Enums must have a `0`-valued `Unknown` or `None` member.
- **Serialization Efficiency:** Never call `.ToString()` on `BinaryData` or `Stream` before deserialization; deserialize directly from the stream.
- **Type Safety (TypeScript/Angular):** - **No `any` Policy:** The use of the `any` type is prohibited.
    - **Strong Typing:** Always create specific interfaces or generics for complex data structures to improve maintainability.
    - **No Magic Strings:** Avoid string-based property access for dynamic binding; use type-safe accessors or defined interfaces to catch breaking changes at compile time.
- **String Comparison:** Always use `StringComparison.OrdinalIgnoreCase` for comparisons.