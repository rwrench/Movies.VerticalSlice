using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Movies.VerticalSlice.Api.Data.Database;


namespace Movies.VerticalSlice.Api.Features.Movies.Delete
{
    public class DeleteMovieHandler : IRequestHandler<DeleteMovieCommand, bool>
    {
        private readonly MoviesDbContext _context;
        private readonly ILogger<DeleteMovieHandler> _logger;

        public DeleteMovieHandler(MoviesDbContext context,ILogger<DeleteMovieHandler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> Handle(
            DeleteMovieCommand command,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Attempting to delete movie with ID: {MovieId}",
                command.MovieId);

            var movie = await _context.Movies
                .Where(m => m.MovieId == command.MovieId)
                .FirstOrDefaultAsync(cancellationToken);

            if (movie == null)
            {
                _logger.LogWarning("Movie not found for deletion with ID: {MovieId}", command.MovieId);
                return false;
            }

            _context.Movies.Remove(movie);
            var affectedRows = await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Successfully deleted movie with ID: {MovieId}", 
                command.MovieId);
            return affectedRows > 0;
        }
    }
}
