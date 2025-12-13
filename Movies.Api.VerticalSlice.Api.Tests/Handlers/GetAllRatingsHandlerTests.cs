using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Movies.VerticalSlice.Api.Data.Database;
using Movies.VerticalSlice.Api.Data.Models;
using Movies.VerticalSlice.Api.Features.Ratings.GetAll;
using Movies.VerticalSlice.Api.Shared.Dtos;
using Xunit;

namespace Movies.Api.VerticalSlice.Api.Tests.Handlers;

public class GetAllRatingsHandlerTests
{
    [Fact]
    public async Task Handle_WithRatings_ReturnsAllRatings()
    {
        var query = Given_we_have_a_get_all_query();
        var (handler, context) = And_we_have_a_handler_with_multiple_ratings();
        var result = await When_we_handle_the_query(handler, query);
        Then_all_ratings_should_be_returned(result);
    }

    [Fact]
    public async Task Handle_WithNoRatings_ReturnsEmptyList()
    {
        var query = Given_we_have_a_get_all_query();
        var (handler, _) = And_we_have_a_handler_without_ratings();
        var result = await When_we_handle_the_query(handler, query);
        Then_empty_list_should_be_returned(result);
    }

    [Fact]
    public async Task Handle_WithRatings_IncludesMovieNames()
    {
        var query = Given_we_have_a_get_all_query();
        var (handler, _) = And_we_have_a_handler_with_multiple_ratings();
        var result = await When_we_handle_the_query(handler, query);
        Then_movie_names_should_be_included(result);
    }

    [Fact]
    public async Task Handle_WithRatings_IncludesUserNames()
    {
        var query = Given_we_have_a_get_all_query();
        var (handler, _) = And_we_have_a_handler_with_multiple_ratings();
        var result = await When_we_handle_the_query(handler, query);
        Then_user_names_should_be_included(result);
    }

    [Fact]
    public async Task Handle_WithRatings_OrdersByMovieName()
    {
        var query = Given_we_have_a_get_all_query();
        var (handler, _) = And_we_have_a_handler_with_multiple_ratings();
        var result = await When_we_handle_the_query(handler, query);
        Then_results_should_be_ordered_by_movie_name(result);
    }

    [Fact]
    public async Task Handle_WithRatings_IncludesAllProperties()
    {
        var query = Given_we_have_a_get_all_query();
        var (handler, _) = And_we_have_a_handler_with_single_rating();
        var result = await When_we_handle_the_query(handler, query);
        Then_all_properties_should_be_populated(result);
    }

    [Fact]
    public async Task Handle_WithRatings_LogsInformation()
    {
        var query = Given_we_have_a_get_all_query();
        var (handler, _, mockLogger) = And_we_have_a_handler_with_ratings_and_logger();
        var result = await When_we_handle_the_query(handler, query);
        Then_information_should_be_logged(mockLogger, result.Count());
    }

    GetAllRatingsQuery Given_we_have_a_get_all_query()
    {
        return new GetAllRatingsQuery();
    }

    (GetAllRatingsHandler handler, MoviesDbContext context) And_we_have_a_handler_with_multiple_ratings()
    {
        var context = CreateTestContext();

        var movie1 = new Movie
        {
            MovieId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Title = "Inception",
            YearOfRelease = 2010,
            Genres = "Sci-Fi",
            UserId = "user-123"
        };

        var movie2 = new Movie
        {
            MovieId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Title = "The Matrix",
            YearOfRelease = 1999,
            Genres = "Action",
            UserId = "user-123"
        };

        var movie3 = new Movie
        {
            MovieId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            Title = "Interstellar",
            YearOfRelease = 2014,
            Genres = "Sci-Fi",
            UserId = "user-123"
        };

        var user1 = new ApplicationUser
        {
            Id = "user-123",
            UserName = "testuser1",
            Email = "test1@example.com"
        };

        var user2 = new ApplicationUser
        {
            Id = "user-456",
            UserName = "testuser2",
            Email = "test2@example.com"
        };

        var rating1 = new MovieRating
        {
            Id = Guid.NewGuid(),
            MovieId = movie1.MovieId,
            Rating = 5.0f,
            UserId = user1.Id,
            DateUpdated = DateTime.UtcNow
        };

        var rating2 = new MovieRating
        {
            Id = Guid.NewGuid(),
            MovieId = movie2.MovieId,
            Rating = 4.5f,
            UserId = user2.Id,
            DateUpdated = DateTime.UtcNow
        };

        var rating3 = new MovieRating
        {
            Id = Guid.NewGuid(),
            MovieId = movie3.MovieId,
            Rating = 4.0f,
            UserId = user1.Id,
            DateUpdated = DateTime.UtcNow
        };

        context.Movies.AddRange(movie1, movie2, movie3);
        context.Users.AddRange(user1, user2);
        context.Ratings.AddRange(rating1, rating2, rating3);
        context.SaveChanges();

        var handler = CreateHandler(context);
        return (handler, context);
    }

