using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using Movies.VerticalSlice.Api.Data.Database;
using Movies.VerticalSlice.Api.Data.Models;
using Movies.VerticalSlice.Api.Features.Movies.Update;
using Xunit;

namespace Movies.Api.VerticalSlice.Api.Tests.Validators;

public class UpdateMovieValidatorTests
{
    [Fact]
    public async Task Validate_ValidCommand_PassesValidation()
    {
        var command = Given_we_have_a_valid_command();
        var validator = And_we_have_a_validator_with_existing_movie();
        var result = await When_we_validate_the_command(validator, command);
        Then_validation_should_pass(result);
    }

    [Fact]
    public async Task Validate_NonExistentMovie_FailsValidation()
    {
        var command = Given_we_have_a_valid_command();
        var validator = And_we_have_a_validator_without_movie();
        var result = await When_we_validate_the_command(validator, command);
        Then_validation_should_fail_for_non_existent_movie(result);
    }

    [Fact]
    public async Task Validate_EmptyTitle_FailsValidation()
    {
        var command = Given_we_have_a_command_with_empty_title();
        var validator = And_we_have_a_validator_with_existing_movie();
        var result = await When_we_validate_the_command(validator, command);
        Then_validation_should_fail_for_empty_title(result);
    }

    [Fact]
    public async Task Validate_TitleTooLong_FailsValidation()
    {
        var command = Given_we_have_a_command_with_title_too_long();
        var validator = And_we_have_a_validator_with_existing_movie();
        var result = await When_we_validate_the_command(validator, command);
        Then_validation_should_fail_for_title_length(result);
    }

    [Fact]
    public async Task Validate_YearTooEarly_FailsValidation()
    {
        var command = Given_we_have_a_command_with_year_too_early();
        var validator = And_we_have_a_validator_with_existing_movie();
        var result = await When_we_validate_the_command(validator, command);
        Then_validation_should_fail_for_year_too_early(result);
    }

    [Fact]
    public async Task Validate_YearTooFarInFuture_FailsValidation()
    {
        var command = Given_we_have_a_command_with_year_too_far_in_future();
        var validator = And_we_have_a_validator_with_existing_movie();
        var result = await When_we_validate_the_command(validator, command);
        Then_validation_should_fail_for_year_too_far_in_future(result);
    }

    [Fact]
    public async Task Validate_YearAtMinimum_PassesValidation()
    {
        var command = Given_we_have_a_command_with_minimum_year();
        var validator = And_we_have_a_validator_with_existing_movie();
        var result = await When_we_validate_the_command(validator, command);
        Then_validation_should_pass(result);
    }

    [Fact]
    public async Task Validate_YearAtMaximum_PassesValidation()
    {
        var command = Given_we_have_a_command_with_maximum_year();
        var validator = And_we_have_a_validator_with_existing_movie();
        var result = await When_we_validate_the_command(validator, command);
        Then_validation_should_pass(result);
    }

    [Fact]
    public async Task Validate_EmptyMovieId_FailsValidation()
    {
        var command = Given_we_have_a_command_with_empty_movie_id();
        var validator = And_we_have_a_validator_without_movie();
        var result = await When_we_validate_the_command(validator, command);
        Then_validation_should_fail_for_empty_movie_id(result);
    }

    [Fact]
    public async Task Validate_UpdatingTitleAndYear_PassesValidation()
    {
        var command = Given_we_have_a_command_updating_title_and_year();
        var validator = And_we_have_a_validator_with_existing_movie();
        var result = await When_we_validate_the_command(validator, command);
        Then_validation_should_pass(result);
    }

    UpdateMovieCommand Given_we_have_a_valid_command()
    {
        return new UpdateMovieCommand(
            MovieId: Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Title: "Updated Movie",
            YearOfRelease: 2024,
            Genres: "Action,Drama",
            UserId: "user-123");
    }

    UpdateMovieCommand Given_we_have_a_command_with_empty_title()
    {
        return new UpdateMovieCommand(
            MovieId: Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Title: "",
            YearOfRelease: 2024,
            Genres: "Action",
            UserId: "user-123");
    }

