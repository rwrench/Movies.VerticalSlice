using Movies.VerticalSlice.Api.Services;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
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

        public DelegateCommand<string> NavigateCommand { get; }

        private readonly IDialogService _dialogService;
        private readonly IContainerProvider _containerProvider;
        private readonly RatingsViewModel _ratingsViewModel;
        private readonly TokenStore _tokenStore;
        private readonly IRegionManager _regionManager;

        public MainWindowViewModel(
            IContainerProvider containerProvider,
            IRegionManager regionManager,
            IDialogService dialogService)
        {
            ExitCommand = new DelegateCommand(OnExit);
            LoginCommand = new DelegateCommand(OnLogin);
            LoadRatingsCommand = new DelegateCommand(async () => await OnLoadRatingsAsync());
            _containerProvider = containerProvider;
            _ratingsViewModel = _containerProvider.Resolve<RatingsViewModel>();
            _tokenStore = _containerProvider.Resolve<TokenStore>();
            _regionManager = regionManager; 
            NavigateCommand = new DelegateCommand<string>(OnNavigate);
            _dialogService = dialogService;
            OnLogin();
        }

        private void OnNavigate(string viewName)
        {
            _regionManager.RequestNavigate("MainRegion", viewName);
        }

        void OnLogin()
        {
            _dialogService.ShowDialog("LoginWindow", null, result =>
            {
                if (result.Result == ButtonResult.OK && _tokenStore.IsAuthenticated)
                {
                    MessageBox.Show("Login successful!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            });
        }

        async Task OnLoadRatingsAsync()
        {
            _regionManager.RequestNavigate("MainRegion", "RatingsView");
            await _ratingsViewModel.LoadRatingsAsync();
        }
        void OnExit()
        {
            Application.Current.Shutdown();
        }
    }
}
