using Xunit;
using Moq;
using Movies.VerticalSlice.Api.Wpf.ViewModels;
using Prism.Services.Dialogs;
using FluentValidation;
using Movies.VerticalSlice.Api.Services;

public class LoginViewModelTests
{
    [Fact]
    public void Constructor_SetsUpCommands()
    {
        // Arrange
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        var tokenStore = new TokenStore();
        var dialogServiceMock = new Mock<IDialogService>();
        var validatorMock = new Mock<IValidator<LoginViewModel>>();

        // Act
        var viewModel = new LoginViewModel(
            httpClientFactoryMock.Object,
            tokenStore,
            dialogServiceMock.Object,
            validatorMock.Object
        );

        // Assert
        Assert.NotNull(viewModel.LoginCommand);
        Assert.NotNull(viewModel.ForgotCommand);
        Assert.NotNull(viewModel.CancelCommand);
    }

  
}
