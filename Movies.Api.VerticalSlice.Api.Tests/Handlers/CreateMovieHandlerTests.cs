using FluentAssertions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Movies.VerticalSlice.Api.Data.Database;
using Movies.VerticalSlice.Api.Data.Models;
using Movies.VerticalSlice.Api.Features.Movies.Create;
using Xunit;

namespace Movies.Api.VerticalSlice.Api.Tests.Handlers;

public class CreateMovieHandlerTests
{
    [Fact]
    public async Task Handle_ValidCommand_CreatesMovieSuccessfully()
    {
        var command = Given_we_have_a_valid_create_command();
        var (handler, context) = And_we_have_a_handler();
        var result = await When_we_handle_the_command(handler, command);
        Then_a_movie_should_be_created(result, context, command);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsNewMovieId()
    {
        var command = Given_we_have_a_valid_create_command();
        var (handler, _) = And_we_have_a_handler();
        var result = await When_we_handle_the_command(handler, command);
        Then_the_movie_id_should_be_valid(result);
    }

    [Fact]
    public async Task Handle_InvalidCommand_ThrowsValidationException()
    {
        var command = Given_we_have_an_invalid_create_command();
        var (handler, _) = And_we_have_a_handler();
        await Then_validation_exception_should_be_thrown(handler, command);
    }

    [Fact]
    public async Task Handle_DuplicateMovie_ThrowsValidationException()
    {
        var command = Given_we_have_a_valid_create_command();
        var (handler, context) = And_we_have_a_handler_with_existing_movie();
        await Then_validation_exception_should_be_thrown(handler, command);
    }

    [Fact]
    public async Task Handle_InvalidYearOfRelease_ThrowsValidationException()
    {
        var command = Given_we_have_a_command_with_invalid_year();
        var (handler, _) = And_we_have_a_handler();
        await Then_validation_exception_should_be_thrown(handler, command);
    }

    CreateMovieCommand Given_we_have_a_valid_create_command()
    {
        return new CreateMovieCommand(
            Title: "Test Movie",
            YearOfRelease: 2024,
            Genres: "Action,Drama",
            UserId: "user-123");
    }

    CreateMovieCommand Given_we_have_an_invalid_create_command()
    {
        return new CreateMovieCommand(
            Title: "",
            YearOfRelease: 2024,
            Genres: "Action",
            UserId: "user-123");
    }

    CreateMovieCommand Given_we_have_a_command_with_invalid_year()
    {
        return new CreateMovieCommand(
            Title: "Future Movie",
            YearOfRelease: 1800,
            Genres: "Action",
            UserId: "user-123");
    }

    (CreateMovieHandler handler, MoviesDbContext context) And_we_have_a_handler()
    {
        var context = CreateTestContext();
        var handler = CreateHandler(context);
        return (handler, context);
    }

    (CreateMovieHandler handler, MoviesDbContext context) And_we_have_a_handler_with_existing_movie()
    {
        var context = CreateTestContext();

        var existingMovie = new Movie
        {
            MovieId = Guid.NewGuid(),
            Title = "Test Movie",
            YearOfRelease = 2024,
            Genres = "Action,Drama",
            UserId = "user-123",
            DateUpdated = DateTime.UtcNow
        };

        context.Movies.Add(existingMovie);
        context.SaveChanges();

        var handler = CreateHandler(context);
        return (handler, context);
    }

    async Task<Guid> When_we_handle_the_command(
        CreateMovieHandler handler,
        CreateMovieCommand command)
    {
        return await handler.Handle(command, CancellationToken.None);
    }

    void Then_a_movie_should_be_created(
        Guid result,
        MoviesDbContext context,
        CreateMovieCommand command)
    {
        result.Should().NotBeEmpty();

        var createdMovie = context.Movies.FirstOrDefault(m => m.MovieId == result);
        createdMovie.Should().NotBeNull();
        createdMovie!.Title.Should().Be(command.Title);
        createdMovie.YearOfRelease.Should().Be(command.YearOfRelease);
        createdMovie.Genres.Should().Be(command.Genres);
        createdMovie.UserId.Should().Be(command.UserId);
    }

    void Then_the_movie_id_should_be_valid(Guid result)
    {
        result.Should().NotBeEmpty();
    }

    async Task Then_validation_exception_should_be_thrown(
        CreateMovieHandler handler,
        CreateMovieCommand command)
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

    private CreateMovieHandler CreateHandler(MoviesDbContext context)
    {
        var validator = new CreateMovieValidator(context);
        var mockLogger = new Mock<ILogger<CreateMovieHandler>>();
        return new CreateMovieHandler(context, validator, mockLogger.Object);
    }
}
