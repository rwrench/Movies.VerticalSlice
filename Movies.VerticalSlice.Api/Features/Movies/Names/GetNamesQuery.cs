using MediatR;
using Movies.VerticalSlice.Api.Shared.Dtos;

namespace Movies.VerticalSlice.Api.Features.Movies.Names
{
    public record GetNamesQuery(string Title) : IRequest<IEnumerable<MovieNameDto>>;
}
