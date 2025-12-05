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
    }
}
