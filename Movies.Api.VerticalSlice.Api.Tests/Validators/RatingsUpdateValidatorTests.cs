using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using Movies.VerticalSlice.Api.Data.Database;
using Movies.VerticalSlice.Api.Data.Models;
using Movies.VerticalSlice.Api.Features.Ratings.Update;
using Xunit;

namespace Movies.Api.VerticalSlice.Api.Tests.Validators;

public class RatingsUpdateValidatorTests
{
    [Fact]
    public async Task Validate_ValidCommand_PassesValidation()
    {
        var command = Given_we_have_a_valid_command();
        var validator = And_we_have_a_validator_with_valid_rating_and_movie();
        var result = await When_we_validate_the_command(validator, command);
        Then_validation_should_pass(result);
    }

    [Fact]
    public async Task Validate_NonExistentRating_FailsValidation()
    {
        var command = Given_we_have_a_valid_command();
        var validator = And_we_have_a_validator_without_rating();
        var result = await When_we_validate_the_command(validator, command);
        Then_validation_should_fail_for_rating(result);
    }

    [Fact]
    public async Task Validate_NonExistentMovie_FailsValidation()
    {
        var command = Given_we_have_a_valid_command();
        var validator = And_we_have_a_validator_with_rating_but_no_movie();
        var result = await When_we_validate_the_command(validator, command);
        Then_validation_should_fail_for_movie(result);
    }

    [Fact]
    public async Task Validate_RatingTooLow_FailsValidation()
    {
        var command = Given_we_have_a_command_with_rating_too_low();
        var validator = And_we_have_a_validator_with_valid_rating_and_movie();
        var result = await When_we_validate_the_command(validator, command);
        Then_validation_should_fail_for_rating_range(result);
    }

    [Fact]
    public async Task Validate_RatingTooHigh_FailsValidation()
    {
        var command = Given_we_have_a_command_with_rating_too_high();
        var validator = And_we_have_a_validator_with_valid_rating_and_movie();
        var result = await When_we_validate_the_command(validator, command);
        Then_validation_should_fail_for_rating_range(result);
    }

    [Fact]
    public async Task Validate_RatingAtMinimum_PassesValidation()
    {
        var command = Given_we_have_a_command_with_minimum_rating();
        var validator = And_we_have_a_validator_with_valid_rating_and_movie();
        var result = await When_we_validate_the_command(validator, command);
        Then_validation_should_pass(result);
    }

    [Fact]
    public async Task Validate_RatingAtMaximum_PassesValidation()
    {
        var command = Given_we_have_a_command_with_maximum_rating();
        var validator = And_we_have_a_validator_with_valid_rating_and_movie();
        var result = await When_we_validate_the_command(validator, command);
        Then_validation_should_pass(result);
    }

    [Fact]
    public async Task Validate_ChangingMovie_PassesValidation()
    {
        var command = Given_we_have_a_command_changing_movie();
        var validator = And_we_have_a_validator_with_rating_and_multiple_movies();
        var result = await When_we_validate_the_command(validator, command);
        Then_validation_should_pass(result);
    }

    RatingsUpdateCommand Given_we_have_a_valid_command()
    {
        return new RatingsUpdateCommand(
            RatingsId: Guid.Parse("22222222-2222-2222-2222-222222222222"),
            MovieId: Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Rating: 4.5f,
            DateUpdated: DateTime.UtcNow,
            UserId: "user-123");
    }

    RatingsUpdateCommand Given_we_have_a_command_with_rating_too_low()
    {
        return new RatingsUpdateCommand(
            RatingsId: Guid.Parse("22222222-2222-2222-2222-222222222222"),
            MovieId: Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Rating: 0.0f,
            DateUpdated: DateTime.UtcNow,
            UserId: "user-123");
    }

