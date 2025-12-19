using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Movies.VerticalSlice.Api.Data.Database;
using Movies.VerticalSlice.Api.Data.Models;
using Movies.VerticalSlice.Api.Shared.Dtos;
using Xunit;

namespace Movies.Api.VerticalSlice.Api.Tests.Features.Movies;

public class GetAllMoviesEndpointTests : IDisposable
{
    private readonly MoviesDbContext _context;

    public GetAllMoviesEndpointTests()
    {
        var options = new DbContextOptionsBuilder<MoviesDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new MoviesDbContext(options);
        SeedTestData();
    }

    #region Filter Tests

    [Fact]
    public async Task GetMovies_FilterByTitle_ReturnsOnlyMatchingMovies()
    {
        Given_we_have_seeded_test_data();
        var title = And_we_have_title_filter();
        var movies = await When_we_filter_movies_by_title(title);
        Then_movies_should_have_count(movies, 2);
        And_all_movies_should_contain_title(movies, title);
    }

    [Fact]
    public async Task GetMovies_FilterByYearOfRelease_ReturnsOnlyMatchingMovies()
    {
        Given_we_have_seeded_test_data();
        var year = And_we_have_year_filter();
        var movies = await When_we_filter_movies_by_year(year);
        Then_movies_should_have_count(movies, 2);
        And_all_movies_should_have_year(movies, year);
    }

    [Fact]
    public async Task GetMovies_CombinedFilters_ReturnsCorrectSubset()
    {
        Given_we_have_seeded_test_data();
        var (title, year) = And_we_have_combined_filters();
        var movies = await When_we_filter_movies_by_title_and_year(title, year);
        Then_movies_should_have_count(movies, 2);
        And_all_movies_should_match_filters(movies, title, year);
    }

    #endregion

    #region Sort Tests

    [Fact]
    public async Task GetMovies_SortByTitleAscending_ReturnsSortedMovies()
    {
        Given_we_have_seeded_test_data();
        var (sortField, sortOrder) = And_we_have_title_ascending_sort();
        var movies = await When_we_get_movies_with_sorting(sortField, sortOrder);
        Then_movies_should_be_sorted_by_title_ascending(movies);
    }

    [Fact]
    public async Task GetMovies_SortByTitleDescending_ReturnsSortedMovies()
    {
        Given_we_have_seeded_test_data();
        var (sortField, sortOrder) = And_we_have_title_descending_sort();
        var movies = await When_we_get_movies_with_sorting(sortField, sortOrder);
        Then_movies_should_be_sorted_by_title_descending(movies);
    }

    [Fact]
    public async Task GetMovies_SortByYearAscending_ReturnsSortedMovies()
    {
        Given_we_have_seeded_test_data();
        var (sortField, sortOrder) = And_we_have_year_ascending_sort();
        var movies = await When_we_get_movies_with_sorting(sortField, sortOrder);
        Then_movies_should_be_sorted_by_year_ascending(movies);
    }

    [Fact]
    public async Task GetMovies_SortByYearDescending_ReturnsSortedMovies()
    {
        Given_we_have_seeded_test_data();
        var (sortField, sortOrder) = And_we_have_year_descending_sort();
        var movies = await When_we_get_movies_with_sorting(sortField, sortOrder);
        Then_movies_should_be_sorted_by_year_descending(movies);
    }

    #endregion

    #region Limit Tests

    [Fact]
    public async Task GetMovies_WithLimit_ReturnsLimitedResults()
    {
        Given_we_have_seeded_test_data();
        var limit = And_we_have_limit(3);
        var movies = await When_we_get_movies_with_limit(limit);
        Then_movies_should_have_count(movies, 3);
    }

    [Fact]
    public async Task GetMovies_NoLimit_ReturnsAllMovies()
    {
        Given_we_have_seeded_test_data();
        var movies = await When_we_get_all_movies();
        Then_movies_should_have_count(movies, 5);
    }

    #endregion

    #region Given Methods

    void Given_we_have_seeded_test_data()
    {
        // Data already seeded in constructor
    }

    #endregion

    #region And Methods

    string And_we_have_title_filter()
    {
        return "Test Movie";
    }

    int And_we_have_year_filter()
    {
        return 2020;
    }

    (string title, int year) And_we_have_combined_filters()
    {
        return ("Test Movie", 2020);
    }

    (string sortField, SortOrder sortOrder) And_we_have_title_ascending_sort()
    {
        return ("title", SortOrder.Ascending);
    }

    (string sortField, SortOrder sortOrder) And_we_have_title_descending_sort()
    {
        return ("title", SortOrder.Descending);
    }

    (string sortField, SortOrder sortOrder) And_we_have_year_ascending_sort()
    {
        return ("yearofrelease", SortOrder.Ascending);
    }

