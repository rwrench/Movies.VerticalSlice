using MediatR;
using Microsoft.AspNetCore.Mvc;
using Movies.VerticalSlice.Api.Features.Users.Register;

namespace Movies.VerticalSlice.Api.Features.Users.Update;

public static class UpdateUserEndpoint
{
    public static void MapUpdateUser(this IEndpointRouteBuilder app)
    {
        app.MapPut("/api/users/{userName}", async (
            string userName,
            [FromBody] UpdateUserRequest request,
            IMediator mediator,
            CancellationToken token) =>
        {
            try
            {
                var command = new UpdateUserCommand(
                    userName,
                    request.Email,
                    request.Password);

                var result = await mediator.Send(command, token);

                return result
                    ? Results.Ok(new { success = true, 
                        message = "User updated successfully" })
                    : Results.NotFound(new { success = false, 
                        message = "User not found" });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { 
                    success = false, message = ex.Message });
            }
        })
        .WithName("UpdateUser")
        .WithTags("Users")
        .WithOpenApi();
    }
}