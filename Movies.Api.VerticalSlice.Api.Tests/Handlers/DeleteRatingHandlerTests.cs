using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Movies.VerticalSlice.Api.Data.Database;
using Movies.VerticalSlice.Api.Data.Models;
using Movies.VerticalSlice.Api.Features.Ratings.Delete;
using Xunit;

namespace Movies.Api.VerticalSlice.Api.Tests.Handlers;

public class DeleteRatingHandlerTests
{
    [Fact]
    public async Task Handle_ExistingRating_DeletesSuccessfully()
    {
        var (command, existingRatingId) = Given_we_have_a_valid_delete_command();
        var (handler, context) = And_we_have_a_handler_with_existing_rating(existingRatingId);
        var result = await When_we_handle_the_command(handler, command);
        Then_the_rating_should_be_deleted(result, context, existingRatingId);
    }

    [Fact]
    public async Task Handle_ExistingRating_ReturnsTrue()
    {
        var (command, existingRatingId) = Given_we_have_a_valid_delete_command();
        var (handler, _) = And_we_have_a_handler_with_existing_rating(existingRatingId);
        var result = await When_we_handle_the_command(handler, command);
        Then_the_result_should_be_true(result);
    }

    [Fact]
    public async Task Handle_NonExistentRating_ReturnsFalse()
    {
        var (command, _) = Given_we_have_a_valid_delete_command();
        var (handler, _) = And_we_have_a_handler_without_rating();
        var result = await When_we_handle_the_command(handler, command);
        Then_the_result_should_be_false(result);
    }

    [Fact]
    public async Task Handle_ExistingRating_LogsInformation()
    {
        var (command, existingRatingId) = Given_we_have_a_valid_delete_command();
        var (handler, _, mockLogger) = And_we_have_a_handler_with_existing_rating_and_logger(existingRatingId);
        await When_we_handle_the_command(handler, command);
        Then_information_should_be_logged(mockLogger, existingRatingId);
    }

    [Fact]
    public async Task Handle_NonExistentRating_LogsWarning()
    {
        var (command, ratingId) = Given_we_have_a_valid_delete_command();
        var (handler, _, mockLogger) = And_we_have_a_handler_without_rating_and_logger();
        await When_we_handle_the_command(handler, command);
        Then_warning_should_be_logged(mockLogger, ratingId);
    }

    [Fact]
    public async Task Handle_MultipleRatings_DeletesOnlySpecified()
    {
        var (command, targetRatingId) = Given_we_have_a_valid_delete_command();
        var (handler, context, otherRatingId) = And_we_have_a_handler_with_multiple_ratings(targetRatingId);
        await When_we_handle_the_command(handler, command);
        Then_only_the_target_rating_should_be_deleted(context, targetRatingId, otherRatingId);
    }

    (DeleteRatingCommand command, Guid ratingId) Given_we_have_a_valid_delete_command()
    {
        var ratingId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        return (new DeleteRatingCommand(ratingId), ratingId);
    }

    (DeleteRatingHandler handler, MoviesDbContext context) And_we_have_a_handler_with_existing_rating(
        Guid ratingId)
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

        var rating = new MovieRating
        {
            Id = ratingId,
            MovieId = movie.MovieId,
            Rating = 4.0f,
            UserId = "user-123",
            DateUpdated = DateTime.UtcNow
        };

        context.Movies.Add(movie);
        context.Ratings.Add(rating);
        context.SaveChanges();

        var handler = CreateHandler(context);
        return (handler, context);
    }

    (DeleteRatingHandler handler, MoviesDbContext context, Mock<ILogger<DeleteRatingHandler>> mockLogger) 
        And_we_have_a_handler_with_existing_rating_and_logger(Guid ratingId)
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

        var rating = new MovieRating
        {
            Id = ratingId,
            MovieId = movie.MovieId,
            Rating = 4.0f,
            UserId = "user-123",
            DateUpdated = DateTime.UtcNow
        };

        context.Movies.Add(movie);
        context.Ratings.Add(rating);
        context.SaveChanges();

        var mockLogger = new Mock<ILogger<DeleteRatingHandler>>();
        var handler = new DeleteRatingHandler(context, mockLogger.Object);

        return (handler, context, mockLogger);
    }

    (DeleteRatingHandler handler, MoviesDbContext context) And_we_have_a_handler_without_rating()
    {
        var context = CreateTestContext();
        var handler = CreateHandler(context);
        return (handler, context);
    }

    (DeleteRatingHandler handler, MoviesDbContext context, Mock<ILogger<DeleteRatingHandler>> mockLogger) 
        And_we_have_a_handler_without_rating_and_logger()
    {
        var context = CreateTestContext();

        var mockLogger = new Mock<ILogger<DeleteRatingHandler>>();
        var handler = new DeleteRatingHandler(context, mockLogger.Object);

        return (handler, context, mockLogger);
    }

    (DeleteRatingHandler handler, MoviesDbContext context, Guid otherRatingId) 
        And_we_have_a_handler_with_multiple_ratings(Guid targetRatingId)
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

        var otherRatingId = Guid.Parse("44444444-4444-4444-4444-444444444444");

        var targetRating = new MovieRating
        {
            Id = targetRatingId,
            MovieId = movie.MovieId,
            Rating = 4.0f,
            UserId = "user-123",
            DateUpdated = DateTime.UtcNow
        };

        var otherRating = new MovieRating
        {
            Id = otherRatingId,
            MovieId = movie.MovieId,
            Rating = 3.5f,
            UserId = "user-456",
            DateUpdated = DateTime.UtcNow
        };

        context.Movies.Add(movie);
        context.Ratings.Add(targetRating);
        context.Ratings.Add(otherRating);
        context.SaveChanges();

        var handler = CreateHandler(context);
        return (handler, context, otherRatingId);
    }

    async Task<bool> When_we_handle_the_command(
        DeleteRatingHandler handler,
        DeleteRatingCommand command)
    {
        return await handler.Handle(command, CancellationToken.None);
    }

    void Then_the_rating_should_be_deleted(
        bool result,
        MoviesDbContext context,
        Guid ratingId)
    {
        result.Should().BeTrue();

        var deletedRating = context.Ratings.FirstOrDefault(r => r.Id == ratingId);
        deletedRating.Should().BeNull();
    }

    void Then_the_result_should_be_true(bool result)
    {
        result.Should().BeTrue();
    }

    void Then_the_result_should_be_false(bool result)
    {
        result.Should().BeFalse();
    }

    void Then_information_should_be_logged(
        Mock<ILogger<DeleteRatingHandler>> mockLogger,
        Guid ratingId)
    {
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Successfully deleted")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    void Then_warning_should_be_logged(
        Mock<ILogger<DeleteRatingHandler>> mockLogger,
        Guid ratingId)
    {
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("not found")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    void Then_only_the_target_rating_should_be_deleted(
        MoviesDbContext context,
        Guid targetRatingId,
        Guid otherRatingId)
    {
        var targetRating = context.Ratings.FirstOrDefault(r => r.Id == targetRatingId);
        targetRating.Should().BeNull();

        var otherRating = context.Ratings.FirstOrDefault(r => r.Id == otherRatingId);
        otherRating.Should().NotBeNull();
    }

    // Helper methods to reduce duplication
    private MoviesDbContext CreateTestContext()
    {
        var options = new DbContextOptionsBuilder<MoviesDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new MoviesDbContext(options);
    }

    private DeleteRatingHandler CreateHandler(MoviesDbContext context)
    {
        var mockLogger = new Mock<ILogger<DeleteRatingHandler>>();
        return new DeleteRatingHandler(context, mockLogger.Object);
    }
}
