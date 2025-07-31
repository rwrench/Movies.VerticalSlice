using MediatR;
using Movies.VerticalSlice.Api.Shared.Constants;

namespace Movies.VerticalSlice.Api.Features.Movies.Names;

public static class GetNamesEndpoint

{
    public static void MapGetNames(this IEndpointRouteBuilder app)
    {
        app.MapGet(ApiEndpoints.Movies.Names, async (
            IMediator mediator,
            CancellationToken token,
            string title = "") =>
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


