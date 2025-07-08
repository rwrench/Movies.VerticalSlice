using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Movies.VerticalSlice.Api.Data.Database;
using Movies.VerticalSlice.Api.Data.Models;
using Movies.VerticalSlice.Api.Features.Movies.Update;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Movies.VerticalSlice.Api.Features.Ratings.Create;

public class RateMovieCommandHandler : IRequestHandler<RateMovieCommand, bool>
{
    private readonly MoviesDbContext _context;
    private readonly IValidator<RateMovieCommand> _validator;
    private readonly ILogger<UpdateMovieHandler> _logger;

    public RateMovieCommandHandler(
        MoviesDbContext context, 
        IValidator<RateMovieCommand> validator, 
        ILogger<UpdateMovieHandler> logger)
    {
        _context = context;
        _validator = validator;
        _logger = logger;
    }

    public async Task<bool> Handle(
        RateMovieCommand command, 
        CancellationToken cancellationToken)
    {
  
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        // Check if movie exists
        var movieExists = await _context.Movies
            .AnyAsync(m => m.MovieId == command.MovieId, cancellationToken);

        //if (!movieExists)
        //{
        //    _logger.LogWarning("Movie not found with ID: {MovieId}", command.MovieId);
        //    return false;
        //}
        // Check if user already rated this movie
        var existingRating = await _context.Ratings
            .FirstOrDefaultAsync(r => r.MovieId == command.MovieId 
                && r.UserId == command.UserId, cancellationToken);

        if (existingRating != null)
        {
            // Update existing rating
            existingRating.Rating = command.Rating;
            existingRating.DateUpdated = DateTime.UtcNow;
            _logger.LogInformation("Updated existing rating for user: {UserId}, movie: {MovieId}",
                command.UserId, command.MovieId);
        }
        else
        {
            // Create new rating
            var newRating = new MovieRating
            {
                Id = Guid.NewGuid(),
                MovieId = command.MovieId,
                Rating = command.Rating,
                UserId = command.UserId,
                DateUpdated = command.DateUpdated
            };

            _context.Ratings.Add(newRating);
            _logger.LogInformation("Created new rating for user: {UserId}, movie: {MovieId}",
                command.UserId, command.MovieId);
        }

        var affectedRows = await _context.SaveChangesAsync(cancellationToken);
        return affectedRows > 0;
    }
}
