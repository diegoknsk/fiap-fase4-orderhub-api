using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace FastFood.OrderHub.Infra.Auth
{
    /// <summary>
    /// Configuração de autenticação JWT Bearer para Customer
    /// </summary>
    public static class JwtAuthenticationConfig
    {
        private const string CustomerSchemeName = "CustomerBearer";
        private const string ConfigurationSection = "JwtCustomer";

        /// <summary>
        /// Adiciona autenticação JWT Bearer para Customer
        /// </summary>
        public static AuthenticationBuilder AddCustomerJwtBearer(this AuthenticationBuilder authBuilder, IConfiguration configuration)
        {
            return authBuilder.AddJwtBearer(CustomerSchemeName, options =>
            {
                options.TokenValidationParameters = BuildTokenValidationParameters(configuration, ConfigurationSection);
            });
        }

        /// <summary>
        /// Constrói os parâmetros de validação do token JWT
        /// </summary>
        private static TokenValidationParameters BuildTokenValidationParameters(IConfiguration configuration, string section)
        {
            var issuer = configuration[$"{section}:Issuer"];
            var audience = configuration[$"{section}:Audience"];
            var secret = configuration[$"{section}:SecretKey"];

            if (string.IsNullOrWhiteSpace(issuer))
                throw new InvalidOperationException($"JWT Issuer não configurado na seção {section}:Issuer");
            
            if (string.IsNullOrWhiteSpace(audience))
                throw new InvalidOperationException($"JWT Audience não configurado na seção {section}:Audience");
            
            if (string.IsNullOrWhiteSpace(secret))
                throw new InvalidOperationException($"JWT SecretKey não configurado na seção {section}:SecretKey");

            return new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                ValidIssuer = issuer,
                ValidAudience = audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                ClockSkew = TimeSpan.FromSeconds(30),
                RoleClaimType = "role",
                NameClaimType = JwtRegisteredClaimNames.Sub
            };
        }

        /// <summary>
        /// Configura o JWT Security Token Handler para desabilitar mapeamento automático de claims
        /// </summary>
        public static void ConfigureJwtSecurityTokenHandler()
        {
            JwtSecurityTokenHandler.DefaultMapInboundClaims = false;
        }
    }
}

