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

    #region Pagination Tests

    [Fact]
    public async Task GetLogs_WithValidPagination_ReturnsCorrectPage()
    {
        Given_we_have_seeded_test_data();
        var (page, pageSize) = And_we_have_pagination_parameters(1, 2);
        var logs = await When_we_get_logs_with_pagination(page, pageSize);
        Then_logs_should_have_count(logs, 2);
        And_most_recent_log_should_be_first(logs);
    }

    [Fact]
    public async Task GetLogs_Pagination_CalculatesTotalPagesCorrectly()
    {
        Given_we_have_seeded_test_data();
        var totalCount = await When_we_count_total_logs();
        var pageSize = And_we_have_page_size(2);
        var totalPages = When_we_calculate_total_pages(totalCount, pageSize);
        Then_total_pages_should_be(totalPages, 3);
    }

    #endregion

    #region Filter Tests

    [Fact]
    public async Task GetLogs_FilterByLevel_ReturnsOnlyMatchingLogs()
    {
        Given_we_have_seeded_test_data();
        var level = And_we_have_error_level_filter();
        var logs = await When_we_filter_logs_by_level(level);
        Then_logs_should_have_count(logs, 2);
        And_all_logs_should_have_level(logs, level);
    }

    [Fact]
    public async Task GetLogs_FilterByCategory_ReturnsOnlyMatchingLogs()
    {
        Given_we_have_seeded_test_data();
        var category = And_we_have_api_request_category_filter();
        var logs = await When_we_filter_logs_by_category(category);
        Then_logs_should_have_count(logs, 5);
        And_all_logs_should_have_category(logs, category);
    }

    [Fact]
    public async Task GetLogs_FilterByStartDate_ReturnsOnlyLogsAfterDate()
    {
        Given_we_have_seeded_test_data();
        var startDate = And_we_have_start_date_filter();
        var logs = await When_we_filter_logs_by_start_date(startDate);
        Then_logs_count_should_be_at_least(logs, 2);
    }

    [Fact]
    public async Task GetLogs_FilterByEndDate_ReturnsOnlyLogsBeforeDate()
    {
        Given_we_have_seeded_test_data();
        var endDate = And_we_have_end_date_filter();
        var logs = await When_we_filter_logs_by_end_date(endDate);
        Then_logs_count_should_be_at_least(logs, 3);
    }

    [Fact]
    public async Task GetLogs_CombinedFilters_ReturnsCorrectSubset()
    {
        Given_we_have_seeded_test_data();
        var (level, category) = And_we_have_combined_filters();
        var logs = await When_we_filter_logs_by_level_and_category(level, category);
        Then_logs_should_have_count(logs, 2);
        And_all_logs_should_match_filters(logs, level, category);
    }

    #endregion

    #region Validation Tests

    [Fact]
    public void PageValidation_NegativePage_IsCorrectedToOne()
    {
        var page = Given_we_have_negative_page();
        var correctedPage = When_we_validate_page_number(page);
        Then_page_should_be(correctedPage, 1);
    }

    [Fact]
    public void PageSizeValidation_TooLarge_IsClampedTo100()
    {
        var pageSize = Given_we_have_oversized_page_size();
        var correctedPageSize = When_we_validate_page_size(pageSize);
        Then_page_size_should_be(correctedPageSize, 100);
    }

    [Fact]
    public void PageSizeValidation_Zero_IsCorrectedToOne()
    {
        var pageSize = Given_we_have_zero_page_size();
        var correctedPageSize = When_we_validate_page_size(pageSize);
        Then_page_size_should_be(correctedPageSize, 1);
    }

    #endregion

    #region Given Methods

    void Given_we_have_seeded_test_data()
    {
        // Data already seeded in constructor
    }

    int Given_we_have_negative_page()
    {
        return -1;
    }

    int Given_we_have_oversized_page_size()
    {
        return 1000;
    }

    int Given_we_have_zero_page_size()
    {
        return 0;
    }

    #endregion

    #region And Methods

    (int page, int pageSize) And_we_have_pagination_parameters(int page, int pageSize)
    {
        return (page, pageSize);
    }

    int And_we_have_page_size(int pageSize)
    {
        return pageSize;
    }

    string And_we_have_error_level_filter()
    {
        return "Error";
    }

    string And_we_have_api_request_category_filter()
    {
        return "ApiRequest";
    }

    DateTime And_we_have_start_date_filter()
    {
        return DateTime.UtcNow.AddMinutes(-3);
    }

    DateTime And_we_have_end_date_filter()
    {
        return DateTime.UtcNow.AddMinutes(-3);
    }

    (string level, string category) And_we_have_combined_filters()
    {
        return ("Information", "ApiRequest");
    }

    void And_most_recent_log_should_be_first(List<ApplicationLog> logs)
    {
        logs[0].Message.Should().Contain("Request 5");
    }

    void And_all_logs_should_have_level(List<ApplicationLog> logs, string level)
    {
        logs.Should().AllSatisfy(l => l.Level.Should().Be(level));
    }

    void And_all_logs_should_have_category(List<ApplicationLog> logs, string category)
    {
        logs.Should().AllSatisfy(l => l.Category.Should().Be(category));
    }

    void And_all_logs_should_match_filters(List<ApplicationLog> logs, string level, string category)
    {
        logs.Should().AllSatisfy(l =>
        {
            l.Level.Should().Be(level);
            l.Category.Should().Be(category);
        });
    }

    #endregion

    #region When Methods

    async Task<List<ApplicationLog>> When_we_get_logs_with_pagination(int page, int pageSize)
    {
        return await _context.ApplicationLogs
            .OrderByDescending(l => l.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    async Task<int> When_we_count_total_logs()
    {
        return await _context.ApplicationLogs.CountAsync();
    }

    int When_we_calculate_total_pages(int totalCount, int pageSize)
    {
        return (int)Math.Ceiling(totalCount / (double)pageSize);
    }

    async Task<List<ApplicationLog>> When_we_filter_logs_by_level(string level)
    {
        return await _context.ApplicationLogs
            .Where(l => l.Level == level)
            .ToListAsync();
    }

    async Task<List<ApplicationLog>> When_we_filter_logs_by_category(string category)
    {
        return await _context.ApplicationLogs
            .Where(l => l.Category == category)
            .ToListAsync();
    }

    async Task<List<ApplicationLog>> When_we_filter_logs_by_start_date(DateTime startDate)
    {
        return await _context.ApplicationLogs
            .Where(l => l.Timestamp >= startDate)
            .ToListAsync();
    }

    async Task<List<ApplicationLog>> When_we_filter_logs_by_end_date(DateTime endDate)
    {
        return await _context.ApplicationLogs
            .Where(l => l.Timestamp <= endDate)
            .ToListAsync();
    }

    async Task<List<ApplicationLog>> When_we_filter_logs_by_level_and_category(string level, string category)
    {
        return await _context.ApplicationLogs
            .Where(l => l.Level == level && l.Category == category)
            .ToListAsync();
    }

    int When_we_validate_page_number(int page)
    {
        return Math.Max(page, 1);
    }

    int When_we_validate_page_size(int pageSize)
    {
        return Math.Clamp(pageSize, 1, 100);
    }

    #endregion

    #region Then Methods

    void Then_logs_should_have_count(List<ApplicationLog> logs, int expectedCount)
    {
        logs.Should().HaveCount(expectedCount);
    }

    void Then_logs_count_should_be_at_least(List<ApplicationLog> logs, int minimumCount)
    {
        logs.Count.Should().BeGreaterThanOrEqualTo(minimumCount);
    }

    void Then_total_pages_should_be(int totalPages, int expected)
    {
        totalPages.Should().Be(expected);
    }

    void Then_page_should_be(int page, int expected)
    {
        page.Should().Be(expected);
    }

    void Then_page_size_should_be(int pageSize, int expected)
    {
        pageSize.Should().Be(expected);
    }

    #endregion

    #region Helper Methods

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

    #endregion

    public void Dispose()
    {
        _context.Dispose();
    }
}
