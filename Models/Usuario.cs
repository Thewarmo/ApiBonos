using System;

namespace BonosEsteticaApi.Models
{
    public class Usuario
    {
        public int UsuarioId { get; set; }
        public string Nombre { get; set; }
        public string Correo { get; set; }
        public string ContraseñaHash { get; set; }
        public string ContraseñaSalt { get; set; } // Nuevo campo para almacenar la sal
        public string Rol { get; set; } // 'Admin', 'Recepcion', etc.
        public bool Activo { get; set; }
    }
    public class UsuarioxId
    {
        public int idUsuario { get; set; }
    }

    public class UsuarioActualizacion
    {
        public int UsuarioId { get; set; }
        public string Correo { get; set; }
        public string Rol { get; set; }
        public string NuevaContraseña { get; set; }
        public bool Activo { get; set; }
    }
}