# IronLogic AI - Global Project Standards & Architecture

## 🌐 Language & Localization Policy
- **Strict English Policy:** All code, variable names, method names, inline comments, and UI strings must be strictly in professional English.
- **Persian to English:** Automatically translate any Persian (Farsi) strings or comments to English during code generation or refactoring.
- **Timezones:** ALWAYS use `DateTime.UtcNow` for storing dates/times in the database.
- **Currencies:** System must support Multi-Currency (e.g., USD, CAD, EUR, GBP).
- **Units:** Support dynamic Unit Systems (Metric/Imperial) for weights and measurements.

## 🎯 Tech Stack & Architecture Preferences
- **Frontend:** Angular 21+, Tailwind CSS, Angular Signals (Avoid RxJS for simple state). Use Standalone Components.
- **Backend:** .NET 10, C# 13, Entity Framework Core 10.
- **Architecture:** Strictly follow Clean Architecture principles (Domain, Application, Infrastructure, API) and Domain-Driven Design (DDD).

## 🛠️ C# & Backend Development Rules
- **Naming:** Use PascalCase for C# and camelCase for TypeScript/JSON API responses.
- **C# Features:** Always use File-scoped namespaces and Global Usings where appropriate.
- **Dependency Injection:** Use Primary Constructors (C# 12/13) for injecting services into Controllers, Services, and Repositories.
- **EF Core:** Follow the 'Code First' approach. Every Entity MUST have a dedicated Configuration class (`IEntityTypeConfiguration`).
- **Financial Data:** Always use `decimal(18,2)` for money-related fields. Tax calculations (e.g., 13% HST for Ontario) must dynamically rely on the user's `CountryCode` and `RegionCode`.
- **Media Handling:** NEVER store raw files/images in the database. Use `IFileStorageService` (Azure Blob) and store only the `ImageUrl`.

## 📚 Documentation (XML Docs)
- **Mandatory XML Tags:** Every public class, interface, and method in all backend layers (Domain, Application, Infrastructure, Api) MUST have `<summary>` XML documentation.
- **Detailed Params:** Use `<param name="...">` and `<returns>` tags for complex business logic, especially in Core Services (like WorkoutParserService, PersonalRecordService, StripeService).

## 🧠 Business Logic Constraints
- **Exercises:** User-created exercises must default to `Status = Pending` and `IsPublic = false`. Private exercises are visible ONLY to the `CreatorUserId`.
- **Admin Flow:** Only admins can approve exercises (toggle `IsGlobal` to true and `Status` to Approved).
- **Subscriptions:** Rely strictly on Stripe Webhooks (`invoice.paid`, `checkout.session.completed`, `customer.subscription.deleted`) to manage the `UserSubscription` active status.
- **Integration:** Ensure Backend DTOs strictly align with frontend Angular models (check `src/app/core/models` if available).