using MediatR;
using Microsoft.AspNetCore.Mvc;
using Movies.VerticalSlice.Api.Shared.Constants;

namespace Movies.VerticalSlice.Api.Features.Users.Login;

public static class LoginEndpoint
{
    public static IEndpointRouteBuilder MapLoginEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost(ApiEndpoints.Users.Login, async (
            [FromBody] LoginCommand command,
            IMediator mediator) =>
        {
            try
            {
                var response = await mediator.Send(command);
                return Results.Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Results.Unauthorized();
            }
            catch (Exception ex)
            {
                return Results.BadRequest(ex.Message);
            }
        })
        .WithName("LoginUser")
        .WithTags("Users")
        .WithSummary("Login user")
        .WithDescription("Authenticate a user and return a JWT token")
        .Produces<LoginResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status400BadRequest);

        return app;
    }
}