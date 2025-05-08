using Microsoft.AspNetCore.Mvc;
using BonosEsteticaApi.Services;
using BonosEsteticaApi.Models;
using BonosEsteticaApi.Data.Repositories;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

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
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var usuario = await _usuarioRepository.GetByEmailAsync(model.Username);
            
            if (usuario == null)
            {
                return Unauthorized();
            }

            // Verificar la contraseña
            bool passwordValid;
            
            // Si el usuario tiene salt, usar PBKDF2
            if (!string.IsNullOrEmpty(usuario.ContraseñaSalt))
            {
                passwordValid = VerifyPasswordWithPbkdf2(model.Password, usuario.ContraseñaHash, usuario.ContraseñaSalt);
            }
            else
            {
                // Fallback al método SHA256 para usuarios existentes
                passwordValid = VerifyPasswordWithSha256(model.Password, usuario.ContraseñaHash);
            }
            
            if (!passwordValid)
            {
                return Unauthorized();
            }

            var roles = new List<string> { usuario.Rol };
            var token = _tokenService.GenerateToken(usuario.UsuarioId.ToString(), usuario.Nombre, roles);
            
            return Ok(new { token });
        }

        private bool VerifyPasswordWithSha256(string password, string storedHash)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                var hash = BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
                return hash == storedHash;
            }
        }
        
        private bool VerifyPasswordWithPbkdf2(string password, string storedHash, string storedSalt)
        {
            byte[] saltBytes = Convert.FromBase64String(storedSalt);
            
            // Derivar un hash de 256 bits de la contraseña con PBKDF2
            byte[] computedHash = KeyDerivation.Pbkdf2(
                password: password,
                salt: saltBytes,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256 / 8);
            
            string computedHashString = Convert.ToBase64String(computedHash);
            
            return computedHashString == storedHash;
        }
    }
}