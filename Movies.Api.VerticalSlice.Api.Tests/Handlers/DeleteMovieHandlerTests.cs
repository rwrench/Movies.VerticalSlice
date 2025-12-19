using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Movies.VerticalSlice.Api.Data.Database;
using Movies.VerticalSlice.Api.Data.Models;
using Movies.VerticalSlice.Api.Features.Movies.Delete;
using Xunit;

namespace Movies.Api.VerticalSlice.Api.Tests.Handlers;

public class DeleteMovieHandlerTests
{
    [Fact]
    public async Task Handle_ValidCommand_DeletesMovieSuccessfully()
    {
        var command = Given_we_have_a_valid_delete_command();
        var (handler, context) = And_we_have_a_handler_with_existing_movie(command.MovieId);
        var result = await When_we_handle_the_command(handler, command);
        Then_a_movie_should_be_deleted(result, context, command.MovieId);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsTrue()
    {
        var command = Given_we_have_a_valid_delete_command();
        var (handler, _) = And_we_have_a_handler_with_existing_movie(command.MovieId);
        var result = await When_we_handle_the_command(handler, command);
        Then_the_result_should_be_true(result);
    }

    [Fact]
    public async Task Handle_NonExistentMovie_ReturnsFalse()
    {
        var command = Given_we_have_a_valid_delete_command();
        var (handler, _) = And_we_have_a_handler_without_movie();
        var result = await When_we_handle_the_command(handler, command);
        Then_the_result_should_be_false(result);
    }

    DeleteMovieCommand Given_we_have_a_valid_delete_command()
    {
        return new DeleteMovieCommand(
            MovieId: Guid.Parse("11111111-1111-1111-1111-111111111111"));
    }

    (DeleteMovieHandler handler, MoviesDbContext context) And_we_have_a_handler_with_existing_movie(Guid movieId)
    {
        var context = CreateTestContext();

        var movie = new Movie
        {
            MovieId = movieId,
            Title = "Test Movie",
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

    (DeleteMovieHandler handler, MoviesDbContext context) And_we_have_a_handler_without_movie()
    {
        var context = CreateTestContext();
        var handler = CreateHandler(context);
        return (handler, context);
    }

    async Task<bool> When_we_handle_the_command(
        DeleteMovieHandler handler,
        DeleteMovieCommand command)
    {
        return await handler.Handle(command, CancellationToken.None);
    }

    void Then_a_movie_should_be_deleted(
        bool result,
        MoviesDbContext context,
        Guid movieId)
    {
        result.Should().BeTrue();

        var deletedMovie = context.Movies.FirstOrDefault(m => m.MovieId == movieId);
        deletedMovie.Should().BeNull();
    }

    void Then_the_result_should_be_true(bool result)
    {
        result.Should().BeTrue();
    }

    void Then_the_result_should_be_false(bool result)
    {
        result.Should().BeFalse();
    }

    // Helper methods to reduce duplication
    private MoviesDbContext CreateTestContext()
    {
        var options = new DbContextOptionsBuilder<MoviesDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new MoviesDbContext(options);
    }

    private DeleteMovieHandler CreateHandler(MoviesDbContext context)
    {
        var mockLogger = new Mock<ILogger<DeleteMovieHandler>>();
        return new DeleteMovieHandler(context, mockLogger.Object);
    }
}