    UpdateMovieCommand Given_we_have_a_command_with_title_too_long()
    {
        return new UpdateMovieCommand(
            MovieId: Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Title: new string('A', 201),
            YearOfRelease: 2024,
            Genres: "Action",
            UserId: "user-123");
    }

    UpdateMovieCommand Given_we_have_a_command_with_year_too_early()
    {
        return new UpdateMovieCommand(
            MovieId: Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Title: "Old Movie",
            YearOfRelease: 1888,
            Genres: "Drama",
            UserId: "user-123");
    }

    UpdateMovieCommand Given_we_have_a_command_with_year_too_far_in_future()
    {
        return new UpdateMovieCommand(
            MovieId: Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Title: "Future Movie",
            YearOfRelease: DateTime.UtcNow.Year + 6,
            Genres: "Sci-Fi",
            UserId: "user-123");
    }

    UpdateMovieCommand Given_we_have_a_command_with_minimum_year()
    {
        return new UpdateMovieCommand(
            MovieId: Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Title: "Early Movie",
            YearOfRelease: 1889,
            Genres: "Drama",
            UserId: "user-123");
    }

    UpdateMovieCommand Given_we_have_a_command_with_maximum_year()
    {
        return new UpdateMovieCommand(
            MovieId: Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Title: "Future Movie",
            YearOfRelease: DateTime.UtcNow.Year + 5,
            Genres: "Sci-Fi",
            UserId: "user-123");
    }

    UpdateMovieCommand Given_we_have_a_command_with_empty_movie_id()
    {
        return new UpdateMovieCommand(
            MovieId: Guid.Empty,
            Title: "Updated Movie",
            YearOfRelease: 2024,
            Genres: "Action",
            UserId: "user-123");
    }

    UpdateMovieCommand Given_we_have_a_command_updating_title_and_year()
    {
        return new UpdateMovieCommand(
            MovieId: Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Title: "Completely New Title",
            YearOfRelease: 2025,
            Genres: "Comedy",
            UserId: "user-123");
    }

    UpdateMovieValidator And_we_have_a_validator_with_existing_movie()
    {
        var context = CreateTestContext();

        var movie = new Movie
        {
            MovieId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Title = "Original Movie",
            YearOfRelease = 2023,
            Genres = "Action",
            UserId = "user-123",
            DateUpdated = DateTime.UtcNow
        };

        context.Movies.Add(movie);
        context.SaveChanges();

        return new UpdateMovieValidator(context);
    }

    UpdateMovieValidator And_we_have_a_validator_without_movie()
    {
        var context = CreateTestContext();
        return new UpdateMovieValidator(context);
    }

    async Task<FluentValidation.Results.ValidationResult> When_we_validate_the_command(
        UpdateMovieValidator validator,
        UpdateMovieCommand command)
    {
        return await validator.ValidateAsync(command);
    }

    void Then_validation_should_pass(FluentValidation.Results.ValidationResult result)
    {
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    void Then_validation_should_fail_for_non_existent_movie(FluentValidation.Results.ValidationResult result)
    {
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Movie does not exist");
    }

    void Then_validation_should_fail_for_empty_title(FluentValidation.Results.ValidationResult result)
    {
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Title is required");
    }

    void Then_validation_should_fail_for_title_length(FluentValidation.Results.ValidationResult result)
    {
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Title cannot exceed 200 characters");
    }

    void Then_validation_should_fail_for_year_too_early(FluentValidation.Results.ValidationResult result)
    {
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Year of release must be after 1888");
    }

    void Then_validation_should_fail_for_year_too_far_in_future(FluentValidation.Results.ValidationResult result)
    {
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Year of release cannot be more than 5 years in the future");
    }

    void Then_validation_should_fail_for_empty_movie_id(FluentValidation.Results.ValidationResult result)
    {
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "MovieId is required for updates");
    }

    // Helper method to reduce duplication
    private MoviesDbContext CreateTestContext()
    {
        var options = new DbContextOptionsBuilder<MoviesDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new MoviesDbContext(options);
    }
}
