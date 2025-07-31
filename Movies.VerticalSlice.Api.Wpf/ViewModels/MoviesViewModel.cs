using Movies.VerticalSlice.Api.Services;
using Movies.VerticalSlice.Api.Shared.Dtos;
using Prism.Mvvm;
using Prism.Regions;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Movies.VerticalSlice.Api.Wpf.ViewModels
{
    public class MoviesViewModel : BindableBase, INavigationAware
    {
        private readonly MovieService _movieService;

        public ObservableCollection<MovieDto> Movies { get; } = new();

        public MoviesViewModel(MovieService movieService)
        {
            _movieService = movieService;
        }

        public async Task LoadMoviesAsync()
        {
            var movies = await _movieService.GetAllAsync();
            if (movies != null)
            {
                foreach (var movie in movies)
                    Movies.Add(movie);
            }
        }

        public async void OnNavigatedTo(NavigationContext navigationContext)
        {
            await LoadMoviesAsync();
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) => true;


        public void OnNavigatedFrom(NavigationContext navigationContext) { }
        
    }
}
