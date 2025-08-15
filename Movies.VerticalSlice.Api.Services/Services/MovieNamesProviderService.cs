using Movies.VerticalSlice.Api.Shared.Dtos;
using System.Collections.ObjectModel;

namespace Movies.VerticalSlice.Api.Services
{
    public class MovieNamesProviderService : IMovieNamesProviderService
    {
        readonly IRatingsService _ratingsService;
        bool _isLoaded;
        public ObservableCollection<MovieNameDto> MovieNames { get; } = new ObservableCollection<MovieNameDto>();

        public MovieNamesProviderService(IRatingsService ratingsService)
        {
            _ratingsService = ratingsService;
        }

        public void Clear()
        {
            MovieNames.Clear();
            _isLoaded = false;
        }

        public async Task EnsureLoadedAsync()
        {
            if (_isLoaded) return;
            var movieNames = await _ratingsService.GetAllMovieNamesAsync();
            MovieNames.Clear();
            if (movieNames != null)
            {
                foreach (var movie in movieNames)
                    MovieNames.Add(movie);
            }
            _isLoaded = true;
        }

        public async Task RefreshAsync()
        {
           await EnsureLoadedAsync();
        }
    }
}
