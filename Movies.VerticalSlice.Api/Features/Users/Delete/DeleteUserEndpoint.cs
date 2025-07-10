using MediatR;
using Movies.VerticalSlice.Api.Services;

namespace Movies.VerticalSlice.Api.Features.Users.Delete;

public static class DeleteUserEndpoint
{
    public static void MapDeleteUser(this IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/users/{id:guid}", async (
            string id,
            IMediator mediator,
            UserContextService userContextService, 
            CancellationToken token) =>
        {
            try
            {
                var userId = userContextService.GetCurrentUserId();
                var command = new DeleteUserCommand(id);
                var result = await mediator.Send(command, token);

                return result
                    ? Results.Ok(new { success = true, message = "User deleted successfully" })
                    : Results.NotFound(new { success = false, message = "User not found" });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { success = false, message = ex.Message });
            }
        })
        .WithName("DeleteUser")
        .WithTags("Users")
        .WithOpenApi()
        .RequireAuthorization();
    }
}