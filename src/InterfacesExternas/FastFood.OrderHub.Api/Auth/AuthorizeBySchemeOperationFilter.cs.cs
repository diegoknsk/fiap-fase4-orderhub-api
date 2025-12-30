using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace FastFood.OrderHub.Api.Auth
{
    //public class AuthorizeBySchemeOperationFilter : IOperationFilter
    //{
    //    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    //    {
    //        var authAttrs = context.MethodInfo
    //            .GetCustomAttributes(true).OfType<AuthorizeAttribute>()
    //            .Concat(context.MethodInfo.DeclaringType?.GetCustomAttributes(true).OfType<AuthorizeAttribute>() ?? Enumerable.Empty<AuthorizeAttribute>())
    //            .ToArray();

    //        if (!authAttrs.Any()) return;

    //        operation.Responses.TryAdd("401", new OpenApiResponse { Description = "Unauthorized" });
    //        operation.Responses.TryAdd("403", new OpenApiResponse { Description = "Forbidden" });

    //        var schemes = authAttrs
    //            .SelectMany(a => (a.AuthenticationSchemes ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
    //            .Distinct(StringComparer.Ordinal)
    //            .ToArray();

    //        if (schemes.Length == 0) return;

    //        operation.Security ??= new List<OpenApiSecurityRequirement>();
    //        foreach (var scheme in schemes)
    //        {
    //            var reference = new OpenApiSecurityScheme
    //            {
    //                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = scheme }
    //            };
    //            operation.Security.Add(new OpenApiSecurityRequirement
    //            {
    //                [reference] = Array.Empty<string>()
    //            });
    //        }
    //    }
    //}
}
