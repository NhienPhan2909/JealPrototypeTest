# Testing Strategy

### Unit Testing

**Test Coverage Targets:**
- **Domain Entities:** 100% - Test all business logic and validation
- **Application Use Cases:** 90%+ - Mock dependencies, test workflows
- **Infrastructure Services:** 85%+ - Mock external dependencies (API, DB)
- **Mappers:** 95%+ - Test all field mappings and edge cases
- **Controllers:** 80%+ - Test request validation and response formatting

**Key Test Scenarios:**

**CredentialEncryptionService Tests:**
```csharp
[Fact]
public void Encrypt_Decrypt_RoundTrip_Success()
{
    // Arrange
    var service = new CredentialEncryptionService(config);
    var plaintext = "TestSecret123";
    
    // Act
    var encrypted = service.Encrypt(plaintext);
    var decrypted = service.Decrypt(encrypted.Ciphertext, encrypted.InitializationVector);
    
    // Assert
    Assert.Equal(plaintext, decrypted);
}

[Fact]
public void Decrypt_WithTamperedCiphertext_ThrowsException()
{
    // Arrange
    var service = new CredentialEncryptionService(config);
    var encrypted = service.Encrypt("original");
    var tamperedCiphertext = encrypted.Ciphertext + "X";
    
    // Act & Assert
    Assert.Throws<CryptographicException>(() => 
        service.Decrypt(tamperedCiphertext, encrypted.InitializationVector));
}
```

**EasyCarsStockMapper Tests:**
```csharp
[Fact]
public void MapToVehicle_WithCompleteStockItem_MapsAllFields()
{
    // Arrange
    var mapper = new EasyCarsStockMapper();
    var stockItem = CreateTestStockItem();
    
    // Act
    var vehicle = mapper.MapToVehicle(stockItem, dealershipId: 1);
    
    // Assert
    Assert.Equal(stockItem.Make, vehicle.Make);
    Assert.Equal(stockItem.Model, vehicle.Model);
    Assert.Equal(stockItem.YearGroup, vehicle.Year);
    Assert.Equal(stockItem.Price, vehicle.Price);
    Assert.Equal(stockItem.StockNumber, vehicle.EasyCarsStockNumber);
    Assert.Equal(DataSourceType.EasyCars, vehicle.DataSource);
}

[Theory]
[InlineData(null, "Default Make")]
[InlineData("", "Default Make")]
public void MapToVehicle_WithMissingMake_UsesDefault(string? make, string expected)
{
    // Test null handling
}
```

**EasyCarsApiClient Tests (Mocked HttpClient):**
```csharp
[Fact]
public async Task RequestTokenAsync_WithValidCredentials_ReturnsToken()
{
    // Arrange
    var mockHandler = new MockHttpMessageHandler();
    mockHandler.When("*/RequestToken")
        .Respond("application/json", "{\"ReturnCode\":0,\"Response\":\"mock-token\"}");
    
    var httpClient = mockHandler.ToHttpClient();
    var apiClient = new EasyCarsApiClient(httpClientFactory, logger, config);
    
    // Act
    var token = await apiClient.RequestTokenAsync("account", "secret", CancellationToken.None);
    
    // Assert
    Assert.Equal("mock-token", token);
}

[Fact]
public async Task GetAdvertisementStocksAsync_WithAuthenticationFailure_ThrowsException()
{
    // Arrange
    var mockHandler = new MockHttpMessageHandler();
    mockHandler.When("*/GetAdvertisementStocks")
        .Respond("application/json", "{\"ReturnCode\":1,\"ErrorMsg\":\"Invalid credentials\"}");
    
    // Act & Assert
    await Assert.ThrowsAsync<EasyCarsAuthenticationException>(() => 
        apiClient.GetAdvertisementStocksAsync(token, account, null, CancellationToken.None));
}
```

---

### Integration Testing

**Test Database:**
- Use separate test PostgreSQL database
- Reset database between test runs
- Seed test data for known scenarios

**Key Integration Tests:**

**Sync Service Integration Tests:**
```csharp
[Fact]
public async Task SyncStockAsync_WithTestCredentials_CreatesVehicles()
{
    // Arrange
    var dealership = await CreateTestDealershipWithCredentialsAsync();
    var syncService = GetSyncService(); // Real dependencies, test DB
    
    // Act
    var result = await syncService.SyncStockAsync(dealership.Id, CancellationToken.None);
    
    // Assert
    Assert.True(result.Success);
    Assert.True(result.RecordsProcessed > 0);
    
    var vehicles = await vehicleRepo.GetByDealershipIdAsync(dealership.Id);
    Assert.NotEmpty(vehicles);
    Assert.All(vehicles, v => Assert.Equal(DataSourceType.EasyCars, v.DataSource));
}

[Fact]
public async Task SyncStockAsync_WithInvalidCredentials_LogsFailure()
{
    // Test authentication failure handling
}
```

**API Controller Integration Tests:**
```csharp
[Fact]
public async Task PostCredentials_WithValidRequest_ReturnsCreated()
{
    // Arrange
    var client = _factory.CreateClient();
    var token = await GetAdminTokenAsync();
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    
    var request = new
    {
        accountNumber = "test-account",
        accountSecret = "test-secret",
        environment = "Test"
    };
    
    // Act
    var response = await client.PostAsJsonAsync("/api/admin/easycars/credentials", request);
    
    // Assert
    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    var credential = await response.Content.ReadFromJsonAsync<EasyCarsCredential>();
    Assert.NotNull(credential);
    Assert.Equal("Test", credential.Environment);
}
```

---

### End-to-End Testing

**Test Scenarios:**
1. **Credential Management Flow:**
   - Admin logs in → navigates to EasyCars settings
   - Enters test credentials → clicks "Test Connection"
   - Sees success message → clicks "Save Credentials"
   - Credentials saved and displayed

2. **Manual Stock Sync Flow:**
   - Admin navigates to Sync Dashboard
   - Clicks "Sync Stock Now" → sees spinner
   - Waits for completion → sees success message with counts
   - Navigates to vehicle list → verifies new vehicles present

3. **Automatic Background Sync:**
   - Wait for scheduled sync time (or trigger manually via Hangfire)
   - Verify sync log created
   - Verify vehicles updated in database
   - Verify dashboard shows updated sync status

**E2E Testing Tools:**
- **Playwright** or **Cypress** for frontend automation
- **TestContainers** for PostgreSQL and Hangfire dependencies
- **WireMock** or **MockServer** for mocking EasyCars API

---

### Test Credentials

**EasyCars Test Environment:**
- **Base URL:** `https://testmy.easycars.com.au`
- **PublicID:** `AA20EE61-5CFA-458D-9AFB-C4E929EA18E6`
- **SecretKey:** `7326AF23-714A-41A5-A74F-EC77B4E4F2F2`

**Usage:**
- Integration tests use these credentials to call real EasyCars Test API
- No mocking for full end-to-end validation
- Test data isolated in EasyCars test environment

---
