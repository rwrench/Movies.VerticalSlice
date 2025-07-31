using Movies.VerticalSlice.Api.Services;
using Movies.VerticalSlice.Api.Shared.Dtos;
using Prism.Mvvm;
using Prism.Regions;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Movies.VerticalSlice.Api.Wpf.ViewModels;

public class RatingsViewModel : BindableBase, INavigationAware
{
    private readonly RatingsService _ratingsService;
    private readonly TokenStore _tokenStore;

    public ObservableCollection<MovieRatingWithNameDto> Ratings { get; } = new();

    public RatingsViewModel(RatingsService ratingservice,
            TokenStore tokenStore)
    {
        _ratingsService = ratingservice;
        _tokenStore = tokenStore;
    }

    public async Task LoadRatingsAsync()
    {
        _ratingsService.AuthToken = _tokenStore.Token;
        var ratings = await _ratingsService.GetAllAsync();
        if (ratings != null)
        {
            foreach (var rating in ratings)
                Ratings.Add(rating);
        }
    }

    public async void OnNavigatedTo(NavigationContext navigationContext)
    {
       await LoadRatingsAsync();
    }

    public bool IsNavigationTarget(NavigationContext navigationContext) => true;


    public void OnNavigatedFrom(NavigationContext navigationContext) { }
   
}


