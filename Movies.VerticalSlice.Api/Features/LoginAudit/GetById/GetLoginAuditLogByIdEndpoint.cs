using MediatR;
using Movies.VerticalSlice.Api.Features.LoginAudit.GetById;
using Movies.VerticalSlice.Api.Features.LoginAudit.Shared;

namespace Movies.VerticalSlice.Api.Features.LoginAudit.GetById;

public static class GetLoginAuditLogByIdEndpoint
{
    public static void MapGetLoginAuditLogById(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/login-audit-logs/{id:long}", async (
            long id,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var query = new GetLoginAuditLogByIdQuery(id);
                var response = await mediator.Send(query, cancellationToken);
                
                if (response == null)
                    return Results.NotFound(new { error = "Login audit log not found" });
                
                return Results.Ok(response);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("GetLoginAuditLogById")
        .WithTags("LoginAudit")
        .WithSummary("Get login audit log by ID")
        .WithDescription("Retrieve a specific login audit log by its ID")
        .Produces<LoginAuditLogDetailDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status400BadRequest)
        .RequireAuthorization()
        .WithOpenApi();
    }
}
