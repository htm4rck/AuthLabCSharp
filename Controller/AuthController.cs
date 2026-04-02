using Auth.Service;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Controller;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("registrar")]
    public async Task<IActionResult> Registrar([FromBody] UsuarioRegistroDto usuarioDto)
    {
        if (await _authService.UserIs(usuarioDto.Email))
            return BadRequest("El email ya está registrado");
        
        var usuario = new User()
        {
            Name = usuarioDto.Nombre,
            Email = usuarioDto.Email
        };
        
        var usuarioRegistrado = await _authService.Register(usuario, usuarioDto.Password);
        
        return Ok(new { usuarioRegistrado.UserId, usuarioRegistrado.Email });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UsuarioLoginDto usuarioDto)
    {
        var token = await _authService.Login(usuarioDto.Email, usuarioDto.Password);
        
        if (token == null)
            return Unauthorized("Credenciales incorrectas");
        
        return Ok(new { Token = token });
    }
}

// DTOs para las solicitudes
public class UsuarioRegistroDto
{
    public string Nombre { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
}

public class UsuarioLoginDto
{
    public string Email { get; set; }
    public string Password { get; set; }
}