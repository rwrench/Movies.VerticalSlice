using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Movies.VerticalSlice.Api.Features.Users.Register;

public static class RegisterUserEndpoint
{
    public static void MapRegisterUser(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/users/register", async (
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

                return Results.Created($"/api/users/{userId}", new { id = userId });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { success = false, message = ex.Message });
            }
        })
        .WithName("RegisterUser")
        .WithTags("Users")
        .WithOpenApi();
    }
}