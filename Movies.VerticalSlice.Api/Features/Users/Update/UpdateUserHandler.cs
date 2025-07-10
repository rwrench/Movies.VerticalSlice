using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Movies.VerticalSlice.Api.Data.Database;
using Movies.VerticalSlice.Api.Services;

namespace Movies.VerticalSlice.Api.Features.Users.Update;

public class UpdateUserHandler : IRequestHandler<UpdateUserCommand, bool>
{
    private readonly MoviesDbContext _context;
    private readonly IValidator<UpdateUserCommand> _validator;
    private readonly ILogger<UpdateUserHandler> _logger;
    private readonly IPasswordService _passwordService;

    public UpdateUserHandler(
        MoviesDbContext context,
        IValidator<UpdateUserCommand> validator,
        ILogger<UpdateUserHandler> logger,
        IPasswordService passwordService)
    {
        _context = context;
        _validator = validator;
        _logger = logger;
        _passwordService = passwordService;
    }

    public async Task<bool> Handle(UpdateUserCommand command, 
        CancellationToken cancellationToken)
    {
         await _validator.ValidateAndThrowAsync(command, cancellationToken);

        // Find the user by the current username
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.UserName == command.UserName, 
            cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("User not found with UserName: {UserName}", 
                command.UserName);
            return false;
        }

        // Check if email is being changed and if it already exists for a different user
        if (!string.IsNullOrEmpty(command.Email) && user.Email != command.Email)
        {
            var existingUserWithEmail = await _context.Users
                .AnyAsync(u => u.Email == command.Email && u.Id != user.Id, 
                    cancellationToken);

            if (existingUserWithEmail)
            {
                _logger.LogWarning("User update failed - email already exists: {Email}", 
                    command.Email);
                throw new InvalidOperationException("A user with this email already exists");
            }
            
            // Update email
            user.Email = command.Email;
        }

        // Update password if provided
        if (!string.IsNullOrEmpty(command.Password))
        {
            user.PasswordHash = _passwordService.HashPassword(command.Password);
        }

        var affectedRows = await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User updated successfully: {UserName}", command.UserName);
        return affectedRows > 0;
    }
}