    (string sortField, SortOrder sortOrder) And_we_have_year_descending_sort()
    {
        return ("yearofrelease", SortOrder.Descending);
    }

    int And_we_have_limit(int limit)
    {
        return limit;
    }

    void And_all_movies_should_contain_title(List<Movie> movies, string title)
    {
        movies.Should().AllSatisfy(m => m.Title.Should().Contain(title));
    }

    void And_all_movies_should_have_year(List<Movie> movies, int year)
    {
        movies.Should().AllSatisfy(m => m.YearOfRelease.Should().Be(year));
    }

    void And_all_movies_should_match_filters(List<Movie> movies, string title, int year)
    {
        movies.Should().AllSatisfy(m =>
        {
            m.Title.Should().Contain(title);
            m.YearOfRelease.Should().Be(year);
        });
    }

    #endregion

    #region When Methods

    async Task<List<Movie>> When_we_filter_movies_by_title(string title)
    {
        return await _context.Movies
            .Where(m => m.Title.Contains(title))
            .ToListAsync();
    }

    async Task<List<Movie>> When_we_filter_movies_by_year(int year)
    {
        return await _context.Movies
            .Where(m => m.YearOfRelease == year)
            .ToListAsync();
    }

    async Task<List<Movie>> When_we_filter_movies_by_title_and_year(string title, int year)
    {
        return await _context.Movies
            .Where(m => m.Title.Contains(title) && m.YearOfRelease == year)
            .ToListAsync();
    }

    async Task<List<Movie>> When_we_get_movies_with_sorting(string sortField, SortOrder sortOrder)
    {
        var query = _context.Movies.AsQueryable();

        query = sortField.ToLowerInvariant() switch
        {
            "title" => sortOrder == SortOrder.Ascending
                ? query.OrderBy(m => m.Title)
                : query.OrderByDescending(m => m.Title),
            "yearofrelease" => sortOrder == SortOrder.Ascending
                ? query.OrderBy(m => m.YearOfRelease)
                : query.OrderByDescending(m => m.YearOfRelease),
            _ => query.OrderBy(m => m.Title)
        };

        return await query.ToListAsync();
    }

    async Task<List<Movie>> When_we_get_movies_with_limit(int limit)
    {
        return await _context.Movies
            .OrderBy(m => m.Title)
            .Take(limit)
            .ToListAsync();
    }

    async Task<List<Movie>> When_we_get_all_movies()
    {
        return await _context.Movies.ToListAsync();
    }

    #endregion

    #region Then Methods

    void Then_movies_should_have_count(List<Movie> movies, int expectedCount)
    {
        movies.Should().HaveCount(expectedCount);
    }

    void Then_movies_should_be_sorted_by_title_ascending(List<Movie> movies)
    {
        movies.Should().BeInAscendingOrder(m => m.Title);
    }

    void Then_movies_should_be_sorted_by_title_descending(List<Movie> movies)
    {
        movies.Should().BeInDescendingOrder(m => m.Title);
    }

    void Then_movies_should_be_sorted_by_year_ascending(List<Movie> movies)
    {
        movies.Should().BeInAscendingOrder(m => m.YearOfRelease);
    }

    void Then_movies_should_be_sorted_by_year_descending(List<Movie> movies)
    {
        movies.Should().BeInDescendingOrder(m => m.YearOfRelease);
    }

    #endregion

    #region Helper Methods

    private void SeedTestData()
    {
        var movies = new List<Movie>
        {
            new Movie
            {
                MovieId = Guid.NewGuid(),
                Title = "Test Movie 1",
                YearOfRelease = 2020,
                Genres = "Action,Drama",
                UserId = "user1"
            },
            new Movie
            {
                MovieId = Guid.NewGuid(),
                Title = "Another Movie 2",
                YearOfRelease = 2021,
                Genres = "Comedy",
                UserId = "user1"
            },
            new Movie
            {
                MovieId = Guid.NewGuid(),
                Title = "Test Movie 3",
                YearOfRelease = 2020,
                Genres = "Thriller",
                UserId = "user2"
            },
            new Movie
            {
                MovieId = Guid.NewGuid(),
                Title = "Different Film 4",
                YearOfRelease = 2019,
                Genres = "Horror",
                UserId = "user2"
            },
            new Movie
            {
                MovieId = Guid.NewGuid(),
                Title = "Classic Film 5",
                YearOfRelease = 2018,
                Genres = "Drama",
                UserId = "user3"
            }
        };

        _context.Movies.AddRange(movies);
        _context.SaveChanges();
    }

    #endregion

    public void Dispose()
    {
        _context.Dispose();
    }
}
