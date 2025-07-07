using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Movies.VerticalSlice.Api.Features.Movies.Create
{
    public static class CreateMovieEndpoint
    {
        public static void MapCreateMovie(this IEndpointRouteBuilder app)
        {
            app.MapPost("/api/movies", async (
                [FromBody] CreateMovieRequest request,
                IMediator mediator,
                CancellationToken token) =>
            {
                var command = new CreateMovieCommand(
                    request.Title,
                    request.YearOfRelease,
                    request.Genres,
                    request.UserId);

                var movieId = await mediator.Send(command, token);

                return Results.Created($"/api/movies/{movieId}", new { id = movieId });
            })
            .WithName("CreateMovie")
            .WithTags("Movies")
            .WithOpenApi();
        }
    }
}
