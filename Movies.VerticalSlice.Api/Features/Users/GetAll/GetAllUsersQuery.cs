using MediatR;
using Movies.VerticalSlice.Api.Shared.Dtos;


namespace Movies.VerticalSlice.Api.Features.Users.GetAll;

public record GetAllUsersQuery(
) : IRequest<IEnumerable<UsersDto>>;
