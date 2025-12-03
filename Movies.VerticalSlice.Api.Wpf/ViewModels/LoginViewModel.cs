using Prism.Commands;
using Prism.Mvvm;
using Movies.VerticalSlice.Api.Services;
using Movies.VerticalSlice.Api.Shared.Responses;
#nullable enable
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Prism.Services.Dialogs;
using System;
using FluentValidation;
using System.Linq;

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
        set
        {
            SetProperty(ref _email, value);
            Validate();
        }

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
    public bool IsLoggedIn { get; private set; } = false;

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly TokenStore _tokenStore;
    private readonly IDialogService _dialogService;
    private readonly IValidator _validator;

    public LoginViewModel(
        IHttpClientFactory httpClientFactory, 
        TokenStore tokenStore,
        IDialogService dialogService,
        IValidator<LoginViewModel> validator)
    {
        _httpClientFactory = httpClientFactory;
        _tokenStore = tokenStore;
        _dialogService = dialogService;
        _validator = validator;
        
        LoginCommand = new DelegateCommand(async () => await LoginAsync());
        ForgotCommand = new DelegateCommand(Forgot);
        CancelCommand = new DelegateCommand(Cancel);
    }

    private void Validate()
    {
        var context = new FluentValidation.ValidationContext<LoginViewModel>(this);
        var result = _validator.Validate(context);
        if (result == null)
        {
            ErrorMessage = "Validation failed";
            return;
        }   
        ErrorMessage =  result.IsValid ? string.Empty : result.Errors.FirstOrDefault()?.ErrorMessage;
    }
    public async Task LoginAsync()
    {
        var http = _httpClientFactory.CreateClient("AuthorizedClient");
        var response = await http.PostAsJsonAsync("api/users/login", new { Email, Password });
        if (response.IsSuccessStatusCode)
        {
            IsLoggedIn = true;
            var result = await response.Content.ReadFromJsonAsync<LoginResult>();
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
