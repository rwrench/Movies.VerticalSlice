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
              .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Rating.Id))
              .ForMember(dest => dest.MovieId, opt => opt.MapFrom(src => src.Rating.MovieId))
              .ForMember(dest => dest.Rating, opt => opt.MapFrom(src => src.Rating.Rating))
              .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Rating.UserId))
              .ForMember(dest => dest.DateUpdated, opt => opt.MapFrom(src => src.Rating.DateUpdated))
              .ForMember(dest => dest.MovieName, opt => opt.MapFrom(src => src.Movie.Title));
        }
    }
}
