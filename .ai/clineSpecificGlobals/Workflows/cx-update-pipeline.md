---
agent: agent
description: "Add or update a CX Dockerfile and azure-pipelines YAML — enforces CX ACR images, dotnet-sdk 10.0, --warnaserror, base.v3.yaml, and correct Helm values for all environments."
---
# cx-update-pipeline

Trigger: adding or updating a Dockerfile, `azure-pipelines*.yml`, or Helm values for a CX project.
Activate skill: `cx-azure-pipelines` (when available).

---

## Step 1 — Identify scope

- Is this a new service, an update to an existing one, or a .NET version bump?
- Which pipeline template does the project use? (`base.v3.yaml`, `base.v2.yaml` legacy, or `platform-tools`)
- Read the existing `azure-pipelines.yml` to confirm current pattern before editing.
- New pipelines must use `base.v3.yaml`. If the project is on v2, note it as a separate upgrade task — do not silently mix.

---

## Step 2 — Dockerfile (if adding or updating a service)

- New services: always use `concentrixcx.azurecr.io/cx/dotnet-sdk:10.0` (build) and `dotnet-aspnet:10.0` (runtime).
- Existing services: match the version already in use unless an upgrade was explicitly requested.
- Never use `6.0` — if the project is still on 6.0, raise it as a separate upgrade task before proceeding.
- Follow the two-stage pattern exactly:
  1. Build stage: restore → run tests → build API.
  2. Runtime stage: copy bin/ from build stage, `WORKDIR /dist`, set `ENTRYPOINT`.
- `--warnaserror` is mandatory in `BUILD_ARGS`.
- `COPY` only the sub-projects this service depends on — not the entire project tree.
- Never use the public `mcr.microsoft.com` images — always use the CX ACR images.

**Standard two-stage Dockerfile pattern:**

```dockerfile
FROM concentrixcx.azurecr.io/cx/dotnet-sdk:10.0 AS build
ARG BUILD_VERSION
ARG BUILD_ARGS="-c Release --warnaserror -p:BuildVersion=$BUILD_VERSION -o {ApiProject}/bin"
ARG BRANCH_NAME=$branchName
ARG PR_KEY=$prKey
ARG PR_BRANCH=$prBranch
ARG PR_BASE=$prBase
ARG SCAN_PATH=/src/{ProjectName}

WORKDIR /src/{ProjectName}
COPY Directory.Build.props .
COPY {TestProject}/ ./{TestProject}/
COPY {ApiProject}/ ./{ApiProject}/
RUN dotnet restore ./{TestProject}/{TestProject}.csproj
RUN dotnet test {TestProject} -c Release --collect:"XPlat Code Coverage" --logger "trx;LogFileName=api.tests.results.trx"
RUN dotnet restore ./{ApiProject}/{ApiProject}.csproj
RUN dotnet build ./{ApiProject}/{ApiProject}.csproj $BUILD_ARGS

FROM concentrixcx.azurecr.io/cx/dotnet-aspnet:10.0 AS runtime
WORKDIR /dist
COPY --from=build /src/{ProjectName}/{ApiProject}/bin/ .
ENTRYPOINT ["dotnet", "{ProjectName}.Api.dll"]
```

---

## Step 3 — Azure pipeline YAML (if adding or updating)

- New pipelines: use `base.v3.yaml` — template path: `../.azure-pipelines/templates/base.v3.yaml`
- Existing pipelines on v2: do not upgrade the template version as a side effect — create a separate task.
- Add `dockerfiles[]` entry: `name`, `path`, `publishTests`, `testResultPath`, `codeCoveragePath`.
- Add `helmDeployments[]` entry with values paths for each environment.
- Scope `trigger.paths.include` to only the directories this pipeline owns.
- If adding a second independently deployable service, create a new `azure-pipelines-{service}.yml` — do not combine into one file.

**Standard `base.v3.yaml` pipeline:**

```yaml
trigger:
  batch: true
  branches:
    include:
      - main
  paths:
    include:
      - {ProjectDir}/*
pool:
  vmImage: ubuntu-latest
extends:
  template: ../.azure-pipelines/templates/base.v3.yaml
  parameters:
    namespace: {ProjectName}
    namespacePath: {ProjectDir}/
    deployToDevOnPR: true
    environments:
      - dev
      - uat
      - prod
    dockerfiles:
      - name: {servicename}
        path: {ServiceDir}/Dockerfile
        publishTests: true
        testResultPath: '/build/test/out'
        codeCoveragePath: '/build/coverage/out'
    helmDeployments:
      - name: {servicename}
        values:
          - path: {ServiceDir}/helm/values.yaml
          - path: {ServiceDir}/helm/values.Development.yaml
            environments: dev
          - path: {ServiceDir}/helm/values.UAT.yaml
            environments: uat
          - path: {ServiceDir}/helm/values.Production.yaml
            environments: prod
```

---

## Step 4 — Helm values (if adding or updating)

- Ensure all four values files exist: `values.yaml`, `values.Development.yaml`, `values.UAT.yaml`, `values.Production.yaml`.
- `values.yaml` contains base config; per-environment files contain only overrides.
- Update the `helmDeployments` entry in `azure-pipelines.yml` to reference all four.
- PascalCase environment suffix is preferred for new projects (match existing casing in the project if updating).

---

## Step 5 — .NET version bump (platform-wide only)

- Update `.docker-images/cx-dotnet-sdk/Dockerfile` `ARG baseImage` default to the new version.
- Update `.docker-images/cx-dotnet-aspnet/Dockerfile` `ARG baseImage` default to the new version.
- Update all affected project Dockerfiles `FROM` tags to match the new version.
- Verify `Directory.Build.props` `TargetFramework` (e.g., `net10.0`) is consistent across all updated projects.
- `6.0 → 8.0` or `10.0` upgrades require a separate dedicated task — never as a side effect.

---

## Step 6 — Verify

- [ ] No hardcoded secrets, credentials, or connection strings in any YAML or Dockerfile.
- [ ] All ARG values that carry secrets use `--mount=type=secret`, not ARG injection.
- [ ] `--warnaserror` is present in `BUILD_ARGS`.
- [ ] Tests run in the build stage before the API build.
- [ ] Trigger `paths.include` is scoped correctly — overly broad triggers cause unnecessary pipeline runs.
- [ ] Both build and runtime stages use the same version tag.
- [ ] All four Helm values files exist for every deployed service.