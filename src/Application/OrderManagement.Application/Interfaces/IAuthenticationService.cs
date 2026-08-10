namespace OrderManagement.Application.Interfaces;

public interface IAuthenticationService
{
    bool ValidateCredentials(string email, string password);
    string GetUserRole(string email);
}
