using MediatR;
using Microsoft.EntityFrameworkCore;
using Movies.VerticalSlice.Api.Data.Database;
using Movies.VerticalSlice.Api.Shared.Dtos;
using System.Linq;

namespace Movies.VerticalSlice.Api.Features.Movies.GetAll;

public class GetAllMoviesHandler : IRequestHandler<GetAllMoviesQuery, IEnumerable<MovieDto>>
{
    private readonly MoviesDbContext _context;
    private readonly ILogger<GetAllMoviesHandler> _logger;

    public GetAllMoviesHandler(
        MoviesDbContext context, 
        ILogger<GetAllMoviesHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<MovieDto>> Handle(
        GetAllMoviesQuery query, 
        CancellationToken token)
    {
        var moviesQuery = _context.Movies.AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(query.Title))
        {
            moviesQuery = moviesQuery.Where(m => m.Title.Contains(query.Title));
        }

        if (query.YearOfRelease.HasValue)
        {
            moviesQuery = moviesQuery.Where(m => m.YearOfRelease == query.YearOfRelease.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.SortField) && query.SortOrder.HasValue)
        {
            moviesQuery = query.SortField.ToLowerInvariant() switch
            {
                "title" => query.SortOrder == SortOrder.Ascending
                    ? moviesQuery.OrderBy(m => m.Title)
                    : moviesQuery.OrderByDescending(m => m.Title),
                "yearofrelease" => query.SortOrder == SortOrder.Ascending
                    ? moviesQuery.OrderBy(m => m.YearOfRelease)
                    : moviesQuery.OrderByDescending(m => m.YearOfRelease),
                _ => moviesQuery.OrderBy(m => m.Title)
            };
        }
        else
        {
            moviesQuery = moviesQuery.OrderBy(m => m.Title);
        }

        if (query.Limit.HasValue)
        {
            moviesQuery = moviesQuery
                .Take(query.Limit.Value);
        }
        else         
        {
            moviesQuery = moviesQuery
                .Take(100); // Default limit to prevent excessive data retrieval
        }


        var movies = await moviesQuery.AsNoTracking().ToListAsync(token);

        var result = movies.Select(x => new MovieDto(
              x.MovieId,
              x.Title,
              x.Slug,
              x.YearOfRelease,
              x.Genres,
              x.DateUpdated
          )).AsEnumerable();
        _logger.LogInformation("Retrieved {result} movies", result.Count());
        return result;
    }


}
