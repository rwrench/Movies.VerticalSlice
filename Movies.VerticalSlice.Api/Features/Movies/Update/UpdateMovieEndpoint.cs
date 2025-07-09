using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Movies.VerticalSlice.Api.Features.Movies.Update;

public static class UpdateMovieEndpoint
{
    public static void MapUpdateMovie(this IEndpointRouteBuilder app)
    {
        app.MapPut("/api/movies/{id:guid}", async (
            Guid id,
            [FromBody] UpdateMovieRequest request,
            IMediator mediator,
            CancellationToken token) =>
        {
            var command = new UpdateMovieCommand(
                id,
                request.Title,
                request.YearOfRelease,
                request.Genres,
                request.UserId);

            var result = await mediator.Send(command, token);

            return result
                ? Results.Ok(new { success = true, message = "Movie updated successfully" })
                : Results.NotFound(new { success = false, message = "Movie not found" });
        })
        .WithName("UpdateMovie")
        .WithTags("Movies")
        .WithOpenApi()
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound)
        .RequireAuthorization();
    }
}
