using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Movies.VerticalSlice.Api.Configuration;
using Movies.VerticalSlice.Api.Data.Database;

namespace Movies.Api.VerticalSlice.Api.Tests.Infrastructure;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string>
            {
                ["ConnectionStrings:DefaultConnection"] = "",
                ["JwtSettings:Secret"] = "TestSecretKeyForIntegrationTestsThatIsLongEnough123456",
                ["JwtSettings:Issuer"] = "TestIssuer",
                ["JwtSettings:Audience"] = "TestAudience",
                ["JwtSettings:ExpiryMinutes"] = "60"
            }!);
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<MoviesDbContext>>();
            services.RemoveAll<DbContextOptions>();
            services.RemoveAll<MoviesDbContext>();

            services.AddDbContext<MoviesDbContext>(options =>
            {
                options.UseInMemoryDatabase($"InMemoryTestDb_{Guid.NewGuid()}");
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

        // Configure to use HTTPS in tests
        builder.UseUrls("https://localhost:0");
    }
}
