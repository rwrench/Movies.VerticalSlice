using Bunit;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Movies.VerticalSlice.Api.Blazor.Client.Tests.TestHelpers;
using Movies.VerticalSlice.Api.Services;
using Movies.VerticalSlice.Api.Shared.Dtos;
using Telerik.Blazor.Components;
using Xunit;


namespace Movies.VerticalSlice.Api.Blazor.Client.Tests.Page;
public class MoviePageTests : TelerikTestContext
{
    public MoviePageTests()
    {
        var mockHttpClientFactory = new Mock<IHttpClientFactory>();
        Services.AddSingleton(mockHttpClientFactory.Object);
        var mockAuthStateProvider = new Mock<AuthenticationStateProvider>();
        Services.AddSingleton(mockAuthStateProvider.Object);
    }

    [Fact]
    public void MoviesGrid_Should_RenderMovies()
    {
        Given_we_have_movies();
        var cut = When_we_display_the_movies_Page();
        Then_we_should_see_movies(cut);
    }

    [Fact]
    public void No_Movies_Should_Throw_Exception()
    {
        Given_we_have_do_not_have_movies();
        var cut = When_we_display_the_movies_Page();
        Then_we_should_see_an_error_message(cut);
    }

    [Fact]
    public async Task Adding_Movie_Calls_Create_AndReloadsGrid()
    {
       var movie = Given_we_have_created_a_movie();
        var mockService = And_we_have_setup_create_movie(movie);
        var cut = When_we_display_the_movies_Page();
        await And_Create_Is_Called(cut, movie);
        Then_a_movie_is_created(mockService,cut, movie);
    }

    [Fact]
    public async Task Updating_Movie_Updates_and_Reloads_Grid()
    {
        var movie = Given_we_have_created_a_movie();
        var updatedMovie = And_we_have_updated_a_movie(movie);
        var mockService = And_we_have_setup_update_movie(movie);
        var cut = When_we_display_the_movies_Page();
        await And_Update_Is_Called(cut, movie);
        Then_a_movie_is_updated(mockService, cut, movie);
    }

    [Fact]
    public async Task Deleting_Movie_Deletes_and_Reloads_Grid()
    {
        var movie = Given_we_have_created_a_movie();
        var updatedMovie = And_we_have_updated_a_movie(movie);
        var mockService = And_we_have_setup_delete_movie(movie);
        var cut = When_we_display_the_movies_Page();
        await And_Delete_Is_Called(cut, movie);
        Then_the_movie_is_deleted(mockService, cut, movie);
    }


    void Then_a_movie_is_created(
        Mock<IMovieService> mockService,
        IRenderedComponent<Pages.Movies> cut,
        MovieDto movie)
    {
        cut.WaitForAssertion(() =>
        {
            cut.Markup.Contains("Loaded 1 movies");
            cut.Markup.Contains("Unit Test Movie");
        });
        mockService.Verify(s => s.CreateAsync(It.Is<MovieDto>(m => m.MovieId == movie.MovieId)), Times.Once);
    }


    void Then_a_movie_is_updated(
        Mock<IMovieService> mockService,
        IRenderedComponent<Pages.Movies> cut,
        MovieDto movie)
    {
        cut.WaitForAssertion(() =>
         {
             cut.Markup.Contains("Loaded 1 movies");
             cut.Markup.Contains("Unit Test Movie - Updated");
         });
        mockService.Verify(s => s.UpdateAsync(movie.MovieId, It.Is<MovieDto>(m => m.MovieId == movie.MovieId)));
    }

    void Then_the_movie_is_deleted(
       Mock<IMovieService> mockService,
       IRenderedComponent<Pages.Movies> cut,
       MovieDto movie)
    {
        cut.WaitForAssertion(() =>
        {
            cut.Markup.Contains("Loaded 1 movies");
            cut.Markup.Contains("Unit Test Movie - Updated");
        });
        mockService.Verify(s => s.DeleteAsync(movie.MovieId));
    }

    async Task And_Create_Is_Called(IRenderedComponent<Pages.Movies> cut, MovieDto movie)
    {
        await cut.Instance.OnCreateHandler(new GridCommandEventArgs { Item = movie });
    }

    async Task And_Update_Is_Called(IRenderedComponent<Pages.Movies> cut, MovieDto movie)
    {
        await cut.Instance.OnUpdateHandler(new GridCommandEventArgs { Item = movie });
    }

    async Task And_Delete_Is_Called(IRenderedComponent<Pages.Movies> cut, MovieDto movie)
    {
        await cut.Instance.OnDeleteHandler(new GridCommandEventArgs { Item = movie });
    }

    void Then_we_should_see_an_error_message(IRenderedComponent<Pages.Movies> cut)
    {
        cut.Markup.Contains("Error loading movies");
    }

     void Then_we_should_see_movies(IRenderedComponent<Pages.Movies> cut)
    {
        cut.Markup.Contains("Inception");
        cut.Markup.Contains("Sci-Fi");
    }

    IRenderedComponent<Pages.Movies> When_we_display_the_movies_Page()
    {
        return RenderComponent<Pages.Movies>();
    }

     void Given_we_have_movies()
    {
        // Arrange
        var mockService = new Mock<IMovieService>();
        mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<MovieDto>
        {
            new MovieDto { MovieId = Guid.NewGuid(), Title = "Inception",
                YearOfRelease = 2010, Genres = "Sci-Fi" }
        });
        Services.AddSingleton(mockService.Object);
    }

     void Given_we_have_do_not_have_movies()
    {
        var mockService = new Mock<IMovieService>();
        mockService.Setup(s => s.GetAllAsync()).Throws(new Exception("No movies available"));   
        Services.AddSingleton(mockService.Object);
    }

    MovieDto Given_we_have_created_a_movie()
    {
        return new MovieDto { MovieId = Guid.NewGuid(),
            Title = "Unit Test Movie", YearOfRelease = 2025, Genres = "Test" };
    }


    private MovieDto And_we_have_updated_a_movie(MovieDto dto)
    {
       dto = dto with { Title = "Unit Test Movie - Updated" };  
       return dto;
    }

    Mock<IMovieService> And_we_have_setup_create_movie(MovieDto movie)
    { 
        var mockService = new Mock<IMovieService>();
        mockService.Setup(s => s.CreateAsync(It.Is<MovieDto>(m => m.MovieId == movie.MovieId)))
           .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.Created)); // Or u
        // After create, GetAllAsync returns the saved movie (simulates reload)
        mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<MovieDto> { movie });

        Services.AddSingleton(mockService.Object);
        return mockService;
    }

    Mock<IMovieService> And_we_have_setup_update_movie(MovieDto movie)
    {
        var mockService = new Mock<IMovieService>();
        mockService.Setup(s => s.UpdateAsync(movie.MovieId, movie))
            .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.Created)); 
        mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<MovieDto> { movie });

        Services.AddSingleton(mockService.Object);
        return mockService;
    }

    Mock<IMovieService> And_we_have_setup_delete_movie(MovieDto movie)
    {
        var mockService = new Mock<IMovieService>();
        mockService.Setup(s => s.DeleteAsync(movie.MovieId))
            .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
        mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<MovieDto> { movie });

        Services.AddSingleton(mockService.Object);
        return mockService;
    }
}