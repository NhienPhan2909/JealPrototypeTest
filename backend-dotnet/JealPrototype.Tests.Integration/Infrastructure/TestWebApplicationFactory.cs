using JealPrototype.Domain.Entities;
using JealPrototype.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JealPrototype.Tests.Integration.Infrastructure;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            var dbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

            if (dbContextDescriptor != null)
                services.Remove(dbContextDescriptor);

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestDb");
            });

            var sp = services.BuildServiceProvider();

            using var scope = sp.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<ApplicationDbContext>();
            var logger = scopedServices.GetRequiredService<ILogger<TestWebApplicationFactory>>();

            db.Database.EnsureCreated();

            try
            {
                SeedTestData(db);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred seeding test data.");
            }
        });
    }

    private static void SeedTestData(ApplicationDbContext context)
    {
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        var dealership1 = Dealership.Create(
            name: "Test Dealership 1",
            address: "Test Location 1",
            phone: "1234567890",
            email: "contact1@test.com"
        );

        var dealership2 = Dealership.Create(
            name: "Test Dealership 2",
            address: "Test Location 2",
            phone: "0987654321",
            email: "contact2@test.com"
        );

        context.Dealerships.AddRange(dealership1, dealership2);
        context.SaveChanges(); // Save to get IDs

        var user1 = User.Create(
            username: "manager1",
            passwordHash: BCrypt.Net.BCrypt.HashPassword("Password123!"),
            email: "manager1@dealership1.com",
            fullName: "Manager One",
            userType: JealPrototype.Domain.Enums.UserType.DealershipOwner,
            dealershipId: dealership1.Id
        );

        var user2 = User.Create(
            username: "manager2",
            passwordHash: BCrypt.Net.BCrypt.HashPassword("Password123!"),
            email: "manager2@dealership2.com",
            fullName: "Manager Two",
            userType: JealPrototype.Domain.Enums.UserType.DealershipOwner,
            dealershipId: dealership2.Id
        );

        var adminUser = User.Create(
            username: "admin",
            passwordHash: BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            email: "admin@platform.com",
            fullName: "Admin User",
            userType: JealPrototype.Domain.Enums.UserType.Admin
        );

        context.Users.AddRange(user1, user2, adminUser);

        var vehicle1 = Vehicle.Create(
            dealershipId: dealership1.Id,
            make: "Toyota",
            model: "Camry",
            year: 2022,
            price: 25000,
            mileage: 15000,
            condition: JealPrototype.Domain.Enums.VehicleCondition.Used,
            status: JealPrototype.Domain.Enums.VehicleStatus.Active,
            title: "2022 Toyota Camry"
        );

        var vehicle2 = Vehicle.Create(
            dealershipId: dealership2.Id,
            make: "Honda",
            model: "Accord",
            year: 2023,
            price: 28000,
            mileage: 10000,
            condition: JealPrototype.Domain.Enums.VehicleCondition.Used,
            status: JealPrototype.Domain.Enums.VehicleStatus.Active,
            title: "2023 Honda Accord"
        );

        context.Vehicles.AddRange(vehicle1, vehicle2);

        context.SaveChanges();
    }
}
