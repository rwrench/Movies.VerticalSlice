using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Movies.VerticalSlice.Api.Data.Database;
using Movies.VerticalSlice.Api.Data.Models;

namespace Movies.VerticalSlice.Api.Features.Users.Register;

public class RegisterUserHandler : IRequestHandler<RegisterUserCommand, Guid>
{
    private readonly MoviesDbContext _context;
    private readonly IValidator<RegisterUserCommand> _validator;
    private readonly ILogger<RegisterUserHandler> _logger;

    public RegisterUserHandler(
        MoviesDbContext context,
        IValidator<RegisterUserCommand> validator,
        ILogger<RegisterUserHandler> logger)
    {
        _context = context;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Guid> Handle(RegisterUserCommand command, 
        CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        // Check if user with email already exists
        var existingUser = await _context.Users
            .AnyAsync(u => u.Email == command.Email, cancellationToken);

        if (existingUser)
        {
            _logger.LogWarning("User registration failed - email already exists: {Email}", command.Email);
            throw new InvalidOperationException("A user with this email already exists");
        }

        // Check if username already exists
        var existingUsername = await _context.Users
            .AnyAsync(u => u.UserName == command.UserName, cancellationToken);

        if (existingUsername)
        {
            _logger.LogWarning("User registration failed - username already exists: {UserName}", command.UserName);
            throw new InvalidOperationException("A user with this username already exists");
        }

        var user = new User
        {
            UserId = Guid.NewGuid(),
            UserName = command.UserName,
            Email = command.Email,
            Password = command.Password // Note: In production, hash the password before storing
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User registered successfully: {UserId}, {UserName}", user.UserId, user.UserName);

        return user.UserId;
    }
}