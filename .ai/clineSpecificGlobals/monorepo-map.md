# CX Monorepo Map

Read this file when looking for a CX library, internal package, or project not in the current workspace.
Do not read this file at task startup — only on demand when navigating to another project.

---

## Local Clone Root

Convention: all CX repositories are cloned under `~/sources/cx/` on developer machines.
The monorepo checkout itself lives at `~/sources/cx/` (this repo).

---

## Projects in This Monorepo

| Directory | Description | Primary Language |
|---|---|---|
| CX.Analyzers | Roslyn analyzers for CX coding standards | C# |
| CX.Anemoi | Survey scheduling and invitations | C# |
| CX.Arke | Routing and rules engine | C# |
| CX.Asopos | Data reader workers (DRS, IVR, MRI, QDI) | C# |
| CX.Calendars | Calendar and scheduling service | C# |
| CX.Chilon | Survey pipeline and feedback collection | C# |
| CX.Chronos | Time-tracking and scheduling service | C# |
| CX.CMSServer | CMS backend (Node.js/Payload) | TypeScript/Node |
| CX.Core | Shared platform library — auth, HTTP, serialization, Service Bus base | C# |
| CX.Cyclops | (see README) | C# |
| CX.Damon | (see README) | C# |
| CX.Datamart | Datamart integration service | C# |
| CX.DiskCache | Disk-based caching service | C# |
| CX.Dolos | (see README) | C# |
| CX.Example | Example/starter project | C# |
| CX.Gaia | (see README) | C# |
| CX.Hephaestus | (see README) | C# |
| CX.Hermes | Messaging and notification service | C# |
| CX.Hestia | (see README) | C# |
| CX.Hydra | Shell host micro-frontend | TypeScript/React |
| CX.Iris | (see README) | C# |
| CX.Janus | (see README) | C# |
| CX.Kraken | (see README) | C# |
| CX.Kratos | Authentication and identity service | C# |
| CX.LegacyData | Legacy data migration service | C# |
| CX.Malarkey | (see README) | C# |
| CX.Media | Media storage and streaming service | C# |
| CX.Medusa | (see README) | C# |
| CX.Mercury | Messaging gateway | C# |
| CX.Metadata | Metadata management service | C# |
| CX.Metis | (see README) | C# |
| CX.Migration | Database migration tooling | C# |
| CX.MIM | (see README) | C# |
| CX.Minerva | (see README) | C# |
| CX.ModuleFederation | Module Federation configuration and shared React packages | TypeScript/React |
| CX.Notifications | Push notification service | C# |
| CX.Palamedes | (see README) | C# |
| CX.Pegasus | (see README) | C# |
| CX.Phanes | (see README) | C# |
| CX.Pontus | (see README) | C# |
| CX.Poseidon | (see README) | C# |
| CX.Proteus | (see README) | C# |
| CX.PurgeBot | Data purge automation | C# |
| CX.PushReport | Push reporting service | C# |
| CX.Scapegoat | (see README) | C# |
| CX.SharedServices | Shared service infrastructure | C# |
| CX.StarterPack | New project scaffold template | C# |
| CX.SurveyFeedback.Api | Survey feedback API | C# |
| CX.SurveyFeedback.Worker | Survey feedback background worker | C# |
| CX.SurveyPipeline | Survey processing pipeline | C# |
| CX.SurveyUsageManager | Survey usage tracking | C# |
| CX.Talos | (see README) | C# |
| CX.Tangram | (see README) | C# |
| CX.TokenValidation | Token validation service | C# |
| CX.Triton | (see README) | C# |
| CX.UserManagement.Api | User management API | C# |
| CX.UserManagement.v2 | User management v2 | C# |
| CX.Vesta | (see README) | C# |
| CX.Website | Public-facing website | TypeScript/React |
| CX_Frontend_Stack | Shared React component library (`@cx/ui`) | TypeScript/React |
| CX_Identity_API | Identity and authentication API | C# |

---

## Shared NuGet Packages (Internal)

| Package | Source Project | Local Path |
|---|---|---|
| CX.Core | CX.Core | ./CX.Core/CX.Core/ |
| CX.Core.Api | CX.Core | ./CX.Core/CX.Core.Api/ |
| CX.Core.Caching | CX.Core | ./CX.Core/CX.Core.Caching/ |
| CX.Core.Connectors | CX.Core | ./CX.Core/CX.Core.Connectors/ |
| CX.Core.Cosmos | CX.Core | ./CX.Core/CX.Core.Cosmos/ |
| CX.Core.Data | CX.Core | ./CX.Core/CX.Core.Data/ |
| CX.Core.Data.Testing | CX.Core | ./CX.Core/CX.Core.Data.Testing/ |
| CX.Core.Databricks | CX.Core | ./CX.Core/CX.Core.Databricks/ |
| CX.Core.Functions | CX.Core | ./CX.Core/CX.Core.Functions/ |
| CX.Core.Mongo | CX.Core | ./CX.Core/CX.Core.Mongo/ |
| CX.Core.ServiceBus | CX.Core | ./CX.Core/CX.Core.ServiceBus/ |
| CX.Core.Storage | CX.Core | ./CX.Core/CX.Core.Storage/ |
| CX.Core.Testing | CX.Core | ./CX.Core/CX.Core.Testing/ |

---

## Shared npm Packages (Internal)

| Package | Source Project | Local Path |
|---|---|---|
| @cx/ui | CX_Frontend_Stack | ./CX_Frontend_Stack/ |
| @cx/zustand-saga | CX.ModuleFederation | ./CX.ModuleFederation/ |
| @cx/login | CX.ModuleFederation | ./CX.ModuleFederation/ |
| @cx/create-js-app | CX.ModuleFederation | ./CX.ModuleFederation/ |
| @cx/eslint-config | CX.ModuleFederation | ./CX.ModuleFederation/ |
| react-web-compile | CX.ModuleFederation | ./CX.ModuleFederation/ |

---

## External Libraries (NuGet)

For external NuGet packages not in this monorepo, refer to:
- https://www.nuget.org/packages/{PackageName}
- GitHub source if open-source (linked from the NuGet page)

## External Libraries (npm)

For external npm packages, refer to:
- https://www.npmjs.com/package/{package-name}