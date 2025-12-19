using FluentAssertions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Movies.VerticalSlice.Api.Data.Database;
using Movies.VerticalSlice.Api.Data.Models;
using Movies.VerticalSlice.Api.Features.Movies.Update;
using Xunit;

namespace Movies.Api.VerticalSlice.Api.Tests.Handlers;

public class UpdateMovieHandlerTests
{
    [Fact]
    public async Task Handle_ValidCommand_UpdatesMovieSuccessfully()
    {
        var command = Given_we_have_a_valid_update_command();
        var (handler, context) = And_we_have_a_handler_with_existing_movie(command.MovieId);
        var result = await When_we_handle_the_command(handler, command);
        Then_a_movie_should_be_updated(result, context, command);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsTrue()
    {
        var command = Given_we_have_a_valid_update_command();
        var (handler, _) = And_we_have_a_handler_with_existing_movie(command.MovieId);
        var result = await When_we_handle_the_command(handler, command);
        Then_the_result_should_be_true(result);
    }

    [Fact]
    public async Task Handle_NonExistentMovie_ReturnsFalse()
    {
        var command = Given_we_have_a_valid_update_command();
        var (handler, _) = And_we_have_a_handler_without_movie();
        await Then_validation_exception_should_be_thrown(handler, command);
    }

    [Fact]
    public async Task Handle_InvalidCommand_ThrowsValidationException()
    {
        var command = Given_we_have_an_invalid_update_command();
        var (handler, _) = And_we_have_a_handler_without_movie();
        await Then_validation_exception_should_be_thrown(handler, command);
    }

    [Fact]
    public async Task Handle_InvalidYearOfRelease_ThrowsValidationException()
    {
        var command = Given_we_have_a_command_with_invalid_year();
        var (handler, _) = And_we_have_a_handler_with_existing_movie(command.MovieId);
        await Then_validation_exception_should_be_thrown(handler, command);
    }

    UpdateMovieCommand Given_we_have_a_valid_update_command()
    {
        return new UpdateMovieCommand(
            MovieId: Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Title: "Updated Movie",
            YearOfRelease: 2024,
            Genres: "Action,Drama",
            UserId: "user-123");
    }

    UpdateMovieCommand Given_we_have_an_invalid_update_command()
    {
        return new UpdateMovieCommand(
            MovieId: Guid.Empty,
            Title: "",
            YearOfRelease: 2024,
            Genres: "Action",
            UserId: "user-123");
    }

    UpdateMovieCommand Given_we_have_a_command_with_invalid_year()
    {
        return new UpdateMovieCommand(
            MovieId: Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Title: "Updated Movie",
            YearOfRelease: 1700,
            Genres: "Action",
            UserId: "user-123");
    }

    (UpdateMovieHandler handler, MoviesDbContext context) And_we_have_a_handler_with_existing_movie(Guid movieId)
    {
        var context = CreateTestContext();

        var movie = new Movie
        {
            MovieId = movieId,
            Title = "Original Movie",
            YearOfRelease = 2023,
            Genres = "Action",
            UserId = "user-123",
            DateUpdated = DateTime.UtcNow
        };

        context.Movies.Add(movie);
        context.SaveChanges();

        var handler = CreateHandler(context);
        return (handler, context);
    }

    (UpdateMovieHandler handler, MoviesDbContext context) And_we_have_a_handler_without_movie()
    {
        var context = CreateTestContext();
        var handler = CreateHandler(context);
        return (handler, context);
    }

    async Task<bool> When_we_handle_the_command(
        UpdateMovieHandler handler,
        UpdateMovieCommand command)
    {
        return await handler.Handle(command, CancellationToken.None);
    }

    void Then_a_movie_should_be_updated(
        bool result,
        MoviesDbContext context,
        UpdateMovieCommand command)
    {
        result.Should().BeTrue();

        var updatedMovie = context.Movies.FirstOrDefault(m => m.MovieId == command.MovieId);
        updatedMovie.Should().NotBeNull();
        updatedMovie!.Title.Should().Be(command.Title);
        updatedMovie.YearOfRelease.Should().Be(command.YearOfRelease);
        updatedMovie.Genres.Should().Be(command.Genres);
        updatedMovie.UserId.Should().Be(command.UserId);
    }

    void Then_the_result_should_be_true(bool result)
    {
        result.Should().BeTrue();
    }

    async Task Then_validation_exception_should_be_thrown(
        UpdateMovieHandler handler,
        UpdateMovieCommand command)
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

    private UpdateMovieHandler CreateHandler(MoviesDbContext context)
    {
        var validator = new UpdateMovieValidator(context);
        var mockLogger = new Mock<ILogger<UpdateMovieHandler>>();
        return new UpdateMovieHandler(context, validator, mockLogger.Object);
    }
}
