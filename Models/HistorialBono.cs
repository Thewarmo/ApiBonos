using System;

namespace BonosEsteticaApi.Models
{
    public class HistorialBono
    {
        public int HistorialId { get; set; }
        public int BonoId { get; set; }
        public string Accion { get; set; } // 'Creado', 'Aplicado', 'Expirado'
        public DateTime Fecha { get; set; }
        public int UsuarioId { get; set; }
        
        // Propiedades de navegación (opcionales para ADO.NET)
        public Bono Bono { get; set; }
        public Usuario Usuario { get; set; }
    }
}