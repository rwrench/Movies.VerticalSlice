using Movies.VerticalSlice.Api.Features.Users.Register;
using Movies.VerticalSlice.Api.Features.Users.Delete;
using Movies.VerticalSlice.Api.Features.Users.Update;
using Movies.VerticalSlice.Api.Features.Users.Login;

namespace Movies.VerticalSlice.Api.Features.Users;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapRegisterUser();
        app.MapUpdateUser();
        app.MapDeleteUser();
        app.MapLoginEndpoint();
    }
}