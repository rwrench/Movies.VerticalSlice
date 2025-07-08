using Movies.VerticalSlice.Api.Features.Ratings.Create;
using Movies.VerticalSlice.Api.Features.Ratings.GetAll;


namespace Movies.VerticalSlice.Api.Features.Ratings;

public static class RatingsEndpoints
{
    public static void MapRatingsEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapRateMovie();
        app.MapGetAllRatings();
    }
}
