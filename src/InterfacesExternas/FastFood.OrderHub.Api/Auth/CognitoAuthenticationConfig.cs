using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
namespace FastFood.OrderHub.Api.Auth
{

    public static class CognitoAuthenticationConfig
    {
        public static AuthenticationBuilder AddCognitoJwtBearer(this AuthenticationBuilder authBuilder, IConfiguration configuration)
        {
            // Configurar CognitoOptions
            var services = authBuilder.Services;
            services.Configure<CognitoOptions>(
                configuration.GetSection(CognitoOptions.SectionName));

            var cognito = new CognitoOptions();
            configuration.GetSection(CognitoOptions.SectionName).Bind(cognito);

            // Adicionar JWT Bearer para Cognito
            return authBuilder
                .AddJwtBearer("Cognito", options =>
                {
                    options.Authority = cognito.Authority;
                    options.RequireHttpsMetadata = false; // Para desenvolvimento local
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = cognito.Authority,
                        ValidateAudience = false, // Cognito Access Token não tem 'aud'
                        ValidAudience = cognito.ClientId,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ClockSkew = TimeSpan.FromMinutes(cognito.ClockSkewMinutes ?? 5),
                        NameClaimType = "username"
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = ctx =>
                        {
                            Console.WriteLine($"Authentication failed: {ctx.Exception?.Message}");
                            Console.WriteLine($"Exception type: {ctx.Exception?.GetType().Name}");
                            if (ctx.Exception?.InnerException != null)
                            {
                                Console.WriteLine($"Inner exception: {ctx.Exception.InnerException.Message}");
                            }
                            return Task.CompletedTask;
                        },
                        OnChallenge = ctx =>
                        {
                            Console.WriteLine($"Challenge: {ctx.Error} - {ctx.ErrorDescription}");
                            return Task.CompletedTask;
                        },
                        OnTokenValidated = ctx =>
                        {
                            var claims = ctx.Principal?.Claims?.ToDictionary(c => c.Type, c => c.Value);
                            Console.WriteLine($"Token validated. Claims count: {claims?.Count ?? 0}");

                            if (claims is null)
                            {
                                Console.WriteLine("Token sem claims.");
                                ctx.Fail("Token sem claims.");
                                return Task.CompletedTask;
                            }

                            if (!claims.TryGetValue("token_use", out var tokenUse) || tokenUse != "access")
                            {
                                Console.WriteLine($"Token use inválido: {tokenUse}");
                                ctx.Fail("Token não é Access Token.");
                                return Task.CompletedTask;
                            }

                            if (!claims.TryGetValue("client_id", out var clientId) || clientId != cognito.ClientId)
                            {
                                Console.WriteLine($"Client ID inválido. Esperado: {cognito.ClientId}, Recebido: {clientId}");
                                ctx.Fail("client_id inválido para esta API.");
                                return Task.CompletedTask;
                            }

                            Console.WriteLine("Token validado com sucesso!");
                            return Task.CompletedTask;
                        }
                    };
                });

            return authBuilder;
        }
    }
}