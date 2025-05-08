using System;

namespace BonosEsteticaApi.Models
{
    public class Procedimiento
    {
        public int? ProcedimientoId { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public decimal Precio { get; set; }
        public int Duracion { get; set; } // Duración en minutos
        public bool Activo { get; set; }
    }

    public class ProcedimientoxId
    {
        public int idProcedimiento { get; set; }
    }

    public class ProcedimientoActualizacion
    {
        public int ProcedimientoId { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public decimal Precio { get; set; }
        public int Duracion { get; set; }
        public bool Activo { get; set; }
    }
}