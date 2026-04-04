# IronLogic AI Project Standards

## Language & Translation
- **Strict English Policy:** All code, variable names, method names, and inline comments must be in English.
- **Persian to English:** If any Persian (Farsi) strings or comments are found (except for specific workout data if required), they must be translated to professional English.
- **UI Strings:** All hardcoded strings in the API or Angular dashboard should be in English.

## Documentation (XML Docs)
- **Mandatory XML Tags:** Every public class, interface, and method in the following projects must have <summary> XML documentation:
    - IronLogic.Domain
    - IronLogic.Application
    - IronLogic.Infrastructure
    - IronLogic.Api
- **Parameter Docs:** Use <param name="..."> and <returns> tags for complex business logic, especially in WorkoutParserService and PersonalRecordService.

## Technical Preferences
- **Architecture:** Follow Clean Architecture principles.
- **C# Features:** Use primary constructors (C# 12+), file-scoped namespaces, and global usings.
- **Angular:** Use Signals and Standalone Components for the frontend.
