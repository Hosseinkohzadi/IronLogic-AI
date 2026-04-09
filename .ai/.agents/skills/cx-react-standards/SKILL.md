---
name: cx-react-standards
description: Comprehensive CX React/TypeScript/Module Federation coding standards. Activate when writing, reviewing, or refactoring React/TypeScript code in any CX project.
---

# CX React / TypeScript / Module Federation Standards

## CX.ModuleFederation
- The canonical base for all CX frontend work. Check it before implementing any routing, federation config, or shared dependency.
- All micro-frontends live in `modules/` or reference the host from a separate repo.
- Use the patterns in `modules/menu` as the reference implementation.

## Project Structure

Every CX2 micro-frontend follows this structure:

```
src/
  components/       — all reusable components (Presentational + Smart)
  store/
    sagas/          — one file per saga domain; index.ts exports all
    selectors/      — one file per state domain; index.ts exports all
    actions.ts      — action type constants + action creators + createActions()
    initialState.ts — typed initial state
    slice.ts        — createSlice() combining initialState + createActions()
    index.ts        — configures useStore, wires rootSaga, exports useStore
  api/              — one function per API endpoint using request() from @cx/login
  types/            — shared TypeScript types and interfaces
  services/         — re-exports of core library modules (e.g., react-router-dom)
  theme/            — ThemeProvider setup and theme config
  routes/
    views/          — full-page view components (Home, Profile, etc.)
  App.tsx           — root component, no AuthMethodProvider when inside host
  bootstrap.tsx     — real entry point; renders app; add AuthMethodProvider here (standalone only)
  index.tsx         — ONLY: import './bootstrap'
module-federation.json           — production federation config
module-federation.development.json
module-federation.local.json
package.json
tsconfig.json
```

`index.tsx` must contain exactly one line — the bootstrap import. This is required for webpack module federation eager consumption.

## Tooling

- **Build**: `@cx/react-web-compile` — never invoke webpack or configure it directly.
- **Linter**: `@cx/eslint-config` extends `airbnb-typescript` + `prettier`. Use it unmodified; no custom rule overrides without justification.
- **Formatter**: `@cx/eslint-config/prettier.config.js` — never a local prettier config.
- **Tests**: Jest + `@testing-library/react`.
- `ESLINT_USE_FLAT_CONFIG=true` is mandatory — all projects use ESLint flat config.
- **Package manager**: `npm` exclusively — never `yarn` (Windows compatibility issue in this repo).

### Standard scripts (CX.Hydra baseline)

```json
"eslint:fix":   "cross-env ESLINT_USE_FLAT_CONFIG=true eslint \"src/**/*.{ts,tsx}\" --fix",
"eslint:check": "cross-env ESLINT_USE_FLAT_CONFIG=true eslint \"src/**/*.{ts,tsx}\" --format=compact",
"typecheck":    "tsc --noEmit -p tsconfig.json",
"lint":         "npm run eslint:fix && npm run typecheck",
"lint:check":   "npm run eslint:check || echo 'ESLint found issues.'",
"build":        "react-web-compile --mode=production --ENV=production",
"build-dev":    "react-web-compile --mode=production --ENV=development --generateSourceMapForBuild",
"build-uat":    "react-web-compile --mode=production --ENV=uat --generateSourceMapForBuild",
"build-prod":   "react-web-compile --mode=production --ENV=production",
"test":         "jest",
"start":        "react-web-compile --mode=development --ENV=local --PORT=<port>"
```

`npm run lint` is the authoritative pre-commit check — it runs both ESLint fix and `tsc --noEmit`. Always run it before committing.

## Component Architecture

### Presentational components
Concern: how things look.
- Receive all state and callbacks via props.
- Never import from the store, sagas, or any Zustand hook.
- No side effects.
- Can be composed from other presentational components.

### Smart components (`index.ts`)
Concern: how things work.
- Compose hooks + presentational components.
- No significant HTML markup — only wrapper `<div>`s if needed.
- Connect to the Zustand store, dispatch actions, call selectors.
- Pass data and callbacks down to presentational children.

### Component folder structure

```
MyComponent/
  hooks/        — React hooks (useState, useEffect, custom hooks)
  components/   — Presentational sub-components
  utils/        — Pure JS/TS functions only; no React hooks, no side effects
  constants/    — Constant values used by the component
  types/        — Types and interfaces scoped to this component
  index.ts      — Smart component; assembles hooks + sub-components
```

### Naming
- Components: `PascalCase`
- Custom hooks: `use` + capital letter (e.g., `useFilterBar`)
- Utils: camelCase pure functions — never `useState`, `useEffect`, or any React hook inside
- Saga functions: must end with `saga` suffix (e.g., `initSaga`, `fetchMenuSaga`)

