using MediatR;

namespace Movies.VerticalSlice.Api.Features.Movies.Update;

public record UpdateMovieCommand(
    Guid MovieId,
    string Title,
    int YearOfRelease,
    List<string> Genres,
    Guid UserId
) : IRequest<bool>;
