using System;

namespace BonosEsteticaApi.DTOs
{
    public class ClienteCreacionDto
    {
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Correo { get; set; }
        public string Telefono { get; set; }
        public DateTime FechaRegistro { get; set; }
    }

    public class ClienteActualizacionDto
    {
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Correo { get; set; }
        public string Telefono { get; set; }
        public DateTime FechaNacimiento { get; set; }
        public bool Activo { get; set; }
    }
}