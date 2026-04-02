using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Auth.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Auth.Service;

public class AuthService : IAuthService
{
    private readonly AuthContext _context;
    private readonly IConfiguration _config;
    private readonly ISessionService _sessionService;

    public AuthService(AuthContext context, IConfiguration config, ISessionService sessionService)
    {
        _context = context;
        _config = config;
        _sessionService = sessionService;
    }

    public async Task<User> Register(User user, string password)
    {
        CrearPasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);
        
        user.PasswordHash = passwordHash;
        user.PasswordSalt = passwordSalt;
        
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        
        return user;
    }

    public async Task<string> Login(string email, string password)
    {
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == email);
        
        if (user == null || !VerificarPasswordHash(password, user.PasswordHash, user.PasswordSalt))
            return null;
        
        // Crear token JWT
        var token = GenerarToken(user);
        
        // Registrar la sesión
        await _sessionService.RegisterSesion(user.UserId, token);
        
        return token;
    }

    public async Task<bool> UserIs(string email)
    {
        return await _context.Users.AnyAsync(x => x.Email == email);
    }

    private void CrearPasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
    {
        using var hmac = new HMACSHA512();
        passwordSalt = hmac.Key;
        passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
    }

    private bool VerificarPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
    {
        using var hmac = new HMACSHA512(passwordSalt);
        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        return computedHash.SequenceEqual(passwordHash);
    }

    private string GenerarToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_config["JwtSettings:SecretKey"]);
        
        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email)
            }),
            Expires = DateTime.UtcNow.AddDays(1),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key), 
                SecurityAlgorithms.HmacSha256Signature)
        };
        
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}