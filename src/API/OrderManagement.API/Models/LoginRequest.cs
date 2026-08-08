using System.ComponentModel.DataAnnotations;

namespace OrderManagement.API.Models;

public sealed class LoginRequest
{
    [Required(ErrorMessage = "Username is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
    public string Password { get; set; } = string.Empty;
}
