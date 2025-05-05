using System;

namespace BonosEsteticaApi.Models
{
    public class Procedimiento
    {
        public int ProcedimientoId { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public decimal Precio { get; set; }
        public bool Activo { get; set; }
    }
}