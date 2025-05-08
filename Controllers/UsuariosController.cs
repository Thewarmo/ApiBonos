using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BonosEsteticaApi.Models;
using BonosEsteticaApi.Data.Repositories;
using BonosEsteticaApi.DTOs;  // Añade esta referencia
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Identity;

namespace BonosEsteticaApi.Controllers
{
    [Route("/[controller]")]
    [ApiController]
    [Authorize] // Requiere autenticación para todas las acciones
    public class UsuariosController : ControllerBase
    {
        private readonly UsuarioRepository _usuarioRepository;

        public UsuariosController(UsuarioRepository usuarioRepository)
        {
            _usuarioRepository = usuarioRepository;
        }

        // GET: /Usuarios
        [HttpGet]
        [Authorize(Roles = "Admin")] // Solo administradores pueden ver todos los usuarios
        public async Task<ActionResult<IEnumerable<Usuario>>> GetUsuarios()
        {
            try
            {
                var usuarios = await _usuarioRepository.GetAllAsync();
                return Ok(usuarios);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        
        [HttpPost("Usuario")]
        [Authorize(Roles = "Admin")] // Solo administradores pueden ver detalles de usuarios
        public async Task<ActionResult<Usuario>> GetUsuario(UsuarioxId user)
        {
            try
            {
                var usuario = await _usuarioRepository.GetByIdAsync(user.idUsuario);

                if (usuario == null)
                {
                    return NotFound($"Usuario con ID {user.idUsuario} no encontrado");
                }

                // No devolver la contraseña hash por seguridad
                usuario.ContraseñaHash = null;
                usuario.ContraseñaSalt = null;

                return Ok(usuario);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        
        [HttpPost("CrearUsuario")]
        [Authorize(Roles = "Admin")] // Solo administradores pueden crear usuarios
        public async Task<ActionResult<Usuario>> CreateUsuario(UsuarioCreacionDto usuarioDto)
        {
            try
            {
                // Validar que el correo no exista ya
                var usuarioExistente = await _usuarioRepository.GetByEmailAsync(usuarioDto.Correo);
                if (usuarioExistente != null)
                {
                    return BadRequest($"Ya existe un usuario con el correo {usuarioDto.Correo}");
                }

                // Crear el hash de la contraseña
                var (passwordHash, passwordSalt) = HashPassword(usuarioDto.Contraseña);
                usuarioDto.ContraseñaHash = Convert.ToBase64String(passwordHash);
                usuarioDto.ContraseñaSalt = Convert.ToBase64String(passwordSalt);

                var nuevoUsuario = new Usuario
                {
                    Nombre = usuarioDto.Nombre,
                    Correo = usuarioDto.Correo,
                    ContraseñaHash = usuarioDto.ContraseñaHash,
                    ContraseñaSalt = usuarioDto.ContraseñaSalt,
                    Rol = usuarioDto.Rol,
                    Activo = true
                };

                var usuarioId = await _usuarioRepository.CreateAsync(nuevoUsuario);
                
                // Recuperar el usuario creado para devolverlo
                var usuarioCreado = await _usuarioRepository.GetByIdAsync(usuarioId);
                
                // No devolver la contraseña hash por seguridad
                usuarioCreado.ContraseñaHash = null;
                usuarioCreado.ContraseñaHash = null;

                return CreatedAtAction(nameof(GetUsuario), new { id = usuarioId }, usuarioCreado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpPost("ActualizarUsuario")]
        [Authorize(Roles = "Admin")] // Solo administradores pueden actualizar usuarios
        public async Task<IActionResult> UpdateUsuario(UsuarioActualizacion usuarioDto)
        {
            try
            {
                var usuario = await _usuarioRepository.GetByIdAsync(usuarioDto.UsuarioId);
                if (usuario == null)
                {
                    return NotFound($"Usuario con ID {usuarioDto.UsuarioId} no encontrado");
                }

                // Si se está actualizando el correo, verificar que no exista ya
                if (usuario.Correo != usuarioDto.Correo)
                {
                    var usuarioExistente = await _usuarioRepository.GetByEmailAsync(usuarioDto.Correo);
                    if (usuarioExistente != null && usuarioExistente.UsuarioId != usuarioDto.UsuarioId)
                    {
                        return BadRequest($"Ya existe un usuario con el correo {usuarioDto.Correo}");
                    }
                }

                // Actualizar los campos del usuario
                usuario.Correo = usuarioDto.Correo;
                usuario.Rol = usuarioDto.Rol;
                usuario.Activo = usuarioDto.Activo;

                // Si se proporciona una nueva contraseña, actualizarla
                if (!string.IsNullOrEmpty(usuarioDto.NuevaContraseña))
                {
                    var (passwordHash, passwordSalt) = HashPassword(usuarioDto.NuevaContraseña);
                    usuario.ContraseñaHash = Convert.ToBase64String(passwordHash);
                    usuario.ContraseñaSalt = Convert.ToBase64String(passwordSalt);
                }

                var resultado = await _usuarioRepository.UpdateAsync(usuario);
                if (resultado)
                {
                    return Ok(new {estado=true,Mensaje="Actualizacion de usuario realizada"});
                }
                else
                {
                    return StatusCode(500, "No se pudo actualizar el usuario");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpPost("EliminarUsuario")]
        [Authorize(Roles = "Admin")] // Solo administradores pueden eliminar usuarios
        public async Task<IActionResult> DeleteUsuario(UsuarioxId usuarioEliminar)
        {
            try
            {
                var usuario = await _usuarioRepository.GetByIdAsync(usuarioEliminar.idUsuario);
                if (usuario == null)
                {
                    return NotFound($"Usuario con ID {usuarioEliminar.idUsuario} no encontrado");
                }

                var resultado = await _usuarioRepository.DeleteAsync(usuarioEliminar.idUsuario);
                if (resultado)
                {
                    return Ok(new { estado = true, Mensaje = "Eliminacion de usuario realizada" });
                }
                else
                {
                    return StatusCode(500, "No se pudo eliminar el usuario");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }
        static (byte[] hash, byte[] salt) HashPassword(string password)
        {
            // Generar una sal aleatoria
            byte[] salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // Derivar un hash de 256 bits de la contraseña con PBKDF2
            byte[] hash = KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256 / 8);

            return (hash, salt);
        }

        // Método para verificar contraseñas
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