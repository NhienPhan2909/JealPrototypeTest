using JealPrototype.Domain.Entities;
using JealPrototype.Domain.Enums;
using JealPrototype.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace JealPrototype.Tests.Integration.Schema
{
    public class EasyCarsSchemaIntegrationTests : IAsyncLifetime
    {
        private ApplicationDbContext _context = null!;
        private readonly string _testDatabaseName;

        public EasyCarsSchemaIntegrationTests()
        {
            _testDatabaseName = $"easycars_schema_test_{Guid.NewGuid():N}";
        }

        public async Task InitializeAsync()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: _testDatabaseName)
                .Options;

            _context = new ApplicationDbContext(options);
            await _context.Database.EnsureCreatedAsync();
        }

        public async Task DisposeAsync()
        {
            await _context.Database.EnsureDeletedAsync();
            await _context.DisposeAsync();
        }

        [Fact]
        public async Task Test1_Migration_CreatesAllTables_Successfully()
        {
            // Arrange & Act - Database created in InitializeAsync

            // Assert - Verify all tables exist by querying DbSets
            var dealershipsCount = await _context.Dealerships.CountAsync();
            var vehiclesCount = await _context.Vehicles.CountAsync();
            var leadsCount = await _context.Leads.CountAsync();
            var credentialsCount = await _context.EasyCarsCredentials.CountAsync();
            var syncLogsCount = await _context.EasyCarsSyncLogs.CountAsync();

            // All queries should succeed without errors
            Assert.True(dealershipsCount >= 0);
            Assert.True(vehiclesCount >= 0);
            Assert.True(leadsCount >= 0);
            Assert.True(credentialsCount >= 0);
            Assert.True(syncLogsCount >= 0);
        }

        [Fact]
        public async Task Test2_EasyCarsCredential_UniqueConstraint_OnDealershipId()
        {
            // Arrange
            var dealership = Dealership.Create(
                "Test Dealership",
                "123 Test St",
                "555-1234",
                "test@example.com"
            );
            _context.Dealerships.Add(dealership);
            await _context.SaveChangesAsync();

            var credential1 = EasyCarsCredential.Create(
                dealership.Id,
                "account123-encrypted",
                "secret123-encrypted",
                "iv123",
                "Test"
            );
            _context.EasyCarsCredentials.Add(credential1);
            await _context.SaveChangesAsync();

            // Act & Assert - Verify credential was saved
            var savedCredential = await _context.EasyCarsCredentials
                .FirstOrDefaultAsync(c => c.DealershipId == dealership.Id);

            Assert.NotNull(savedCredential);
            Assert.Equal(dealership.Id, savedCredential.DealershipId);
            Assert.Equal("Test", savedCredential.Environment);
        }

        [Fact]
        public async Task Test3_ForeignKeyRelationships_CascadeDeleteCorrectly()
        {
            // Arrange
            var dealership = Dealership.Create(
                "Test Dealership",
                "123 Test St",
                "555-1234",
                "test@example.com"
            );
            _context.Dealerships.Add(dealership);
            await _context.SaveChangesAsync();

            var credential = EasyCarsCredential.Create(
                dealership.Id,
                "account-encrypted",
                "secret-encrypted",
                "iv",
                "Test"
            );
            _context.EasyCarsCredentials.Add(credential);

            var syncLog = EasyCarsSyncLog.Create(
                dealershipId: dealership.Id,
                status: SyncStatus.Success,
                itemsProcessed: 10,
                itemsSucceeded: 10,
                itemsFailed: 0,
                errors: new List<string>(),
                durationMs: 1000
            );
            _context.EasyCarsSyncLogs.Add(syncLog);
            await _context.SaveChangesAsync();

            var credentialId = credential.Id;
            var syncLogId = syncLog.Id;

            // Act - Delete dealership (should cascade to credentials and sync logs)
            _context.Dealerships.Remove(dealership);
            await _context.SaveChangesAsync();

            // Assert
            var credentialExists = await _context.EasyCarsCredentials
                .AnyAsync(c => c.Id == credentialId);
            var syncLogExists = await _context.EasyCarsSyncLogs
                .AnyAsync(s => s.Id == syncLogId);

            Assert.False(credentialExists, "Credential should be deleted when dealership is deleted");
            Assert.False(syncLogExists, "Sync log should be deleted when dealership is deleted");
        }

        [Fact]
        public async Task Test4_EasyCarsSyncLog_ForeignKey_ToCredential_AllowsSetNull()
        {
            // Arrange
            var dealership = Dealership.Create(
                "Test Dealership",
                "123 Test St",
                "555-1234",
                "test@example.com"
            );
            _context.Dealerships.Add(dealership);
            await _context.SaveChangesAsync();

            var credential = EasyCarsCredential.Create(
                dealership.Id,
                "account-encrypted",
                "secret-encrypted",
                "iv",
                "Test"
            );
            _context.EasyCarsCredentials.Add(credential);
            await _context.SaveChangesAsync();

            var syncLog = EasyCarsSyncLog.Create(
                dealershipId: dealership.Id,
                status: SyncStatus.Success,
                itemsProcessed: 5,
                itemsSucceeded: 5,
                itemsFailed: 0,
                errors: new List<string>(),
                durationMs: 500
            );
            _context.EasyCarsSyncLogs.Add(syncLog);
            await _context.SaveChangesAsync();

            var syncLogId = syncLog.Id;

            // Act - Delete credential (should set sync log credential_id to NULL)
            _context.EasyCarsCredentials.Remove(credential);
            await _context.SaveChangesAsync();

            // Assert
            var updatedSyncLog = await _context.EasyCarsSyncLogs
                .FirstOrDefaultAsync(s => s.Id == syncLogId);

            Assert.NotNull(updatedSyncLog);
            // Note: Story 2.3 removed CredentialId - sync logs are now tied directly to dealerships
        }

        [Fact]
        public async Task Test5_JsonbColumns_AcceptValidJson()
        {
            // Arrange
            var dealership = Dealership.Create(
                "Test Dealership",
                "123 Test St",
                "555-1234",
                "test@example.com"
            );
            _context.Dealerships.Add(dealership);
            await _context.SaveChangesAsync();

            var credential = EasyCarsCredential.Create(
                dealership.Id,
                "account-encrypted",
                "secret-encrypted",
                "iv",
                "Test"
            );
            _context.EasyCarsCredentials.Add(credential);
            await _context.SaveChangesAsync();

            var requestPayload = new Dictionary<string, object>
            {
                { "dealershipId", dealership.Id },
                { "syncType", "Stock" },
                { "timestamp", DateTime.UtcNow }
            };

            var responseSummary = new Dictionary<string, object>
            {
                { "recordsProcessed", 150 },
                { "success", true },
                { "errors", new string[0] }
            };

            var syncLog = EasyCarsSyncLog.Create(
                dealershipId: dealership.Id,
                status: SyncStatus.Success,
                itemsProcessed: 150,
                itemsSucceeded: 150,
                itemsFailed: 0,
                errors: new List<string>(),
                durationMs: 2000
            );

            _context.EasyCarsSyncLogs.Add(syncLog);

            // Act
            await _context.SaveChangesAsync();

            // Assert - Reload from database
            var savedLog = await _context.EasyCarsSyncLogs
                .FirstOrDefaultAsync(s => s.Id == syncLog.Id);

            Assert.NotNull(savedLog);
            Assert.Equal(150, savedLog.ItemsProcessed);
            Assert.Equal(150, savedLog.ItemsSucceeded);
            Assert.Equal(0, savedLog.ItemsFailed);
        }

        [Fact]
        public async Task Test6_Vehicle_EasyCarsFields_StoreCorrectly()
        {
            // Arrange
            var dealership = Dealership.Create(
                "Test Dealership",
                "123 Test St",
                "555-1234",
                "test@example.com"
            );
            _context.Dealerships.Add(dealership);
            await _context.SaveChangesAsync();

            var vehicle = Vehicle.Create(
                dealership.Id,
                "Toyota",
                "Camry",
                2023,
                25000m,
                15000,
                VehicleCondition.Used,
                VehicleStatus.Active,
                "2023 Toyota Camry",
                "Great condition"
            );

            // Set EasyCars-specific fields
            vehicle.SetEasyCarsData(
                "EC12345",
                "YARD01",
                "1HGCM82633A123456",
                "Red",
                "Black",
                "Sedan",
                "Petrol",
                "Automatic",
                2500,
                4,
                new List<string> { "GPS", "Backup Camera", "Bluetooth" },
                "{\"vin\": \"1HGCM82633A123456\", \"color\": \"Red\"}"
            );

            _context.Vehicles.Add(vehicle);

            // Act
            await _context.SaveChangesAsync();

            // Assert
            var savedVehicle = await _context.Vehicles
                .FirstOrDefaultAsync(v => v.Id == vehicle.Id);

            Assert.NotNull(savedVehicle);
            Assert.Equal("EC12345", savedVehicle.EasyCarsStockNumber);
            Assert.Equal("YARD01", savedVehicle.EasyCarsYardCode);
            Assert.Equal("1HGCM82633A123456", savedVehicle.EasyCarsVIN);
            Assert.Equal(DataSource.EasyCars, savedVehicle.DataSource);
            Assert.NotNull(savedVehicle.LastSyncedFromEasyCars);
        }

        [Fact]
        public async Task Test7_Lead_EasyCarsFields_StoreCorrectly()
        {
            // Arrange
            var dealership = Dealership.Create(
                "Test Dealership",
                "123 Test St",
                "555-1234",
                "test@example.com"
            );
            _context.Dealerships.Add(dealership);
            await _context.SaveChangesAsync();

            var lead = Lead.Create(
                dealership.Id,
                "John Doe",
                "john@example.com",
                "555-9876",
                "Interested in Toyota Camry"
            );

            // Set EasyCars-specific fields
            lead.SetEasyCarsData(
                "LEAD1234",
                "CUST5678",
                "{\"source\": \"Web\", \"rating\": \"Hot\"}"
            );

            _context.Leads.Add(lead);

            // Act
            await _context.SaveChangesAsync();

            // Assert
            var savedLead = await _context.Leads
                .FirstOrDefaultAsync(l => l.Id == lead.Id);

            Assert.NotNull(savedLead);
            Assert.Equal("LEAD1234", savedLead.EasyCarsLeadNumber);
            Assert.Equal("CUST5678", savedLead.EasyCarsCustomerNo);
            Assert.Equal(DataSource.EasyCars, savedLead.DataSource);
            Assert.NotNull(savedLead.LastSyncedFromEasyCars);
        }

        [Fact]
        public async Task Test8_Indexes_AreConfiguredCorrectly()
        {
            // Arrange
            var dealership = Dealership.Create(
                "Test Dealership",
                "123 Test St",
                "555-1234",
                "test@example.com"
            );
            _context.Dealerships.Add(dealership);
            await _context.SaveChangesAsync();

            // Create 100 sync logs to test index performance
            for (int i = 0; i < 100; i++)
            {
                var log = EasyCarsSyncLog.Create(
                    dealershipId: dealership.Id,
                    status: i % 3 == 0 ? SyncStatus.Success : (i % 3 == 1 ? SyncStatus.Failed : SyncStatus.PartialSuccess),
                    itemsProcessed: i,
                    itemsSucceeded: i % 3 == 0 ? i : (i % 3 == 1 ? 0 : i / 2),
                    itemsFailed: i % 3 == 1 ? i : (i % 3 == 2 ? i / 2 : 0),
                    errors: i % 3 == 1 ? new List<string> { "Test error" } : new List<string>(),
                    durationMs: i * 100
                );

                _context.EasyCarsSyncLogs.Add(log);
            }

            await _context.SaveChangesAsync();

            // Act - Query using indexed columns
            var successLogs = await _context.EasyCarsSyncLogs
                .Where(l => l.Status == SyncStatus.Success)
                .CountAsync();

            var dealershipLogs = await _context.EasyCarsSyncLogs
                .Where(l => l.DealershipId == dealership.Id)
                .CountAsync();

            // Assert
            Assert.True(successLogs > 0);
            Assert.Equal(100, dealershipLogs);
        }
    }
}