    (GetAllRatingsHandler handler, MoviesDbContext context) And_we_have_a_handler_without_ratings()
    {
        var context = CreateTestContext();
        var handler = CreateHandler(context);
        return (handler, context);
    }

    (GetAllRatingsHandler handler, MoviesDbContext context) And_we_have_a_handler_with_single_rating()
    {
        var context = CreateTestContext();

        var movie = new Movie
        {
            MovieId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Title = "Test Movie",
            YearOfRelease = 2023,
            Genres = "Drama",
            UserId = "user-123"
        };

        var user = new ApplicationUser
        {
            Id = "user-123",
            UserName = "testuser",
            Email = "test@example.com"
        };

        var rating = new MovieRating
        {
            Id = Guid.Parse("99999999-9999-9999-9999-999999999999"),
            MovieId = movie.MovieId,
            Rating = 4.5f,
            UserId = user.Id,
            DateUpdated = DateTime.UtcNow
        };

        context.Movies.Add(movie);
        context.Users.Add(user);
        context.Ratings.Add(rating);
        context.SaveChanges();

        var handler = CreateHandler(context);
        return (handler, context);
    }

    (GetAllRatingsHandler handler, MoviesDbContext context, Mock<ILogger<GetAllRatingsHandler>> mockLogger) 
        And_we_have_a_handler_with_ratings_and_logger()
    {
        var context = CreateTestContext();

        var movie = new Movie
        {
            MovieId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Title = "Test Movie",
            YearOfRelease = 2023,
            Genres = "Action",
            UserId = "user-123"
        };

        var user = new ApplicationUser
        {
            Id = "user-123",
            UserName = "testuser",
            Email = "test@example.com"
        };

        var rating = new MovieRating
        {
            Id = Guid.NewGuid(),
            MovieId = movie.MovieId,
            Rating = 4.0f,
            UserId = user.Id,
            DateUpdated = DateTime.UtcNow
        };

        context.Movies.Add(movie);
        context.Users.Add(user);
        context.Ratings.Add(rating);
        context.SaveChanges();

        var mockLogger = new Mock<ILogger<GetAllRatingsHandler>>();
        var handler = new GetAllRatingsHandler(context, mockLogger.Object);

        return (handler, context, mockLogger);
    }

    async Task<IEnumerable<MovieRatingWithNameDto>> When_we_handle_the_query(
        GetAllRatingsHandler handler,
        GetAllRatingsQuery query)
    {
        return await handler.Handle(query, CancellationToken.None);
    }

    void Then_all_ratings_should_be_returned(IEnumerable<MovieRatingWithNameDto> result)
    {
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
    }

    void Then_empty_list_should_be_returned(IEnumerable<MovieRatingWithNameDto> result)
    {
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    void Then_movie_names_should_be_included(IEnumerable<MovieRatingWithNameDto> result)
    {
        result.Should().AllSatisfy(r => r.MovieName.Should().NotBeNullOrEmpty());
        result.Select(r => r.MovieName).Should().Contain(new[] { "Inception", "The Matrix", "Interstellar" });
    }

    void Then_user_names_should_be_included(IEnumerable<MovieRatingWithNameDto> result)
    {
        result.Should().AllSatisfy(r => r.UserName.Should().NotBeNullOrEmpty());
    }

    void Then_results_should_be_ordered_by_movie_name(IEnumerable<MovieRatingWithNameDto> result)
    {
        var movieNames = result.Select(r => r.MovieName).ToList();
        var orderedMovieNames = movieNames.OrderBy(n => n).ToList();
        movieNames.Should().Equal(orderedMovieNames);
    }

    void Then_all_properties_should_be_populated(IEnumerable<MovieRatingWithNameDto> result)
    {
        var rating = result.First();
        rating.Id.Should().NotBeEmpty();
        rating.MovieId.Should().NotBeEmpty();
        rating.Rating.Should().BeGreaterThan(0);
        rating.UserId.Should().NotBeNullOrEmpty();
        rating.MovieName.Should().Be("Test Movie");
        rating.Genres.Should().Be("Drama");
        rating.UserName.Should().Be("testuser");
    }

    void Then_information_should_be_logged(
        Mock<ILogger<GetAllRatingsHandler>> mockLogger,
        int count)
    {
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Retrieved")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    // Helper methods to reduce duplication
    private MoviesDbContext CreateTestContext()
    {
        var options = new DbContextOptionsBuilder<MoviesDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new MoviesDbContext(options);
    }

    private GetAllRatingsHandler CreateHandler(MoviesDbContext context)
    {
        var mockLogger = new Mock<ILogger<GetAllRatingsHandler>>();
        return new GetAllRatingsHandler(context, mockLogger.Object);
    }
}
