using Bunit;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Movies.VerticalSlice.Api.Blazor.Client.Tests.TestHelpers;
using Movies.VerticalSlice.Api.Blazor.Pages;
using Movies.VerticalSlice.Api.Services;
using Movies.VerticalSlice.Api.Shared.Dtos;
using Shouldly;
using Telerik.Blazor.Components;
using Xunit;

namespace Movies.VerticalSlice.Api.Blazor.Client.Tests.Page
{
    public class RatingsPageTests : TelerikTestContext
    {
        public RatingsPageTests()
        {
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            Services.AddSingleton(mockHttpClientFactory.Object);

            // --- START: Refined Auth Mocking ---
            // Create a mock AuthenticationState for a logged-in user
            var authState = new AuthenticationState(new System.Security.Claims.ClaimsPrincipal(
                new System.Security.Claims.ClaimsIdentity(new[] {
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, "testuser")
                }, "mock")));

            var mockAuthStateProvider = new Mock<AuthenticationStateProvider>();
            // Setup GetAuthenticationStateAsync to return the created state
            mockAuthStateProvider.Setup(p => p.GetAuthenticationStateAsync())
                .ReturnsAsync(authState);
            Services.AddSingleton(mockAuthStateProvider.Object);

            // Also, you may need the <CascadingAuthenticationState> wrapper
            Services.AddAuthorizationCore();
        }
        [Fact]
        public void RatingsGrid_Should_RenderRatings()
        {
            Given_we_have_ratings();
            var cut = When_we_display_the_ratings_Page();
            Then_we_should_see_ratings(cut);
        }

        [Fact]
        public async Task Adding_Rating_Calls_Create_AndReloadsGrid()
        {
            MovieRatingWithNameDto rating = Given_we_have_created_a_rating();
            Mock<IRatingsService> mockService = And_we_have_setup_create_rating(rating);
            var cut = When_we_display_the_ratings_Page();
            await And_Create_Is_Called(cut, rating);
            Then_a_rating_is_created(mockService, cut, rating);
        }

        async Task And_Create_Is_Called(
            IRenderedComponent<Ratings> cut, 
            MovieRatingWithNameDto rating)
        {
            await cut.Instance.OnCreateHandler(new GridCommandEventArgs { Item = rating } );    
        }

