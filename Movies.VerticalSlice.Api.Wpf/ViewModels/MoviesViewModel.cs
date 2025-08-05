using Movies.VerticalSlice.Api.Services;
using Movies.VerticalSlice.Api.Shared.Dtos;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Telerik.Windows.Documents.RichTextBoxCommands;

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
        

        public MoviesViewModel(MovieService movieService, TokenStore tokenStore)
        {
            _movieService = movieService;
            _tokenStore = tokenStore;
            _movieService.AuthToken = tokenStore.Token;
            AddMovieCommand = new DelegateCommand<MovieDto>(OnAddMovie);
            EditMovieCommand = new DelegateCommand<MovieDto>(OnEditMovie);
            DeleteMovieCommand = new DelegateCommand<IList>(OnDeleteMovie);
        }

        async void OnDeleteMovie(IList selectedItems)
        {
            if (selectedItems == null || selectedItems.Count == 0)
            {
                return;
            }
            foreach (var item in selectedItems)
            {
                if (item is MovieDto dto)
                {
                    
                    await _movieService.DeleteAsync(dto.MovieId);
                }
            }
        }

        async void OnEditMovie(MovieDto dto)
        {
            await _movieService.UpdateAsync(dto.MovieId, dto);
        }

        async void OnAddMovie(MovieDto dto)
        {
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
        
    }
}
