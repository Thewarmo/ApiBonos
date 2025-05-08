using System;

namespace BonosEsteticaApi.Models
{
    public class Bono
    {
        public int BonoId { get; set; }
        public string Codigo { get; set; }
        public int ClienteId { get; set; }
        public int ProcedimientoId { get; set; }
        public string TipoDescuento { get; set; } // 'Porcentaje', 'Monto'
        public decimal ValorDescuento { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaExpiracion { get; set; }
        public bool Usado { get; set; }
        public DateTime? FechaUso { get; set; }
        
        // Propiedades de navegación (opcionales para ADO.NET)
        public Cliente Cliente { get; set; }
        public Procedimiento Procedimiento { get; set; }
    }
}