    RatingsUpdateCommand Given_we_have_a_command_with_rating_too_high()
    {
        return new RatingsUpdateCommand(
            RatingsId: Guid.Parse("22222222-2222-2222-2222-222222222222"),
            MovieId: Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Rating: 5.5f,
            DateUpdated: DateTime.UtcNow,
            UserId: "user-123");
    }

    RatingsUpdateCommand Given_we_have_a_command_with_minimum_rating()
    {
        return new RatingsUpdateCommand(
            RatingsId: Guid.Parse("22222222-2222-2222-2222-222222222222"),
            MovieId: Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Rating: 0.5f,
            DateUpdated: DateTime.UtcNow,
            UserId: "user-123");
    }

    RatingsUpdateCommand Given_we_have_a_command_with_maximum_rating()
    {
        return new RatingsUpdateCommand(
            RatingsId: Guid.Parse("22222222-2222-2222-2222-222222222222"),
            MovieId: Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Rating: 5.0f,
            DateUpdated: DateTime.UtcNow,
            UserId: "user-123");
    }

    RatingsUpdateCommand Given_we_have_a_command_changing_movie()
    {
        return new RatingsUpdateCommand(
            RatingsId: Guid.Parse("22222222-2222-2222-2222-222222222222"),
            MovieId: Guid.Parse("33333333-3333-3333-3333-333333333333"),
            Rating: 4.5f,
            DateUpdated: DateTime.UtcNow,
            UserId: "user-123");
    }

    RatingsUpdateValidator And_we_have_a_validator_with_valid_rating_and_movie()
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
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            MovieId = movie.MovieId,
            Rating = 3.0f,
            UserId = "user-123",
            DateUpdated = DateTime.UtcNow
        };

        context.Movies.Add(movie);
        context.Ratings.Add(rating);
        context.SaveChanges();

        return new RatingsUpdateValidator(context);
    }

    RatingsUpdateValidator And_we_have_a_validator_without_rating()
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

        context.Movies.Add(movie);
        context.SaveChanges();

        return new RatingsUpdateValidator(context);
    }

    RatingsUpdateValidator And_we_have_a_validator_with_rating_but_no_movie()
    {
        var context = CreateTestContext();

        var rating = new MovieRating
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            MovieId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Rating = 3.0f,
            UserId = "user-123",
            DateUpdated = DateTime.UtcNow
        };

        context.Ratings.Add(rating);
        context.SaveChanges();

        return new RatingsUpdateValidator(context);
    }

    RatingsUpdateValidator And_we_have_a_validator_with_rating_and_multiple_movies()
    {
        var context = CreateTestContext();

        var movie1 = new Movie
        {
            MovieId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Title = "Original Movie",
            YearOfRelease = 2023,
            Genres = "Action",
            UserId = "user-123"
        };

        var movie2 = new Movie
        {
            MovieId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            Title = "New Movie",
            YearOfRelease = 2024,
            Genres = "Drama",
            UserId = "user-123"
        };

        var rating = new MovieRating
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            MovieId = movie1.MovieId,
            Rating = 3.0f,
            UserId = "user-123",
            DateUpdated = DateTime.UtcNow
        };

        context.Movies.AddRange(movie1, movie2);
        context.Ratings.Add(rating);
        context.SaveChanges();

        return new RatingsUpdateValidator(context);
    }

    async Task<FluentValidation.Results.ValidationResult> When_we_validate_the_command(
        RatingsUpdateValidator validator,
        RatingsUpdateCommand command)
    {
        return await validator.ValidateAsync(command);
    }

    void Then_validation_should_pass(FluentValidation.Results.ValidationResult result)
    {
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    void Then_validation_should_fail_for_rating(FluentValidation.Results.ValidationResult result)
    {
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Ratings must exist");
    }

    void Then_validation_should_fail_for_movie(FluentValidation.Results.ValidationResult result)
    {
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Movie must exist");
    }

    void Then_validation_should_fail_for_rating_range(FluentValidation.Results.ValidationResult result)
    {
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Rating");
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
