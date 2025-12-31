using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace FastFood.OrderHub.Infra.Auth
{
    /// <summary>
    /// Configuração de políticas de autorização
    /// </summary>
    public static class AuthorizationConfig
    {
        public const string AdminPolicy = "Admin";
        public const string CustomerPolicy = "Customer";
        public const string CustomerWithScopePolicy = "CustomerWithScope";

        /// <summary>
        /// Adiciona as políticas de autorização
        /// </summary>
        public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                // Política para Admin (Cognito)
                options.AddPolicy(AdminPolicy, policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim("scope", "aws.cognito.signin.user.admin");
                });

                // Política para Customer (JWT Bearer)
                options.AddPolicy(CustomerPolicy, policy =>
                {
                    policy.RequireAuthenticatedUser();
                });

                // Política para Customer com validação de scope e role
                options.AddPolicy(CustomerWithScopePolicy, policy =>
                {
                    policy.RequireAssertion(context =>
                        context.User.HasClaim("role", "customer") && 
                        context.User.HasClaim("scope", "customer"));
                });
            });

            return services;
        }
    }
}

