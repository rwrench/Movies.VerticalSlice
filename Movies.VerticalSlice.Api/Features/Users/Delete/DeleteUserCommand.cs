using MediatR;

namespace Movies.VerticalSlice.Api.Features.Users.Delete;

public record DeleteUserCommand(Guid UserId) : IRequest<bool>;