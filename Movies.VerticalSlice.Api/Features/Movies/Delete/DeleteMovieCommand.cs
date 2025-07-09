using MediatR;

namespace Movies.VerticalSlice.Api.Features.Movies.Delete;

public record DeleteMovieCommand(Guid MovieId): IRequest<bool>;
