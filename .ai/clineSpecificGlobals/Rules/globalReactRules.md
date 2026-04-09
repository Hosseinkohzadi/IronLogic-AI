---
applyTo: "**/*.ts,**/*.tsx,**/*.jsx,**/src/**/*.js"
description: "CX React/TypeScript coding invariants"
---
# CX React / TypeScript / Node.js Rules

For full guidance when writing, reviewing, or refactoring React/TypeScript code, activate skill `cx-react-standards`.

## Invariants — Never Do

- Never use `yarn` — use `npm` exclusively (`yarn` has Windows compatibility issues in this repo).
- Prefer `@cx/ui` components over creating new ones. When a UI need arises, first determine whether it can be satisfied by composing existing `@cx/ui` components.
  - If it **can** be composed: build it as a composition — do not create a new component.
  - If it is a **compound of multiple components** (several `@cx/ui` parts grouped into a reusable pattern): before generating code, ask the user whether they want this extracted as a standalone reusable component or kept as an inline composition.
  - If it is **truly a new atomic component** that does not exist in `@cx/ui`: sequence the development plan as **(1) create and build the component in `@cx/ui`, (2) import and consume it in the application**. Inform the user upfront: *\"This looks like a new reusable component. I've planned this as: build it in `@cx/ui` first, then wire it into the app — so it's available to everyone from the start.\"*
- Never use raw `fetch` or `axios` — use `request` from `@cx/login` for all authenticated requests.
- Never do async work in components or event handlers — all side effects go in sagas.
- Never access store state shape directly in components — use selectors from `src/store/selectors/`.
- Never add `AuthMethodProvider` to `App.tsx` when the app runs inside the host — the host provides it.
- Never put secrets, tokens, or sensitive data in `.env` files — all values appear in the JS bundle.
- Never write global CSS selectors — always scope styles to the component class prefix.
- Never add business logic to presentational components — no store or saga dependencies allowed.
- Never use `useHostStore` to set shared state unless this app is the sole owner of that state key.
- Never configure webpack directly — use `@cx/react-web-compile`.
- Never add inline `eslint-disable` comments without documented justification.
- No `any`, `@ts-ignore`, or `@ts-expect-error` without documented justification.
- **300-line hard limit per file.** Approaching this ceiling means the file is doing too much. Split using the existing CX2 layering:
  - **Smart component (`index.tsx`)**: if growing, the presentational component is absorbing logic or the component is too wide in scope. Extract sub-sections into child presentational components; keep `index.tsx` as a thin orchestrator.
  - **Sagas**: split by concern — one saga file per feature slice (e.g. `fetchUserSaga.ts`, `submitFormSaga.ts`). A large saga file means multiple unrelated workflows are co-located; extract each into its own file under `src/store/sagas/`.
  - **Custom hooks**: reusable stateful logic in a component is a candidate for extraction to `src/hooks/`. A hook must have a single purpose expressible in one sentence.
  - **Utilities/helpers**: pure functions that appear in multiple places or that grow complex belong in `src/utils/` as pure, side-effect-free modules.
  - **Selectors**: if the selectors file grows large, group selectors by domain sub-slice into separate files under `src/store/selectors/`.

## Invariants — Always Do

- Run `npm run lint` (ESLint fix + `tsc --noEmit`) before committing any change.
- Run `npm run test` after any logic change to verify no regressions.
- Use `@cx/create-js-app` to scaffold new micro-frontend projects — never scaffold manually.
- `index.tsx` must only `import './bootstrap'` — no other code (required for module federation eager loading).
- Every micro-frontend must expose at minimum `./App` in `module-federation.json`.
- Inter-app events must follow the `[app-name]/[event-name]` naming convention.
- Pass `window.globalNonceKey` to `ThemeProvider`'s `cacheConfig.nonce`.
- All API calls go through `src/api/` using `request` from `@cx/login`.
- Use redux-saga for all side effects — generator functions, suffix each saga function with `saga`.
- Use `@cx/zustand-saga` as the bridge between Zustand stores and Redux-Saga — required in all CX2 apps; do not wire Zustand and sagas separately.
- Selectors live in `src/store/selectors/` — components never access state shape directly.
- Component names: PascalCase. Hook names: `use` + capital letter. Utilities: pure JS/TS, no React hooks.
- All component props, interfaces, and exported types must have JSDoc `/** */` documentation.

## C# API Contract

- camelCase properties for standard endpoints (`CXJsonSerializerOptions.Default`); PascalCase for legacy — document which on the interface.
- When C# uses `[JsonPropertyName]`, the TypeScript key must match the JSON key exactly — overrides all casing rules.
- Nullable C# fields: `T | null` — never `T | undefined`. Optional (`?`) only for genuinely absent JSON keys (e.g., partial update request bodies).
- Collections: never `T[] | null` — C# never returns null for collection properties.
- Enums: PascalCase string literals matching C# member names exactly (e.g., `'Active'` not `'active'`). Always include the zero-member (`Unknown` / `None`) as the first union member. Use string literal unions or `as const` — never the `enum` keyword.
- `DateTime` / `DateTimeOffset`: always `string` in TypeScript — never `Date` in DTO definitions.
- DTO naming mirrors C# exactly: `DTO` suffix ALL_CAPS, closed-form compound words match (`Datamart`, `Dataset`, `Metadata`), no abbreviation drift.
- C# `record` types: use `readonly` on all TypeScript equivalent properties.
- `[Required]` C# fields: required TypeScript property — no `?`.
- Error responses: typed as `ProblemDetails` — never `any`.
- `BinaryData`: typed as `string` with a JSDoc base64 comment.

## Base Project

- All CX frontends extend `CX.ModuleFederation`. Check it before adding routing, federation config, or shared deps.
- Never duplicate or override federation config inherited from the base.