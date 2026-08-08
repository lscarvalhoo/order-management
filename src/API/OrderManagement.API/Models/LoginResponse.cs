namespace OrderManagement.API.Models;

public sealed class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}
