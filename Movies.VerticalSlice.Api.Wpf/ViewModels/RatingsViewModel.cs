using Movies.VerticalSlice.Api.Services;
using Movies.VerticalSlice.Api.Shared.Dtos;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Movies.VerticalSlice.Api.Wpf.ViewModels;

public class RatingsViewModel : BindableBase
{
    private readonly RatingsService _ratingsService;

    public ObservableCollection<MovieRatingWithNameDto> Ratings { get; } = new();

    public RatingsViewModel(RatingsService ratingservice)
    {
        _ratingsService = ratingservice;
    }

    public async Task LoadRatingsAsync()
    {
        var ratings = await _ratingsService.GetAllAsync();
        if (ratings != null)
        {
            foreach (var rating in ratings)
                Ratings.Add(rating);
        }
    }
}


