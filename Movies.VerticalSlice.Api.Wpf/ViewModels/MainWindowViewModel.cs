using Movies.VerticalSlice.Api.Services;
using Movies.VerticalSlice.Api.Wpf.Views;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using System.Threading.Tasks;
using System.Windows;

namespace Movies.VerticalSlice.Api.Wpf.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private string _title = "Movies App";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        public DelegateCommand ExitCommand { get; }
        public DelegateCommand LoginCommand { get; }
        public DelegateCommand LoadRatingsCommand { get; }

        private readonly IContainerProvider _containerProvider;
        private readonly RatingsViewModel _ratingsViewModel;
        private readonly TokenStore _tokenStore;

        public MainWindowViewModel(IContainerProvider containerProvider)
        {
            ExitCommand = new DelegateCommand(OnExit);
            LoginCommand = new DelegateCommand(OnLogin);
            LoadRatingsCommand = new DelegateCommand(async () => await OnLoadRatingsAsync());
            _containerProvider = containerProvider;
            _ratingsViewModel = _containerProvider.Resolve<RatingsViewModel>();
            _tokenStore = _containerProvider.Resolve<TokenStore>();
        }

        async void OnLogin()
        {
            var loginWindow = _containerProvider.Resolve<LoginWindow>();
            if (loginWindow.ShowDialog() == true && _tokenStore.IsAuthenticated)
            {
                MessageBox.Show("Login successful!", 
                    "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                await OnLoadRatingsAsync();
            }
        }

        async Task OnLoadRatingsAsync()
        {
            if (!_tokenStore.IsAuthenticated)
            {
                MessageBox.Show("You must be logged in to load ratings.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            await _ratingsViewModel.LoadRatingsAsync();
        }
        void OnExit()
        {
            Application.Current.Shutdown();
        }
    }
}
