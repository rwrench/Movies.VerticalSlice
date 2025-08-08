
using Moq;
using Movies.VerticalSlice.Api.Services;
using Movies.VerticalSlice.Api.Shared.Dtos;
using Movies.VerticalSlice.Api.Wpf.ViewModels;
using Prism.Regions;
    
namespace Movies.VerticalSlice.Api.Wpf.Tests
{
    public class MoviesViewModelTests
    {
        [Fact]
        public void Constructor_InitializesMoviesCollection()
        {
            var movieServiceMock = new Mock<IMovieService>();
            var viewModel = new MoviesViewModel(movieServiceMock.Object);
            Assert.NotNull(viewModel.Movies);
            Assert.Empty(viewModel.Movies);
        }

        [Fact]
        public async Task LoadMoviesAsync_PopulatesMoviesCollection()
        {
            // Arrange
            var movies = new List<MovieDto>
            {
                new MovieDto(Guid.NewGuid(), "Movie 1", "movie-1", 2020, "Action",null),
                new MovieDto(Guid.NewGuid(), "Movie 2", "movie-2", 2021, "Drama", null)
            };

            var movieServiceMock = new Mock<IMovieService>();
            movieServiceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(movies);

            var viewModel = new MoviesViewModel(movieServiceMock.Object);

            // Act
            await viewModel.LoadMoviesAsync();

            // Assert
            Assert.Equal(2, viewModel.Movies.Count);
            Assert.Contains(viewModel.Movies, m => m.Title == "Movie 1");
            Assert.Contains(viewModel.Movies, m => m.Title == "Movie 2");
        }


        [Fact]
        public async Task OnNavigatedTo_CallsLoadMoviesAsync()
        {
            // Arrange
            var movies = new List<MovieDto>
            {
                new MovieDto(Guid.NewGuid(), "Movie 1", "movie-1", 2020, "Action", null)
            };
            var movieServiceMock = new Mock<IMovieService>();
            movieServiceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(movies);

            var viewModel = new MoviesViewModel(movieServiceMock.Object);
           

            var navServiceMock = new Mock<IRegionNavigationService>();
            var uri = new Uri("http://localhost");
            var parameters = new NavigationParameters();
            var navigationContext = new NavigationContext(navServiceMock.Object, uri, parameters);

            // Act
            viewModel.OnNavigatedTo(navigationContext);

            // Wait for async void to complete (not ideal, but covers the method)
            await Task.Delay(100);

            // Assert
            Assert.Single(viewModel.Movies);
            Assert.Equal("Movie 1", viewModel.Movies[0].Title);
        }

        //[Fact]
        //public void IsNavigationTarget_AlwaysReturnsTrue()
        //{
        //    // Arrange
        //    var movieServiceMock = new Mock<MovieService>();
        //    var viewModel = new MoviesViewModel(movieServiceMock.Object);
        //    var navigationContext = new Mock<NavigationContext>(null, null, null).Object;

        //    // Act
        //    var result = viewModel.IsNavigationTarget(navigationContext);

        //    // Assert
        //    Assert.True(result);
        //}

        //[Fact]
        //public void OnNavigatedFrom_DoesNotThrow()
        //{
        //    // Arrange
        //    var movieServiceMock = new Mock<MovieService>();
        //    var viewModel = new MoviesViewModel(movieServiceMock.Object);
        //    var navigationContext = new Mock<NavigationContext>(null, null, null).Object;

        //    // Act & Assert
        //    var exception = Record.Exception(() => viewModel.OnNavigatedFrom(navigationContext));
        //    Assert.Null(exception);
        //}
    }
}
