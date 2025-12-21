using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Movies.Api.VerticalSlice.Api.Tests.Infrastructure;
using Movies.VerticalSlice.Api.Data.Models;
using Movies.VerticalSlice.Api.Features.LoginAudit.GetAll;
using Movies.VerticalSlice.Api.Features.LoginAudit.GetByEmail;
using Movies.VerticalSlice.Api.Features.LoginAudit.GetByStatus;
using Movies.VerticalSlice.Api.Features.LoginAudit.GetById;
using Movies.VerticalSlice.Api.Features.LoginAudit.Shared;
using Movies.VerticalSlice.Api.Services;
using Xunit;

namespace Movies.Api.VerticalSlice.Api.Tests.Integration;

public class LoginAuditLogsEndpointsIntegrationTests : IntegrationTestBase
{
    public LoginAuditLogsEndpointsIntegrationTests(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    #region GetAllLoginAuditLogs Tests

    [Fact]
    public async Task GetLoginAuditLogs_WithoutAuthentication_Returns401Unauthorized()
    {
        var response = await When_we_get_all_login_audit_logs_without_authentication();
        Then_the_response_should_be_unauthorized(response);
    }

    [Fact]
    public async Task GetLoginAuditLogs_WithAuthentication_Returns200OK()
    {
        var authenticatedClient = And_we_have_an_authenticated_client(UserId, UserName);
        var response = await When_we_get_all_login_audit_logs(authenticatedClient);
        Then_the_response_should_be_ok(response);
    }

    [Fact]
    public async Task GetLoginAuditLogs_WithValidData_ReturnsPaginatedResults()
    {
        await Given_we_have_multiple_login_audit_logs();
        var authenticatedClient = And_we_have_an_authenticated_client(UserId, UserName);
        
        var response = await When_we_get_all_login_audit_logs(authenticatedClient, page: 1, pageSize: 5);
        
        Then_the_response_should_be_ok(response);
        var result = await response.Content.ReadFromJsonAsync<GetLoginAuditLogsResponse>();
        result.Data.Should().HaveCount(5);
        result.TotalCount.Should().BeGreaterThan(0);
        result.TotalPages.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetLoginAuditLogs_FilterByEmail_ReturnsMatchingLogs()
    {
        await Given_we_have_multiple_login_audit_logs();
        var authenticatedClient = And_we_have_an_authenticated_client(UserId, UserName);
        
        var response = await When_we_get_all_login_audit_logs(authenticatedClient, email: "test@example.com");
        
        Then_the_response_should_be_ok(response);
        var result = await response.Content.ReadFromJsonAsync<GetLoginAuditLogsResponse>();
        result.Data.Should().AllSatisfy(l => l.Email.Should().Contain("test@example.com"));
    }

    [Fact]
    public async Task GetLoginAuditLogs_FilterByStatus_ReturnsMatchingLogs()
    {
        await Given_we_have_multiple_login_audit_logs();
        var authenticatedClient = And_we_have_an_authenticated_client(UserId, UserName);
        
        var response = await When_we_get_all_login_audit_logs(authenticatedClient, status: "Success");
        
        Then_the_response_should_be_ok(response);
        var result = await response.Content.ReadFromJsonAsync<GetLoginAuditLogsResponse>();
        result.Data.Should().AllSatisfy(l => l.Status.Should().Be("Success"));
    }

    #endregion

    #region GetLoginAuditLogById Tests

    [Fact]
    public async Task GetLoginAuditLogById_ValidId_Returns200OK()
    {
        var logId = await Given_we_have_a_single_login_audit_log();
        var authenticatedClient = And_we_have_an_authenticated_client(UserId, UserName);
        
        var response = await When_we_get_login_audit_log_by_id(authenticatedClient, logId);
        
        Then_the_response_should_be_ok(response);
        var result = await response.Content.ReadFromJsonAsync<LoginAuditLogDetailDto>();
        result.Id.Should().Be(logId);
    }

    [Fact]
    public async Task GetLoginAuditLogById_InvalidId_Returns404NotFound()
    {
        var authenticatedClient = And_we_have_an_authenticated_client(UserId, UserName);
        var response = await When_we_get_login_audit_log_by_id(authenticatedClient, 99999);
        
        Then_the_response_should_be_not_found(response);
    }

    #endregion

    #region GetLoginAuditLogsByEmail Tests

    [Fact]
    public async Task GetLoginAuditLogsByEmail_ValidEmail_Returns200OK()
    {
        await Given_we_have_multiple_login_audit_logs();
        var authenticatedClient = And_we_have_an_authenticated_client(UserId, UserName);
        
        var response = await When_we_get_login_audit_logs_by_email(authenticatedClient, "test@example.com");
        
        Then_the_response_should_be_ok(response);
        var result = await response.Content.ReadFromJsonAsync<GetLoginAuditLogsByEmailResponse>();
        result.Data.Should().AllSatisfy(l => l.Email.Should().Be("test@example.com"));
        result.SuccessCount.Should().BeGreaterThanOrEqualTo(0);
        result.FailureCount.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task GetLoginAuditLogsByEmail_IncludesSuccessAndFailureCounts()
    {
        await Given_we_have_multiple_login_audit_logs();
        var authenticatedClient = And_we_have_an_authenticated_client(UserId, UserName);
        
        var response = await When_we_get_login_audit_logs_by_email(authenticatedClient, "test@example.com");
        
        var result = await response.Content.ReadFromJsonAsync<GetLoginAuditLogsByEmailResponse>();
        (result.SuccessCount + result.FailureCount).Should().Be(result.TotalCount);
    }

    #endregion

    #region GetLoginAuditLogsByStatus Tests

    [Fact]
    public async Task GetLoginAuditLogsByStatus_ValidStatus_Returns200OK()
    {
        await Given_we_have_multiple_login_audit_logs();
        var authenticatedClient = And_we_have_an_authenticated_client(UserId, UserName);
        
        var response = await When_we_get_login_audit_logs_by_status(authenticatedClient, "Success");
        
        Then_the_response_should_be_ok(response);
        var result = await response.Content.ReadFromJsonAsync<GetLoginAuditLogsByStatusResponse>();
        result.Status.Should().Be("Success");
        result.StatusBreakdown.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetLoginAuditLogsByStatus_IncludesStatusBreakdown()
    {
        await Given_we_have_multiple_login_audit_logs();
        var authenticatedClient = And_we_have_an_authenticated_client(UserId, UserName);
        
        var response = await When_we_get_login_audit_logs_by_status(authenticatedClient, "FailedLogin");
        
        var result = await response.Content.ReadFromJsonAsync<GetLoginAuditLogsByStatusResponse>();
        result.StatusBreakdown.Should().ContainKey("Success");
        result.StatusBreakdown.Should().ContainKey("FailedLogin");
    }

    #endregion

    #region Given Methods

    async Task Given_we_have_multiple_login_audit_logs()
    {
        var logs = new List<LoginAuditLog>
        {
            new LoginAuditLog
            {
                Timestamp = DateTime.UtcNow.AddHours(-5),
                Email = "test@example.com",
                Status = "Success",
                UserId = "user-1",
                UserName = "testuser",
                IpAddress = "192.168.1.1"
            },
            new LoginAuditLog
            {
                Timestamp = DateTime.UtcNow.AddHours(-4),
                Email = "test@example.com",
                Status = "InvalidCredentials",
                FailureReason = "Invalid password",
                IpAddress = "192.168.1.2"
            },
            new LoginAuditLog
            {
                Timestamp = DateTime.UtcNow.AddHours(-3),
                Email = "other@example.com",
                Status = "UserNotFound",
                FailureReason = "User not found",
                IpAddress = "192.168.1.3"
            },
            new LoginAuditLog
            {
                Timestamp = DateTime.UtcNow.AddHours(-2),
                Email = "test@example.com",
                Status = "Success",
                UserId = "user-1",
                UserName = "testuser",
                IpAddress = "192.168.1.1"
            },
            new LoginAuditLog
            {
                Timestamp = DateTime.UtcNow.AddHours(-1),
                Email = "another@example.com",
                Status = "FailedLogin",
                FailureReason = "Invalid password",
                IpAddress = "192.168.1.4"
            }
        };

        DbContext.LoginAuditLogs.AddRange(logs);
        await DbContext.SaveChangesAsync();
    }

    async Task<long> Given_we_have_a_single_login_audit_log()
    {
        var log = new LoginAuditLog
        {
            Timestamp = DateTime.UtcNow,
            Email = "single@example.com",
            Status = "Success",
            UserId = "user-123",
            UserName = "singleuser",
            IpAddress = "192.168.1.100"
        };

        DbContext.LoginAuditLogs.Add(log);
        await DbContext.SaveChangesAsync();
        return log.Id;
    }

    HttpClient And_we_have_an_authenticated_client(string userId, string userName)
    {
        return CreateAuthenticatedClient(userId, userName);
    }

    #endregion

    #region When Methods

    async Task<HttpResponseMessage> When_we_get_all_login_audit_logs(
        HttpClient client,
        int page = 1,
        int pageSize = 50,
        string? email = null,
        string? status = null)
    {
        var url = $"/api/login-audit-logs?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrEmpty(email))
            url += $"&email={email}";
        if (!string.IsNullOrEmpty(status))
            url += $"&status={status}";
        
        return await client.GetAsync(url);
    }

    async Task<HttpResponseMessage> When_we_get_all_login_audit_logs_without_authentication()
    {
        return await Client.GetAsync("/api/login-audit-logs");
    }

    async Task<HttpResponseMessage> When_we_get_login_audit_log_by_id(HttpClient client, long id)
    {
        return await client.GetAsync($"/api/login-audit-logs/{id}");
    }

    async Task<HttpResponseMessage> When_we_get_login_audit_logs_by_email(HttpClient client, string email, int days = 7)
    {
        return await client.GetAsync($"/api/login-audit-logs/by-email/{email}?days={days}");
    }

    async Task<HttpResponseMessage> When_we_get_login_audit_logs_by_status(HttpClient client, string status, int days = 7)
    {
        return await client.GetAsync($"/api/login-audit-logs/by-status/{status}?days={days}");
    }

    #endregion

    #region Then Methods

    void Then_the_response_should_be_ok(HttpResponseMessage response)
    {
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    void Then_the_response_should_be_unauthorized(HttpResponseMessage response)
    {
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    void Then_the_response_should_be_not_found(HttpResponseMessage response)
    {
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion
}
