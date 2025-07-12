using Movies.VerticalSlice.Api.Features.Ratings.Create;
using Movies.VerticalSlice.Api.Features.Ratings.GetAll;
using Movies.VerticalSlice.Api.Features.Rating.Delete;
using Movies.VerticalSlice.Api.Features.Ratings.Update;


namespace Movies.VerticalSlice.Api.Features.Ratings;

public static class RatingsEndpoints
{
    public static void MapRatingsEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapRatingsCreate();
        app.MapRatingsUpdate();
        app.MapGetAllRatings();
        app.MapDeleteRating();
    }
}
