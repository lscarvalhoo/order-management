namespace OrderManagement.Infrastructure.Configuration;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string? Key { get; set; }
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
}
