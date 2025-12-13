using MediatR;
using Microsoft.AspNetCore.Mvc;
using Movies.VerticalSlice.Api.Services;
using Movies.VerticalSlice.Api.Shared.Constants;
using Movies.VerticalSlice.Api.Shared.Requests;

namespace Movies.VerticalSlice.Api.Features.Movies.Update;

public static class UpdateMovieEndpoint
{
    public static void MapUpdateMovie(this IEndpointRouteBuilder app)
    {
        app.MapPut(ApiEndpoints.Movies.Update, async (
            Guid id,
            [FromBody] UpdateMovieRequest request,
            IMediator mediator,
            UserContextService userContextService,
            CancellationToken token) =>
        {
            var userId = userContextService.GetCurrentUserId();
            if (userId == null) 
                return Results.Unauthorized();

            var command = new UpdateMovieCommand(
                id,
                request.Title,
                request.YearOfRelease,
                request.Genres,
                userId);

            var result = await mediator.Send(command, token);
            if (!result)
                return Results.NotFound();
            return Results.NoContent();
        })
        .WithName("UpdateMovie")
        .WithTags("Movies")
        .WithOpenApi()
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status400BadRequest)
        .RequireAuthorization();
    }
}
