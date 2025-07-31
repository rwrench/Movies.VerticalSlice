using MediatR;
using Movies.VerticalSlice.Api.Data.Models;
using Movies.VerticalSlice.Api.Services;
using Movies.VerticalSlice.Api.Shared.Constants;

namespace Movies.VerticalSlice.Api.Features.Movies.Delete
{
    public static class DeleteMovieEndpoint
    {
        public static void MapDeleteMovie(this IEndpointRouteBuilder app)
        {
            app.MapDelete(ApiEndpoints.Movies.Delete, async (
                Guid id,
                IMediator mediator,
                UserContextService userContextService,
                CancellationToken token) =>
            {
                var userId = userContextService.GetCurrentUserId();
                if (userId == null) throw new UnauthorizedAccessException();
                var command = new DeleteMovieCommand(id);
                var result = await mediator.Send(command, token);

                return result
                    ? Results.Ok(new { success = true, message = "Movie deleted successfully" })
                    : Results.NotFound(new { success = false, message = "Movie not found" });
            })
            .WithName("DeleteMovie")
            .WithTags("Movies")
            .RequireAuthorization()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound)
            .WithOpenApi();
        }
    }
}
