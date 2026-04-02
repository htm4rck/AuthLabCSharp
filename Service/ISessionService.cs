namespace Auth.Service;

public interface ISessionService
{
    Task RegisterSesion(int usuarioId, string token);
    Task OutSession(string token);
    Task<bool> SessionActive(string token);
}