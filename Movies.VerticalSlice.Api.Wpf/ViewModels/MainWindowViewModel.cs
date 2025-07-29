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

        public MainWindowViewModel(IContainerProvider containerProvider)
        {
            ExitCommand = new DelegateCommand(OnExit);
            LoginCommand = new DelegateCommand(OnLogin);
            LoadRatingsCommand = new DelegateCommand(async () => await OnLoadRatingsAsync());
            _containerProvider = containerProvider;
            _ratingsViewModel = _containerProvider.Resolve<RatingsViewModel>();
        }

        async void OnLogin()
        {
            var loginWindow = _containerProvider.Resolve<LoginWindow>();
            if (loginWindow.ShowDialog() == true && loginWindow.IsAuthenticated)
            {
                MessageBox.Show("Login successful!", 
                    "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                await OnLoadRatingsAsync();
            }
        }

        async Task OnLoadRatingsAsync()
        {
            await _ratingsViewModel.LoadRatingsAsync();
        }
        void OnExit()
        {
            Application.Current.Shutdown();
        }
    }
}
