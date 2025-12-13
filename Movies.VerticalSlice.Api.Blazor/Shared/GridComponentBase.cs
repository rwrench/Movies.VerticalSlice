using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Telerik.Blazor.Components;

namespace Movies.VerticalSlice.Api.Blazor.Shared
{
    public abstract class GridComponentBase : OwningComponentBase
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
            try
            {
                var authState = await task;
                if (authState.User?.Identity == null || !authState.User.Identity.IsAuthenticated)
                {
                    Navigation.NavigateTo("/login", true);
                }
            }
            catch (Exception ex)
            {
                statusMessage = "Authentication error: " + ex.Message;
                isError = true;
                await InvokeAsync(StateHasChanged);
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

            // Fire-and-forget with exception handling to avoid unobserved exceptions
            FireAndForgetSafeAsync(ClearStatusMessageAfterDelayAsync());
        }

        // Helper to safely fire-and-forget a Task with exception handling
        private async void FireAndForgetSafeAsync(Task task)
        {
            try
            {
                await task;
            }
            catch (Exception ex)
            {
                // Optionally log the exception or handle it as needed
                Console.Error.WriteLine($"Exception in fire-and-forget task: {ex}");
            }
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
