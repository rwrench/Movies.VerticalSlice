using MediatR;
using Movies.VerticalSlice.Api.Features.Ratings.GetAll;
using Movies.VerticalSlice.Api.Services;
using Movies.VerticalSlice.Api.Shared.Requests;

namespace Movies.VerticalSlice.Api.Features.Movies.Names;

public static class GetNamesEndpoint

{
    public static void MapGetNames(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/movies/names", async (
            string title,
            IMediator mediator,
            CancellationToken token) =>
        {
            var query = new GetNamesQuery(title);
            var movieNames = await mediator.Send(query, token);
            return Results.Ok(movieNames);
        })
        .WithName("GetNames")
        .WithTags("Movies")
        .WithOpenApi();
    }
}


