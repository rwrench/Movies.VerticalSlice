using MediatR;
using Movies.VerticalSlice.Api.Shared.Dtos;

namespace Movies.VerticalSlice.Api.Features.Movies.Names
{
 public record CreateNamesCommand(
 string Title,
 string? UserId = null
 ) : IRequest<IEnumerable<MovieNameDto>>;
}
