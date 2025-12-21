using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Movies.VerticalSlice.Api.Configuration;
using Movies.VerticalSlice.Api.Data.Database;

namespace Movies.Api.VerticalSlice.Api.Tests.Infrastructure;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    // Use a unique database name per factory instance to prevent test isolation issues
    private readonly string _databaseName = $"IntegrationTestDb_{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string>
            {
                ["ConnectionStrings:DefaultConnection"] = "",
                ["JwtSettings: Secret"] = "TestSecretKeyForIntegrationTestsThatIsLongEnough123456",
                ["JwtSettings:Issuer"] = "TestIssuer",
                ["JwtSettings: Audience"] = "TestAudience",
                ["JwtSettings:ExpiryMinutes"] = "60"
            }!);
        });

        builder.ConfigureServices(services =>
        {
            // Remove existing DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<MoviesDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add in-memory database with unique name per factory instance
            services.AddDbContext<MoviesDbContext>(options =>
            {
                options.UseInMemoryDatabase(_databaseName);
            });

            // Override JWT settings for tests
            services.Configure<JwtSettings>(options =>
            {
                options.Secret = "TestSecretKeyForIntegrationTestsThatIsLongEnough123456";
                options.Issuer = "TestIssuer";
                options.Audience = "TestAudience";
                options.ExpiryMinutes = 60;
            });
        });

        builder.UseEnvironment("Testing");
    }
}