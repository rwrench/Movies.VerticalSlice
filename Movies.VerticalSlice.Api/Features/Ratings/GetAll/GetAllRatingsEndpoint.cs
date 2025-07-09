using MediatR;

namespace Movies.VerticalSlice.Api.Features.Ratings.GetAll;

public static class GetAllRatingsEndpoint
{
    public static void MapGetAllRatings(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/movies/ratings", async (
            IMediator mediator,
            CancellationToken token) =>
        {
        var query = new GetAllRatingsQuery();
            var ratings = await mediator.Send(query, token);
            return Results.Ok(ratings);
        })
        .WithName("GetAllRatings")
        .WithTags("Ratings")
        .WithOpenApi()
        .RequireAuthorization();
    }
}
