namespace Movies.VerticalSlice.Api.Features.Users.Update;

public record UpdateUserRequest(
    string Email = "",
    string Password = ""
);