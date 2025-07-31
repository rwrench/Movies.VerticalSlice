using MediatR;
using Movies.VerticalSlice.Api.Shared.Constants;
using Movies.VerticalSlice.Api.Shared.Dtos;


namespace Movies.VerticalSlice.Api.Features.Movies.GetAll;

public static class GetAllMoviesEndpoint
{
    public static void MapGetAllMovies(this IEndpointRouteBuilder app)
    {
        app.MapGet(ApiEndpoints.Movies.GetAll, async (
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
        .Produces(StatusCodes.Status200OK)
        .WithTags("Movies")
        .WithOpenApi();
    }
}
