namespace FastFood.OrderHub.Infra.Auth
{
    public sealed class CognitoOptions
    {
        public const string SectionName = "Authentication:Cognito";
        public string UserPoolId { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string Region { get; set; } = "us-east-1";
        public int? ClockSkewMinutes { get; set; } = 5;
        public string Authority => $"https://cognito-idp.{Region}.amazonaws.com/{UserPoolId}";
    }
}



