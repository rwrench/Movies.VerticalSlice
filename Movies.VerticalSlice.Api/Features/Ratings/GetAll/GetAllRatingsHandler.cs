using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Movies.VerticalSlice.Api.Data.Database;

namespace Movies.VerticalSlice.Api.Features.Ratings.GetAll;

public class GetAllRatingsHandler : 
    IRequestHandler<GetAllRatingsQuery, IEnumerable<MovieRatingWithNameDto>>
{
    private readonly MoviesDbContext _context;
    private readonly ILogger<GetAllRatingsHandler> _logger;
    private readonly IMapper _mapper;

    public GetAllRatingsHandler(MoviesDbContext context, 
        ILogger<GetAllRatingsHandler> logger,
        IMapper mapper)
    {
        _context = context;
        _logger = logger;
        _mapper = mapper;   
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

        var result = ratingsWithMovies.Select(x =>
            _mapper.Map<MovieRatingWithNameDto>((x.Rating, x.Movie)));

        _logger.LogInformation("Retrieved {Count} movie ratings", result.Count());
        return result;

    }
}
