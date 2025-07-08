using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Movies.VerticalSlice.Api.Data.Database;

namespace Movies.VerticalSlice.Api.Features.Users.Update;

public class UpdateUserHandler : IRequestHandler<UpdateUserCommand, bool>
{
    private readonly MoviesDbContext _context;
    private readonly IValidator<UpdateUserCommand> _validator;
    private readonly ILogger<UpdateUserHandler> _logger;

    public UpdateUserHandler(
        MoviesDbContext context,
        IValidator<UpdateUserCommand> validator,
        ILogger<UpdateUserHandler> logger)
    {
        _context = context;
        _validator = validator;
        _logger = logger;
    }

    public async Task<bool> Handle(UpdateUserCommand command, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.UserId == command.UserId, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("User not found with ID: {UserId}", command.UserId);
            return false;
        }

        // Check if email is being changed and if it already exists
        if (user.Email != command.Email)
        {
            var existingUser = await _context.Users
                .AnyAsync(u => u.Email == command.Email && u.UserId != command.UserId, cancellationToken);

            if (existingUser)
            {
                _logger.LogWarning("User update failed - email already exists: {Email}", command.Email);
                throw new InvalidOperationException("A user with this email already exists");
            }
        }

        // Check if username is being changed and if it already exists
        if (user.UserName != command.UserName)
        {
            var existingUsername = await _context.Users
                .AnyAsync(u => u.UserName == command.UserName && u.UserId != command.UserId, cancellationToken);

            if (existingUsername)
            {
                _logger.LogWarning("User update failed - username already exists: {UserName}", command.UserName);
                throw new InvalidOperationException("A user with this username already exists");
            }
        }

        user.UserName = command.UserName;
        user.Email = command.Email;

        if (!string.IsNullOrEmpty(command.Password))
        {
            user.Password = command.Password; // Note: In production, hash the password before storing
        }

        var affectedRows = await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User updated successfully: {UserId}", command.UserId);

        return affectedRows > 0;
    }
}