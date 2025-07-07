using MediatR;
using Microsoft.EntityFrameworkCore;
using Movies.VerticalSlice.Api.Data;

namespace Movies.VerticalSlice.Api.Features.Movies.GetAll;

public class GetAllMoviesHandler : IRequestHandler<GetAllMoviesQuery, List<MovieDto>>
{
    private readonly MoviesDbContext _context;
    private readonly ILogger<GetAllMoviesHandler> _logger;

    public GetAllMoviesHandler(MoviesDbContext context, ILogger<GetAllMoviesHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<MovieDto>> Handle(GetAllMoviesQuery query, CancellationToken token)
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

        // Apply sorting
        if (!string.IsNullOrWhiteSpace(query.SortField) && query.SortOrder.HasValue)
        {
            moviesQuery = query.SortField.ToLowerInvariant() switch
            {
                "title" => query.SortOrder == Movies.GetAll.SortOrder.Ascending
                    ? moviesQuery.OrderBy(m => m.Title)
                    : moviesQuery.OrderByDescending(m => m.Title),
                "yearofrelease" => query.SortOrder == Movies.GetAll.SortOrder.Ascending
                    ? moviesQuery.OrderBy(m => m.YearOfRelease)
                    : moviesQuery.OrderByDescending(m => m.YearOfRelease),
                _ => moviesQuery.OrderBy(m => m.Title)
            };
        }
        else
        {
            moviesQuery = moviesQuery.OrderBy(m => m.Title);
        }

        // Apply pagination
        if (query.Page.HasValue && query.PageSize.HasValue)
        {
            moviesQuery = moviesQuery
                .Skip((query.Page.Value - 1) * query.PageSize.Value)
                .Take(query.PageSize.Value);
        }

        var movies = await moviesQuery.AsNoTracking().ToListAsync(token);

        var result = movies.Select(m => new MovieDto(
            m.MovieId,
            m.Title,
            m.Slug,
            m.YearOfRelease,
            m.Genres
        )).ToList();

        _logger.LogInformation("Retrieved {Count} movies", result.Count);
        return result;
    }
}