## State Management (Zustand + Redux-Saga)

### Store setup

```ts
// store/index.ts
import { create } from 'zustand';
import { devtools, persist } from 'zustand/middleware';
import sagaMiddleware from '@cx/zustand-saga';
import { combineWatchers } from 'redux-saga-combine-watchers';
import { all } from 'redux-saga/effects';
import { createSlice } from './slice';
import * as sagas from './sagas';

const watchers = Object.values(sagas);

export function* rootSaga(): any {
  yield all(combineWatchers(watchers as GeneratorFunction[]));
}

export const useStore = create(
  devtools(
    persist(
      sagaMiddleware(rootSaga, (...args) => ({ ...createSlice(...args) })),
      { name: 'app-storage', partialize: ({ user }) => ({ user }) }
    )
  )
);
```

### Actions

```ts
// store/actions.ts
export const INIT = 'myapp/init';
export const init = (params = {}) => ({ type: INIT, params });

export const createActions = (store: any) => ({
  init: (params = {}) => store.putActionToSaga(init(params)),
});
```

Action type constants: `SCREAMING_SNAKE_CASE`, prefixed with `appName/`.

### Sagas

```ts
// store/sagas/init.ts
import { takeEvery } from 'redux-saga/effects';
import { setState } from 'zustand-saga';
import { INIT } from '../actions';
import api from 'api';

function* initSaga(): any {
  try {
    const data = yield api.getData();
    yield setState({ data });
  } catch (error) {
    // handle error
  }
}

export default function* saga() {
  yield takeEvery(INIT, initSaga);
}
```

Export all sagas from `store/sagas/index.ts` — they are auto-combined by `combineWatchers`.

### Selectors

```ts
// store/selectors/user.ts
const getUser = (state: AppState) => state.user;
const getUserName = (state: AppState) => getUser(state).name;
export default { getUser, getUserName };

// store/selectors/index.ts
import user from './user';
export default { user };

// In component:
const userName = useStore(selectors.user.getUserName);
```

Never access state shape directly in a component. Always go through a selector.

## API & Authentication

### API module

```ts
// src/api/index.ts
import { request } from '@cx/login';

export const api = {
  getUser: () => request('api/User/GetUser', { method: 'GET' }),
  saveData: (body: SaveDataDto) => request('api/Data/Save', { method: 'POST', body }),
};
```

`request` automatically injects the Authorization header. Never use raw `fetch` or `axios`.

### Authentication — Standalone mode

In `bootstrap.tsx` only when the app runs standalone (not inside the host):

```tsx
import { AuthMethodProvider } from '@cx/login';

root.render(
  <AuthMethodProvider
    clientId={process.env.CLIENT_ID}
    authorityUrl={process.env.AUTHORITY_URL}
    knownAuthorities={process.env.KNOWN_AUTHORITIES?.split(',')}
    scopes={process.env.SCOPES?.split(',')}
    redirectUri={window.location.origin}
  >
    <App />
  </AuthMethodProvider>
);
```

### Authentication — Inside host

Do NOT add `AuthMethodProvider`. The host provides it. Only expose `App.tsx` in `module-federation.json`.

### Environment config

`.env` files are for non-sensitive build-time config only (`CLIENT_ID`, `AUTHORITY_URL`, etc.).
Runtime config (feature flags, API URLs) comes from `window.*` — injected by the C# server or Azure App Configuration.
**Never store secrets, tokens, or passwords in `.env` — they ship in the JS bundle.**

## C# API Contract (TypeScript ↔ C# DTOs)

Rules for all TypeScript types, interfaces, and API functions that communicate with C# backend endpoints.

### Serialization & Property Casing

- **Standard endpoints** use `CXJsonSerializerOptions.Default` — C# PascalCase properties serialize to **camelCase** JSON. TypeScript DTO properties must be camelCase: `public string UserName` → `userName: string`.
- **Legacy endpoints** use `CXJsonSerializerOptions.Legacy` — PascalCase JSON. TypeScript types for legacy endpoints use PascalCase. Document with a JSDoc comment referencing the legacy endpoint.
- **`[JsonPropertyName]` overrides**: `[JsonPropertyName]` is a **legacy-only** attribute — it must not appear in new C# code. If you encounter it on a legacy endpoint, the TypeScript DTO must use that exact key regardless of casing convention. Never replicate `[JsonPropertyName]` keys in TypeScript types for new endpoints.

```ts
// C#: [JsonPropertyName("dejn")] public string JobNumber { get; set; }  ← legacy only
interface SearchResult {
  /** Job number — legacy key: dejn */
  dejn: string;
}
```

