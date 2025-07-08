using MediatR;
using Microsoft.AspNetCore.Mvc;


namespace Movies.VerticalSlice.Api.Features.Ratings.Create;

public static class RateMovieCommandEndpoint
{
    public static void MapRateMovie(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/movies/ratings/{id:guid}", async (
            Guid id,
            [FromBody] RateMovieRequest request,
            IMediator mediator,
            CancellationToken token) =>
        {
            var command = new RateMovieCommand(id, request.Rating, request.UserId, request.DateUpdated);
            var ratingsId = await mediator.Send(command, token);

            return Results.Created();
        })
        .WithName("CreateRating")
        .WithTags("Ratings")
        .WithOpenApi();
    }
}
