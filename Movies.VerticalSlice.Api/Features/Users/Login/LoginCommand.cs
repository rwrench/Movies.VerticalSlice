using MediatR;

namespace Movies.VerticalSlice.Api.Features.Users.Login;

public record LoginCommand(
    string Email,
    string Password
) : IRequest<LoginResponse>;

public record LoginResponse(
    string Token,
    string UserId,
    string? UserName,
    string? Email
);