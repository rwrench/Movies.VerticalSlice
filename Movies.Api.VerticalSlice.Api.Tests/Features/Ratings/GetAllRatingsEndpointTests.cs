using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Movies.VerticalSlice.Api.Data.Database;
using Movies.VerticalSlice.Api.Data.Models;
using Xunit;

namespace Movies.Api.VerticalSlice.Api.Tests.Features.Ratings;

public class GetAllRatingsEndpointTests : IDisposable
{
    private readonly MoviesDbContext _context;
    private readonly Guid _movie1Id = Guid.NewGuid();
    private readonly Guid _movie2Id = Guid.NewGuid();
    private readonly Guid _movie3Id = Guid.NewGuid();

    public GetAllRatingsEndpointTests()
    {
        var options = new DbContextOptionsBuilder<MoviesDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new MoviesDbContext(options);
        SeedTestData();
    }

    #region Basic Retrieval Tests

    [Fact]
    public async Task GetRatings_WithSeededData_ReturnsAllRatings()
    {
        Given_we_have_seeded_test_data();
        var ratings = await When_we_get_all_ratings();
        Then_ratings_should_have_count(ratings, 5);
    }

    [Fact]
    public async Task GetRatings_WithJoinedData_IncludesMovieAndUserInformation()
    {
        Given_we_have_seeded_test_data();
        var ratings = await When_we_get_ratings_with_movie_and_user_info();
        Then_all_ratings_should_have_movie_name(ratings);
        And_all_ratings_should_have_user_name(ratings);
    }

    #endregion

    #region Filter Tests

    [Fact]
    public async Task GetRatings_FilterByMovieId_ReturnsOnlyMatchingRatings()
    {
        Given_we_have_seeded_test_data();
        var movieId = And_we_have_first_movie_id();
        var ratings = await When_we_filter_ratings_by_movie_id(movieId);
        Then_ratings_should_have_count(ratings, 2);
        And_all_ratings_should_have_movie_id(ratings, movieId);
    }

    [Fact]
    public async Task GetRatings_FilterByUserId_ReturnsOnlyMatchingRatings()
    {
        Given_we_have_seeded_test_data();
        var userId = And_we_have_first_user_id();
        var ratings = await When_we_filter_ratings_by_user_id(userId);
        Then_ratings_should_have_count(ratings, 2);
        And_all_ratings_should_have_user_id(ratings, userId);
    }

    [Fact]
    public async Task GetRatings_FilterByRatingValue_ReturnsOnlyMatchingRatings()
    {
        Given_we_have_seeded_test_data();
        var ratingValue = And_we_have_rating_value_filter();
        var ratings = await When_we_filter_ratings_by_value(ratingValue);
        Then_ratings_should_have_count(ratings, 1);
        And_all_ratings_should_have_value(ratings, ratingValue);
    }

    #endregion

    #region Sort Tests

    [Fact]
    public async Task GetRatings_SortByMovieName_ReturnsSortedRatings()
    {
        Given_we_have_seeded_test_data();
        var ratings = await When_we_get_ratings_sorted_by_movie_name();
        Then_ratings_should_be_sorted_by_movie_name(ratings);
    }

    [Fact]
    public async Task GetRatings_SortByRatingDescending_ReturnsSortedRatings()
    {
        Given_we_have_seeded_test_data();
        var ratings = await When_we_get_ratings_sorted_by_value_descending();
        Then_ratings_should_be_sorted_by_value_descending(ratings);
    }

    [Fact]
    public async Task GetRatings_SortByDateUpdated_ReturnsSortedRatings()
    {
        Given_we_have_seeded_test_data();
        var ratings = await When_we_get_ratings_sorted_by_date_updated();
        Then_ratings_should_be_sorted_by_date_updated(ratings);
    }

    #endregion

    #region Aggregation Tests

    [Fact]
    public async Task GetRatings_AverageRatingPerMovie_CalculatesCorrectly()
    {
        Given_we_have_seeded_test_data();
        var movieId = And_we_have_first_movie_id();
        var averageRating = await When_we_calculate_average_rating_for_movie(movieId);
        Then_average_rating_should_be(averageRating, 4.0f);
    }

    [Fact]
    public async Task GetRatings_CountRatingsPerMovie_CountsCorrectly()
    {
        Given_we_have_seeded_test_data();
        var movieId = And_we_have_first_movie_id();
        var count = await When_we_count_ratings_for_movie(movieId);
        Then_rating_count_should_be(count, 2);
    }

    #endregion

