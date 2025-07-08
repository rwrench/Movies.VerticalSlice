using MediatR;
using Microsoft.EntityFrameworkCore;
using Movies.VerticalSlice.Api.Data.Database;

namespace Movies.VerticalSlice.Api.Features.Users.Delete;

public class DeleteUserHandler : IRequestHandler<DeleteUserCommand, bool>
{
    private readonly MoviesDbContext _context;
    private readonly ILogger<DeleteUserHandler> _logger;

    public DeleteUserHandler(
        MoviesDbContext context,
        ILogger<DeleteUserHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteUserCommand command, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.UserId == command.UserId, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("User not found with ID: {UserId}", command.UserId);
            return false;
        }

        // Optional: Check for related data and handle accordingly
        var hasRatings = await _context.Ratings
            .AnyAsync(r => r.UserId == command.UserId, cancellationToken);

        var hasMovies = await _context.Movies
            .AnyAsync(m => m.UserId == command.UserId, cancellationToken);

        if (hasRatings || hasMovies)
        {
            _logger.LogWarning("Cannot delete user with existing ratings or movies: {UserId}", command.UserId);
            throw new InvalidOperationException("Cannot delete user with existing ratings or movies");
        }

        _context.Users.Remove(user);
        var affectedRows = await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User deleted successfully: {UserId}", command.UserId);

        return affectedRows > 0;
    }
}