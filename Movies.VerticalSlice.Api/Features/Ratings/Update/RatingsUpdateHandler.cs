using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Movies.VerticalSlice.Api.Data.Database;

namespace Movies.VerticalSlice.Api.Features.Ratings.Update;

public class RatingsUpdateHandler : IRequestHandler<RatingsUpdateCommand, bool>
{
    private readonly MoviesDbContext _context;
    private readonly IValidator<RatingsUpdateCommand> _validator;
    private readonly ILogger<RatingsUpdateHandler> _logger;

    public RatingsUpdateHandler(
        MoviesDbContext context,
        IValidator<RatingsUpdateCommand> validator,
        ILogger<RatingsUpdateHandler> logger)
    {
        _context = context;
        _validator = validator;
        _logger = logger;
    }

    public async Task<bool> Handle(
        RatingsUpdateCommand command,
        CancellationToken cancellationToken)
    {

        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var existingRating = await _context.Ratings
            .FirstAsync(r => r.Id == command.RatingsId, cancellationToken);

        if (existingRating != null)
        {
            existingRating.MovieId = command.MovieId;
            existingRating.Rating = command.Rating;
            existingRating.DateUpdated = DateTime.Now;
           
        }
        var affectedRows = await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Updated existing rating for {RatingsId}",
               command.RatingsId);
        return affectedRows > 0;
    }
}