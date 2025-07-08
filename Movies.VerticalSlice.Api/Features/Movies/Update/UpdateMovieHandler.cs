using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Movies.VerticalSlice.Api.Data.Database;

namespace Movies.VerticalSlice.Api.Features.Movies.Update;

public class UpdateMovieHandler : IRequestHandler<UpdateMovieCommand, bool>
{
    private readonly MoviesDbContext _context;
    private readonly IValidator<UpdateMovieCommand> _validator;
    private readonly ILogger<UpdateMovieHandler> _logger;

    public UpdateMovieHandler(MoviesDbContext context, IValidator<UpdateMovieCommand> validator, ILogger<UpdateMovieHandler> logger)
    {
        _context = context;
        _validator = validator;
        _logger = logger;
    }

    public async Task<bool> Handle(UpdateMovieCommand command, CancellationToken token)
    {
        await _validator.ValidateAndThrowAsync(command, token);

        var movie = await _context.Movies
            .FirstOrDefaultAsync(m => m.MovieId == command.MovieId, token);

        if (movie == null)
        {
            _logger.LogWarning("Movie not found with ID: {MovieId}", command.MovieId);
            return false;
        }

        movie.Title = command.Title;
        movie.YearOfRelease = command.YearOfRelease;
        movie.Genres = command.Genres;
        movie.UserId = command.UserId;

        var affectedRows = await _context.SaveChangesAsync(token);

        _logger.LogInformation("Updated movie {Title} with ID {MovieId}", command.Title, command.MovieId);
        return affectedRows > 0;
    }
}

