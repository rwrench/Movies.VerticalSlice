namespace Movies.VerticalSlice.Api.Data.Models;

public class User
{
    public required Guid UserId { get; set; }
    public required string UserName { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
}
