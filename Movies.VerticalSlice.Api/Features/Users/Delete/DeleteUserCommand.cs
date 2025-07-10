using MediatR;

namespace Movies.VerticalSlice.Api.Features.Users.Delete;

public record DeleteUserCommand(string UserId) : IRequest<bool>;