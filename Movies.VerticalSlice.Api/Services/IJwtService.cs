using Movies.VerticalSlice.Api.Data.Models;

namespace Movies.VerticalSlice.Api.Services;

public interface IJwtService
{
    string GenerateToken(User user);
}