using MediatR;

namespace Movies.VerticalSlice.Api.Features.Users.Register;

public record RegisterUserCommand(
    string UserName,
    string Email,
    string Password
) : IRequest<string>;