using Movies.VerticalSlice.Api.Services;
using Movies.VerticalSlice.Api.Shared.Responses;
using System.Net.Http;
using System.Net.Http.Json;
using System.Windows;
using System.Windows.Controls;

namespace Movies.VerticalSlice.Api.Wpf.Views;

/// <summary>
/// Interaction logic for LoginWindow.xaml
/// </summary>
public partial class LoginWindow : Window
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly TokenStore _tokenStore;

#nullable enable

    public string? Email => EmailBox.Text;
    public string? Password => PasswordBox.Password;
    public bool IsAuthenticated { get; private set; }

    public LoginWindow(IHttpClientFactory httpClientFactory, TokenStore tokenStore)
    {
        InitializeComponent();
        _httpClientFactory = httpClientFactory;
        _tokenStore = tokenStore;
    }

    private async void Login_Click(object sender, RoutedEventArgs e)
    {
        var http = _httpClientFactory.CreateClient("AuthorizedClient");
        var response = await http.PostAsJsonAsync("api/users/login", new { Email, Password });
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<LoginResult>();
            _tokenStore.Token = result?.Token;
            IsAuthenticated = true;
            DialogResult = true;
            Close();
        }
        else
        {
            ErrorText.Text = "Invalid login";
        }
    }
}
