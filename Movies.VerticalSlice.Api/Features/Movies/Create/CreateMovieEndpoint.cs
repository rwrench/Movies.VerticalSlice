using MediatR;
using Microsoft.AspNetCore.Mvc;
using Movies.VerticalSlice.Api.Features.Users.Login;
using Movies.VerticalSlice.Api.Services;
using Movies.VerticalSlice.Api.Shared.Requests;

namespace Movies.VerticalSlice.Api.Features.Movies.Create
{
    public static class CreateMovieEndpoint
    {
        public static void MapCreateMovie(this IEndpointRouteBuilder app)
        {
            app.MapPost("/api/movies", async (
                [FromBody] CreateMovieRequest request,
                IMediator mediator,
                UserContextService userContextService,
                CancellationToken token) =>
            {
                var userId = userContextService.GetCurrentUserId();
                if (userId == null) throw new UnauthorizedAccessException();
                var command = new CreateMovieCommand(
                    request.Title,
                    request.YearOfRelease,
                    request.Genres,
                    userId);

                var movieId = await mediator.Send(command, token);

                return Results.Created($"/api/movies/{movieId}", new { id = movieId });
            })
            .WithName("CreateMovie")
            .WithTags("Movies")
            .WithOpenApi()
            .RequireAuthorization()
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status400BadRequest);
        }
    }
}
