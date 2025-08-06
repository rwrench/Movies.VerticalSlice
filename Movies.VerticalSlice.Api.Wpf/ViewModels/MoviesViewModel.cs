using Movies.VerticalSlice.Api.Services;
using Movies.VerticalSlice.Api.Shared.Dtos;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Movies.VerticalSlice.Api.Wpf.ViewModels
{
    public class MoviesViewModel : BindableBase, INavigationAware
    {
        private readonly MovieService _movieService;
        private readonly TokenStore _tokenStore;


        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }
        public ObservableCollection<MovieDto> Movies { get; } = new();

        public DelegateCommand<MovieDto> AddMovieCommand { get; }
        public DelegateCommand<MovieDto> EditMovieCommand { get; }
        public DelegateCommand<IList> DeleteMovieCommand { get; }

        private MovieDto _selectedMovie;
        public MovieDto SelectedMovie
        {
            get => _selectedMovie;
            set
            {
                SetProperty(ref _selectedMovie, value);
                EditMovieCommand.RaiseCanExecuteChanged();
            }
        }

        public MoviesViewModel(MovieService movieService, TokenStore tokenStore)
        {
            _movieService = movieService;
            _tokenStore = tokenStore;
           
            AddMovieCommand = new DelegateCommand<MovieDto>(_ => StartAddMovie());
            EditMovieCommand = new DelegateCommand<MovieDto>(OnEditMovie, CanEditMovie);
            DeleteMovieCommand = new DelegateCommand<IList>(OnDeleteMovie);
        }

        async void OnDeleteMovie(IList selectedItems)
        {
            if (selectedItems == null || selectedItems.Count == 0)
            {
                return;
            }
            var itemsToDelete = selectedItems.Cast<MovieDto>().ToList();
            foreach (var dto in itemsToDelete)
            {
                _movieService.AuthToken = _tokenStore.Token;
                var response = await _movieService.DeleteAsync(dto.MovieId);
                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show($"Movie '{dto.Title}' deleted successfully.");  
                    Movies.Remove(dto);
                }
            }
        }

        bool CanEditMovie(MovieDto dto)
        {

            return dto != null;
        }

        async void OnEditMovie(MovieDto dto)
        {
            if (dto == null)
            {
                return;
            }
            _movieService.AuthToken = _tokenStore.Token;
            var response = await _movieService.UpdateAsync(dto.MovieId, dto);
            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show($"Movie updated successfully.");
            }
        }

        async void OnAddMovie(MovieDto dto)
        {
            _movieService.AuthToken = _tokenStore.Token;
            await _movieService.CreateAsync(dto);
        }
        public async Task LoadMoviesAsync()
        {
            IsLoading = true;
            var movies = await _movieService.GetAllAsync();
            Movies.Clear();
            if (movies != null)
            {
                foreach (var movie in movies)
                    Movies.Add(movie);
            }
            IsLoading = false;
        }

        public async void OnNavigatedTo(NavigationContext navigationContext)
        {
            await LoadMoviesAsync();
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) => true;


        public void OnNavigatedFrom(NavigationContext navigationContext) { }

        public void StartAddMovie()
        {
            var newMovie = new MovieDto(); // Defaults, or set as needed
            Movies.Add(newMovie);
            SelectedMovie = newMovie;
        }
    }
}
