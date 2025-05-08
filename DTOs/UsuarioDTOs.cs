using System;

namespace BonosEsteticaApi.DTOs
{
    public class UsuarioCreacionDto
    {
        public string Nombre { get; set; }
        public string Correo { get; set; }
        public string Contraseña { get; set; }
        public string? ContraseñaHash { get; set; }
        public string? ContraseñaSalt { get; set; } // Nuevo campo para almacenar la sal
        public string Rol { get; set; }
    }

    public class UsuarioActualizacionDto
    {
        public string Nombre { get; set; }
        public string Correo { get; set; }
        public string Rol { get; set; }
        public bool Activo { get; set; }
        public string NuevaContraseña { get; set; } // Opcional
    }

    public class CambioPasswordDto
    {
        public string PasswordActual { get; set; }
        public string NuevaPassword { get; set; }
    }
}