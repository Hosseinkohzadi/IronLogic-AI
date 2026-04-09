# {ProjectName} — Cline Workspace Rules

## Purpose
One or two sentences: what this service does and what domain it owns.

## Stack
- Runtime: (e.g., .NET 10 / Node 20 / React 18)
- Key frameworks: (e.g., ASP.NET Core, EF Core, Azure Functions)
- Tests: (e.g., xUnit + NSubstitute + FluentAssertions)

## Key Entry Points
- API: {ProjectName}.Api/Program.cs
- Tests: {ProjectName}.Tests/
- Config: appsettings.json, appsettings.Development.json

## Owned Service Bus Topics / Queues
- (list topics this project publishes or subscribes to, or "None")

## CX Project Dependencies
- CX.Core — shared platform library (auth, HTTP, serialization, Service Bus base)
- (list other CX projects this service calls or depends on)

## Known Patterns / Gotchas
- (project-specific architectural decisions, non-obvious conventions, legacy quirks)

## aiReferenceFiles
- aiReferenceFiles/aiKnowledgeBase.md — dense project context, updated as knowledge is gained

## Cross-Workspace Navigation
If asked about another CX project, look in order:
1. {cx-root}/{ProjectName}/.clinerules
2. {cx-root}/{ProjectName}/aiReferenceFiles/aiKnowledgeBase.md
3. {cx-root}/{ProjectName}/README.md
Do not read entire source trees — use targeted file reads based on the above context.