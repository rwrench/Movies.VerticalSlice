using Movies.VerticalSlice.Api.Shared.Dtos;
using System.Collections.ObjectModel;

namespace Movies.VerticalSlice.Api.Services
{
    public interface IMovieNamesProviderService
    {
        Task EnsureLoadedAsync();
        Task RefreshAsync(); 
        ObservableCollection<MovieNameDto> MovieNames { get; }
        void Clear();
    }
}
