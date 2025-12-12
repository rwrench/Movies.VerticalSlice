using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using Movies.VerticalSlice.Api.Data.Database;
using Movies.VerticalSlice.Api.Data.Models;
using Movies.VerticalSlice.Api.Features.Ratings.Create;
using Xunit;

namespace Movies.Api.VerticalSlice.Api.Tests.Validators;

public class RatingsCreateValidatorTests
{
    [Fact]
    public async Task Validate_ValidCommand_PassesValidation()
    {
        var command = Given_we_have_a_valid_command();
        var validator = And_we_have_a_validator_with_valid_movie();
        var result = await When_we_validate_the_command(validator, command);
        Then_validation_should_pass(result);
    }

    [Fact]
    public async Task Validate_NonExistentMovie_FailsValidation()
    {
        var command = Given_we_have_a_valid_command();
        var validator = And_we_have_a_validator_without_movie();
        var result = await When_we_validate_the_command(validator, command);
        Then_validation_should_fail_for_movie(result);
    }

    [Fact]
    public async Task Validate_DuplicateRating_FailsValidation()
    {
        var command = Given_we_have_a_valid_command();
        var validator = And_we_have_a_validator_with_existing_rating();
        var result = await When_we_validate_the_command(validator, command);
        Then_validation_should_fail_for_duplicate(result);
    }

    [Fact]
    public async Task Validate_RatingTooLow_FailsValidation()
    {
        var command = Given_we_have_a_command_with_rating_too_low();
        var validator = And_we_have_a_validator_with_valid_movie();
        var result = await When_we_validate_the_command(validator, command);
        Then_validation_should_fail_for_rating_range(result);
    }

    [Fact]
    public async Task Validate_RatingTooHigh_FailsValidation()
    {
        var command = Given_we_have_a_command_with_rating_too_high();
        var validator = And_we_have_a_validator_with_valid_movie();
        var result = await When_we_validate_the_command(validator, command);
        Then_validation_should_fail_for_rating_range(result);
    }

    [Fact]
    public async Task Validate_RatingAtMinimum_PassesValidation()
    {
        var command = Given_we_have_a_command_with_minimum_rating();
        var validator = And_we_have_a_validator_with_valid_movie();
        var result = await When_we_validate_the_command(validator, command);
        Then_validation_should_pass(result);
    }

    [Fact]
    public async Task Validate_RatingAtMaximum_PassesValidation()
    {
        var command = Given_we_have_a_command_with_maximum_rating();
        var validator = And_we_have_a_validator_with_valid_movie();
        var result = await When_we_validate_the_command(validator, command);
        Then_validation_should_pass(result);
    }

    [Fact]
    public async Task Validate_DifferentUserSameMovie_PassesValidation()
    {
        var command = Given_we_have_a_command_for_different_user();
        var validator = And_we_have_a_validator_with_rating_for_different_user();
        var result = await When_we_validate_the_command(validator, command);
        Then_validation_should_pass(result);
    }

    RatingsCreateCommand Given_we_have_a_valid_command()
    {
        return new RatingsCreateCommand(
            MovieId: Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Rating: 4.5f,
            DateUpdated: DateTime.UtcNow,
            UserId: "user-123");
    }

    RatingsCreateCommand Given_we_have_a_command_with_rating_too_low()
    {
        return new RatingsCreateCommand(
            MovieId: Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Rating: 0.0f,
            DateUpdated: DateTime.UtcNow,
            UserId: "user-123");
    }

    RatingsCreateCommand Given_we_have_a_command_with_rating_too_high()
    {
        return new RatingsCreateCommand(
            MovieId: Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Rating: 6.0f,
            DateUpdated: DateTime.UtcNow,
            UserId: "user-123");
    }

    RatingsCreateCommand Given_we_have_a_command_with_minimum_rating()
    {
        return new RatingsCreateCommand(
            MovieId: Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Rating: 0.5f,
            DateUpdated: DateTime.UtcNow,
            UserId: "user-123");
    }

    RatingsCreateCommand Given_we_have_a_command_with_maximum_rating()
    {
        return new RatingsCreateCommand(
            MovieId: Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Rating: 5.0f,
            DateUpdated: DateTime.UtcNow,
            UserId: "user-123");
    }

    RatingsCreateCommand Given_we_have_a_command_for_different_user()
    {
        return new RatingsCreateCommand(
            MovieId: Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Rating: 4.5f,
            DateUpdated: DateTime.UtcNow,
            UserId: "user-456");
    }

    RatingsCreateValidator And_we_have_a_validator_with_valid_movie()
    {
        var options = new DbContextOptionsBuilder<MoviesDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new MoviesDbContext(options);

        var movie = new Movie
        {
            MovieId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Title = "Test Movie",
            YearOfRelease = 2023,
            Genres = "Action",
            UserId = "user-123"
        };

        context.Movies.Add(movie);
        context.SaveChanges();

        return new RatingsCreateValidator(context);
    }

    RatingsCreateValidator And_we_have_a_validator_without_movie()
    {
        var options = new DbContextOptionsBuilder<MoviesDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new MoviesDbContext(options);

        return new RatingsCreateValidator(context);
    }

    RatingsCreateValidator And_we_have_a_validator_with_existing_rating()
    {
        var options = new DbContextOptionsBuilder<MoviesDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new MoviesDbContext(options);

        var movie = new Movie
        {
            MovieId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Title = "Test Movie",
            YearOfRelease = 2023,
            Genres = "Action",
            UserId = "user-123"
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
        context.Ratings.Add(existingRating);
        context.SaveChanges();

        return new RatingsCreateValidator(context);
    }

    RatingsCreateValidator And_we_have_a_validator_with_rating_for_different_user()
    {
        var options = new DbContextOptionsBuilder<MoviesDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new MoviesDbContext(options);

        var movie = new Movie
        {
            MovieId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Title = "Test Movie",
            YearOfRelease = 2023,
            Genres = "Action",
            UserId = "user-123"
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
        context.Ratings.Add(existingRating);
        context.SaveChanges();

        return new RatingsCreateValidator(context);
    }

    async Task<FluentValidation.Results.ValidationResult> When_we_validate_the_command(
        RatingsCreateValidator validator,
        RatingsCreateCommand command)
    {
        return await validator.ValidateAsync(command);
    }

    void Then_validation_should_pass(FluentValidation.Results.ValidationResult result)
    {
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    void Then_validation_should_fail_for_movie(FluentValidation.Results.ValidationResult result)
    {
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Movie does not exist");
    }

    void Then_validation_should_fail_for_duplicate(FluentValidation.Results.ValidationResult result)
    {
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "User has already rated this movie.");
    }

    void Then_validation_should_fail_for_rating_range(FluentValidation.Results.ValidationResult result)
    {
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Rating");
    }
}
