using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Movies.VerticalSlice.Api.Data.Database;
using Movies.VerticalSlice.Api.Data.Models;
using Movies.VerticalSlice.Api.Features.Movies.Update;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Movies.VerticalSlice.Api.Features.Ratings.Create;

public class RatingsCreateHandler : IRequestHandler<RatingsCreateCommand, bool>
{
    private readonly MoviesDbContext _context;
    private readonly IValidator<RatingsCreateCommand> _validator;
    private readonly ILogger<UpdateMovieHandler> _logger;

    public RatingsCreateHandler(
        MoviesDbContext context, 
        IValidator<RatingsCreateCommand> validator, 
        ILogger<UpdateMovieHandler> logger)
    {
        _context = context;
        _validator = validator;
        _logger = logger;
    }

    public async Task<bool> Handle(
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
        
        var affectedRows = await _context.SaveChangesAsync(cancellationToken);
        return affectedRows > 0;
    }
}
