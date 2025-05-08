using System;

namespace BonosEsteticaApi.DTOs
{
    // DTO para crear un bono
    public class BonoCreacionDto
    {
        public int ClienteId { get; set; }
        public int ProcedimientoId { get; set; }
        public decimal ValorDescuento { get; set; } // Por defecto será un porcentaje
    }

    // DTO para aplicar un bono
    public class BonoAplicacionDto
    {
        public int BonoId { get; set; }
        public int ProcedimientoId { get; set; }
    }

    // DTO para revertir un bono
    public class BonoReversionDto
    {
        public int BonoId { get; set; }
    }

    // DTO para consultar un bono por ID
    public class BonoxId
    {
        public int idBono { get; set; }
    }

    public class BonoxCodigo
    {
        public string CodigoBono { get; set; }
    }
    public class BonosxCliente
    {
        public int idCliente { get; set; }
    }
}