using MediatR;
using Movies.VerticalSlice.Api.Features.LoginAudit.GetAll;

namespace Movies.VerticalSlice.Api.Features.LoginAudit.GetAll;

public static class GetLoginAuditLogsEndpoint
{
    public static void MapGetLoginAuditLogs(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/login-audit-logs", async (
            IMediator mediator,
            int page = 1,
            int pageSize = 50,
            string? email = null,
            string? status = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            CancellationToken cancellationToken = default) =>
        {
            try
            {
                var query = new GetLoginAuditLogsQuery(page, pageSize, email, status, startDate, endDate);
                var response = await mediator.Send(query, cancellationToken);
                return Results.Ok(response);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("GetLoginAuditLogs")
        .WithTags("LoginAudit")
        .WithSummary("Get login audit logs with pagination and filtering")
        .WithDescription("Retrieve login audit logs with optional filtering by email, status, and date range")
        .Produces<GetLoginAuditLogsResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .RequireAuthorization()
        .WithOpenApi();
    }
}
