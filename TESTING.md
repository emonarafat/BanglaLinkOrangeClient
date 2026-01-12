# Testing Guide - Othoba.BanglaLinkOrange

This document provides comprehensive guidance on running, writing, and maintaining tests for the Banglalink OAuth 2.0 client library.

## Table of Contents

- [Overview](#overview)
- [Test Structure](#test-structure)
- [Running Tests](#running-tests)
- [Test Coverage](#test-coverage)
- [Test Fixtures and Mocking](#test-fixtures-and-mocking)
- [Writing New Tests](#writing-new-tests)
- [Best Practices](#best-practices)
- [Troubleshooting](#troubleshooting)

## Overview

The test suite provides comprehensive coverage for the Banglalink OAuth 2.0 client library using:

- **xUnit**: Unit testing framework for .NET
- **Moq**: Mocking library for isolating dependencies
- **FluentAssertions**: Fluent assertion library for readable test assertions
- **Multi-framework support**: Tests run on .NET 6.0 and .NET 8.0

## Test Structure

```
tests/
└── Othoba.BanglaLinkOrangeClient.Tests/
    ├── Othoba.BanglaLinkOrangeClient.Tests.csproj (references Othoba.BanglaLinkOrange)
    ├── Unit/
    │   ├── BanglalinkAuthClientTests.cs
    │   ├── BanglalinkClientConfigurationTests.cs
    │   ├── BanglalinkTokenResponseTests.cs
    │   ├── BasicAuthenticationGeneratorTests.cs
    │   ├── ServiceCollectionExtensionsTests.cs
    │   ├── ExceptionTests.cs
    │   └── AuthenticationFlowIntegrationTests.cs
    └── Fixtures/
        └── BanglalinkAuthClientFixture.cs
```

### Test Categories

#### 1. **Unit Tests** (Unit/ folder)
- **BanglalinkAuthClientTests**: Main OAuth client functionality
  - Constructor validation
  - Authentication flows
  - Token refresh
  - Token caching
  - Error handling

- **BanglalinkClientConfigurationTests**: Configuration validation
  - Validation logic
  - Error messages
  - Default values

- **BanglalinkTokenResponseTests**: Token response model
  - Token validity checks
  - Expiration calculations
  - Token storage

- **BasicAuthenticationGeneratorTests**: Utility functions
  - Base64 encoding
  - Parameter validation
  - Deterministic behavior

- **ServiceCollectionExtensionsTests**: Dependency injection
  - Registration with configuration action
  - Registration with configuration instance
  - Lifetime management
  - Multiple registrations

- **ExceptionTests**: Exception hierarchy
  - Exception creation
  - Inheritance validation
  - Exception catching

#### 2. **Integration Tests** (Unit/ folder)
- **AuthenticationFlowIntegrationTests**: End-to-end scenarios
  - Complete authentication flow
  - Token refresh flow
  - Cache management
  - Error recovery
  - Concurrent access

#### 3. **Test Fixtures** (Fixtures/ folder)
- **BanglalinkAuthClientFixture**: Reusable test data and mocks
  - Valid/invalid configurations
  - Token response templates
  - Mock HttpClient creation

## Running Tests

### Prerequisites

- .NET SDK 6.0 or 8.0 or higher
- Visual Studio Code with C# extension or Visual Studio

### Run All Tests

```bash
cd tests/Othoba.BanglaLinkOrangeClient.Tests (references src/Othoba.BanglaLinkOrange/)
dotnet test
```

### Run Tests for Specific Framework

```bash
# .NET 6.0
dotnet test -f net6.0

# .NET 8.0
dotnet test -f net8.0
```

### Run Specific Test Class

```bash
dotnet test --filter "FullyQualifiedName~BanglalinkAuthClientTests"
```

### Run Specific Test Method

```bash
dotnet test --filter "FullyQualifiedName~BanglalinkAuthClientTests.AuthenticateAsync_WithValidCredentials_ShouldReturnTokenResponse"
```

### Run Tests with Verbose Output

```bash
dotnet test -v d  # Debug verbosity
dotnet test -v m  # Minimal verbosity
```

### Run Tests with Coverage Report

```bash
# Install coverage tool
dotnet tool install -g coverlet.console

# Run coverage
coverlet ./bin/Debug/net8.0/Othoba.BanglaLinkOrangeClient.Tests.dll \
  --target "dotnet" \
  --targetargs "test --no-build" \
  --format opencover
```

## Test Coverage

### Coverage Summary

The test suite covers:

| Component | Coverage | Tests |
|-----------|----------|-------|
| BanglalinkAuthClient | 95%+ | 15 tests |
| BanglalinkClientConfiguration | 100% | 11 tests |
| BanglalinkTokenResponse | 100% | 14 tests |
| BasicAuthenticationGenerator | 100% | 12 tests |
| ServiceCollectionExtensions | 95%+ | 11 tests |
| Exception Hierarchy | 100% | 10 tests |
| Integration Flows | 90%+ | 11 tests |
| **Total** | **95%+** | **84 tests** |

### Coverage Goals

- **Core Authentication**: 95%+ coverage
- **Configuration Validation**: 100% coverage
- **Error Handling**: 100% coverage
- **Integration Flows**: 90%+ coverage

## Test Fixtures and Mocking

### BanglalinkAuthClientFixture

The fixture provides reusable test data and mock creation:

```csharp
var fixture = new BanglalinkAuthClientFixture();

// Create valid configuration
var config = fixture.CreateValidConfiguration();

// Create valid token response
var responseContent = fixture.CreateValidTokenResponse();

// Create mock HttpClient
var httpClient = fixture.CreateHttpClient(responseContent);

// Create invalid configuration for negative tests
var invalidConfig = fixture.CreateInvalidConfiguration();
```

### Mocking HttpClient

Tests mock the HTTP transport layer using Moq:

```csharp
var mockHandler = new Mock<HttpMessageHandler>();
mockHandler
    .Protected()
    .Setup<Task<HttpResponseMessage>>(
        "SendAsync",
        ItExpr.IsAny<HttpRequestMessage>(),
        ItExpr.IsAny<CancellationToken>())
    .ReturnsAsync(response);

var httpClient = new HttpClient(mockHandler.Object);
```

### Test Constants

```csharp
const string TestBaseUrl = "http://localhost:8080";
const string TestClientId = "test-client-id";
const string TestClientSecret = "test-client-secret";
const string TestUsername = "test-user";
const string TestPassword = "test-password";
const string TestAccessToken = "eyJhbGci..."; // Valid JWT format
const string TestRefreshToken = "refresh-token-value";
```

## Writing New Tests

### Test Naming Convention

Tests follow the format: `MethodName_Scenario_ExpectedResult`

```csharp
[Fact]
public void Constructor_WithValidConfiguration_ShouldInitialize()
{
    // Arrange
    var config = _fixture.CreateValidConfiguration();
    
    // Act
    var client = new BanglalinkAuthClient(httpClient, config);
    
    // Assert
    client.Should().NotBeNull();
}
```

### Test Structure (AAA Pattern)

Every test follows Arrange-Act-Assert:

1. **Arrange**: Set up test data and dependencies
2. **Act**: Execute the code being tested
3. **Assert**: Verify the results

### Using xUnit Attributes

```csharp
// Single test case
[Fact]
public void SingleTest() { }

// Multiple test cases with data
[Theory]
[InlineData(400)]
[InlineData(401)]
public void MultipleTestCases(int statusCode) { }

// Skip test
[Fact(Skip = "Not ready yet")]
public void SkippedTest() { }
```

### Using Moq for Mocking

```csharp
var mockClient = new Mock<IBanglalinkAuthClient>();
mockClient
    .Setup(x => x.AuthenticateAsync())
    .ReturnsAsync(new BanglalinkTokenResponse { ... });

var result = await mockClient.Object.AuthenticateAsync();
```

### Using FluentAssertions

```csharp
// String assertions
result.Should().NotBeNullOrEmpty();
result.Should().StartWith("prefix");

// Collection assertions
collection.Should().HaveCount(5);
collection.Should().Contain(item);

// Type assertions
exception.Should().BeOfType<BanglalinkAuthenticationException>();

// Numeric assertions
value.Should().BeGreaterThan(0);
value.Should().BeLessThanOrEqualTo(100);
```

## Best Practices

### 1. Use Test Fixtures

Always use `BanglalinkAuthClientFixture` for test data:

```csharp
private readonly BanglalinkAuthClientFixture _fixture = new();

// Not this:
var config = new BanglalinkClientConfiguration { ... };

// Do this:
var config = _fixture.CreateValidConfiguration();
```

### 2. Mock External Dependencies

Always mock HttpClient and external services:

```csharp
// Mock the HTTP handler to avoid real network calls
var mockHandler = new Mock<HttpMessageHandler>();
var httpClient = new HttpClient(mockHandler.Object);
```

### 3. Test Both Success and Failure Paths

```csharp
[Fact]
public async Task AuthenticateAsync_WithValidCredentials_ShouldSucceed() { }

[Fact]
public async Task AuthenticateAsync_WithInvalidCredentials_ShouldThrow() { }
```

### 4. Use Descriptive Assertions

```csharp
// Good
result.AccessToken.Should().Be(expected);

// Not so good
Assert.NotNull(result);
Assert.NotEmpty(result.AccessToken);
```

### 5. Keep Tests Focused

Each test should verify one behavior:

```csharp
// Good - Tests one thing
[Fact]
public void IsValid_WithMissingBaseUrl_ShouldReturnFalse() { }

// Not good - Tests multiple things
[Fact]
public void IsValid_WithVariousInvalidConfigurations() { }
```

### 6. Use Theory for Multiple Scenarios

```csharp
[Theory]
[InlineData(400)]
[InlineData(401)]
[InlineData(500)]
public async Task AuthenticateAsync_WithErrorStatus_ShouldThrow(int statusCode) { }
```

## Troubleshooting

### Test Failures

#### Common Issues

1. **Timeout Errors**
   - Check mock setup is returning responses
   - Verify HttpClient timeout is set appropriately
   - Ensure async/await is used correctly

2. **Null Reference Exceptions**
   - Verify fixture initialization: `private readonly BanglalinkAuthClientFixture _fixture = new();`
   - Check mock setup with `Returns` or `ReturnsAsync`
   - Verify object properties are initialized

3. **Mock Verification Failures**
   - Ensure correct method names in mock setup
   - Verify `It.IsAny<>()` usage for parameters
   - Check `Times.Once()`, `Times.AtLeastOnce()` expectations

### Debug Tests

```bash
# Run with maximum verbosity
dotnet test -v diag

# Attach debugger in Visual Studio
# Set breakpoint and run test from Test Explorer
```

### Performance Issues

- Tests should complete in < 1 second each
- Avoid unnecessary file I/O
- Use in-memory mocks for external services
- Consider parallel test execution with `-p`

## Continuous Integration

### GitHub Actions Example

```yaml
name: Tests

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    strategy:
      matrix:
        dotnet-version: [6.0, 8.0]
    
    steps:
      - uses: actions/checkout@v3
      - uses: actions/setup-dotnet@v3
        with:
          dotnet-version: ${{ matrix.dotnet-version }}
      
      - name: Restore
        run: dotnet restore
      
      - name: Test
        run: dotnet test
```

## Contributing Tests

When contributing new code:

1. Write tests before implementation (TDD)
2. Ensure all tests pass locally
3. Maintain 95%+ code coverage
4. Follow naming conventions
5. Update this documentation if needed
6. Run tests on both .NET 6.0 and 8.0

## Test Statistics

- **Total Tests**: 84
- **Framework**: xUnit
- **Mocking**: Moq
- **Assertions**: FluentAssertions
- **Target Frameworks**: net6.0, net8.0
- **Average Test Duration**: 50-100ms
- **Total Test Suite Duration**: ~5-10 seconds

## Resources

- [xUnit Documentation](https://xunit.net/)
- [Moq GitHub](https://github.com/moq/moq4)
- [FluentAssertions](https://fluentassertions.com/)
- [Unit Testing Best Practices](https://learn.microsoft.com/en-us/dotnet/core/testing/)
