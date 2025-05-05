using Microsoft.AspNetCore.Mvc;
using BonosEsteticaApi.Services;
using BonosEsteticaApi.Models;
using BonosEsteticaApi.Data.Repositories;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace BonosEsteticaApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly TokenService _tokenService;
        private readonly UsuarioRepository _usuarioRepository;

        public AuthController(TokenService tokenService, UsuarioRepository usuarioRepository)
        {
            _tokenService = tokenService;
            _usuarioRepository = usuarioRepository;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var usuario = await _usuarioRepository.GetByEmailAsync(model.Username);
            
            if (usuario == null)
            {
                return Unauthorized();
            }

            // Verificar la contraseña
            if (!VerifyPassword(model.Password, usuario.ContraseñaHash))
            {
                return Unauthorized();
            }

            var roles = new List<string> { usuario.Rol };
            var token = _tokenService.GenerateToken(usuario.UsuarioId.ToString(), usuario.Nombre, roles);
            
            return Ok(new { token });
        }

        private bool VerifyPassword(string password, string storedHash)
        {
            // En un escenario real, deberías usar un algoritmo de hash seguro como bcrypt
            // Este es un ejemplo simple usando SHA256
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                var hash = BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
                return hash == storedHash;
            }
        }
    }
}