### Enums

Enum casing depends on the serializer option used by the endpoint:

- **`Default` / `Compact`** — enum members serialize to **camelCase**. TypeScript union must use camelCase:

```ts
// C# with Default or Compact serializer
type AgentStatus = 'unknown' | 'available' | 'onBreak' | 'offline';
```

- **`Legacy`** — enum members serialize as-is (**PascalCase**). TypeScript union must use PascalCase:

```ts
// C# with Legacy serializer
type AgentStatus = 'Unknown' | 'Available' | 'OnBreak' | 'Offline';
```

When in doubt, check the controller/service to see which `CXJsonSerializerOptions` variant is applied.

- C# enums always have a `0`-valued `Unknown` or `None` member — include it as the **first** union member.
- Never use the `enum` keyword for C# mirror types — use string literal unions or `as const` objects.

### Nullability

- C# uses `null` for absent values — `undefined` does not exist in a C# JSON response.
- Nullable C# fields must be `T | null`, **never** `T | undefined`.
- Use optional (`?`) only when the property may be genuinely absent from the JSON payload (e.g., partial update request body). For response DTOs always use `| null`.
- C# never returns `null` for collection-type properties — TypeScript array fields are always `T[]`, never `T[] | null`.

```ts
// Correct
interface UserDTO {
  displayName: string | null;  // C# string?
  items: ItemDTO[];             // C# List<ItemDTO> — never null
}

// Wrong
interface UserDTO {
  displayName?: string;
  items: ItemDTO[] | null;
}
```

### DateTime

- C# serializes `DateTime` / `DateTimeOffset` as ISO 8601 strings. TypeScript: always `string`.
- Convert to `Date` only at the presentation layer — never in API or DTO definitions.

```ts
interface AuditRecord {
  createdAt: string;        // ISO 8601 — never Date
  modifiedAt: string | null;
}
```

### Required vs. Optional Fields

- `[Required]` C# fields are never absent — map to required TypeScript properties (no `?`).
- C# `record` types are immutable — use `readonly` on all TypeScript equivalent properties:

```ts
interface UserDTO {
  readonly id: string;
  readonly userName: string;
  readonly email: string | null;
}
```

### DTO Naming Conventions

