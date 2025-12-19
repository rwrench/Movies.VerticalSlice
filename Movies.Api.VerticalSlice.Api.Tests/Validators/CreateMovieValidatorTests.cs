using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using Movies.VerticalSlice.Api.Data.Database;
using Movies.VerticalSlice.Api.Data.Models;
using Movies.VerticalSlice.Api.Features.Movies.Create;
using Xunit;

namespace Movies.Api.VerticalSlice.Api.Tests.Validators;

public class CreateMovieValidatorTests
{
    [Fact]
    public async Task Validate_ValidCommand_PassesValidation()
    {
        var command = Given_we_have_a_valid_command();
        var validator = And_we_have_a_validator_without_existing_movie();
        var result = await When_we_validate_the_command(validator, command);
        Then_validation_should_pass(result);
    }

    [Fact]
    public async Task Validate_EmptyTitle_FailsValidation()
    {
        var command = Given_we_have_a_command_with_empty_title();
        var validator = And_we_have_a_validator_without_existing_movie();
        var result = await When_we_validate_the_command(validator, command);
        Then_validation_should_fail_for_empty_title(result);
    }

    [Fact]
    public async Task Validate_TitleTooLong_FailsValidation()
    {
        var command = Given_we_have_a_command_with_title_too_long();
        var validator = And_we_have_a_validator_without_existing_movie();
        var result = await When_we_validate_the_command(validator, command);
        Then_validation_should_fail_for_title_length(result);
    }

    [Fact]
    public async Task Validate_DuplicateTitleAndYear_FailsValidation()
    {
        var command = Given_we_have_a_valid_command();
        var validator = And_we_have_a_validator_with_existing_movie();
        var result = await When_we_validate_the_command(validator, command);
        Then_validation_should_fail_for_duplicate(result);
    }

    [Fact]
    public async Task Validate_YearTooEarly_FailsValidation()
    {
        var command = Given_we_have_a_command_with_year_too_early();
        var validator = And_we_have_a_validator_without_existing_movie();
        var result = await When_we_validate_the_command(validator, command);
        Then_validation_should_fail_for_year_too_early(result);
    }

    [Fact]
    public async Task Validate_YearTooFarInFuture_FailsValidation()
    {
        var command = Given_we_have_a_command_with_year_too_far_in_future();
        var validator = And_we_have_a_validator_without_existing_movie();
        var result = await When_we_validate_the_command(validator, command);
        Then_validation_should_fail_for_year_too_far_in_future(result);
    }

    [Fact]
    public async Task Validate_YearAtMinimum_PassesValidation()
    {
        var command = Given_we_have_a_command_with_minimum_year();
        var validator = And_we_have_a_validator_without_existing_movie();
        var result = await When_we_validate_the_command(validator, command);
        Then_validation_should_pass(result);
    }

    [Fact]
    public async Task Validate_YearAtMaximum_PassesValidation()
    {
        var command = Given_we_have_a_command_with_maximum_year();
        var validator = And_we_have_a_validator_without_existing_movie();
        var result = await When_we_validate_the_command(validator, command);
        Then_validation_should_pass(result);
    }

    [Fact]
    public async Task Validate_SameTitleDifferentYear_PassesValidation()
    {
        var command = Given_we_have_a_command_with_different_year();
        var validator = And_we_have_a_validator_with_existing_movie();
        var result = await When_we_validate_the_command(validator, command);
        Then_validation_should_pass(result);
    }

    CreateMovieCommand Given_we_have_a_valid_command()
    {
        return new CreateMovieCommand(
            Title: "Test Movie",
            YearOfRelease: 2024,
            Genres: "Action,Drama",
            UserId: "user-123");
    }

    CreateMovieCommand Given_we_have_a_command_with_empty_title()
    {
        return new CreateMovieCommand(
            Title: "",
            YearOfRelease: 2024,
            Genres: "Action",
            UserId: "user-123");
    }

    CreateMovieCommand Given_we_have_a_command_with_title_too_long()
    {
        return new CreateMovieCommand(
            Title: new string('A', 201),
            YearOfRelease: 2024,
            Genres: "Action",
            UserId: "user-123");
    }

    CreateMovieCommand Given_we_have_a_command_with_year_too_early()
    {
        return new CreateMovieCommand(
            Title: "Old Movie",
            YearOfRelease: 1888,
            Genres: "Drama",
            UserId: "user-123");
    }

    CreateMovieCommand Given_we_have_a_command_with_year_too_far_in_future()
    {
        return new CreateMovieCommand(
            Title: "Future Movie",
            YearOfRelease: DateTime.UtcNow.Year + 6,
            Genres: "Sci-Fi",
            UserId: "user-123");
    }

    CreateMovieCommand Given_we_have_a_command_with_minimum_year()
    {
        return new CreateMovieCommand(
            Title: "Early Movie",
            YearOfRelease: 1889,
            Genres: "Drama",
            UserId: "user-123");
    }

    CreateMovieCommand Given_we_have_a_command_with_maximum_year()
    {
        return new CreateMovieCommand(
            Title: "Future Movie",
            YearOfRelease: DateTime.UtcNow.Year + 5,
            Genres: "Sci-Fi",
            UserId: "user-123");
    }

    CreateMovieCommand Given_we_have_a_command_with_different_year()
    {
        return new CreateMovieCommand(
            Title: "Test Movie",
            YearOfRelease: 2025,
            Genres: "Action,Drama",
            UserId: "user-123");
    }

    CreateMovieValidator And_we_have_a_validator_without_existing_movie()
    {
        var context = CreateTestContext();
        return new CreateMovieValidator(context);
    }

    CreateMovieValidator And_we_have_a_validator_with_existing_movie()
    {
        var context = CreateTestContext();

        var existingMovie = new Movie
        {
            MovieId = Guid.NewGuid(),
            Title = "Test Movie",
            YearOfRelease = 2024,
            Genres = "Action",
            UserId = "user-456",
            DateUpdated = DateTime.UtcNow
        };

        context.Movies.Add(existingMovie);
        context.SaveChanges();

        return new CreateMovieValidator(context);
    }

    async Task<FluentValidation.Results.ValidationResult> When_we_validate_the_command(
        CreateMovieValidator validator,
        CreateMovieCommand command)
    {
        return await validator.ValidateAsync(command);
    }

    void Then_validation_should_pass(FluentValidation.Results.ValidationResult result)
    {
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
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

    void Then_validation_should_fail_for_duplicate(FluentValidation.Results.ValidationResult result)
    {
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "A movie with this title and year already exists");
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

    // Helper method to reduce duplication
    private MoviesDbContext CreateTestContext()
    {
        var options = new DbContextOptionsBuilder<MoviesDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new MoviesDbContext(options);
    }
}
