using MediatR;
using Movies.VerticalSlice.Api.Features.Movies.GetAll;
using Movies.VerticalSlice.Api.Shared.Constants;
using Movies.VerticalSlice.Api.Shared.Dtos;


namespace Movies.VerticalSlice.Api.Features.Users.GetAll;

public static class GetAllUsersEndpoint
{
    public static void MapGetAllUsers(this IEndpointRouteBuilder app)
    {
        app.MapGet(ApiEndpoints.Users.GetAll, async (
            IMediator mediator,
            CancellationToken token) =>
        {
            var query = new GetAllUsersQuery();

            var users = await mediator.Send(query, token);
            return Results.Ok(users);
        })
        .WithName("GetAllUsers")
        .Produces(StatusCodes.Status200OK)
        .WithTags("Users")
        .WithOpenApi();
    }
}
