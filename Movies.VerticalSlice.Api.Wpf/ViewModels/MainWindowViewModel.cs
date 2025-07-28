using Prism.Commands;
using Prism.Mvvm;
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

        public MainWindowViewModel()
        {
            ExitCommand = new DelegateCommand(OnExit);
        }

        private void OnExit()
        {
            Application.Current.Shutdown();
        }
    }
}
