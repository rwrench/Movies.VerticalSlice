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
    bool _isEditing;
    bool _isLoading;

    public ObservableCollection<MovieRatingWithNameDto> Ratings { get; } = new();
    public ObservableCollection<MovieNameDto> MovieNames { get; } = new();

    public DelegateCommand<GridViewAddingNewEventArgs> AddRatingCommand { get; }
    public DelegateCommand<GridViewBeginningEditRoutedEventArgs> BeginEditCommand { get; }
    public DelegateCommand<GridViewRowEditEndedEventArgs> EndEditCommand { get; }
    public DelegateCommand<MovieRatingWithNameDto> EditRatingCommand { get; }
    public DelegateCommand<IList> DeleteRatingCommand { get; }
    public DelegateCommand<IList> SelectedRatingsCommand { get; }

    MovieRatingWithNameDto _selectedrating;

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }
    public MovieRatingWithNameDto SelectedRating
    {
        get => _selectedrating;
        set
        {
            SetProperty(ref _selectedrating, value);
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
            TokenStore tokenStore)
    {
        _ratingsService = ratingservice;
        _tokenStore = tokenStore;

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
        _ratingsService.AuthToken = _tokenStore.Token;
        var response = await _ratingsService.UpdateAsync(dto);
        if (response.IsSuccessStatusCode)
        {
            MessageBox.Show($"Movie updated successfully.");
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

    private void OnDeleteRating(IList list)
    {
        throw new NotImplementedException();
    }

    private bool CanAddRating(GridViewAddingNewEventArgs args)
    {
        return !IsEditing && _tokenStore.IsAuthenticated;
    }

    async void OnAddRating(GridViewAddingNewEventArgs args)
    {
        MovieRatingWithNameDto newRating = AddNewRatingToCollection();
        SelectedRating = newRating;
        _ratingsService.AuthToken = _tokenStore.Token;
        await _ratingsService.CreateAsync(newRating);
    }

    MovieRatingWithNameDto AddNewRatingToCollection()
    {
        var newRating = new MovieRatingWithNameDto();
        Ratings.Add(newRating);
        return newRating;
    }

    public async void OnNavigatedTo(NavigationContext navigationContext)
    {
        await LoadMovieNamesAsync();
        await LoadRatingsAsync();
    }

    public async Task LoadRatingsAsync()
    {
        IsLoading = true;
        _ratingsService.AuthToken = _tokenStore.Token;
        var ratings = await _ratingsService.GetAllAsync();
        if (ratings != null)
        {
            foreach (var rating in ratings)
                Ratings.Add(rating);
        }
    }

    private async Task LoadMovieNamesAsync()
    {
        var movieNames = await _ratingsService.GetAllMovieNamesAsync();
        if (movieNames != null)
        {
            MovieNames.Clear();
            foreach (var movie in movieNames)
                MovieNames.Add(movie);
        }
        IsLoading = false;
    }

    public bool IsNavigationTarget(NavigationContext navigationContext) => true;



    public void OnNavigatedFrom(NavigationContext navigationContext) { }
   
}


