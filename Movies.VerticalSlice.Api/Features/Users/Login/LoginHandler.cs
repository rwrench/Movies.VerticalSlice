using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Movies.VerticalSlice.Api.Data.Database;
using Movies.VerticalSlice.Api.Services;

namespace Movies.VerticalSlice.Api.Features.Users.Login;

public class LoginHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly MoviesDbContext _context;
    private readonly IValidator<LoginCommand> _validator;
    private readonly IPasswordService _passwordService;
    private readonly IJwtService _jwtService;
    private readonly ILogger<LoginHandler> _logger;

    public LoginHandler(
        MoviesDbContext context,
        IValidator<LoginCommand> validator,
        IPasswordService passwordService,
        IJwtService jwtService,
        ILogger<LoginHandler> logger)
    {
        _context = context;
        _validator = validator;
        _passwordService = passwordService;
        _jwtService = jwtService;
        _logger = logger;
    }

    public async Task<LoginResponse> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == command.Email, cancellationToken);

        if (user == null || !_passwordService.VerifyPassword(command.Password, user.PasswordHash!))
        {
            _logger.LogWarning("Login failed for email: {Email}", command.Email);
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        var token = _jwtService.GenerateToken(user);

        _logger.LogInformation("User logged in successfully: {Email}", command.Email);

        return new LoginResponse(token, user.Id, user.UserName, user.Email);
    }
}