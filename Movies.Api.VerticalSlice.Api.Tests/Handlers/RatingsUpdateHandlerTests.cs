using FluentAssertions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Movies.VerticalSlice.Api.Data.Database;
using Movies.VerticalSlice.Api.Data.Models;
using Movies.VerticalSlice.Api.Features.Ratings.Update;
using Xunit;

namespace Movies.Api.VerticalSlice.Api.Tests.Handlers;

public class RatingsUpdateHandlerTests
{
    [Fact]
    public async Task Handle_ValidCommand_UpdatesRatingSuccessfully()
    {
        var (command, existingRatingId) = Given_we_have_a_valid_update_command();
        var (handler, context) = And_we_have_a_handler_with_existing_rating(existingRatingId, command.MovieId);
        var result = await When_we_handle_the_command(handler, command);
        Then_the_rating_should_be_updated(result, context, command);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsTrue()
    {
        var (command, existingRatingId) = Given_we_have_a_valid_update_command();
        var (handler, _) = And_we_have_a_handler_with_existing_rating(existingRatingId, command.MovieId);
        var result = await When_we_handle_the_command(handler, command);
        Then_the_result_should_be_true(result);
    }

    [Fact]
    public async Task Handle_NonExistentRating_ThrowsValidationException()
    {
        var (command, _) = Given_we_have_a_valid_update_command();
        var (handler, _) = And_we_have_a_handler_without_rating();
        await Then_validation_exception_should_be_thrown(handler, command);
    }

    [Fact]
    public async Task Handle_NonExistentMovie_ThrowsValidationException()
    {
        var (command, existingRatingId) = Given_we_have_a_valid_update_command();
        var (handler, _) = And_we_have_a_handler_with_rating_but_no_movie(existingRatingId);
        await Then_validation_exception_should_be_thrown(handler, command);
    }

    [Fact]
    public async Task Handle_InvalidRatingRange_ThrowsValidationException()
    {
        var (command, existingRatingId) = Given_we_have_a_command_with_invalid_rating();
        var (handler, _) = And_we_have_a_handler_with_existing_rating(existingRatingId, command.MovieId);
        await Then_validation_exception_should_be_thrown(handler, command);
    }

    [Fact]
    public async Task Handle_ValidCommand_UpdatesDateUpdated()
    {
        var (command, existingRatingId) = Given_we_have_a_valid_update_command();
        var (handler, context) = And_we_have_a_handler_with_existing_rating(existingRatingId, command.MovieId);
        await When_we_handle_the_command(handler, command);
        Then_the_date_updated_should_be_modified(context, command.RatingsId);
    }

    (RatingsUpdateCommand command, Guid existingRatingId) Given_we_have_a_valid_update_command()
    {
        var ratingId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        return (new RatingsUpdateCommand(
            RatingsId: ratingId,
            MovieId: Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Rating: 4.5f,
            DateUpdated: DateTime.UtcNow,
            UserId: "user-123"), ratingId);
    }

    (RatingsUpdateCommand command, Guid existingRatingId) Given_we_have_a_command_with_invalid_rating()
    {
        var ratingId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        return (new RatingsUpdateCommand(
            RatingsId: ratingId,
            MovieId: Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Rating: 0.0f,
            DateUpdated: DateTime.UtcNow,
            UserId: "user-123"), ratingId);
    }

    (RatingsUpdateHandler handler, MoviesDbContext context) And_we_have_a_handler_with_existing_rating(
        Guid ratingId,
        Guid movieId)
    {
        var options = new DbContextOptionsBuilder<MoviesDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new MoviesDbContext(options);

        var movie = new Movie
        {
            MovieId = movieId,
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
            Id = ratingId,
            MovieId = movieId,
            Rating = 3.0f,
            UserId = "user-123",
            DateUpdated = DateTime.UtcNow.AddDays(-1)
        };

        context.Movies.Add(movie);
        context.Users.Add(user);
        context.Ratings.Add(rating);
        context.SaveChanges();

        var validator = new RatingsUpdateValidator(context);
        var mockLogger = new Mock<ILogger<RatingsUpdateHandler>>();

        var handler = new RatingsUpdateHandler(context, validator, mockLogger.Object);

        return (handler, context);
    }

    (RatingsUpdateHandler handler, MoviesDbContext context) And_we_have_a_handler_without_rating()
    {
        var options = new DbContextOptionsBuilder<MoviesDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new MoviesDbContext(options);

        var validator = new RatingsUpdateValidator(context);
        var mockLogger = new Mock<ILogger<RatingsUpdateHandler>>();

        var handler = new RatingsUpdateHandler(context, validator, mockLogger.Object);

        return (handler, context);
    }

    (RatingsUpdateHandler handler, MoviesDbContext context) And_we_have_a_handler_with_rating_but_no_movie(
        Guid ratingId)
    {
        var options = new DbContextOptionsBuilder<MoviesDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new MoviesDbContext(options);

        var user = new ApplicationUser
        {
            Id = "user-123",
            UserName = "testuser",
            Email = "test@example.com"
        };

        var rating = new MovieRating
        {
            Id = ratingId,
            MovieId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Rating = 3.0f,
            UserId = "user-123",
            DateUpdated = DateTime.UtcNow.AddDays(-1)
        };

        context.Users.Add(user);
        context.Ratings.Add(rating);
        context.SaveChanges();

        var validator = new RatingsUpdateValidator(context);
        var mockLogger = new Mock<ILogger<RatingsUpdateHandler>>();

        var handler = new RatingsUpdateHandler(context, validator, mockLogger.Object);

        return (handler, context);
    }

    async Task<bool> When_we_handle_the_command(
        RatingsUpdateHandler handler,
        RatingsUpdateCommand command)
    {
        return await handler.Handle(command, CancellationToken.None);
    }

    void Then_the_rating_should_be_updated(
        bool result,
        MoviesDbContext context,
        RatingsUpdateCommand command)
    {
        result.Should().BeTrue();

        var updatedRating = context.Ratings.FirstOrDefault(r => r.Id == command.RatingsId);
        updatedRating.Should().NotBeNull();
        updatedRating!.Rating.Should().Be(command.Rating);
        updatedRating.MovieId.Should().Be(command.MovieId);
    }

    void Then_the_result_should_be_true(bool result)
    {
        result.Should().BeTrue();
    }

    void Then_the_date_updated_should_be_modified(
        MoviesDbContext context,
        Guid ratingId)
    {
        var updatedRating = context.Ratings.FirstOrDefault(r => r.Id == ratingId);
        updatedRating.Should().NotBeNull();
        updatedRating!.DateUpdated.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(5));
    }

    async Task Then_validation_exception_should_be_thrown(
        RatingsUpdateHandler handler,
        RatingsUpdateCommand command)
    {
        await Assert.ThrowsAsync<ValidationException>(
            () => handler.Handle(command, CancellationToken.None));
    }
}
