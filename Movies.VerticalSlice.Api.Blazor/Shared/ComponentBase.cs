using Microsoft.AspNetCore.Components;

namespace Movies.VerticalSlice.Api.Blazor.Shared
{
    public abstract class GridComponentBase : OwningComponentBase
    {
        // Protected fields so derived components can access them in Razor markup
        protected string statusMessage = "";
        protected bool isError = false;

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