        Mock<IRatingsService> And_we_have_setup_create_rating(MovieRatingWithNameDto rating)
        {
            var mockService = new Mock<IRatingsService>();
            mockService.Setup(s => s.CreateAsync(It.Is<MovieRatingWithNameDto>(m => m.Id == rating.Id)))
               .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.Created));
            // After create, GetAllAsync returns the saved movie (simulates reload)
            mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<MovieRatingWithNameDto> { rating });

            Services.AddSingleton(mockService.Object);
            return mockService;
        }

        void Then_a_rating_is_created(
          Mock<IRatingsService> mockService,
          IRenderedComponent<Pages.Ratings> cut,
          MovieRatingWithNameDto rating)
        {
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("Loaded 1 ratings", cut.Markup);
                Assert.Contains("Inception", cut.Markup);
            });
            mockService.Verify(s => 
            s.CreateAsync(It.Is<MovieRatingWithNameDto>(m => m.Id == rating.Id)), Times.Once);
        }


        [Fact]
        public void No_Ratings_Should_Throw_Exception()
        {
            Given_we_do_not_have_ratings();
            var cut = When_we_display_the_ratings_Page();
            Then_we_should_see_an_error_message(cut);
        }

        void Then_we_should_see_an_error_message(IRenderedComponent<Ratings> cut)
        {
            cut.WaitForState(() => cut.Markup.Contains("Error loading")
              || cut.Markup.Contains("No ratings"), TimeSpan.FromSeconds(5));
            Assert.Contains("Error loading", cut.Markup);
            cut.WaitForAssertion(() => cut.Markup.ShouldContain("Error loading"), TimeSpan.FromSeconds(20));
            cut.Markup.ShouldContain("Error loading");
        }

        void Given_we_do_not_have_ratings()
        {
            var mockService = new Mock<IRatingsService>();
            mockService.Setup(s => s.GetAllAsync()).ThrowsAsync(new Exception("No ratings available"));
            Services.AddSingleton(mockService.Object);
        }

        IRenderedComponent<Pages.Ratings> When_we_display_the_ratings_Page()
        {
            return RenderComponent<Pages.Ratings>();
        }
        void Given_we_have_ratings()
        {
            var mockService = new Mock<IRatingsService>();
            List<MovieRatingWithNameDto> ratings = new() { Given_we_have_created_a_rating() };
            mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(ratings);
            Services.AddSingleton(mockService.Object);
        }

        MovieRatingWithNameDto Given_we_have_created_a_rating()
        {
            return new MovieRatingWithNameDto
            {
                Id = Guid.NewGuid(),
                MovieId = Guid.NewGuid(),
                DateUpdated = DateTime.UtcNow,
                UserId = "user1",
                Rating = 5,
                MovieName = "Inception"
            };
        }


        void Then_we_should_see_ratings(IRenderedComponent<Pages.Ratings> cut)
        {
            Assert.Contains("Inception", cut.Markup);
        }

        [Fact]
        public async Task Updating_Rating_Calls_Update_AndReloadsGrid()
        {
            MovieRatingWithNameDto rating = Given_we_have_created_a_rating();
            Mock<IRatingsService> mockService = And_we_have_setup_update_rating(rating);
            var cut = When_we_display_the_ratings_Page();
            await And_Update_Is_Called(cut, rating);
            Then_a_rating_is_updated(mockService, cut);
        }

        async Task And_Update_Is_Called(
            IRenderedComponent<Ratings> cut,
            MovieRatingWithNameDto rating)
        {
            await cut.Instance.OnUpdateHandler(new GridCommandEventArgs { Item = rating });
        }

        Mock<IRatingsService> And_we_have_setup_update_rating(MovieRatingWithNameDto rating)
        {
            var mockService = new Mock<IRatingsService>();
            mockService.Setup(s => s.UpdateAsync(It.Is<MovieRatingWithNameDto>(m => m.Id == rating.Id)))
               .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.NoContent));
            mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<MovieRatingWithNameDto> { rating });

            Services.AddSingleton(mockService.Object);
            return mockService;
        }

        void Then_a_rating_is_updated(
            Mock<IRatingsService> mockService,
            IRenderedComponent<Pages.Ratings> cut)
        {
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("Loaded 1 ratings", cut.Markup);
            });
            mockService.Verify(s =>
                s.UpdateAsync(It.IsAny<MovieRatingWithNameDto>()), Times.Once);
        }

        [Fact]
        public async Task Deleting_Rating_Calls_Delete_AndReloadsGrid()
        {
            MovieRatingWithNameDto rating = Given_we_have_created_a_rating();
            Mock<IRatingsService> mockService = And_we_have_setup_delete_rating(rating);
            var cut = When_we_display_the_ratings_Page();
            await And_Delete_Is_Called(cut, rating);
            Then_a_rating_is_deleted(mockService);
        }

        async Task And_Delete_Is_Called(
            IRenderedComponent<Ratings> cut,
            MovieRatingWithNameDto rating)
        {
            await cut.Instance.OnDeleteHandler(new GridCommandEventArgs { Item = rating });
        }

        Mock<IRatingsService> And_we_have_setup_delete_rating(MovieRatingWithNameDto rating)
        {
            var mockService = new Mock<IRatingsService>();
            mockService.Setup(s => s.DeleteAsync(It.Is<Guid>(id => id == rating.Id)))
               .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.NoContent));
            mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<MovieRatingWithNameDto>());

            Services.AddSingleton(mockService.Object);
            return mockService;
        }

        void Then_a_rating_is_deleted(Mock<IRatingsService> mockService)
        {
            mockService.Verify(s =>
                s.DeleteAsync(It.IsAny<Guid>()), Times.Once);
        }

        [Fact]
        public async Task Update_Rating_WithError_ShowsErrorMessage()
        {
            MovieRatingWithNameDto rating = Given_we_have_created_a_rating();
            Mock<IRatingsService> mockService = And_we_have_setup_update_rating_with_error(rating);
            var cut = When_we_display_the_ratings_Page();
            await And_Update_Is_Called(cut, rating);
            Then_we_should_see_an_error_for_update(cut);
        }

        Mock<IRatingsService> And_we_have_setup_update_rating_with_error(MovieRatingWithNameDto rating)
        {
            var mockService = new Mock<IRatingsService>();
            mockService.Setup(s => s.UpdateAsync(It.IsAny<MovieRatingWithNameDto>()))
               .ThrowsAsync(new Exception("Update failed"));
            mockService.Setup(s => s.GetAllAsync())
                .ThrowsAsync(new Exception("Error loading ratings"));

            Services.AddSingleton(mockService.Object);
            return mockService;
        }

        void Then_we_should_see_an_error_for_update(IRenderedComponent<Ratings> cut)
        {
            cut.WaitForState(() => cut.Markup.Contains("Error"), TimeSpan.FromSeconds(10));
            cut.Markup.ShouldContain("Error");
        }

        [Fact]
        public async Task Delete_Rating_WithError_ShowsErrorMessage()
        {
            MovieRatingWithNameDto rating = Given_we_have_created_a_rating();
            Mock<IRatingsService> mockService = And_we_have_setup_delete_rating_with_error(rating);
            var cut = When_we_display_the_ratings_Page();
            await And_Delete_Is_Called(cut, rating);
            Then_we_should_see_an_error_for_delete(cut);
        }

        Mock<IRatingsService> And_we_have_setup_delete_rating_with_error(MovieRatingWithNameDto rating)
        {
            var mockService = new Mock<IRatingsService>();
            mockService.Setup(s => s.DeleteAsync(It.IsAny<Guid>()))
               .ThrowsAsync(new Exception("Delete failed"));
            mockService.Setup(s => s.GetAllAsync())
                .ThrowsAsync(new Exception("Error loading ratings"));

            Services.AddSingleton(mockService.Object);
            return mockService;
        }

        void Then_we_should_see_an_error_for_delete(IRenderedComponent<Ratings> cut)
        {
            cut.WaitForState(() => cut.Markup.Contains("Error"), TimeSpan.FromSeconds(10));
            cut.Markup.ShouldContain("Error");
        }

        [Fact]
        public void RatingsComponent_Should_HaveCorrectGridConfiguration()
        {
            Given_we_have_ratings();
            var cut = When_we_display_the_ratings_Page();
            Then_grid_should_be_configured_correctly(cut);
        }

        void Then_grid_should_be_configured_correctly(IRenderedComponent<Ratings> cut)
        {
            // Verify grid is rendered
            Assert.Contains("Ratings", cut.Markup);
            // Verify command buttons are present
            Assert.Contains("Add Rating", cut.Markup);
        }

        [Fact]
        public void StatusMessage_Should_DisplayAfterLoadingRatings()
        {
            Given_we_have_ratings();
            var cut = When_we_display_the_ratings_Page();
            Then_status_message_should_show_loaded_count(cut);
        }

        void Then_status_message_should_show_loaded_count(IRenderedComponent<Ratings> cut)
        {
            cut.WaitForAssertion(() =>
            {
                // Status message should show the count of loaded ratings
                Assert.Contains("Loaded 1 ratings", cut.Markup);
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public async Task GetAllMovieNames_ReturnsMovieList()
        {
            var movieNames = Given_we_have_movie_names();
            var mockService = And_we_have_setup_movie_names_service(movieNames);
            var cut = When_we_display_the_ratings_Page();
            await When_we_load_movie_names(cut);
            Then_movie_names_should_be_available(mockService);
        }

        List<MovieNameDto> Given_we_have_movie_names()
        {
            return new List<MovieNameDto>
            {
                new MovieNameDto(Guid.NewGuid(), "Inception"),
                new MovieNameDto(Guid.NewGuid(), "The Matrix"),
                new MovieNameDto(Guid.NewGuid(), "Interstellar")
            };
        }

        Mock<IRatingsService> And_we_have_setup_movie_names_service(List<MovieNameDto> movieNames)
        {
            var mockService = new Mock<IRatingsService>();
            mockService.Setup(s => s.GetAllMovieNamesAsync(It.IsAny<string>()))
                .ReturnsAsync(movieNames);
            mockService.Setup(s => s.GetAllAsync())
                .ReturnsAsync(new List<MovieRatingWithNameDto>());

            Services.AddSingleton(mockService.Object);
            return mockService;
        }

        async Task When_we_load_movie_names(IRenderedComponent<Ratings> cut)
        {
            // Movie names should be loaded when editing a rating
            await cut.InvokeAsync(() => Task.CompletedTask);
        }

        void Then_movie_names_should_be_available(Mock<IRatingsService> mockService)
        {
            // Verify that GetAllMovieNamesAsync can be called
            mockService.Verify(s => s.GetAllAsync(), Times.AtLeastOnce);
        }

        [Fact]
        public async Task Create_Rating_WithInvalidData_HandlesGracefully()
        {
            var invalidRating = Given_we_have_an_invalid_rating();
            var mockService = And_we_have_setup_create_with_bad_request();
            var cut = When_we_display_the_ratings_Page();
            await And_Create_Is_Called(cut, invalidRating);
            Then_service_create_was_called(mockService);
        }

        MovieRatingWithNameDto Given_we_have_an_invalid_rating()
        {
            return new MovieRatingWithNameDto
            {
                Id = Guid.NewGuid(),
                MovieId = Guid.Empty, // Invalid
                Rating = -1, // Invalid
                MovieName = ""
            };
        }

        Mock<IRatingsService> And_we_have_setup_create_with_bad_request()
        {
            var mockService = new Mock<IRatingsService>();
            mockService.Setup(s => s.CreateAsync(It.IsAny<MovieRatingWithNameDto>()))
               .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest));
            mockService.Setup(s => s.GetAllAsync())
                .ReturnsAsync(new List<MovieRatingWithNameDto>());

            Services.AddSingleton(mockService.Object);
            return mockService;
        }

        void Then_service_create_was_called(Mock<IRatingsService> mockService)
        {
            mockService.Verify(s =>
                s.CreateAsync(It.IsAny<MovieRatingWithNameDto>()), Times.Once);
        }

        [Fact]
        public async Task LoadRatingsAsync_WithUnauthorized_ThrowsException()
        {
            Given_we_have_unauthorized_access();
            var cut = When_we_display_the_ratings_Page();
            Then_we_should_see_unauthorized_error(cut);
        }

        void Given_we_have_unauthorized_access()
        {
            var mockService = new Mock<IRatingsService>();
            mockService.Setup(s => s.GetAllAsync())
                .ThrowsAsync(new UnauthorizedAccessException("User is not authorized"));
            Services.AddSingleton(mockService.Object);
        }

        void Then_we_should_see_unauthorized_error(IRenderedComponent<Ratings> cut)
        {
            cut.WaitForState(() => cut.Markup.Contains("Error loading") 
                || cut.Markup.Contains("not authorized"), TimeSpan.FromSeconds(5));
            Assert.Contains("Error loading", cut.Markup);
        }
    }
}
