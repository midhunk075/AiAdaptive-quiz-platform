namespace API.Configuration;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string? Key { get; set; }
    public string? Issuer { get; set; }
    public string? Audience { get; set; }
    public int ExpiryDays { get; set; } = 7;
}
