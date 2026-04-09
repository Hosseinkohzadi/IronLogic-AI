---
agent: agent
description: "Scaffold a new CX2 micro-frontend — runs create-js-app, wires Module Federation config, sets up the Zustand/Saga store, and validates the full checklist."
---
# Workflow: cx-react-new-microfrontend

Step-by-step checklist for creating a new CX2 micro-frontend. Follow in order.

---

## 1. Scaffold

- [ ] Run `npx @cx/create-js-app <app-name>` from the monorepo root
- [ ] Confirm the generated folder structure matches the expected layout:
  ```
  src/
  ├── api/
  ├── components/
  ├── hooks/
  ├── store/
  │   ├── actions.ts
  │   ├── initialState.ts
  │   ├── slice.ts
  │   ├── index.ts
  │   ├── sagas/
  │   └── selectors/
  ├── types/
  ├── utils/
  ├── App.tsx
  ├── bootstrap.tsx
  └── index.tsx
  ```
- [ ] Verify `index.tsx` contains only `import './bootstrap'` — no other code

---

## 2. Module Federation Config

- [ ] Open `module-federation.json` — confirm `./App` is exposed at minimum
- [ ] Set `name` to the app identifier (kebab-case, e.g. `my-feature`)
- [ ] Copy `module-federation.json` to `.development.json` and `.local.json` for local overrides
- [ ] Never configure webpack directly — all build config goes through `@cx/react-web-compile`

---

## 3. Package Scripts (CX.Hydra Standard)

Ensure `package.json` contains the full Hydra script set:

```json
"scripts": {
  "start":       "react-web-compile --mode=development --ENV=local --PORT=<port>",
  "build":       "cross-env ESLINT_USE_FLAT_CONFIG=true react-web-compile build",
  "build-dev":   "cross-env ESLINT_USE_FLAT_CONFIG=true react-web-compile build --env=dev",
  "build-uat":   "cross-env ESLINT_USE_FLAT_CONFIG=true react-web-compile build --env=uat",
  "build-prod":  "cross-env ESLINT_USE_FLAT_CONFIG=true react-web-compile build --env=prod",
  "eslint:fix":  "cross-env ESLINT_USE_FLAT_CONFIG=true eslint --fix src",
  "eslint:check":"cross-env ESLINT_USE_FLAT_CONFIG=true eslint src",
  "typecheck":   "tsc --noEmit",
  "lint":        "npm run eslint:fix && npm run typecheck",
  "lint:check":  "npm run eslint:check && npm run typecheck",
  "test":        "jest"
}
```

- [ ] `cross-env ESLINT_USE_FLAT_CONFIG=true` is present on all eslint and build commands (Windows compatibility)
- [ ] Never add a `yarn` script or reference yarn anywhere

---

## 4. Global Store

- [ ] `store/actions.ts` — define all Zustand action names as string constants
- [ ] `store/initialState.ts` — define the typed initial state object
- [ ] `store/slice.ts` — create the Zustand slice (state + setters, no side effects)
- [ ] `store/index.ts` — export `useStore` via `@cx/zustand-saga` `createStore`
- [ ] `store/selectors/index.ts` — export all selector functions; components never access state shape directly

---

## 5. Sagas

- [ ] `store/sagas/init.ts` — create `initSaga` generator for app initialization
- [ ] `store/sagas/index.ts` — combine watchers with `redux-saga-combine-watchers`
- [ ] All saga functions use generator syntax and end with the `saga` suffix
- [ ] All async operations, API calls, and event reactions go in sagas — never in components or event handlers

---

## 6. API Layer

- [ ] Create `src/api/index.ts` (or per-domain files under `src/api/`)
- [ ] All calls use `request` from `@cx/login` — never raw `fetch` or `axios`
- [ ] Export typed async functions; no business logic in API files

---

## 7. Authentication

- [ ] `bootstrap.tsx` (standalone mode): wrap `<App />` with `<AuthMethodProvider>`
- [ ] `App.tsx` (exposed to host): do NOT include `AuthMethodProvider` — the host provides it
- [ ] `CLIENT_ID`, `AUTHORITY_URL`, `KNOWN_AUTHORITIES`, `SCOPES` sourced from `.env`

---

## 8. Theming & CSP

- [ ] Wrap the app root with `<ThemeProvider>` from `@cx/ui`
- [ ] Pass `cacheConfig={{ key: '<app-name>', nonce: window.globalNonceKey, prepend: true, container }}` to `ThemeProvider` — `key` is required
- [ ] No third-party CSS that leaks into global styles

---

## 9. Components

- [ ] Presentational components: props-driven, no imports from store/sagas/`src/api/`
- [ ] Smart components (`index.ts`): compose hooks + presentational; minimal wrapper markup only
- [ ] All props interfaces have JSDoc `/** */` on every property
- [ ] No `any`, `@ts-ignore`, or `@ts-expect-error` without documented justification
- [ ] All UI from `@cx/ui` — never build custom UI primitives

---

## 10. Inter-App Events

- [ ] Use `emitter`, `useSubscription`, `useEmitEvent` from `host/index`
- [ ] Follow naming convention: `[app-name]/[event-name]` (e.g. `my-feature/item-selected`)

---

## 11. README

Ensure `README.md` includes all required sections:

- [ ] Project description and purpose
- [ ] Module federation config (exposed components, remotes)
- [ ] Requirements (Node version, env vars)
- [ ] Environment variables table
- [ ] Exposed components with prop tables (`Prop | Type | Default | Description`)

---

## 12. Final Checks

- [ ] Run `npm run lint` — must pass with zero errors (ESLint + `tsc --noEmit`)
- [ ] Run `npm run test` — must pass with zero failures
- [ ] No secrets, tokens, or credentials in `.env` files
- [ ] No new npm packages added without approval