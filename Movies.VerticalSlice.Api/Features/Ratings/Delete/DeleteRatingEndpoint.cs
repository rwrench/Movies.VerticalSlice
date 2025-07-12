using MediatR;
using Movies.VerticalSlice.Api.Data.Models;
using Movies.VerticalSlice.Api.Features.Ratings.Delete;
using Movies.VerticalSlice.Api.Services;

namespace Movies.VerticalSlice.Api.Features.Rating.Delete
{
    public static class DeleteRatingEndpoint
    {
        public static void MapDeleteRating(this IEndpointRouteBuilder app)
        {
            app.MapDelete("/api/movies/ratings/{id:guid}", async (
                Guid id,
                IMediator mediator,
                UserContextService userContextService,
                CancellationToken token) =>
            {
                var userId = userContextService.GetCurrentUserId();
                if (userId == null) throw new UnauthorizedAccessException();
                var command = new DeleteRatingCommand(id);
                var result = await mediator.Send(command, token);

                return result
                    ? Results.Ok(new { success = true, 
                        message = "Rating deleted successfully" })
                    : Results.NotFound(new { success = false, 
                        message = "Rating not found" });
            })
            .WithName("DeleteRating")
            .WithTags("Rating")
            .RequireAuthorization()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound)
            .WithOpenApi();
        }
    }
}
