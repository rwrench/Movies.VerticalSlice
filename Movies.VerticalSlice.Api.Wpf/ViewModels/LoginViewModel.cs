using Prism.Commands;
using Prism.Mvvm;
using Movies.VerticalSlice.Api.Services;
using Movies.VerticalSlice.Api.Shared.Responses;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Prism.Services.Dialogs;
using System;

namespace Movies.VerticalSlice.Api.Wpf.ViewModels;

public class LoginViewModel : BindableBase, IDialogAware
{

    public event Action<IDialogResult>? RequestClose;

    // IDialogAware members
    public string Title => "Login";
    public bool CanCloseDialog() => true;
    public void OnDialogClosed() { }
    public void OnDialogOpened(IDialogParameters parameters) { }

    private string? _email;
    public string? Email
    {
        get => _email;
        set => SetProperty(ref _email, value);
    }

    private string? _password;
    public string? Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    private string? _errorMessage;
    public string? ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    private bool? _dialogResult;
    public bool? DialogResult
    {
        get => _dialogResult;
        set => SetProperty(ref _dialogResult, value);
    }
    public DelegateCommand LoginCommand { get; }
    public DelegateCommand ForgotCommand { get; }
    public DelegateCommand CancelCommand { get; }

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly TokenStore _tokenStore;
    private readonly IDialogService _dialogService;

    public LoginViewModel(
        IHttpClientFactory httpClientFactory, 
        TokenStore tokenStore,
        IDialogService dialogService)
    {
        _httpClientFactory = httpClientFactory;
        _tokenStore = tokenStore;
        _dialogService = dialogService;
        
        LoginCommand = new DelegateCommand(async () => await LoginAsync());
        ForgotCommand = new DelegateCommand(Forgot);
        CancelCommand = new DelegateCommand(Cancel);
    }

    async Task LoginAsync()
    {
        var http = _httpClientFactory.CreateClient("AuthorizedClient");
        var response = await http.PostAsJsonAsync("api/users/login", new { Email, Password });
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<LoginResult>();
            _tokenStore.Token = result?.Token;
            ErrorMessage = null;
            CloseDialog(true);
        }
        else
        {
            ErrorMessage = "Invalid login";
        }
    }

    void Forgot() 
    {
        _dialogService.ShowDialog("ForgotPasswordDialog", null, r =>
        {
            if (r.Result == ButtonResult.OK)
            {
                // Handle success (e.g., show a message)
            }
            else
            {
                // Handle cancel or failure
            }
        });
    }

    private void Cancel()
    {
       CloseDialog(false);
    }

  void CloseDialog(bool isSuccess)
    {
        var result = new DialogResult(isSuccess ? ButtonResult.OK : ButtonResult.Cancel);
        RequestClose?.Invoke(result);
    }

}
