using MediatR;
using Movies.VerticalSlice.Api.Features.LoginAudit.GetByEmail;

namespace Movies.VerticalSlice.Api.Features.LoginAudit.GetByEmail;

public static class GetLoginAuditLogsByEmailEndpoint
{
    public static void MapGetLoginAuditLogsByEmail(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/login-audit-logs/by-email/{email}", async (
            string email,
            IMediator mediator,
            int days = 7,
            int page = 1,
            int pageSize = 50,
            CancellationToken cancellationToken = default) =>
        {
            try
            {
                var query = new GetLoginAuditLogsByEmailQuery(email, days, page, pageSize);
                var response = await mediator.Send(query, cancellationToken);
                return Results.Ok(response);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("GetLoginAuditLogsByEmail")
        .WithTags("LoginAudit")
        .WithSummary("Get login audit logs for a specific email")
        .WithDescription("Retrieve login audit logs for a specific email address with optional date range filtering")
        .Produces<GetLoginAuditLogsByEmailResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .RequireAuthorization()
        .WithOpenApi();
    }
}
