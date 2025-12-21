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
    private readonly ILoginAuditService _loginAuditService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<LoginHandler> _logger;

    public LoginHandler(
        MoviesDbContext context,
        IValidator<LoginCommand> validator,
        IPasswordService passwordService,
        IJwtService jwtService,
        ILoginAuditService loginAuditService,
        IHttpContextAccessor httpContextAccessor,
        ILogger<LoginHandler> logger)
    {
        _context = context;
        _validator = validator;
        _passwordService = passwordService;
        _jwtService = jwtService;
        _loginAuditService = loginAuditService;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<LoginResponse> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == command.Email, cancellationToken);

        // Capture request information for audit logging
        var httpContext = _httpContextAccessor.HttpContext;
        var ipAddress = httpContext?.Connection?.RemoteIpAddress?.ToString();
        var userAgent = httpContext?.Request?.Headers["User-Agent"].ToString();

        if (user == null || !_passwordService.VerifyPassword(command.Password, user.PasswordHash!))
        {
            _logger.LogWarning("Login failed for email: {Email}", command.Email);
            
            // Log failed login attempt
            var failureReason = user == null ? "User not found" : "Invalid password";
            await _loginAuditService.LogFailedLoginAsync(
                command.Email,
                failureReason,
                ipAddress,
                userAgent,
                cancellationToken);
            
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        var token = _jwtService.GenerateToken(user);

        // Log successful login attempt
        await _loginAuditService.LogSuccessfulLoginAsync(
            command.Email,
            user.Id,
            user.UserName,
            ipAddress,
            userAgent,
            cancellationToken);

        _logger.LogInformation("User logged in successfully: {Email}, UserId: {UserId}", command.Email, user.Id);

        return new LoginResponse(token, user.Id, user.UserName, user.Email);
    }
}