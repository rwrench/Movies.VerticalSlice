using Movies.VerticalSlice.Api.Services;
using Movies.VerticalSlice.Api.Shared.Dtos;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;

namespace Movies.VerticalSlice.Api.Wpf.ViewModels;

public class RatingsViewModel : BindableBase, INavigationAware
{

    #region "Properties"
    readonly IRatingsService _ratingsService;
    readonly TokenStore _tokenStore;
    private readonly IMovieNamesProviderService _namesProviderService;
    bool _isEditing;
    bool _isLoading;

    public ObservableCollection<MovieRatingWithNameDto> Ratings { get; } = new();
    public ObservableCollection<MovieNameDto> MovieNames => _namesProviderService.MovieNames;

    public DelegateCommand<GridViewAddingNewEventArgs> AddRatingCommand { get; }
    public DelegateCommand<GridViewBeginningEditRoutedEventArgs> BeginEditCommand { get; }
    public DelegateCommand<GridViewRowEditEndedEventArgs> EndEditCommand { get; }
    public DelegateCommand<MovieRatingWithNameDto> EditRatingCommand { get; }
    public DelegateCommand<IList> DeleteRatingCommand { get; }
    public DelegateCommand<IList> SelectedRatingsCommand { get; }

    MovieRatingWithNameDto _selectedRating;

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }
    public MovieRatingWithNameDto SelectedRating
    {
        get => _selectedRating;
        set
        {
            SetProperty(ref _selectedRating, value);
        }
    }
    IList<MovieRatingWithNameDto> _selectedRatings;
    public IList<MovieRatingWithNameDto> SelectedRatings
    {
        get => _selectedRatings;
        set
        {
            SetProperty(ref _selectedRatings, value);
        }
    }



    public bool IsEditing
    {
        get => _isEditing;
        set => SetProperty(ref _isEditing, value);
    }

    #endregion



    public RatingsViewModel(IRatingsService ratingservice,
            TokenStore tokenStore, IMovieNamesProviderService namesProviderService)
    {
        _ratingsService = ratingservice;
        _tokenStore = tokenStore;
        _namesProviderService = namesProviderService;

        AddRatingCommand = new DelegateCommand<GridViewAddingNewEventArgs>(OnAddRating, CanAddRating)
              .ObservesProperty(() => IsEditing);

        DeleteRatingCommand = new DelegateCommand<IList>(OnDeleteRating, CanSelectOrDeleteRatings)
             .ObservesProperty(() => SelectedRating)
             .ObservesProperty(() => IsEditing);

        EditRatingCommand = new DelegateCommand<MovieRatingWithNameDto>(OnEditRating, CanEditRating)
          .ObservesProperty(() => IsEditing);

        SelectedRatingsCommand = new DelegateCommand<IList>(OnSelectRatings, CanSelectOrDeleteRatings);
        BeginEditCommand = new DelegateCommand<GridViewBeginningEditRoutedEventArgs>(OnBeginEdit);
        EndEditCommand = new DelegateCommand<GridViewRowEditEndedEventArgs>(OnEndEdit);
    }

    async void OnEditRating(MovieRatingWithNameDto dto)
    {
        var response = await _ratingsService.UpdateAsync(dto);
        if (response.IsSuccessStatusCode)
        {
            MessageBox.Show($"Movie updated successfully.");
            await LoadRatingsAsync(); // Refresh after edit
        }
    }

    private void OnEndEdit(GridViewRowEditEndedEventArgs args)
    {
       IsEditing = false;
    }

    private void OnBeginEdit(GridViewBeginningEditRoutedEventArgs args)
    {
       IsEditing = true;
    }

    private void OnSelectRatings(IList selectedItems)
    {
        SelectedRatings = selectedItems?.Cast<MovieRatingWithNameDto>().ToList() ?? new List<MovieRatingWithNameDto>();
    }

    private bool CanEditRating(MovieRatingWithNameDto dto)
    {
        return dto != null;
    }

    private bool CanSelectOrDeleteRatings(IList list)
    {
        return list != null && list.Count > 0;
    }

    private async void OnDeleteRating(IList selectedItems)
    {
        if (selectedItems == null || selectedItems.Count == 0)
            return;

        var itemsToDelete = selectedItems.Cast<MovieRatingWithNameDto>().ToList();
        foreach (var dto in itemsToDelete)
        {
            var response = await _ratingsService.DeleteAsync(dto.Id);
            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show($"Movie '{dto.MovieName}' deleted successfully.");
            }
        }
        await LoadRatingsAsync(); // Refresh after delete
    }

    private bool CanAddRating(GridViewAddingNewEventArgs args)
    {
        return !IsEditing && _tokenStore.IsAuthenticated;
    }

    async void OnAddRating(GridViewAddingNewEventArgs args)
    {
        MovieRatingWithNameDto newRating = AddNewRatingToCollection();
        SelectedRating = newRating;
        await _ratingsService.CreateAsync(newRating);
        await LoadRatingsAsync(); // Refresh after add
    }

    private MovieRatingWithNameDto AddNewRatingToCollection()
    {
        var newRating = new MovieRatingWithNameDto();

        // Set default MovieId if available
        if (MovieNames.Any())
        {
            newRating.MovieId = MovieNames.First().MovieId; // or .Id, depending on your DTO
            newRating.MovieName = MovieNames.First().MovieName; // optional, for display
        }

        Ratings.Add(newRating);
        return newRating;
    }

    public async void OnNavigatedTo(NavigationContext navigationContext)
    {
        IsLoading = true;
        await _namesProviderService.EnsureLoadedAsync();
        await LoadRatingsAsync();
        IsLoading = false;
    }

    public async Task LoadRatingsAsync()
    {
        var ratings = await _ratingsService.GetAllAsync();
        Ratings.Clear(); // <-- Clear before adding new items
        if (ratings != null)
        {
            foreach (var rating in ratings)
                Ratings.Add(rating);
        }
    }

   

    public bool IsNavigationTarget(NavigationContext navigationContext) => true;



    public void OnNavigatedFrom(NavigationContext navigationContext) { }
   
}


