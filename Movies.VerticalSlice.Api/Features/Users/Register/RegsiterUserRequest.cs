namespace Movies.VerticalSlice.Api.Features.Users.Register;

public record RegisterUserRequest(
    string UserName,
    string Email,
    string Password
);