namespace Movies.VerticalSlice.Api.Features.Users.Update;

public record UpdateUserRequest(
    string UserName,
    string Email,
    string? Password = null
);