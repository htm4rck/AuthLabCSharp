using Auth.Data;
using Microsoft.EntityFrameworkCore;

namespace Auth.Service;

public class SessionService : ISessionService
{
    private readonly AuthContext _context;

    public SessionService(AuthContext context)
    {
        _context = context;
    }

    public async Task RegisterSesion(int userCode, string token)
    {
        var session = new Session
        {
            UserId = userCode,
            Token = token,
            StartDate = DateTime.UtcNow,
            Device = "Web", // Puedes personalizar esto
            Ip = "127.0.0.1"     // Obtener la IP real del request
        };
        
        _context.Sessions.Add(session);
        await _context.SaveChangesAsync();
    }

    public async Task OutSession(string token)
    {
        var session = await _context.Sessions
            .FirstOrDefaultAsync(s => s.Token == token && s.EndDate == null);
        
        if (session != null)
        {
            session.EndDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> SessionActive(string token)
    {
        return await _context.Sessions
            .AnyAsync(s => s.Token == token && s.EndDate == null);
    }
}