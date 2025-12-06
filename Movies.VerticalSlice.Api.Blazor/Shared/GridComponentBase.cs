using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Telerik.Blazor.Components;

namespace Movies.VerticalSlice.Api.Blazor.Shared
{
    public abstract class GridComponentBase : OwningComponentBase, IDisposable
    {
        // Protected fields so derived components can access them in Razor markup
        protected string statusMessage = "";
        protected bool isError = false;

        [Inject] protected NavigationManager Navigation { get; set; } = default!;
        [Inject] protected AuthenticationStateProvider AuthStateProvider { get; set; } = default!;

       protected override async Task OnInitializedAsync()
        {
            AuthStateProvider.AuthenticationStateChanged += OnAuthenticationStateChanged;
            await base.OnInitializedAsync();
        }

        private async void OnAuthenticationStateChanged(Task<AuthenticationState> task)
        {
            var authState = await task;
            if (authState.User?.Identity == null || !authState.User.Identity.IsAuthenticated)
            {
                Navigation.NavigateTo("/login", true);
            }
        }

        // Dispose(bool) is overridden to clean up the authentication state change event subscription
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                AuthStateProvider.AuthenticationStateChanged -= OnAuthenticationStateChanged;
            }
            base.Dispose(disposing);
        }

        // Shared Methods
        protected async Task SetStatusMessage(string message, bool error)
        {
            statusMessage = message;
            isError = error;
            // StateHasChanged is invoked here to update the derived component's UI
            await InvokeAsync(StateHasChanged);

            // Fire-and-forget: you might consider the safe wrapper discussed earlier
            _ = ClearStatusMessageAfterDelayAsync();
        }

        private async Task ClearStatusMessageAfterDelayAsync()
        {
            await Task.Delay(5000);
            await ClearStatusMessage();
        }

        protected async Task ClearStatusMessage()
        {
            statusMessage = "";
            await InvokeAsync(StateHasChanged);
        }

       
    }
}
