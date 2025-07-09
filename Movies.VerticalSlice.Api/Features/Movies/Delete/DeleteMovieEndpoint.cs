using MediatR;

namespace Movies.VerticalSlice.Api.Features.Movies.Delete
{
    public static class DeleteMovieEndpoint
    {
        public static void MapDeleteMovie(this IEndpointRouteBuilder app)
        {
            app.MapDelete("/api/movies/{id:guid}", async (
                Guid id,
                IMediator mediator,
                CancellationToken token) =>
            {
                var command = new DeleteMovieCommand(id,null);
                var result = await mediator.Send(command, token);

                return result
                    ? Results.Ok(new { success = true, message = "Movie deleted successfully" })
                    : Results.NotFound(new { success = false, message = "Movie not found" });
            })
            .WithName("DeleteMovie")
            .WithTags("Movies")
            .RequireAuthorization()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound)
            .WithOpenApi();
        }
    }
}
