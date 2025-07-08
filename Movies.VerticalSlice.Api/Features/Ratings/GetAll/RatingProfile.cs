using AutoMapper;
using Movies.VerticalSlice.Api.Data.Models;

namespace Movies.VerticalSlice.Api.Features.Ratings.GetAll
{
    public class RatingsProfile : Profile
    {
        public RatingsProfile()
        {
            CreateMap<MovieRating, RatingDto>();
            CreateMap<(MovieRating Rating, Movie Movie), MovieRatingWithNameDto>()
                .ConstructUsing(src => new MovieRatingWithNameDto(
                    src.Rating.Id,
                    src.Rating.MovieId,
                    src.Rating.Rating,
                    src.Rating.UserId,
                    src.Rating.DateUpdated ?? DateTime.UtcNow,
                    src.Movie.Title));
        }
    }
}
