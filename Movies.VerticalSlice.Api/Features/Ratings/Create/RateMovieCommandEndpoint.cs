using MediatR;
using Microsoft.AspNetCore.Mvc;
using Movies.VerticalSlice.Api.Services;
using Movies.VerticalSlice.Api.Shared.Requests;


namespace Movies.VerticalSlice.Api.Features.Ratings.Create;

public static class RateMovieCommandEndpoint
{
    public static void MapRateMovie(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/movies/ratings/{id:guid}", async (
            Guid id,
            [FromBody] RateMovieRequest request,
            IMediator mediator,
            UserContextService userContextService,
            CancellationToken token) =>
        {
            var userId = userContextService.GetCurrentUserId();
            if (userId == null) throw new UnauthorizedAccessException();
            var command = new RateMovieCommand(
                id, 
                request.Rating, 
                userId,
                request.DateUpdated);
            var ratingsId = await mediator.Send(command, token);

            return Results.Created();
        })
        .WithName("CreateRating")
        .WithTags("Ratings")
        .RequireAuthorization()
        .Produces(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status400BadRequest)
        .WithOpenApi();
    }
}
