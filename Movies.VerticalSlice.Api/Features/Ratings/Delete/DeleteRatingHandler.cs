using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Movies.VerticalSlice.Api.Data.Database;



namespace Movies.VerticalSlice.Api.Features.Ratings.Delete;

public class DeleteRatingHandler : IRequestHandler<DeleteRatingCommand, bool>
{
    private readonly MoviesDbContext _context;
    private readonly ILogger<DeleteRatingHandler> _logger;

    public DeleteRatingHandler(MoviesDbContext context,ILogger<DeleteRatingHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> Handle(
        DeleteRatingCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Attempting to delete rating with ID: {RatingId}",
            command.RatingId);

        var rating = await _context.Ratings
            .Where(m => m.Id == command.RatingId)
            .FirstOrDefaultAsync(cancellationToken);

        if (rating == null)
        {
            _logger.LogWarning("Rating not found for deletion with ID: {MovieId}", command.RatingId);
            return false;
        }

        // Authorization check: Ensure the user owns this rating
        if (rating.UserId != command.UserId)
        {
            throw new UnauthorizedAccessException("You are not authorized to delete this rating.");
        }

        _context.Ratings.Remove(rating);
        var affectedRows = await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Successfully deleted movie with ID: {RatingId}", 
            command.RatingId);
        return affectedRows > 0;
    }
}
