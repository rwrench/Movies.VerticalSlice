using MediatR;
using Microsoft.AspNetCore.Mvc;
using Movies.VerticalSlice.Api.Services;

namespace Movies.VerticalSlice.Api.Features.Movies.Update;

public static class UpdateMovieEndpoint
{
    public static void MapUpdateMovie(this IEndpointRouteBuilder app)
    {
        app.MapPut("/api/movies/{id:guid}", async (
            Guid id,
            [FromBody] UpdateMovieRequest request,
            IMediator mediator,
            UserContextService userContextService,
            CancellationToken token) =>
        {
            var userId = userContextService.GetCurrentUserId();
            if (userId == null) throw new UnauthorizedAccessException();
            var command = new UpdateMovieCommand(
                id,
                request.Title,
                request.YearOfRelease,
                request.Genres,
                userId!.Value);

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
