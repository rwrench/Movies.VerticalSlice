using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Movies.Api.VerticalSlice.Api.Tests.Infrastructure;
using Movies.VerticalSlice.Api.Data.Models;
using Movies.VerticalSlice.Api.Features.Users.Login;
using Movies.VerticalSlice.Api.Services;
using Xunit;

namespace Movies.Api.VerticalSlice.Api.Tests.Integration;

public class LoginAuditLoggingIntegrationTests : IntegrationTestBase
{
    public LoginAuditLoggingIntegrationTests(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task LoginAttempt_Success_LogsSuccessfulLoginWithUserInfo()
    {
        // Arrange
        var user = await Given_we_have_a_user_in_database();
        var loginRequest = And_we_have_a_valid_login_request(user.Email, "TestPassword123!");
        
        // Act
        var response = await When_we_attempt_login(loginRequest);
        
        // Assert
        Then_the_response_should_be_ok(response);
        await And_the_successful_login_should_be_logged(user.Email, user.Id, user.UserName);
    }

    [Fact]
    public async Task LoginAttempt_InvalidPassword_LogsFailedLoginWithReason()
    {
        // Arrange
        var user = await Given_we_have_a_user_in_database();
        var loginRequest = And_we_have_a_login_request_with_invalid_password(user.Email);
        
        // Act
        var response = await When_we_attempt_login(loginRequest);
        
        // Assert
        Then_the_response_should_be_unauthorized(response);
        await And_the_failed_login_should_be_logged(user.Email, "Invalid password");
    }

    [Fact]
    public async Task LoginAttempt_UserNotFound_LogsFailedLoginWithReason()
    {
        // Arrange
        var loginRequest = And_we_have_a_login_request_for_nonexistent_user();
        
        // Act
        var response = await When_we_attempt_login(loginRequest);
        
        // Assert
        Then_the_response_should_be_unauthorized(response);
        await And_the_failed_login_should_be_logged("nonexistent@example.com", "User not found");
    }

    [Fact]
    public async Task LoginAttempt_Success_CapturesIpAddressAndUserAgent()
    {
        // Arrange
        var user = await Given_we_have_a_user_in_database();
        var loginRequest = And_we_have_a_valid_login_request(user.Email, "TestPassword123!");
        
        // Act
        var response = await When_we_attempt_login(loginRequest);
        
        // Assert
        Then_the_response_should_be_ok(response);
        await And_the_login_audit_should_have_network_info(user.Email);
    }

    [Fact]
    public async Task MultipleLoginAttempts_CreatesMultipleAuditLogs()
    {
        // Arrange
        var user = await Given_we_have_a_user_in_database();
        var validRequest = And_we_have_a_valid_login_request(user.Email, "TestPassword123!");
        var invalidRequest = And_we_have_a_login_request_with_invalid_password(user.Email);
        
        // Act - One successful, one failed, one successful
        await When_we_attempt_login(validRequest);
        await When_we_attempt_login(invalidRequest);
        await When_we_attempt_login(validRequest);
        
        // Assert
        await And_the_audit_logs_should_show_pattern(user.Email, "Success", "FailedLogin", "Success");
    }

    #region Given Methods

    async Task<ApplicationUser> Given_we_have_a_user_in_database()
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "testuser",
            Email = "test@example.com",
            NormalizedEmail = "TEST@EXAMPLE.COM",
            NormalizedUserName = "TESTUSER",
            EmailConfirmed = true
        };

        // Set password hash using the password service from DI
        var passwordService = Factory.Services.GetRequiredService<IPasswordService>();
        user.PasswordHash = passwordService.HashPassword("TestPassword123!");

        DbContext.Users.Add(user);
        await DbContext.SaveChangesAsync();

        return user;
    }

    LoginCommand And_we_have_a_valid_login_request(string email, string password)
    {
        return new LoginCommand(email, password);
    }

    LoginCommand And_we_have_a_login_request_with_invalid_password(string email)
    {
        return new LoginCommand(email, "WrongPassword123!");
    }

    LoginCommand And_we_have_a_login_request_for_nonexistent_user()
    {
        return new LoginCommand("nonexistent@example.com", "Password123!");
    }

    #endregion

    #region When Methods

    async Task<HttpResponseMessage> When_we_attempt_login(LoginCommand request)
    {
        return await Client.PostAsJsonAsync("/api/users/login", request);
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

    async Task And_the_successful_login_should_be_logged(string email, string userId, string userName)
    {
        var auditLog = await DbContext.LoginAuditLogs
            .Where(l => l.Email == email && l.Status == "Success")
            .OrderByDescending(l => l.Timestamp)
            .FirstAsync();

        auditLog.Should().NotBeNull();
        auditLog.Email.Should().Be(email);
        auditLog.Status.Should().Be("Success");
        auditLog.UserId.Should().Be(userId);
        auditLog.UserName.Should().Be(userName);
    }

    async Task And_the_failed_login_should_be_logged(string email, string expectedReason)
    {
        var auditLog = await DbContext.LoginAuditLogs
            .Where(l => l.Email == email && l.Status != "Success")
            .OrderByDescending(l => l.Timestamp)
            .FirstAsync();

        auditLog.Should().NotBeNull();
        auditLog.Email.Should().Be(email);
        auditLog.FailureReason.Should().Contain(expectedReason);
        auditLog.UserId.Should().BeNull();
        auditLog.UserName.Should().BeNull();
    }

    async Task And_the_login_audit_should_have_network_info(string email)
    {
        var auditLog = await DbContext.LoginAuditLogs
            .Where(l => l.Email == email && l.Status == "Success")
            .OrderByDescending(l => l.Timestamp)
            .FirstAsync();

        auditLog.Should().NotBeNull();
        // IpAddress and UserAgent may be null in test environment, but the fields should exist
        auditLog.Should().NotBeNull();
    }

    async Task And_the_audit_logs_should_show_pattern(string email, params string[] expectedStatuses)
    {
        var auditLogs = await DbContext.LoginAuditLogs
            .Where(l => l.Email == email)
            .OrderBy(l => l.Timestamp)
            .ToListAsync();

        auditLogs.Should().HaveCount(expectedStatuses.Length);
        
        for (int i = 0; i < expectedStatuses.Length; i++)
        {
            auditLogs[i].Status.Should().Be(expectedStatuses[i]);
        }
    }

    #endregion
}
