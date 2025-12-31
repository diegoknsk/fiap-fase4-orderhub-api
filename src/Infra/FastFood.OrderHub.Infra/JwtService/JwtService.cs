using FastFood.OrderHub.Application.InfraServices;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FastFood.OrderHub.Infra.JwtService
{
    //public class JwtService : IJwtService
    //{
    //    private readonly IConfiguration _config;

    //    public JwtService(IConfiguration config)
    //    {
    //        _config = config;
    //    }

    //    public string GenerateToken(Guid userId, string username)
    //    {
    //        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtSettings:SecretKey"]!));
    //        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    //        var claims = new[]
    //        {
    //            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
    //            new Claim(JwtRegisteredClaimNames.UniqueName, username)
    //        };

    //        var token = new JwtSecurityToken(
    //            issuer: _config["JwtSettings:Issuer"],
    //            audience: _config["JwtSettings:Audience"],
    //            claims: claims,
    //            expires: DateTime.UtcNow.AddMinutes(int.Parse(_config["JwtSettings:ExpiresInMinutes"]!)),
    //            signingCredentials: creds);

    //        return new JwtSecurityTokenHandler().WriteToken(token);
    //    }

    //    public string GenerateTokenV2(Guid subjectId, string role, string scope, string[] amr, string audience, int expiresMinutes)
    //    {
    //        var issuer = _config["JwtAdmin:Issuer"] ?? _config["JwtCustomer:Issuer"]; // mesmo issuer
    //        // Seleciona secret conforme audience
    //        var secret = audience == _config["JwtAdmin:Audience"]
    //            ? _config["JwtAdmin:SecretKey"]
    //            : _config["JwtCustomer:SecretKey"];

    //        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret!));
    //        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    //        var claims = new List<Claim>
    //        {
    //            new(JwtRegisteredClaimNames.Sub, subjectId.ToString()),
    //            new("role", role),
    //            new("scope", scope)
    //        };
    //        foreach (var m in amr ?? Array.Empty<string>())
    //            claims.Add(new Claim("amr", m));

    //        var token = new JwtSecurityToken(
    //            issuer: issuer,
    //            audience: audience,
    //            claims: claims,
    //            expires: DateTime.UtcNow.AddMinutes(expiresMinutes),
    //            signingCredentials: creds);

    //        return new JwtSecurityTokenHandler().WriteToken(token);
    //    }
    //}
}
