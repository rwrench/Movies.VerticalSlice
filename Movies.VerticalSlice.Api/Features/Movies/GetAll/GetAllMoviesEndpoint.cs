using MediatR;

namespace Movies.VerticalSlice.Api.Features.Movies.GetAll;

public static class GetAllMoviesEndpoint
{
    public static void MapGetAllMovies(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/movies", async (
            string? title,
            int? yearOfRelease,
            string? sortField,
            SortOrder? sortOrder,
            int? page,
            int? pageSize,
            IMediator mediator,
            CancellationToken token) =>
        {
            var query = new GetAllMoviesQuery(
                title,
                yearOfRelease,
                sortField,
                sortOrder,
                page,
                pageSize);

            var movies = await mediator.Send(query, token);
            return Results.Ok(movies);
        })
        .WithName("GetAllMovies")
        .WithTags("Movies")
        .WithOpenApi();
    }
}