    #region Given Methods

    void Given_we_have_seeded_test_data()
    {
        // Data already seeded in constructor
    }

    #endregion

    #region And Methods

    Guid And_we_have_first_movie_id()
    {
        return _movie1Id;
    }

    string And_we_have_first_user_id()
    {
        return "user1";
    }

    float And_we_have_rating_value_filter()
    {
        return 5.0f;
    }

    void And_all_ratings_should_have_movie_id(List<MovieRating> ratings, Guid movieId)
    {
        ratings.Should().AllSatisfy(r => r.MovieId.Should().Be(movieId));
    }

    void And_all_ratings_should_have_user_id(List<MovieRating> ratings, string userId)
    {
        ratings.Should().AllSatisfy(r => r.UserId.Should().Be(userId));
    }

    void And_all_ratings_should_have_value(List<MovieRating> ratings, float value)
    {
        ratings.Should().AllSatisfy(r => r.Rating.Should().Be(value));
    }

    void And_all_ratings_should_have_movie_name(List<RatingWithMovieAndUser> ratings)
    {
        ratings.Should().AllSatisfy(r => r.MovieName.Should().NotBeNullOrEmpty());
    }

    void And_all_ratings_should_have_user_name(List<RatingWithMovieAndUser> ratings)
    {
        ratings.Should().AllSatisfy(r => r.UserName.Should().NotBeNullOrEmpty());
    }

    #endregion

    #region When Methods

    async Task<List<MovieRating>> When_we_get_all_ratings()
    {
        return await _context.Ratings.ToListAsync();
    }

    async Task<List<RatingWithMovieAndUser>> When_we_get_ratings_with_movie_and_user_info()
    {
        return await _context.Ratings
            .Join(_context.Movies,
                rating => rating.MovieId,
                movie => movie.MovieId,
                (rating, movie) => new { Rating = rating, Movie = movie })
            .Join(_context.Users,
                rm => rm.Rating.UserId,
                user => user.Id,
                (rm, user) => new RatingWithMovieAndUser
                {
                    Id = rm.Rating.Id,
                    MovieId = rm.Rating.MovieId,
                    Rating = rm.Rating.Rating,
                    UserId = rm.Rating.UserId,
                    DateUpdated = rm.Rating.DateUpdated,
                    MovieName = rm.Movie.Title,
                    UserName = user.UserName!
                })
            .ToListAsync();
    }

    async Task<List<MovieRating>> When_we_filter_ratings_by_movie_id(Guid movieId)
    {
        return await _context.Ratings
            .Where(r => r.MovieId == movieId)
            .ToListAsync();
    }

    async Task<List<MovieRating>> When_we_filter_ratings_by_user_id(string userId)
    {
        return await _context.Ratings
            .Where(r => r.UserId == userId)
            .ToListAsync();
    }

    async Task<List<MovieRating>> When_we_filter_ratings_by_value(float ratingValue)
    {
        return await _context.Ratings
            .Where(r => r.Rating == ratingValue)
            .ToListAsync();
    }

    async Task<List<RatingWithMovieAndUser>> When_we_get_ratings_sorted_by_movie_name()
    {
        return await _context.Ratings
            .Join(_context.Movies,
                rating => rating.MovieId,
                movie => movie.MovieId,
                (rating, movie) => new { Rating = rating, Movie = movie })
            .Join(_context.Users,
                rm => rm.Rating.UserId,
                user => user.Id,
                (rm, user) => new RatingWithMovieAndUser
                {
                    Id = rm.Rating.Id,
                    MovieId = rm.Rating.MovieId,
                    Rating = rm.Rating.Rating,
                    UserId = rm.Rating.UserId,
                    DateUpdated = rm.Rating.DateUpdated,
                    MovieName = rm.Movie.Title,
                    UserName = user.UserName!
                })
            .OrderBy(r => r.MovieName)
            .ToListAsync();
    }

    async Task<List<MovieRating>> When_we_get_ratings_sorted_by_value_descending()
    {
        return await _context.Ratings
            .OrderByDescending(r => r.Rating)
            .ToListAsync();
    }

    async Task<List<MovieRating>> When_we_get_ratings_sorted_by_date_updated()
    {
        return await _context.Ratings
            .OrderBy(r => r.DateUpdated)
            .ToListAsync();
    }

    async Task<float> When_we_calculate_average_rating_for_movie(Guid movieId)
    {
        return await _context.Ratings
            .Where(r => r.MovieId == movieId)
            .AverageAsync(r => r.Rating);
    }

