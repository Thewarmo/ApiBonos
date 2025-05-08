using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BonosEsteticaApi.Models;
using BonosEsteticaApi.Data.Repositories;
using BonosEsteticaApi.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BonosEsteticaApi.Controllers
{
    [Route("/[controller]")]
    [ApiController]
    [Authorize] // Requiere autenticación para todas las acciones
    public class ProcedimientosController : ControllerBase
    {
        private readonly ProcedimientoRepository _procedimientoRepository;

        public ProcedimientosController(ProcedimientoRepository procedimientoRepository)
        {
            _procedimientoRepository = procedimientoRepository;
        }

        // GET: /Procedimientos
        [HttpGet]
        [Authorize(Roles = "Admin,Recepcion")] // Administradores y recepcionistas pueden ver todos los procedimientos
        public async Task<ActionResult<IEnumerable<Procedimiento>>> GetProcedimientos()
        {
            try
            {
                var procedimientos = await _procedimientoRepository.GetAllAsync();
                return Ok(procedimientos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpPost("Procedimiento")]
        [Authorize(Roles = "Admin,Recepcion")] // Administradores y recepcionistas pueden ver detalles de procedimientos
        public async Task<ActionResult<Procedimiento>> GetProcedimiento(ProcedimientoxId proc)
        {
            try
            {
                var procedimiento = await _procedimientoRepository.GetByIdAsync(proc.idProcedimiento);

                if (procedimiento == null)
                {
                    return NotFound($"Procedimiento con ID {proc.idProcedimiento} no encontrado");
                }

                return Ok(procedimiento);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpPost("CrearProcedimiento")]
        [Authorize(Roles = "Admin")] // Solo administradores pueden crear procedimientos
        public async Task<ActionResult<Procedimiento>> CreateProcedimiento(Procedimiento procedimientoDto)
        {
            try
            {
                // Validar que el nombre no exista ya
                var procedimientoExistente = await _procedimientoRepository.GetByNombreAsync(procedimientoDto.Nombre);
                if (procedimientoExistente != null)
                {
                    return BadRequest($"Ya existe un procedimiento con el nombre {procedimientoDto.Nombre}");
                }

                var nuevoProcedimiento = new Procedimiento
                {
                    Nombre = procedimientoDto.Nombre,
                    Descripcion = procedimientoDto.Descripcion,
                    Precio = procedimientoDto.Precio,
                    Duracion = procedimientoDto.Duracion,
                    Activo = true
                };

                var procedimientoId = await _procedimientoRepository.CreateAsync(nuevoProcedimiento);
                
                // Recuperar el procedimiento creado para devolverlo
                var procedimientoCreado = await _procedimientoRepository.GetByIdAsync(procedimientoId);
                
                return Ok(new { estado = true, id = procedimientoId, Mensaje = "Procedimiento creado correctamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { estado = false, Mensaje = $"Error interno del servidor:" });
            }
        }

        [HttpPost("ActualizarProcedimiento")]
        [Authorize(Roles = "Admin")] // Solo administradores pueden actualizar procedimientos
        public async Task<IActionResult> UpdateProcedimiento(ProcedimientoActualizacion procedimientoDto)
        {
            try
            {
                var procedimiento = await _procedimientoRepository.GetByIdAsync(procedimientoDto.ProcedimientoId);
                if (procedimiento == null)
                {
                    return NotFound($"Procedimiento con ID {procedimientoDto.ProcedimientoId} no encontrado");
                }

                // Si se está actualizando el nombre, verificar que no exista ya
                if (procedimiento.Nombre != procedimientoDto.Nombre)
                {
                    var procedimientoExistente = await _procedimientoRepository.GetByNombreAsync(procedimientoDto.Nombre);
                    if (procedimientoExistente != null && procedimientoExistente.ProcedimientoId != procedimientoDto.ProcedimientoId)
                    {
                        return BadRequest(new { estado = false, Mensaje = $"Ya existe un procedimiento con el nombre {procedimientoDto.Nombre}" });
                    }
                }

                // Actualizar los campos del procedimiento
                procedimiento.Nombre = procedimientoDto.Nombre;
                procedimiento.Descripcion = procedimientoDto.Descripcion;
                procedimiento.Precio = procedimientoDto.Precio;
                procedimiento.Duracion = procedimientoDto.Duracion;
                procedimiento.Activo = procedimientoDto.Activo;

                var resultado = await _procedimientoRepository.UpdateAsync(procedimiento);
                if (resultado)
                {
                    return Ok(new {estado=true, Mensaje="Actualización de procedimiento realizada"});
                }
                else
                {
                    return StatusCode(500, new { estado = false, Mensaje = "No se pudo actualizar el procedimiento" } );
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new {estado=false,Mensaje= $"Error interno del servidor"});
            }
        }

        [HttpPost("EliminarProcedimiento")]
        [Authorize(Roles = "Admin")] // Solo administradores pueden eliminar procedimientos
        public async Task<IActionResult> DeleteProcedimiento(ProcedimientoxId procedimientoEliminar)
        {
            try
            {
                var procedimiento = await _procedimientoRepository.GetByIdAsync(procedimientoEliminar.idProcedimiento);
                if (procedimiento == null)
                {
                    return NotFound(new { estado = false, Mensaje = $"Procedimiento con ID {procedimientoEliminar.idProcedimiento} no encontrado" });
                }

                var resultado = await _procedimientoRepository.DeleteAsync(procedimientoEliminar.idProcedimiento);
                if (resultado)
                {
                    return Ok(new { estado = true, Mensaje = "Eliminación de procedimiento realizada" });
                }
                else
                {
                    return StatusCode(500, new { estado = false, Mensaje = "No se pudo eliminar el procedimiento" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { estado = false, Mensaje = "Error interno del servidor" });
            }
        }

        [HttpPost("ActivarProcedimiento")]
        [Authorize(Roles = "Admin")] // Solo administradores pueden eliminar procedimientos
        public async Task<IActionResult> ActivarProcedimiento(ProcedimientoxId procedimientoEliminar)
        {
            try
            {
                var procedimiento = await _procedimientoRepository.GetByIdAsync(procedimientoEliminar.idProcedimiento);
                if (procedimiento == null)
                {
                    return NotFound(new {estado=false,Mensaje = $"Procedimiento con ID {procedimientoEliminar.idProcedimiento} no encontrado" });
                }

                var resultado = await _procedimientoRepository.ActivarAsync(procedimientoEliminar.idProcedimiento);
                if (resultado)
                {
                    return Ok(new { estado = true, Mensaje = "Activacion de procedimiento realizada" });
                }
                else
                {
                    return StatusCode(500, new { estado = false, Mensaje = "No se pudo Activacion el procedimiento" } );
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { estado = false, Mensaje = $"Error interno del servidor:"});
            }
        }
    }
}