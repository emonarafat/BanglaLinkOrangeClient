# Banglalink Orange OAuth 2.0 - OpenAPI Specification v1.1

## Overview
This document incorporates the official Banglalink Orange OpenAPI Authorization Documentation v1.1 into the project. The specification defines the OAuth 2.0 authentication and authorization flows for Banglalink services.

## Document Reference
- **Source**: OPENAPI_Spec_v1.1.pdf
- **Location**: `c:\Pran-RFL\Banglalink\OPENAPI_Spec_v1.1.pdf`
- **Purpose**: Official authentication specification for Banglalink Orange services

## Key Endpoints

### Authorization Server
The Banglalink OAuth 2.0 authorization server provides the following endpoints:

- **Token Endpoint**: `/oauth/oauth/token` (POST)
  - Used for obtaining and refreshing access tokens
  - Supports Password Grant and Refresh Token Grant flows

### Supported Grant Types
1. **Password Grant** (Resource Owner Password Credentials)
   - Used for initial authentication with username and password
   - Request format: `application/x-www-form-urlencoded`
   - Returns: Access Token, Token Type, Expires In, Refresh Token

2. **Refresh Token Grant** 
   - Used for obtaining new access tokens using refresh token
   - Automatically extends session without user re-authentication
   - Returns: New Access Token, Token Type, Expires In

## Authentication Method

### Basic Authentication
All requests to the OAuth 2.0 endpoint must include Basic Authentication:
```
Authorization: Basic <Base64(client_id:client_secret)>
```

## Token Response Format

### Success Response (200 OK)
```json
{
  "access_token": "string",
  "token_type": "Bearer",
  "expires_in": 36000,
  "refresh_token": "string"
}
```

### Error Response
- **401 Unauthorized**: Invalid credentials
- **400 Bad Request**: Invalid request parameters
- **500 Internal Server Error**: Server-side error

## Implementation in This Library

The `Othoba.BanglaLinkOrange` library fully implements the specification as defined:

### 1. Password Grant Flow
- **Class**: `BanglalinkAuthClient`
- **Method**: `AuthenticateAsync(string username, string password)`
- Handles automatic token caching and lifecycle management

### 2. Refresh Token Grant Flow
- **Method**: `RefreshTokenAsync()`
- Automatic invocation when token expires
- Maintains seamless authentication experience

### 3. Configuration
- **Class**: `BanglalinkClientConfiguration`
- Stores endpoint URL, credentials, and settings
- Validates configuration before use

### 4. Token Management
- **Class**: `BanglalinkTokenResponse`
- Parses OAuth 2.0 responses
- Tracks token expiration and lifetime
- Supports automatic refresh

## Security Features

### 1. Token Lifecycle Management
- Automatic expiration tracking
- Proactive token refresh before expiration
- Secure token storage in memory cache

### 2. Error Handling
- **`BanglalinkClientException`**: Custom exceptions for OAuth errors
- Comprehensive error messages for troubleshooting
- Specific exception types for different error scenarios

### 3. Retry Policy
- Polly-based resilience with exponential backoff
- Automatic retry on transient failures
- Configurable retry count and delays

## Integration Points

### Dependency Injection
```csharp
services.AddBanglalinkAuthClient(config => {
    config.TokenEndpoint = "https://oauth-server/oauth/oauth/token";
    config.ClientId = "your-client-id";
    config.ClientSecret = "your-client-secret";
});
```

### Direct Usage
```csharp
var client = new BanglalinkAuthClient(configuration);
var token = await client.AuthenticateAsync("username", "password");
```

## Compliance Notes

✅ **Fully Compliant with OpenAPI Specification v1.1**
- Implements both Password and Refresh Token grant flows
- Supports Basic authentication as specified
- Handles all required response fields
- Provides proper error handling for specified error conditions

## Additional Resources

- **Official PDF**: See attached `OPENAPI_Spec_v1.1.pdf` for complete specification
- **Library Documentation**: See [README.md](../README.md) for usage examples
- **Getting Started**: See [GETTING_STARTED.md](../GETTING_STARTED.md) for quick start guide
- **API Reference**: See [API_REFERENCE.md](../API_REFERENCE.md) for detailed API documentation

## Version History

| Version | Date | Notes |
|---------|------|-------|
| 1.0 | Jan 12, 2026 | Initial OpenAPI spec v1.1 incorporation |

## Notes

- All timestamps in responses are in Unix epoch format (seconds since Jan 1, 1970)
- Token expiration is always in seconds (`expires_in`)
- The library automatically handles all token lifecycle management
- Configuration validation ensures secure operation
