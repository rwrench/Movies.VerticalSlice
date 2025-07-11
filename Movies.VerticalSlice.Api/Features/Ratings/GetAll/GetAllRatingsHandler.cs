using MediatR;
using Microsoft.EntityFrameworkCore;
using Movies.VerticalSlice.Api.Data.Database;

namespace Movies.VerticalSlice.Api.Features.Ratings.GetAll;

public class GetAllRatingsHandler : 
    IRequestHandler<GetAllRatingsQuery, IEnumerable<MovieRatingWithNameDto>>
{
    private readonly MoviesDbContext _context;
    private readonly ILogger<GetAllRatingsHandler> _logger;

    public GetAllRatingsHandler(MoviesDbContext context, 
        ILogger<GetAllRatingsHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async  Task<IEnumerable<MovieRatingWithNameDto>> Handle(
        GetAllRatingsQuery request, 
        CancellationToken cancellationToken)
    {

        var ratingsWithMovies = await _context.Ratings
            .Join(_context.Movies,
                rating => rating.MovieId,
                movie => movie.MovieId,
                (rating, movie) => new { Rating = rating, Movie = movie })
            .ToListAsync(cancellationToken);

        var result = ratingsWithMovies.Select(x => new MovieRatingWithNameDto(
              x.Rating.Id,
                x.Rating.MovieId,
                x.Rating.Rating,
                x.Rating.UserId,
                x.Rating.DateUpdated,
                x.Movie.Title,
                x.Movie.Genres)).AsEnumerable();


        _logger.LogInformation("Retrieved {Count} movie ratings", result.Count());
        return result;

    }
}
