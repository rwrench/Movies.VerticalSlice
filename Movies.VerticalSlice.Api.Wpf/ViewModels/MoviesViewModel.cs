using Movies.VerticalSlice.Api.Services;
using Movies.VerticalSlice.Api.Shared.Dtos;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Movies.VerticalSlice.Api.Wpf.ViewModels
{
    public class MoviesViewModel : BindableBase
    {
        private readonly MovieService _movieService;

        public ObservableCollection<MovieDto> Movies { get; } = new();

        public MoviesViewModel(MovieService movieService)
        {
            _movieService = movieService;
            LoadMovies();
        }

        private async void LoadMovies()
        {
            var movies = await _movieService.GetAllAsync();
            if (movies != null)
            {
                foreach (var movie in movies)
                    Movies.Add(movie);
            }
        }
    }
}
