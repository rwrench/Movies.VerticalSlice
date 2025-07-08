using Movies.VerticalSlice.Api.Features.Movies.Create;
using Movies.VerticalSlice.Api.Features.Movies.Delete;
using Movies.VerticalSlice.Api.Features.Movies.GetAll;
using Movies.VerticalSlice.Api.Features.Movies.Update;

namespace Movies.VerticalSlice.Api.Features.Movies;

public static class MoviesEndpoints
{
    public static void MapMovieEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapCreateMovie();
        app.MapGetAllMovies();
        app.MapUpdateMovie();
        app.MapDeleteMovie();
    }
}
