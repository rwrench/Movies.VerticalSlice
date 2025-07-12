using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Movies.VerticalSlice.Api.Data.Database;

namespace Movies.VerticalSlice.Api.Features.Ratings.Update;

public class RatingsUpdateValidator : AbstractValidator<RatingsUpdateCommand>
{
    private readonly MoviesDbContext _context;

    public RatingsUpdateValidator(MoviesDbContext context)
    {
        _context = context;

        RuleFor(x => x.RatingsId)
            .MustAsync(RatingsExists)
            .WithMessage("Ratings must exist");

        RuleFor(x => x.MovieId)
            .MustAsync(MovieExists)
            .WithMessage("Movie must exist");

        RuleFor(x => x.Rating)
            .InclusiveBetween(0.5f, 5.0f);

       
    }

    private async Task<bool> MovieExists(Guid id, CancellationToken token)
    {
        var exists = await _context.Movies
          .AnyAsync(r => r.MovieId == id, token);
        return exists;
    }

    async Task<bool> RatingsExists(Guid id, CancellationToken token)
    {
        var exists = await _context.Ratings
           .AnyAsync(r => r.Id == id, token);
        return exists;
    }
}