    async Task<int> When_we_count_ratings_for_movie(Guid movieId)
    {
        return await _context.Ratings
            .Where(r => r.MovieId == movieId)
            .CountAsync();
    }

    #endregion

    #region Then Methods

    void Then_ratings_should_have_count(List<MovieRating> ratings, int expectedCount)
    {
        ratings.Should().HaveCount(expectedCount);
    }

    void Then_ratings_should_be_sorted_by_movie_name(List<RatingWithMovieAndUser> ratings)
    {
        ratings.Should().BeInAscendingOrder(r => r.MovieName);
    }

    void Then_ratings_should_be_sorted_by_value_descending(List<MovieRating> ratings)
    {
        ratings.Should().BeInDescendingOrder(r => r.Rating);
    }

    void Then_ratings_should_be_sorted_by_date_updated(List<MovieRating> ratings)
    {
        ratings.Should().BeInAscendingOrder(r => r.DateUpdated);
    }

    void Then_average_rating_should_be(float actualRating, float expectedRating)
    {
        actualRating.Should().BeApproximately(expectedRating, 0.01f);
    }

    void Then_rating_count_should_be(int actualCount, int expectedCount)
    {
        actualCount.Should().Be(expectedCount);
    }

    void Then_all_ratings_should_have_movie_name(List<RatingWithMovieAndUser> ratings)
    {
        ratings.Should().AllSatisfy(r => r.MovieName.Should().NotBeNullOrEmpty());
    }

    #endregion

    #region Helper Methods

    private void SeedTestData()
    {
        var users = new List<ApplicationUser>
        {
            new ApplicationUser
            {
                Id = "user1",
                UserName = "testuser1",
                Email = "test1@example.com",
                EmailConfirmed = true
            },
            new ApplicationUser
            {
                Id = "user2",
                UserName = "testuser2",
                Email = "test2@example.com",
                EmailConfirmed = true
            },
            new ApplicationUser
            {
                Id = "user3",
                UserName = "testuser3",
                Email = "test3@example.com",
                EmailConfirmed = true
            }
        };

        var movies = new List<Movie>
        {
            new Movie
            {
                MovieId = _movie1Id,
                Title = "Test Movie 1",
                YearOfRelease = 2020,
                Genres = "Action,Drama",
                UserId = "user1"
            },
            new Movie
            {
                MovieId = _movie2Id,
                Title = "Another Movie 2",
                YearOfRelease = 2021,
                Genres = "Comedy",
                UserId = "user2"
            },
            new Movie
            {
                MovieId = _movie3Id,
                Title = "Third Movie 3",
                YearOfRelease = 2019,
                Genres = "Thriller",
                UserId = "user3"
            }
        };

        var ratings = new List<MovieRating>
        {
            new MovieRating
            {
                Id = Guid.NewGuid(),
                MovieId = _movie1Id,
                Rating = 4.5f,
                UserId = "user1",
                DateUpdated = DateTime.UtcNow.AddDays(-5)
            },
            new MovieRating
            {
                Id = Guid.NewGuid(),
                MovieId = _movie1Id,
                Rating = 3.5f,
                UserId = "user2",
                DateUpdated = DateTime.UtcNow.AddDays(-4)
            },
            new MovieRating
            {
                Id = Guid.NewGuid(),
                MovieId = _movie2Id,
                Rating = 5.0f,
                UserId = "user1",
                DateUpdated = DateTime.UtcNow.AddDays(-3)
            },
            new MovieRating
            {
                Id = Guid.NewGuid(),
                MovieId = _movie2Id,
                Rating = 4.0f,
                UserId = "user3",
                DateUpdated = DateTime.UtcNow.AddDays(-2)
            },
            new MovieRating
            {
                Id = Guid.NewGuid(),
                MovieId = _movie3Id,
                Rating = 3.0f,
                UserId = "user2",
                DateUpdated = DateTime.UtcNow.AddDays(-1)
            }
        };

        _context.Users.AddRange(users);
        _context.Movies.AddRange(movies);
        _context.Ratings.AddRange(ratings);
        _context.SaveChanges();
    }

    #endregion

    public void Dispose()
    {
        _context.Dispose();
    }

    private class RatingWithMovieAndUser
    {
        public Guid Id { get; set; }
        public Guid MovieId { get; set; }
        public float Rating { get; set; }
        public string UserId { get; set; } = string.Empty;
        public DateTime? DateUpdated { get; set; }
        public string MovieName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
    }
}
