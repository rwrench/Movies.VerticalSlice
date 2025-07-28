using Microsoft.Extensions.DependencyInjection;
using Movies.VerticalSlice.Api.Services;
using Movies.VerticalSlice.Api.Wpf.Views;
using Prism.Ioc;
using System;
using System.Net.Http;
using System.Windows;

namespace Movies.VerticalSlice.Api.Wpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<MovieService>();
            containerRegistry.RegisterSingleton<RatingsService>();
            containerRegistry.RegisterForNavigation<MoviesView>();
            containerRegistry.RegisterForNavigation<RatingsView>();


            var services = new ServiceCollection();
            services.AddHttpClient("AuthorizedClient", client =>
            {
                client.BaseAddress = new Uri("https://localhost:7299/"); // Use your API base URL
            });

            var provider = services.BuildServiceProvider();
            containerRegistry.RegisterInstance(provider.GetRequiredService<IHttpClientFactory>());
        }
    }
}
