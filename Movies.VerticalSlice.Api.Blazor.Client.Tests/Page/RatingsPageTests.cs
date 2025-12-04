using Bunit;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Movies.VerticalSlice.Api.Blazor.Client.Tests.TestHelpers;
using Movies.VerticalSlice.Api.Blazor.Pages;
using Movies.VerticalSlice.Api.Services;
using Movies.VerticalSlice.Api.Shared.Dtos;
using Shouldly;
using System.Xml;
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

        //[Fact]
        //public void No_Ratings_Should_Throw_Exception()
        //{
        //    Given_we_do_not_have_ratings();
        //    var cut = When_we_display_the_ratings_Page();
        //    Then_we_should_see_an_error_message(cut);
        //}

        private void Then_we_should_see_an_error_message(IRenderedComponent<Ratings> cut)
        {
            //cut.WaitForState(() => cut.Markup.Contains("Error loading")
            //  || cut.Markup.Contains("No ratings"), TimeSpan.FromSeconds(5));
            //Assert.Contains("Error loading", cut.Markup);
            cut.WaitForAssertion(() => cut.Markup.ShouldContain("Error loading"), TimeSpan.FromSeconds(20));
           // cut.Markup.ShouldContain("Error loading");
        }

        private void Given_we_do_not_have_ratings()
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
            mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<MovieRatingWithNameDto>
            {
                new MovieRatingWithNameDto
                {
                    Id = Guid.NewGuid(),
                    MovieId = Guid.NewGuid(),
                    DateUpdated = DateTime.UtcNow,
                    UserId = "user1",   
                    Rating = 5,
                    MovieName = "Inception"
                }
            });
            Services.AddSingleton(mockService.Object);
        }

        void Then_we_should_see_ratings(IRenderedComponent<Pages.Ratings> cut)
        {
            Assert.Contains("Inception", cut.Markup);
        }
    }
}
