using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Movies.VerticalSlice.Api.Data.Database;

namespace Movies.VerticalSlice.Api.Features.Movies.Create;

public class CreateMovieValidator : AbstractValidator<CreateMovieCommand>
{
    private readonly MoviesDbContext _context;

    public CreateMovieValidator(MoviesDbContext context)
    {
        _context = context;

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Title is required")
            .MaximumLength(200)
            .WithMessage("Title cannot exceed 200 characters")
            .MustAsync(BeUniqueTitle)
            .WithMessage("A movie with this title and year already exists");

        RuleFor(x => x.YearOfRelease)
            .GreaterThan(1888)
            .WithMessage("Year of release must be after 1888")
            .LessThanOrEqualTo(DateTime.UtcNow.Year + 5)
            .WithMessage("Year of release cannot be more than 5 years in the future");

        RuleFor(x => x.Genres)
            .NotNull()
            .WithMessage("Genres list cannot be null")
            .Must(genres => genres.Count > 0)
            .WithMessage("At least one genre is required")
            .Must(genres => genres.Count <= 10)
            .WithMessage("Cannot have more than 10 genres");

        RuleForEach(x => x.Genres)
            .NotEmpty()
            .WithMessage("Genre names cannot be empty")
            .MaximumLength(50)
            .WithMessage("Genre names cannot exceed 50 characters");
    }

    private async Task<bool> BeUniqueTitle(CreateMovieCommand command, string title, CancellationToken token)
    {
        return !await _context.Movies
            .AnyAsync(m => m.Title == title && m.YearOfRelease == command.YearOfRelease, token);
    }
}
