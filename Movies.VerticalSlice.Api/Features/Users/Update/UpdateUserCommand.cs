using MediatR;

namespace Movies.VerticalSlice.Api.Features.Users.Update;

public record UpdateUserCommand(
    Guid UserId,
    string UserName,
    string Email,
    string? Password = null
) : IRequest<bool>;