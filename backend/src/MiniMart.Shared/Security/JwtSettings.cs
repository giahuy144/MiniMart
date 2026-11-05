namespace MiniMart.Shared.Security;

public class JwtSettings
{
    public string SecretKey { get; set; } = "MiniMartSecretKey_SuperSecureKeyForSOAProject2026!@#$%^";
    public string Issuer { get; set; } = "MiniMartAuthService";
    public string Audience { get; set; } = "MiniMartClients";
    public int ExpiryMinutes { get; set; } = 1440; // 24 hours
}
