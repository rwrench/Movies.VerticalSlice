using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Movies.VerticalSlice.Api.Configuration;

public class SecurityRequirementsOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Check if the endpoint has [Authorize] attribute or RequireAuthorization()
        var hasAuthorize = context.MethodInfo.DeclaringType?
            .GetCustomAttributes(true)
            .OfType<AuthorizeAttribute>()
            .Any() ?? false;

        if (!hasAuthorize)
        {
            hasAuthorize = context.MethodInfo
                .GetCustomAttributes(true)
                .OfType<AuthorizeAttribute>()
                .Any();
        }

        // Check metadata for authorization requirement (for minimal APIs)
        var metadata = context.ApiDescription.ActionDescriptor.EndpointMetadata;
        var requiresAuth = metadata.OfType<AuthorizeAttribute>().Any();
        var allowsAnonymous = metadata.OfType<AllowAnonymousAttribute>().Any();

        // If endpoint requires authorization and doesn't explicitly allow anonymous
        if ((hasAuthorize || requiresAuth) && !allowsAnonymous)
        {
            operation.Security = new List<OpenApiSecurityRequirement>
            {
                new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                }
            };

            // Add 401 response if not already present
            if (!operation.Responses.ContainsKey("401"))
            {
                operation.Responses.Add("401", new OpenApiResponse 
                { 
                    Description = "Unauthorized - Authentication required" 
                });
            }
        }
    }
}
