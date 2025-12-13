using FluentAssertions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Movies.VerticalSlice.Api.Data.Database;
using Movies.VerticalSlice.Api.Data.Models;
using Movies.VerticalSlice.Api.Features.Ratings.Create;
using Xunit;

namespace Movies.Api.VerticalSlice.Api.Tests.Handlers;

public class RatingsCreateHandlerTests
{
    [Fact]
    public async Task Handle_ValidCommand_CreatesRatingSuccessfully()
    {
        var command = Given_we_have_a_valid_create_command();
        var (handler, context) = And_we_have_a_handler_with_valid_movie();
        var result = await When_we_handle_the_command(handler, command);
        Then_a_rating_should_be_created(result, context, command);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsNewRatingId()
    {
        var command = Given_we_have_a_valid_create_command();
        var (handler, _) = And_we_have_a_handler_with_valid_movie();
        var result = await When_we_handle_the_command(handler, command);
        Then_the_rating_id_should_be_valid(result);
    }

    [Fact]
    public async Task Handle_InvalidCommand_ThrowsValidationException()
    {
        var command = Given_we_have_an_invalid_create_command();
        var (handler, _) = And_we_have_a_handler_without_movie();
        await Then_validation_exception_should_be_thrown(handler, command);
    }

    [Fact]
    public async Task Handle_DuplicateRating_ThrowsValidationException()
    {
        var command = Given_we_have_a_valid_create_command();
        var (handler, context) = And_we_have_a_handler_with_existing_rating();
        await Then_validation_exception_should_be_thrown(handler, command);
    }

    [Fact]
    public async Task Handle_NonExistentMovie_ThrowsValidationException()
    {
        var command = Given_we_have_a_valid_create_command();
        var (handler, _) = And_we_have_a_handler_without_movie();
        await Then_validation_exception_should_be_thrown(handler, command);
    }

    [Fact]
    public async Task Handle_InvalidRatingRange_ThrowsValidationException()
    {
        var command = Given_we_have_a_command_with_invalid_rating();
        var (handler, _) = And_we_have_a_handler_with_valid_movie();
        await Then_validation_exception_should_be_thrown(handler, command);
    }

    RatingsCreateCommand Given_we_have_a_valid_create_command()
    {
        return new RatingsCreateCommand(
            MovieId: Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Rating: 4.5f,
            DateUpdated: DateTime.UtcNow,
            UserId: "user-123");
    }

    RatingsCreateCommand Given_we_have_an_invalid_create_command()
    {
        return new RatingsCreateCommand(
            MovieId: Guid.Empty,
            Rating: 4.5f,
            DateUpdated: DateTime.UtcNow,
            UserId: "user-123");
    }

    RatingsCreateCommand Given_we_have_a_command_with_invalid_rating()
    {
        return new RatingsCreateCommand(
            MovieId: Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Rating: 6.0f,
            DateUpdated: DateTime.UtcNow,
            UserId: "user-123");
    }

    (RatingsCreateHandler handler, MoviesDbContext context) And_we_have_a_handler_with_valid_movie()
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

        context.Movies.Add(movie);
        context.Users.Add(user);
        context.SaveChanges();

        var handler = CreateHandler(context);
        return (handler, context);
    }

    (RatingsCreateHandler handler, MoviesDbContext context) And_we_have_a_handler_without_movie()
    {
        var context = CreateTestContext();
        var handler = CreateHandler(context);
        return (handler, context);
    }

    (RatingsCreateHandler handler, MoviesDbContext context) And_we_have_a_handler_with_existing_rating()
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

        var existingRating = new MovieRating
        {
            Id = Guid.NewGuid(),
            MovieId = movie.MovieId,
            Rating = 3.5f,
            UserId = "user-123",
            DateUpdated = DateTime.UtcNow
        };

        context.Movies.Add(movie);
        context.Users.Add(user);
        context.Ratings.Add(existingRating);
        context.SaveChanges();

        var handler = CreateHandler(context);
        return (handler, context);
    }

    async Task<Guid> When_we_handle_the_command(
        RatingsCreateHandler handler,
        RatingsCreateCommand command)
    {
        return await handler.Handle(command, CancellationToken.None);
    }

    void Then_a_rating_should_be_created(
        Guid result,
        MoviesDbContext context,
        RatingsCreateCommand command)
    {
        result.Should().NotBeEmpty();

        var createdRating = context.Ratings.FirstOrDefault(r => r.Id == result);
        createdRating.Should().NotBeNull();
        createdRating!.MovieId.Should().Be(command.MovieId);
        createdRating.Rating.Should().Be(command.Rating);
        createdRating.UserId.Should().Be(command.UserId);
    }

    void Then_the_rating_id_should_be_valid(Guid result)
    {
        result.Should().NotBeEmpty();
    }

    async Task Then_validation_exception_should_be_thrown(
        RatingsCreateHandler handler,
        RatingsCreateCommand command)
    {
        await Assert.ThrowsAsync<ValidationException>(
            () => handler.Handle(command, CancellationToken.None));
    }

    // Helper methods to reduce duplication
    private MoviesDbContext CreateTestContext()
    {
        var options = new DbContextOptionsBuilder<MoviesDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new MoviesDbContext(options);
    }

    private RatingsCreateHandler CreateHandler(MoviesDbContext context)
    {
        var validator = new RatingsCreateValidator(context);
        var mockLogger = new Mock<ILogger<RatingsCreateHandler>>();
        return new RatingsCreateHandler(context, validator, mockLogger.Object);
    }
}
