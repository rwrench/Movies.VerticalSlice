using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Movies.VerticalSlice.Api.Data.Database;
using Movies.VerticalSlice.Api.Data.Models;
using Movies.VerticalSlice.Api.Features.Movies.Update;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Movies.VerticalSlice.Api.Features.Ratings.Create;

public class RatingsCreateHandler : IRequestHandler<RatingsCreateCommand, Guid>
{
    private readonly MoviesDbContext _context;
    private readonly IValidator<RatingsCreateCommand> _validator;
    private readonly ILogger<RatingsCreateHandler> _logger;

    public RatingsCreateHandler(
        MoviesDbContext context, 
        IValidator<RatingsCreateCommand> validator, 
        ILogger<RatingsCreateHandler> logger)
    {
        _context = context;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Guid> Handle(
        RatingsCreateCommand command, 
        CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);
      
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
        
        await _context.SaveChangesAsync(cancellationToken);
        return newRating.Id;
    }
}
