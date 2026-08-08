namespace OrderManagement.API.Configuration;

public class DevelopmentAuthOptions
{
    public const string SectionName = "DevelopmentAuth";

    public FixedUserCredentials FixedUser { get; set; } = new();
}

public class FixedUserCredentials
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = "User";
}
