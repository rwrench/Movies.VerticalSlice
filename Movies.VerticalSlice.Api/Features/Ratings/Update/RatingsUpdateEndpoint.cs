using MediatR;
using Microsoft.AspNetCore.Mvc;
using Movies.VerticalSlice.Api.Services;
using Movies.VerticalSlice.Api.Shared.Requests;

namespace Movies.VerticalSlice.Api.Features.Ratings.Update;

public static class RatingsUpdateEndpoint
{
    public static void MapRatingsUpdate(this IEndpointRouteBuilder app)
    {
        app.MapPut("/api/movies/ratings/{id}", async (
            Guid id,    
            [FromBody] UpdateRatingsRequest request,
            IMediator mediator,
            UserContextService userContextService,
            CancellationToken token) =>
        {
            try
            {
                var userId = userContextService.GetCurrentUserId();
                if (userId == null) throw new UnauthorizedAccessException();
                var command = new RatingsUpdateCommand(
                    id,
                    request.MovieId,
                    request.Rating,
                    request.DateUpdated ?? DateTime.Now,
                    userId);
                var ratingsId = await mediator.Send(command, token);

                return Results.NoContent();
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Forbid();
            }
        })
        .WithName("UpdateRating")
        .WithTags("Ratings")
        .RequireAuthorization()
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status400BadRequest)
        .WithOpenApi();
    }
}
