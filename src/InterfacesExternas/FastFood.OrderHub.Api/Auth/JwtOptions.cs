namespace FastFood.OrderHub.Api.Auth
{
    public sealed record JwtOptions(string Issuer, string Audience, string SecretKey, int ExpiresInMinutes);
}
