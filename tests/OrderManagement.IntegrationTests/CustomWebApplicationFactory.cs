using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrderManagement.Infrastructure.Persistence;

namespace OrderManagement.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private static int _databaseCounter = 0;
    private readonly string _databaseName;

    public CustomWebApplicationFactory()
    {
        _databaseName = $"InMemoryTestDb_{Interlocked.Increment(ref _databaseCounter)}";
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // Configure test settings
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DevelopmentAuth:FixedUser:Email"] = "dev@martech.com",
                ["DevelopmentAuth:FixedUser:Password"] = "Senha@123",
                ["DevelopmentAuth:FixedUser:Role"] = "Admin",
                ["Jwt:Key"] = "YourSuperSecretKeyForJWTTokenGenerationWithMinimum32Characters",
                ["Jwt:Issuer"] = "OrderManagementAPI",
                ["Jwt:Audience"] = "OrderManagementClient"
            });
        });

        builder.ConfigureServices(services =>
        {
            // Remove all DbContext related registrations
            services.RemoveAll(typeof(ApplicationDbContext));
            services.RemoveAll(typeof(DbContextOptions<ApplicationDbContext>));
            services.RemoveAll<DbContextOptions>();

            // Add DbContext using in-memory database for tests with unique name
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase(_databaseName);
            });
        });
    }
}
