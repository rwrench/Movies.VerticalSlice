using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Movies.VerticalSlice.Api.Data.Database;

namespace Movies.VerticalSlice.Api.Features.Ratings.Create
{
    public class RatingsCreateValidator : AbstractValidator<RatingsCreateCommand>
    {
        private readonly MoviesDbContext _context;

        public RatingsCreateValidator(MoviesDbContext context)
        {
            _context = context;

            RuleFor(x => x.MovieId)
                .MustAsync(MovieExists)
                .WithMessage("Movie does not exist");

            RuleFor(x => x.Rating)
                .InclusiveBetween(0.5f, 5.0f);

            RuleFor(x => x.DateUpdated)
                .LessThanOrEqualTo(DateTime.Now)
                .WithMessage("DateUpdated cannot be in the future");


            RuleFor(x => x)
                .MustAsync(RatingDoesNotExistForUserAndMovie)
                .WithMessage("User has already rated this movie.");
        }

        private async Task<bool> RatingDoesNotExistForUserAndMovie(
            RatingsCreateCommand command, CancellationToken token)
        {
            var exists = await _context.Ratings
                .AnyAsync(r => r.UserId == command.UserId && r.MovieId == command.MovieId, token);
            return !exists;
        }

        private async Task<bool> MovieExists(Guid movieId, CancellationToken token)
        {
            var movie = await _context.Movies
                .FirstOrDefaultAsync(m => m.MovieId == movieId, token);
            return movie != null;
        }
    }
}
