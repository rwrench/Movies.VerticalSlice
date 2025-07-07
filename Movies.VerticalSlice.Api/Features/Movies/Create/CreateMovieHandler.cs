using FluentValidation;
using MediatR;
using Movies.VerticalSlice.Api.Data;
using Movies.VerticalSlice.Api.Data.Models;

namespace Movies.VerticalSlice.Api.Features.Movies.Create
{
    public class CreateMovieHandler : IRequestHandler<CreateMovieCommand, Guid>
    {
        private readonly MoviesDbContext _context;
        private readonly IValidator<CreateMovieCommand> _validator;
        private readonly ILogger<CreateMovieHandler> _logger;

        public CreateMovieHandler(MoviesDbContext context, IValidator<CreateMovieCommand> validator, ILogger<CreateMovieHandler> logger)
        {
            _context = context;
            _validator = validator;
            _logger = logger;
        }

        public async Task<Guid> Handle(CreateMovieCommand command, CancellationToken token)
        {
            await _validator.ValidateAndThrowAsync(command, token);

            var movie = new Movie
            {
                MovieId = Guid.NewGuid(),
                Title = command.Title,
                YearOfRelease = command.YearOfRelease,
                Genres = command.Genres,
                UserId = command.UserId
            };

            _context.Movies.Add(movie);
            await _context.SaveChangesAsync(token);

            _logger.LogInformation("Created movie {Title} with ID {MovieId}", command.Title, movie.MovieId);
            return movie.MovieId;
        }
    }
}
