using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Movies.VerticalSlice.Api.Data.Database;
using Movies.VerticalSlice.Api.Features.Users.Register;
using Movies.VerticalSlice.Api.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Movies.VerticalSlice.Api.Tests.Feature.Users;

public class RegisterUserHandlerTests
{
    [Fact]
    public async Task Handle_ShouldRegisterUser_WhenDataIsValidAndUnique()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<MoviesDbContext>()
            .UseInMemoryDatabase(databaseName: "RegisterUserTestDb")
            .Options;
        var dbContext = new MoviesDbContext(options);

        var validatorMock = new Mock<IValidator<RegisterUserCommand>>();
        validatorMock
            .Setup(v => v.ValidateAndThrowAsync(It.IsAny<RegisterUserCommand>(),
            It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var loggerMock = new Mock<ILogger<RegisterUserHandler>>();
        var passwordServiceMock = new Mock<IPasswordService>();
        passwordServiceMock
            .Setup(p => p.HashPassword(It.IsAny<string>()))
            .Returns("hashed-password");

        var handler = new RegisterUserHandler(
            dbContext,
            validatorMock.Object,
            loggerMock.Object,
            passwordServiceMock.Object
        );

        var command = new RegisterUserCommand("uniqueuser", "unique@example.com", "Password123!");

        // Act
        var userId = await handler.Handle(command, CancellationToken.None);

        // Assert
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
        Assert.NotNull(user);
        Assert.Equal("uniqueuser", user.UserName);
        Assert.Equal("unique@example.com", user.Email);
        Assert.Equal("hashed-password", user.PasswordHash);
    }
}
