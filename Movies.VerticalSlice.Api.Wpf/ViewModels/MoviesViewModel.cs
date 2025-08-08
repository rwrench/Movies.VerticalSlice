using Movies.VerticalSlice.Api.Services;
using Movies.VerticalSlice.Api.Shared.Dtos;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;

namespace Movies.VerticalSlice.Api.Wpf.ViewModels
{
    public class MoviesViewModel : BindableBase, INavigationAware
    {
        private readonly IMovieService _movieService;

        #region "Properties"
        readonly MovieService _movieService;
        readonly TokenStore _tokenStore;
        bool _isEditing;
        bool _isLoading;
      
        public ObservableCollection<MovieDto> Movies { get; } = new();

        public DelegateCommand<GridViewAddingNewEventArgs> AddMovieCommand { get; }
        public DelegateCommand<GridViewBeginningEditRoutedEventArgs> BeginEditCommand { get; }
        public DelegateCommand<GridViewRowEditEndedEventArgs> EndEditCommand { get; }
        public DelegateCommand<MovieDto> EditMovieCommand { get; }
        public DelegateCommand<IList> DeleteMovieCommand { get; }
        public DelegateCommand<IList> SelectedMoviesCommand { get; }

        MovieDto _selectedMovie;

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }
        public MovieDto SelectedMovie
        {
            get => _selectedMovie;
            set
            {
                SetProperty(ref _selectedMovie, value);
            }
        }
        IList<MovieDto> _selectedMovies;
        public IList<MovieDto> SelectedMovies
        {
            get => _selectedMovies;
            set
            {
                SetProperty(ref _selectedMovies, value);
            }
        }

        public bool IsEditing
        {
            get => _isEditing;
            set => SetProperty(ref _isEditing, value);
        }

        #endregion
        public MoviesViewModel(MovieService movieService, TokenStore tokenStore)
        {
            _movieService = movieService;
            _tokenStore = tokenStore;
           
            AddMovieCommand = new DelegateCommand<GridViewAddingNewEventArgs>(OnAddMovie, CanAddMovie)
                .ObservesProperty(() => IsEditing);

            DeleteMovieCommand = new DelegateCommand<IList>(OnDeleteMovie, CanSelectOrDeleteMovies)
                 .ObservesProperty(() => SelectedMovie)
                 .ObservesProperty(() => IsEditing);

            EditMovieCommand = new DelegateCommand<MovieDto>(OnEditMovie, CanEditMovie)
              .ObservesProperty(() => IsEditing);
         
            SelectedMoviesCommand = new DelegateCommand<IList>(OnSelectMovies, CanSelectOrDeleteMovies);
            BeginEditCommand = new DelegateCommand<GridViewBeginningEditRoutedEventArgs>(OnBeginEdit);
            EndEditCommand = new DelegateCommand<GridViewRowEditEndedEventArgs>(OnEndEdit);
        }

        #region "Command events "
        void OnEndEdit(GridViewRowEditEndedEventArgs args)
        {
            IsEditing = false;
        }
            void OnBeginEdit(GridViewBeginningEditRoutedEventArgs args)
        {
            IsEditing = true;   
        }

        bool CanSelectOrDeleteMovies(IList list)
        {
            return list != null && list.Count > 0;
        }
        bool CanEditMovie(MovieDto dto)
        {
            return dto != null;
        }

        bool CanAddMovie(GridViewAddingNewEventArgs args) => !IsEditing;
      

        void OnSelectMovies(IList selectedItems)
        {
            SelectedMovies = selectedItems?.Cast<MovieDto>().ToList() ?? new List<MovieDto>();
        }
        async void OnEditMovie(MovieDto dto)
        {
            _movieService.AuthToken = _tokenStore.Token;
            var response = await _movieService.UpdateAsync(dto.MovieId, dto);
            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show($"Movie updated successfully.");
            }
        }
        async void OnAddMovie(GridViewAddingNewEventArgs args)
        {
            var newMovie = new MovieDto();
            Movies.Add(newMovie);
            SelectedMovie = newMovie;
            _movieService.AuthToken = _tokenStore.Token;
            await _movieService.CreateAsync(newMovie);
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

        #endregion
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


        #region "Navigation"

        public async void OnNavigatedTo(NavigationContext navigationContext)
        {
            await LoadMoviesAsync();
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) => true;


        public void OnNavigatedFrom(NavigationContext navigationContext) { }
        #endregion

    }
}
