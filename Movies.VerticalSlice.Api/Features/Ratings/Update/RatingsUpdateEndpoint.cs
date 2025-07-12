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
            var userId = userContextService.GetCurrentUserId();
            if (userId == null) throw new UnauthorizedAccessException();
            var command = new RatingsUpdateCommand(
                id,
                request.MovieId,
                request.Rating,
                request.DateUpdated ?? DateTime.Now,
                userId);
            var ratingsId = await mediator.Send(command, token);

            return Results.Created();
        })
        .WithName("UpdateRating")
        .WithTags("Ratings")
        .RequireAuthorization()
        .Produces(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status400BadRequest)
        .WithOpenApi();
    }
}
