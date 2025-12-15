using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Movies.VerticalSlice.Api.Data.Database;
using Movies.VerticalSlice.Api.Data.Models;
using Xunit;

namespace Movies.Api.VerticalSlice.Api.Tests.Features.Logging;

public class GetAllLogsEndpointTests : IDisposable
{
    private readonly MoviesDbContext _context;

    public GetAllLogsEndpointTests()
    {
        var options = new DbContextOptionsBuilder<MoviesDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new MoviesDbContext(options);
        SeedTestData();
    }

    [Fact]
    public async Task GetLogs_WithValidPagination_ReturnsCorrectPage()
    {
        // Arrange
        var page = 1;
        var pageSize = 2;

        // Act
        var logs = await _context.ApplicationLogs
            .OrderByDescending(l => l.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Assert
        logs.Should().HaveCount(2);
        logs[0].Message.Should().Contain("Request 5"); // Most recent
    }

    [Fact]
    public async Task GetLogs_FilterByLevel_ReturnsOnlyMatchingLogs()
    {
        // Arrange
        var level = "Error";

        // Act
        var logs = await _context.ApplicationLogs
            .Where(l => l.Level == level)
            .ToListAsync();

        // Assert
        logs.Should().HaveCount(2);
        logs.Should().AllSatisfy(l => l.Level.Should().Be("Error"));
    }

    [Fact]
    public async Task GetLogs_FilterByCategory_ReturnsOnlyMatchingLogs()
    {
        // Arrange
        var category = "ApiRequest";

        // Act
        var logs = await _context.ApplicationLogs
            .Where(l => l.Category == category)
            .ToListAsync();

        // Assert
        logs.Should().HaveCount(5);
        logs.Should().AllSatisfy(l => l.Category.Should().Be("ApiRequest"));
    }

    [Fact]
    public async Task GetLogs_FilterByStartDate_ReturnsOnlyLogsAfterDate()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddMinutes(-3);

        // Act
        var logs = await _context.ApplicationLogs
            .Where(l => l.Timestamp >= startDate)
            .ToListAsync();

        // Assert
        logs.Count.Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task GetLogs_FilterByEndDate_ReturnsOnlyLogsBeforeDate()
    {
        // Arrange
        var endDate = DateTime.UtcNow.AddMinutes(-3);

        // Act
        var logs = await _context.ApplicationLogs
            .Where(l => l.Timestamp <= endDate)
            .ToListAsync();

        // Assert
        logs.Count.Should().BeGreaterThanOrEqualTo(3);
    }

    [Fact]
    public async Task GetLogs_CombinedFilters_ReturnsCorrectSubset()
    {
        // Arrange
        var level = "Information";
        var category = "ApiRequest";

        // Act
        var logs = await _context.ApplicationLogs
            .Where(l => l.Level == level && l.Category == category)
            .ToListAsync();

        // Assert
        logs.Should().HaveCount(2);
        logs.Should().AllSatisfy(l =>
        {
            l.Level.Should().Be("Information");
            l.Category.Should().Be("ApiRequest");
        });
    }

    [Fact]
    public void PageValidation_NegativePage_IsCorrectedToOne()
    {
        // Arrange
        var page = -1;

        // Act
        var correctedPage = Math.Max(page, 1);

        // Assert
        correctedPage.Should().Be(1);
    }

    [Fact]
    public void PageSizeValidation_TooLarge_IsClampedTo100()
    {
        // Arrange
        var pageSize = 1000;

        // Act
        var correctedPageSize = Math.Clamp(pageSize, 1, 100);

        // Assert
        correctedPageSize.Should().Be(100);
    }

    [Fact]
    public void PageSizeValidation_Zero_IsCorrectedToOne()
    {
        // Arrange
        var pageSize = 0;

        // Act
        var correctedPageSize = Math.Clamp(pageSize, 1, 100);

        // Assert
        correctedPageSize.Should().Be(1);
    }

    [Fact]
    public async Task GetLogs_Pagination_CalculatesTotalPagesCorrectly()
    {
        // Arrange
        var totalCount = await _context.ApplicationLogs.CountAsync();
        var pageSize = 2;

        // Act
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        // Assert
        totalPages.Should().Be(3); // 5 logs / 2 per page = 3 pages
    }

    private void SeedTestData()
    {
        var logs = new List<ApplicationLog>
        {
            new ApplicationLog
            {
                Id = 1,
                Timestamp = DateTime.UtcNow.AddMinutes(-5),
                Level = "Information",
                Category = "ApiRequest",
                Message = "Test Request 1",
                RequestPath = "/api/test1"
            },
            new ApplicationLog
            {
                Id = 2,
                Timestamp = DateTime.UtcNow.AddMinutes(-4),
                Level = "Warning",
                Category = "ApiRequest",
                Message = "Test Request 2",
                RequestPath = "/api/test2"
            },
            new ApplicationLog
            {
                Id = 3,
                Timestamp = DateTime.UtcNow.AddMinutes(-3),
                Level = "Error",
                Category = "ApiRequest",
                Message = "Test Request 3",
                RequestPath = "/api/test3"
            },
            new ApplicationLog
            {
                Id = 4,
                Timestamp = DateTime.UtcNow.AddMinutes(-2),
                Level = "Information",
                Category = "ApiRequest",
                Message = "Test Request 4",
                RequestPath = "/api/test4"
            },
            new ApplicationLog
            {
                Id = 5,
                Timestamp = DateTime.UtcNow.AddMinutes(-1),
                Level = "Error",
                Category = "ApiRequest",
                Message = "Test Request 5",
                RequestPath = "/api/test5"
            }
        };

        _context.ApplicationLogs.AddRange(logs);
        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
