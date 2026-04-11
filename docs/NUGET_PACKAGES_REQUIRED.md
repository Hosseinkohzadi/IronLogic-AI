# Required NuGet Packages

## Installation Commands

Run these commands in the `src/IronLogic.Infrastructure` directory:

```bash
# Stripe.net SDK for payment processing
dotnet add package Stripe.net --version 44.0.0

# Azure Storage Blobs for cloud file storage
dotnet add package Azure.Storage.Blobs --version 12.19.1
```

## Package Details

### Stripe.net (v44.0.0)
- **Purpose:** Stripe payment gateway integration
- **Features:**
  - Checkout Session creation
  - Webhook event handling
  - Customer management
  - Subscription lifecycle management
- **Documentation:** https://stripe.com/docs/api?lang=dotnet

### Azure.Storage.Blobs (v12.19.1)
- **Purpose:** Azure Blob Storage integration for file uploads
- **Features:**
  - Blob upload/download
  - Container management
  - Public access configuration
  - CDN integration
- **Documentation:** https://learn.microsoft.com/en-us/azure/storage/blobs/

## Updated IronLogic.Infrastructure.csproj

After running the commands, your project file should include:

```xml
<ItemGroup>
  <PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" />
  <PackageReference Include="Microsoft.EntityFrameworkCore" />
  <PackageReference Include="CsvHelper" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.Design">
    <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    <PrivateAssets>all</PrivateAssets>
  </PackageReference>
  <PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" />
  <PackageReference Include="Microsoft.SemanticKernel" />
  
  <!-- External Integrations -->
  <PackageReference Include="Stripe.net" Version="44.0.0" />
  <PackageReference Include="Azure.Storage.Blobs" Version="12.19.1" />
</ItemGroup>
```

## Verification

After installation, verify the packages:

```bash
dotnet list package
```

Expected output should include:
```
Stripe.net                              44.0.0
Azure.Storage.Blobs                     12.19.1
```

## Build and Restore

```bash
# Restore packages
dotnet restore

# Build solution
dotnet build

# Verify no errors
```
