using MediatR;
using Microsoft.AspNetCore.Mvc;
using Movies.VerticalSlice.Api.Shared.Constants;

namespace Movies.VerticalSlice.Api.Features.Users.Register;

public static class RegisterUserEndpoint
{
    public static void MapRegisterUser(this IEndpointRouteBuilder app)
    {
        app.MapPost(ApiEndpoints.Users.Register, async (
            [FromBody] RegisterUserRequest request,
            IMediator mediator,
            CancellationToken token) =>
        {
            try
            {
                var command = new RegisterUserCommand(
                    request.UserName,
                    request.Email,
                    request.Password);

                var userId = await mediator.Send(command, token);

                return Results.Created(ApiEndpoints.Users.Register, new { id = userId });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { success = false, message = ex.Message });
            }
        })
        .AllowAnonymous()
        .WithName("RegisterUser")
        .WithTags("Users")
        .WithSummary("Register new user")
        .WithDescription("Create a new user account")
        .WithOpenApi()
        .Produces(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);
    }
}