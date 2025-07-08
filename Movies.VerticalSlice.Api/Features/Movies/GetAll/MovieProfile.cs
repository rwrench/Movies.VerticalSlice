using AutoMapper;
using Movies.VerticalSlice.Api.Data.Models;
using Movies.VerticalSlice.Api.Features.Ratings.GetAll;

namespace Movies.VerticalSlice.Api.Features.Movies.GetAll
{
    public class MoviesProfile : Profile
    {
        public MoviesProfile() {
            CreateMap<Movie, MovieDto>();
            CreateMap<Movie, MovieRatingWithNameDto>();
        }
        
    }
}
