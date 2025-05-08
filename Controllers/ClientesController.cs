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
    public class ClientesController : ControllerBase
    {
        private readonly ClienteRepository _clienteRepository;

        public ClientesController(ClienteRepository clienteRepository)
        {
            _clienteRepository = clienteRepository;
        }

        // GET: /Clientes
        [HttpGet]
        [Authorize(Roles = "Admin,Recepcion")] // Administradores y recepcionistas pueden ver todos los clientes
        public async Task<ActionResult<IEnumerable<Cliente>>> GetClientes()
        {
            try
            {
                var clientes = await _clienteRepository.GetAllAsync();
                return Ok(clientes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpPost("Cliente")]
        [Authorize(Roles = "Admin,Recepcion")] // Administradores y recepcionistas pueden ver detalles de clientes
        public async Task<ActionResult<Cliente>> GetCliente(ClientexId cliente)
        {
            try
            {
                var clienteEncontrado = await _clienteRepository.GetByIdAsync(cliente.idCliente);

                if (clienteEncontrado == null)
                {
                    return NotFound($"Cliente con ID {cliente.idCliente} no encontrado");
                }

                return Ok(clienteEncontrado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpPost("CrearCliente")]
        [Authorize(Roles = "Admin,Recepcion")] // Administradores y recepcionistas pueden crear clientes
        public async Task<ActionResult<Cliente>> CreateCliente(ClienteCreacionDto clienteDto)
        {
            try
            {
                // Validar que el correo no exista ya
                var clienteExistente = await _clienteRepository.GetByEmailAsync(clienteDto.Correo);
                if (clienteExistente != null)
                {
                    return BadRequest($"Ya existe un cliente con el correo {clienteDto.Correo}");
                }

                // Validar que el teléfono no exista ya
                var clienteExistenteTelefono = await _clienteRepository.GetByTelefonoAsync(clienteDto.Telefono);
                if (clienteExistenteTelefono != null)
                {
                    return BadRequest($"Ya existe un cliente con el teléfono {clienteDto.Telefono}");
                }

                var nuevoCliente = new Cliente
                {
                    Nombre = clienteDto.Nombre,
                    Apellido = clienteDto.Apellido,
                    Correo = clienteDto.Correo,
                    Telefono = clienteDto.Telefono,
                    FechaRegistro = clienteDto.FechaRegistro,
                    Activo = true
                };

                var clienteId = await _clienteRepository.CreateAsync(nuevoCliente);
                
                // Recuperar el cliente creado para devolverlo
                var clienteCreado = await _clienteRepository.GetByIdAsync(clienteId);
                
                return Ok(new { estado=true,mensaje="Cliente creado correctamente",id = clienteId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpPost("ActualizarCliente")]
        [Authorize(Roles = "Admin,Recepcion")] // Administradores y recepcionistas pueden actualizar clientes
        public async Task<IActionResult> UpdateCliente(ClienteActualizacion clienteDto)
        {
            try
            {
                var cliente = await _clienteRepository.GetByIdAsync(clienteDto.ClienteId);
                if (cliente == null)
                {
                    return NotFound($"Cliente con ID {clienteDto.ClienteId} no encontrado");
                }

                // Si se está actualizando el correo, verificar que no exista ya
                if (cliente.Correo != clienteDto.Correo)
                {
                    var clienteExistente = await _clienteRepository.GetByEmailAsync(clienteDto.Correo);
                    if (clienteExistente != null && clienteExistente.ClienteId != clienteDto.ClienteId)
                    {
                        return BadRequest($"Ya existe un cliente con el correo {clienteDto.Correo}");
                    }
                }

                // Si se está actualizando el teléfono, verificar que no exista ya
                if (cliente.Telefono != clienteDto.Telefono)
                {
                    var clienteExistente = await _clienteRepository.GetByTelefonoAsync(clienteDto.Telefono);
                    if (clienteExistente != null && clienteExistente.ClienteId != clienteDto.ClienteId)
                    {
                        return BadRequest($"Ya existe un cliente con el teléfono {clienteDto.Telefono}");
                    }
                }

                // Actualizar los campos del cliente
                cliente.Nombre = clienteDto.Nombre;
                cliente.Apellido = clienteDto.Apellido;
                cliente.Correo = clienteDto.Correo;
                cliente.Telefono = clienteDto.Telefono;
                cliente.FechaRegistro = clienteDto.FechaRegistro;
                cliente.Activo = clienteDto.Activo;

                var resultado = await _clienteRepository.UpdateAsync(cliente);
                if (resultado)
                {
                    return Ok(new {estado=true, Mensaje="Actualización de cliente realizada"});
                }
                else
                {
                    return StatusCode(500, "No se pudo actualizar el cliente");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpPost("EliminarCliente")]
        [Authorize(Roles = "Admin")] // Solo administradores pueden eliminar clientes
        public async Task<IActionResult> DeleteCliente(ClientexId clienteEliminar)
        {
            try
            {
                var cliente = await _clienteRepository.GetByIdAsync(clienteEliminar.idCliente);
                if (cliente == null)
                {
                    return NotFound(new {estado = false, Mensaje =$"Cliente con ID {clienteEliminar.idCliente} no encontrado" });
                }

                var resultado = await _clienteRepository.DeleteAsync(clienteEliminar.idCliente);
                if (resultado)
                {
                    return Ok(new { estado = true, Mensaje = "Eliminación de cliente realizada" });
                }
                else
                {
                    return StatusCode(500,new { estado=false, Mensaje = "No se pudo eliminar el cliente" } );
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { estado = false, Mensaje = $"Error interno del servidor" });
            }
        }

        [HttpPost("ActivarCliente")]
        [Authorize(Roles = "Admin")] // Solo administradores pueden eliminar clientes
        public async Task<IActionResult> ActivarCliente(ClientexId clienteEliminar)
        {
            try
            {
                var cliente = await _clienteRepository.GetByIdAsync(clienteEliminar.idCliente);
                if (cliente == null)
                {
                    return NotFound(new { estado = false, Mensaje = $"Cliente con ID {clienteEliminar.idCliente} no encontrado" });
                }

                var resultado = await _clienteRepository.ActivateAsync(clienteEliminar.idCliente);
                if (resultado)
                {
                    return Ok(new { estado = true, Mensaje = "Activacion de cliente realizada" });
                }
                else
                {
                    return StatusCode(500, new { estado = false, Mensaje = "No se pudo activar el cliente" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { estado = false, Mensaje = $"Error interno del servidor" });
            }
        }
    }
}