| Rule | C# | TypeScript |
|---|---|---|
| `DTO` suffix | `UserDTO` | `UserDTO` (ALL_CAPS, not `UserDto`) |
| Closed-form compound words | `Datamart`, `Dataset`, `Metadata`, `Workhub` | Same — never split or abbreviate |
| No abbreviation drift | `JobNumber` | `jobNumber` (camelCase of exact C# name) |
| Acronyms 2-letter | `IOResult` | `ioResult` |
| Acronyms 3+ letters | `ApiResponse` | `apiResponse` |

### Error Responses

C# returns `ProblemDetails` on errors — never type as `any` or `unknown` without narrowing:

```ts
interface ProblemDetails {
  title: string;
  status: number;
  detail?: string | null;
  errors?: Record<string, string[]> | null;
}
```

### Binary Data

C# `BinaryData` properties serialize as base64 strings — type as `string` with a JSDoc comment:

```ts
interface AttachmentDTO {
  /** Base64-encoded file content */
  content: string;
}
```

## Module Federation

### Configuration files

```json
// module-federation.json (production)
{
  "name": "myApp",
  "remotes": {
    "host": "host@https://cx-host.concentrixcx.com/remoteEntry.js"
  },
  "exposes": {
    "./App": "src/App.tsx"
  }
}
```

Three config files required:
- `module-federation.json` — production CDN URLs
- `module-federation.development.json` — dev environment URLs
- `module-federation.local.json` — localhost URLs

### What to expose

Every app exposes at minimum `./App`. Expose the store if other apps need to consume its state.

### Inter-app communication

```ts
import { emitter, useSubscription, useEmitEvent } from 'host/index';

// Emit an event:
emitter.fire('menu/filter-button-pressed', { isActive: true });

// Subscribe in a component:
const data = useSubscription('menu/filter-button-pressed');

// Emit on dependency change:
useEmitEvent({ type: 'menu/filter-button-pressed', data: { isActive } }, [isActive]);
```

Event naming: **`[app-name]/[event-name]`** — always prefix with the owning app name.

### Global shared state

`useHostStore` is a last resort — only when this app is the sole owner of that state key. Read from host state freely; write only if you own it.

## Styling

### ThemeProvider setup

```tsx
<ThemeProvider
  cacheConfig={{
    key: 'my-app',
    prepend: true,
    container,
    nonce: window.globalNonceKey,
  }}
>
  {children}
</ThemeProvider>
```

`window.globalNonceKey` is required for CSP compliance. The C# server injects it into the HTML.

### CSS scoping rules

Always scope selectors to the component prefix — never target bare HTML elements:

```css
/* Bad */
input { display: inline-block; }

/* Good */
.my-app-input input { display: inline-block; }
```

Third-party CSS imports: verify they do not mutate global styles.

## Security

- `.env` values appear verbatim in the JS bundle — no secrets, API keys, or credentials.
- Runtime config: injected via `window.*` from the C# server or Azure App Configuration.
- KeyVault secrets must never reach the frontend.
- All backend API requests require a valid auth token; `@cx/login` `request` injects it automatically.
- CSP: pass `window.globalNonceKey` to `ThemeProvider`; all dynamically-injected style tags receive the nonce.

## Testing

### Unit tests — UI components

Use Jest + `@testing-library/react`. Test from the user's perspective:

```tsx
import { render, screen } from '@testing-library/react';

test('shows loading state', () => {
  render(<MyButton loading />);
  expect(screen.getByText('loading...')).toBeInTheDocument();
});
```

Test what appears in the DOM — not internal state, unless the state directly produces a visible change.

### Integration tests — Sagas + Zustand

```ts
import { create } from 'zustand';
import sagaMiddleware from '@cx/zustand-saga';

const getStore = () => create(sagaMiddleware(rootSaga, createSlice));

test('init sets user data', () => {
  const store = getStore();
  expect(store.getState().user).toBeNull(); // initial state

  store.getState().init();

  expect(store.getState().user).not.toBeNull(); // final state
});
```

Assert initial state → dispatch action → assert final state. Do not unit-test saga internals in isolation.

### E2E tests — Cypress

```js
// cypress.config.js
module.exports = defineConfig({
  e2e: {
    experimentalModifyObstructiveThirdPartyCode: true, // required for CX2 auth
    setupNodeEvents(on, config) { ... }
  }
});

// Authentication command
Cypress.Commands.add('CX2Login', (url, user, password, env) => {
  cy.visit(url);
  cy.origin(authOrigin, { args: { user, password } }, ({ user, password }) => {
    cy.get('#signInName').type(user, { log: false });
    cy.get('#password').type(password, { log: false });
    cy.get('#next').click();
  });
});
```

`experimentalModifyObstructiveThirdPartyCode: true` is mandatory for the CX2 authentication flow.

## Documentation

### README.md (required for every shared project)

Sections:
1. **Project Description** — purpose and scope
2. **Module Federation Configuration** — how to consume this app; remotes, exposes, required setup
3. **Requirements** — dependencies and prerequisites
4. **Environment Variables** — all variables with descriptions; mark which are runtime vs build-time
5. **Exposed Code** — prop tables for every exposed component

Prop table format:
| Prop | Type | Default | Description |
|------|------|---------|-------------|
| `onClick` | `() => void` | — | Called when the button is pressed |

### JSDoc

- All interfaces, types, and their properties: `/** */` with summary or `@description`.
- Props interfaces: document every property.
- `@param` only when purpose is unclear from name + type.
- No `@example` tags.
- Implementations: `/** @inheritdoc */` — never duplicate interface docs.

## Code Recipes

### Accessing a hook instance outside React (e.g., from a saga)

Create a module-level ref in `src/services/`:

```ts
// src/services/Toast.ts
export { ToastProvider, useToast } from '@cx/ui';

const toastInstance = { current: null };
export const getToastInstance = () => toastInstance.current;
export const setToastInstance = (instance) => { toastInstance.current = instance; };
export const addToast = (options) => getToastInstance()?.addToast?.(options);
```

Set the instance in `App.tsx` via `useEffect`, then call `addToast()` from any saga.

### Reacting to browser window events in sagas

```ts
// src/utils/listenWindowEvents.ts
import { eventChannel } from 'redux-saga';

export function listenWindowEventsSagas({ eventName }) {
  return eventChannel((emit) => {
    const listener = (event) => emit(event);
    window.addEventListener(eventName, listener);
    return () => window.removeEventListener(eventName, listener);
  });
}

// src/store/sagas/hooks.ts
export function* watchClickSaga(): Generator {
  const chan = yield call(listenWindowEventsSagas, { eventName: 'click' });
  while (true) {
    const event = yield take(chan);
    // handle event
  }
}
```

## AI-Generated Code

- AI-generated code must meet the exact same standards as hand-written code.
- No verbose boilerplate documentation — factual, minimal, purposeful.
- Proper tests required — no filler tests.
- Understand every line of generated code before accepting it.