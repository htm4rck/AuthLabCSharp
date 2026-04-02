namespace Auth.Service;

public interface IAuthService
{
    Task<User> Register(User user, string password);
    Task<string> Login(string email, string password);
    Task<bool> UserIs(string email);
}