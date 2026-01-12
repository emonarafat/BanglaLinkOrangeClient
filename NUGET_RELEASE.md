# NuGet Package Release

## Package Information
- **Package ID**: Othoba.BanglaLinkOrange
- **Version**: 1.0.0
- **Output Location**: `bin/nupkg/`
- **License**: MIT

## Files Generated
- ✅ `Othoba.BanglaLinkOrange.1.0.0.nupkg` (33,664 bytes)
- ✅ `Othoba.BanglaLinkOrange.1.0.0.snupkg` (22,751 bytes) - Debug symbols package

## Package Contents
The package includes compiled assemblies for multiple .NET versions:
- ✅ `lib/net6.0/Othoba.BanglaLinkOrange.dll`
- ✅ `lib/net8.0/Othoba.BanglaLinkOrange.dll`
- ✅ `README.md` - Full project documentation
- ✅ Package metadata and specification files

## API Features Included
1. **OAuth 2.0 Authentication**
   - Bearer token-based authentication
   - Token management and refresh handling
   - Error handling and retry logic

2. **Loyalty API Client**
   - Member profile retrieval
   - Loyalty points management
   - Tier status checking
   - Enriched member data with auth integration

3. **Service Layer**
   - Business logic abstraction
   - Error handling at all layers
   - Async/await support throughout

## How to Use the Package

### Via NuGet CLI
```bash
dotnet add package Othoba.BanglaLinkOrange --version 1.0.0
```

### Via Package Manager Console
```powershell
Install-Package Othoba.BanglaLinkOrange -Version 1.0.0
```

### Via .csproj File
```xml
<ItemGroup>
    <PackageReference Include="Othoba.BanglaLinkOrange" Version="1.0.0" />
</ItemGroup>
```

## Configuration Required

Add to `appsettings.json`:
```json
{
  "Banglalink": {
    "OAuth": {
      "BaseUrl": "https://api.banglalink.net/oauth2/",
      "ClientId": "your-client-id",
      "ClientSecret": "your-client-secret",
      "Username": "your-username",
      "Password": "your-password"
    },
    "Loyalty": {
      "BaseUrl": "https://openapi.banglalink.net/",
      "Timeout": 30,
      "RetryCount": 3,
      "RetryDelay": 500
    }
  }
}
```

## Registration in Program.cs
```csharp
// Single line registration for both services
builder.Services.AddBanglalink(builder.Configuration);
```

## Publishing to NuGet.org

To publish this package to NuGet.org:

```bash
dotnet nuget push bin/nupkg/Othoba.BanglaLinkOrange.1.0.0.nupkg \
  --api-key YOUR_NUGET_API_KEY \
  --source https://api.nuget.org/v3/index.json
```

## Package Metadata

| Property | Value |
|----------|-------|
| Title | Othoba Banglalink Orange OAuth 2.0 and Loyalty API Client |
| Description | Complete OAuth 2.0 authentication and Loyalty API client for Banglalink services with full error handling, retry logic, and dependency injection support |
| Authors | Development Team |
| License | MIT |
| Tags | banglalink, oauth, loyalty, api, client, authentication |
| Repository | https://github.com/your-org/Othoba.BanglaLinkOrange |

## Testing the Package

### 1. Create a test project
```bash
mkdir TestProject
cd TestProject
dotnet new console
dotnet add package Othoba.BanglaLinkOrange --version 1.0.0
```

### 2. Verify types are accessible
```csharp
using Othoba.BanglaLinkOrange.Clients;
using Othoba.BanglaLinkOrange.Services;

// All public types should be accessible
var client = new LoyaltyClient(httpClient, config);
```

## Framework Support
- .NET 6.0
- .NET 8.0

## Dependencies
- System.Net.Http
- Microsoft.Extensions.DependencyInjection
- Microsoft.Extensions.Configuration
- Polly (for resilience)
- System.Text.Json

## Build Information
- Configuration: Release
- Build Status: ✅ Successful
- Target Frameworks: net6.0, net8.0
- Platform: Windows (.NET CLI compatible on all platforms)

## Next Steps

1. **Publish to NuGet.org** (Optional)
   - Create account at https://www.nuget.org/
   - Generate API key
   - Run `dotnet nuget push` command

2. **Internal Distribution**
   - Host in your organization's NuGet feed
   - Distribute to teams via package reference

3. **Version Updates**
   - For next version, increment version number: `2.0.0`
   - Re-run build and pack commands
   - Update changelog and release notes

## Support and Documentation

Full documentation available in:
- [Quick Start Guide](./LOYALTY_QUICK_START.md)
- [API Reference](./LOYALTY_API_GUIDE.md)
- [Code Examples](./LOYALTY_API_EXAMPLES.md)
- [Implementation Checklist](./IMPLEMENTATION_CHECKLIST.md)

---
**Release Date**: January 12, 2026
**Status**: ✅ Ready for Distribution
