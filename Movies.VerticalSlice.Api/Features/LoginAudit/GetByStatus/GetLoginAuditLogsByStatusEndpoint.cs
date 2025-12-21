using MediatR;
using Movies.VerticalSlice.Api.Features.LoginAudit.GetByStatus;

namespace Movies.VerticalSlice.Api.Features.LoginAudit.GetByStatus;

public static class GetLoginAuditLogsByStatusEndpoint
{
    public static void MapGetLoginAuditLogsByStatus(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/login-audit-logs/by-status/{status}", async (
            string status,
            IMediator mediator,
            int days = 7,
            int page = 1,
            int pageSize = 50,
            CancellationToken cancellationToken = default) =>
        {
            try
            {
                var query = new GetLoginAuditLogsByStatusQuery(status, days, page, pageSize);
                var response = await mediator.Send(query, cancellationToken);
                return Results.Ok(response);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("GetLoginAuditLogsByStatus")
        .WithTags("LoginAudit")
        .WithSummary("Get login audit logs by status")
        .WithDescription("Retrieve login audit logs filtered by status (Success, UserNotFound, InvalidCredentials, FailedLogin) with status breakdown")
        .Produces<GetLoginAuditLogsByStatusResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .RequireAuthorization()
        .WithOpenApi();
    }
}
