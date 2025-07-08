using MediatR;

namespace Movies.VerticalSlice.Api.Features.Users.Delete;

public static class DeleteUserEndpoint
{
    public static void MapDeleteUser(this IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/users/{id:guid}", async (
            Guid id,
            IMediator mediator,
            CancellationToken token) =>
        {
            try
            {
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
        .WithOpenApi();
    }
}