using MediatR;
using Microsoft.EntityFrameworkCore;
using Movies.VerticalSlice.Api.Data.Database;
using Movies.VerticalSlice.Api.Features.Movies.GetAll;
using Movies.VerticalSlice.Api.Shared.Dtos;

namespace Movies.VerticalSlice.Api.Features.Movies.Names
{
    public class GetNamesHandler :
        IRequestHandler<GetNamesQuery, IEnumerable<MovieNameDto>>
    {
        private readonly MoviesDbContext _context;
        private readonly ILogger<GetAllMoviesHandler> _logger;

        public GetNamesHandler(
            MoviesDbContext context,
            ILogger<GetAllMoviesHandler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<MovieNameDto>> Handle(
            GetNamesQuery query,
            CancellationToken token)
        {
            var moviesQuery = _context.Movies.AsQueryable();
            if (!string.IsNullOrWhiteSpace(query.Title))
            {
                moviesQuery = moviesQuery
                    .Where(m => m.Title.StartsWith(query.Title))
                    .OrderBy(m => m.Title)
                    .ThenBy(m => m.YearOfRelease)
                    .Take(100);
            }
            var movies = await moviesQuery.AsNoTracking().OrderBy(x => x.Title)
                .ToListAsync(token);

            var result = movies.Select(x => new MovieNameDto (
                  x.MovieId,
                  x.Title
              )).AsEnumerable();
            _logger.LogInformation("Retrieved {result} movies", result.Count());
            return result;
        }


    }
}
