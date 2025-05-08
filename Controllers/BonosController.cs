using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BonosEsteticaApi.Models;
using BonosEsteticaApi.Data.Repositories;
using BonosEsteticaApi.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security.Claims;

namespace BonosEsteticaApi.Controllers
{
    [Route("/[controller]")]
    [ApiController]
    [Authorize] // Requiere autenticación para todas las acciones
    public class BonosController : ControllerBase
    {
        private readonly BonoRepository _bonoRepository;
        private readonly HistorialBonoRepository _historialBonoRepository;
        private readonly ClienteRepository _clienteRepository;
        private readonly ProcedimientoRepository _procedimientoRepository;

        public BonosController(
            BonoRepository bonoRepository,
            HistorialBonoRepository historialBonoRepository,
            ClienteRepository clienteRepository,
            ProcedimientoRepository procedimientoRepository)
        {
            _bonoRepository = bonoRepository;
            _historialBonoRepository = historialBonoRepository;
            _clienteRepository = clienteRepository;
            _procedimientoRepository = procedimientoRepository;
        }

        // GET: /Bonos
        [HttpGet]
        [Authorize(Roles = "Admin,Recepcion")] // Administradores y recepcionistas pueden ver todos los bonos
        public async Task<ActionResult<IEnumerable<Bono>>> GetBonos()
        {
            try
            {
                var bonos = await _bonoRepository.GetAllAsync();
                return Ok(bonos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        // POST: /Bonos/Bono
        [HttpPost("Bono")]
        [Authorize(Roles = "Admin,Recepcion")] // Administradores y recepcionistas pueden ver detalles de bonos
        public async Task<ActionResult<Bono>> GetBono(BonoxCodigo bono)
        {
            try
            {
                var bonoEncontrado = await _bonoRepository.GetByCodigoAsync(bono.CodigoBono);

                if (bonoEncontrado == null)
                {
                    return NotFound($"Bono con ID {bono.CodigoBono} no encontrado");
                }

                return Ok(bonoEncontrado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        // POST: /Bonos/GenerarBono
        [HttpPost("GenerarBono")]
        [Authorize(Roles = "Admin,Recepcion")] // Administradores y recepcionistas pueden generar bonos
        public async Task<ActionResult<Bono>> GenerarBono(BonoCreacionDto bonoDto)
        {
            try
            {
                // 1. Validar que el procedimiento exista y esté activo
                var procedimiento = await _procedimientoRepository.GetByIdAsync(bonoDto.ProcedimientoId);
                if (procedimiento == null)
                {
                    return BadRequest(new { estado = false, mensaje = $"El procedimiento con ID {bonoDto.ProcedimientoId} no existe" });
                }
                if (!procedimiento.Activo)
                {
                    return BadRequest(new { estado = false, mensaje = $"El procedimiento con ID {bonoDto.ProcedimientoId} no está activo" });
                }

                // 2. Validar que el cliente exista
                var cliente = await _clienteRepository.GetByIdAsync(bonoDto.ClienteId);
                if (cliente == null)
                {
                    return BadRequest(new { estado = false, mensaje = $"El cliente con ID {bonoDto.ClienteId} no existe" });
                }

                // 3. Verificar que no exista ya un bono activo para ese cliente y procedimiento
                var existeBono = await _bonoRepository.ExisteBonoActivoAsync(bonoDto.ClienteId, bonoDto.ProcedimientoId);
                if (existeBono)
                {
                    return BadRequest(new { estado = false, mensaje = $"Ya existe un bono activo para el cliente {bonoDto.ClienteId} y procedimiento {bonoDto.ProcedimientoId}" });
                }

                // 4. Generar el código del bono
                var codigo = $"BONO-{bonoDto.ClienteId}-{bonoDto.ProcedimientoId}-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8)}";

                // 5. Crear el bono
                var nuevoBono = new Bono
                {
                    Codigo = codigo,
                    ClienteId = bonoDto.ClienteId,
                    ProcedimientoId = bonoDto.ProcedimientoId,
                    TipoDescuento = "Porcentaje",
                    ValorDescuento = bonoDto.ValorDescuento,
                    FechaCreacion = DateTime.Now,
                    FechaExpiracion = DateTime.Now.AddDays(30), // 30 días de vigencia
                    Usado = false,
                    FechaUso = null
                };

                var bonoId = await _bonoRepository.CreateAsync(nuevoBono);

                // 6. Registrar en el historial
                int usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                await _historialBonoRepository.RegistrarAccionAsync(bonoId, "CREADO", usuarioId);

                // 7. Recuperar el bono creado para devolverlo
                var bonoCreado = await _bonoRepository.GetByIdAsync(bonoId);

                return Ok(new { estado = true, id = codigo, Mensaje = "Bono creado correctamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new {estado=false,mensaje= "Error interno del servidor" });
            }
        }

        // POST: /Bonos/AplicarBono
        [HttpPost("AplicarBono")]
        [Authorize(Roles = "Admin,Recepcion")] // Administradores y recepcionistas pueden aplicar bonos
        public async Task<ActionResult> AplicarBono(BonoAplicacionDto bonoDto)
        {
            try
            {
                // 1. Verificar que el bono exista
                var bono = await _bonoRepository.GetByIdAsync(bonoDto.BonoId);
                if (bono == null)
                {
                    return NotFound(new { estado = false, mensaje = $"Bono con ID {bonoDto.BonoId} no encontrado" });
                }

                // 2. Verificar que el bono no esté usado
                if (bono.Usado)
                {
                    return BadRequest(new { estado = false, mensaje = "El bono ya ha sido utilizado" });
                }

                // 3. Verificar que el bono esté dentro de la vigencia
                var fechaActual = DateTime.Now;
                if (fechaActual < bono.FechaCreacion || fechaActual > bono.FechaExpiracion)
                {
                    return BadRequest(new { estado = false, mensaje = "El bono está fuera de su período de vigencia" });
                }

                // 4. Verificar que el bono corresponda al procedimiento que se está pagando
                if (bono.ProcedimientoId != bonoDto.ProcedimientoId)
                {
                    return BadRequest(new { estado = false, mensaje = "El bono no corresponde al procedimiento especificado" });
                }

                // 5. Obtener el procedimiento para calcular el descuento
                var procedimiento = await _procedimientoRepository.GetByIdAsync(bonoDto.ProcedimientoId);
                if (procedimiento == null)
                {
                    return BadRequest(new { estado = false, mensaje = $"El procedimiento con ID {bonoDto.ProcedimientoId} no existe" });
                }

                // 6. Calcular el precio con descuento
                decimal precioFinal = 0;
                if (bono.TipoDescuento == "Porcentaje")
                {
                    precioFinal = procedimiento.Precio - (procedimiento.Precio * bono.ValorDescuento / 100);
                }
                else // Monto fijo
                {
                    precioFinal = procedimiento.Precio - bono.ValorDescuento;
                    if (precioFinal < 0) precioFinal = 0;
                }

                // 7. Marcar el bono como usado
                await _bonoRepository.MarcarComoUsadoAsync(bono.BonoId);

                // 8. Registrar en el historial
                int usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                await _historialBonoRepository.RegistrarAccionAsync(bono.BonoId, "USADO", usuarioId);

                return Ok(new { 
                    estado = true, 
                    mensaje = "Bono aplicado correctamente", 
                    precioOriginal = procedimiento.Precio, 
                    descuento = bono.ValorDescuento, 
                    tipoDescuento = bono.TipoDescuento,
                    precioFinal = precioFinal 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { estado = false, mensaje = $"Error interno del servidor: {ex.Message}" } );
            }
        }

        // POST: /Bonos/RevertirBono
        [HttpPost("RevertirBono")]
        [Authorize(Roles = "Admin")] // Solo administradores pueden revertir bonos
        public async Task<ActionResult> RevertirBono(BonoReversionDto bonoDto)
        {
            try
            {
                // 1. Verificar que el bono exista
                var bono = await _bonoRepository.GetByIdAsync(bonoDto.BonoId);
                if (bono == null)
                {
                    return NotFound(new { estado = false, mensaje = $"Bono con ID {bonoDto.BonoId} no encontrado" });
                }

                // 2. Verificar que el bono esté usado
                if (!bono.Usado)
                {
                    return BadRequest(new { estado = false, mensaje = "El bono no ha sido utilizado, no se puede revertir" });
                }

                // 3. Revertir el uso del bono
                await _bonoRepository.RevertirUsoAsync(bono.BonoId);

                // 4. Registrar en el historial
                int usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                await _historialBonoRepository.RegistrarAccionAsync(bono.BonoId, "REVERTIDO", usuarioId);

                return Ok(new { estado = true, mensaje = "Uso del bono revertido correctamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { estado = false, mensaje = $"Error interno del servidor: {ex.Message}" });
            }
        }

        // GET: /Bonos/BonosCliente/{clienteId}
        [HttpPost("BonosCliente")]
        [Authorize(Roles = "Admin,Recepcion")] // Administradores y recepcionistas pueden ver bonos de clientes
        public async Task<ActionResult<IEnumerable<Bono>>> GetBonosCliente(BonosxCliente clienteBono)
        {
            try
            {
                // Verificar que el cliente exista
                var cliente = await _clienteRepository.GetByIdAsync(clienteBono.idCliente);
                if (cliente == null)
                {
                    return NotFound($"Cliente con ID {clienteBono.idCliente} no encontrado");
                }

                var bonos = await _bonoRepository.GetBonosActivosClienteAsync(clienteBono.idCliente);
                return Ok(bonos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        
        [HttpPost("HistorialBono")]
        [Authorize(Roles = "Admin")] // Solo administradores pueden ver el historial de bonos
        public async Task<ActionResult<IEnumerable<HistorialBono>>> GetHistorialBono(BonoxId bonos)
        {
            try
            {
                // Verificar que el bono exista
                var bono = await _bonoRepository.GetByIdAsync(bonos.idBono);
                if (bono == null)
                {
                    return NotFound($"Bono con ID {bonos.idBono} no encontrado");
                }

                var historial = await _historialBonoRepository.GetHistorialBonoAsync(bonos.idBono);
                return Ok(historial);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }
    }
}