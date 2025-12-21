using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Movies.Api.VerticalSlice.Api.Tests.Infrastructure;
using Movies.VerticalSlice.Api.Data.Models;
using Movies.VerticalSlice.Api.Shared.Constants;
using Xunit;

namespace Movies.Api.VerticalSlice.Api.Tests.Integration;

public class CreateLogEndpointIntegrationTests : IntegrationTestBase
{
    public CreateLogEndpointIntegrationTests(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreateLog_WithAuthentication_LogsShouldCaptureUserIdAndUserName()
    {
        var createLogRequest = And_we_have_a_valid_create_log_request();
        var authenticatedClient = And_we_have_an_authenticated_client(UserId, UserName);
        var response = await When_we_post_a_log(authenticatedClient, createLogRequest);
        
        Then_the_response_should_be_ok(response);
        await And_the_log_should_be_saved_with_user_info(UserId, UserName);
    }

    [Fact]
    public async Task CreateLog_WithoutAuthentication_LogsShouldHaveNullUserIdAndUserName()
    {
        var createLogRequest = And_we_have_a_valid_create_log_request();
        var response = await When_we_post_a_log_without_authentication(createLogRequest);
        
        Then_the_response_should_be_ok(response);
        await And_the_log_should_be_saved_without_user_info();
    }

    [Fact]
    public async Task CreateLog_WithValidData_Returns200OK()
    {
        var createLogRequest = And_we_have_a_valid_create_log_request();
        var response = await When_we_post_a_log_without_authentication(createLogRequest);
        Then_the_response_should_be_ok(response);
    }

    [Fact]
    public async Task CreateLog_WithAllFields_SavesAllDataCorrectly()
    {
        var createLogRequest = And_we_have_a_complete_create_log_request();
        var authenticatedClient = And_we_have_an_authenticated_client(UserId, UserName);
        var response = await When_we_post_a_log(authenticatedClient, createLogRequest);
        
        Then_the_response_should_be_ok(response);
        await And_the_log_should_contain_all_fields(createLogRequest, UserId, UserName);
    }

    #region Given Methods

    CreateLogRequest And_we_have_a_valid_create_log_request()
    {
        return new CreateLogRequest(
            Level: "Information",
            Category: "TestCategory",
            Message: "Test log message",
            Exception: null,
            RequestPath: "/api/test",
            Properties: null
        );
    }

    CreateLogRequest And_we_have_a_complete_create_log_request()
    {
        return new CreateLogRequest(
            Level: "Error",
            Category: "TestCategory",
            Message: "Test error message",
            Exception: "System.Exception: Test exception",
            RequestPath: "/api/test/error",
            Properties: "{\"key\": \"value\"}"
        );
    }

    #endregion

    #region And Methods

    HttpClient And_we_have_an_authenticated_client(string userId, string userName)
    {
        return CreateAuthenticatedClient(userId, userName);
    }

    #endregion

    #region When Methods

    async Task<HttpResponseMessage> When_we_post_a_log(HttpClient client, CreateLogRequest request)
    {
        return await client.PostAsJsonAsync("/api/logs", request);
    }

    async Task<HttpResponseMessage> When_we_post_a_log_without_authentication(CreateLogRequest request)
    {
        return await Client.PostAsJsonAsync("/api/logs", request);
    }

    #endregion

    #region Then Methods

    void Then_the_response_should_be_ok(HttpResponseMessage response)
    {
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    async Task And_the_log_should_be_saved_with_user_info(string expectedUserId, string expectedUserName)
    {
        var log = await DbContext.ApplicationLogs
            .OrderByDescending(l => l.Timestamp)
            .FirstAsync();

        log.Should().NotBeNull();
        log.UserId.Should().Be(expectedUserId);
        log.UserName.Should().Be(expectedUserName);
    }

    async Task And_the_log_should_be_saved_without_user_info()
    {
        var log = await DbContext.ApplicationLogs
            .OrderByDescending(l => l.Timestamp)
            .FirstAsync();

        log.Should().NotBeNull();
        log.UserId.Should().BeNull();
        log.UserName.Should().BeNull();
    }

    async Task And_the_log_should_contain_all_fields(CreateLogRequest request, string expectedUserId, string expectedUserName)
    {
        var log = await DbContext.ApplicationLogs
            .OrderByDescending(l => l.Timestamp)
            .FirstAsync();

        log.Should().NotBeNull();
        log.Level.Should().Be(request.Level);
        log.Category.Should().Be(request.Category);
        log.Message.Should().Be(request.Message);
        log.Exception.Should().Be(request.Exception);
        log.RequestPath.Should().Be(request.RequestPath);
        log.Properties.Should().Be(request.Properties);
        log.UserId.Should().Be(expectedUserId);
        log.UserName.Should().Be(expectedUserName);
    }

    #endregion
}

public record CreateLogRequest(
    string? Level,
    string? Category,
    string? Message,
    string? Exception,
    string? RequestPath,
    string? Properties
);
