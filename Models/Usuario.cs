using System;

namespace BonosEsteticaApi.Models
{
    public class Usuario
    {
        public int UsuarioId { get; set; }
        public string Nombre { get; set; }
        public string Correo { get; set; }
        public string ContraseñaHash { get; set; }
        public string Rol { get; set; } // 'Admin', 'Recepcion', etc.
        public bool Activo { get; set; }
    }
}