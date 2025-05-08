using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BonosEsteticaApi.Models;
using Dapper;
using System.Data;

namespace BonosEsteticaApi.Data.Repositories
{
    public class BonoRepository : BaseRepository<Bono>
    {
        private string TableName;
        private string PrimaryKeyName;
        public BonoRepository(DatabaseConnection dbConnection) : base(dbConnection)
        {
            TableName = "Bonos";
            PrimaryKeyName = "BonoId";
        }

        // Obtener todos los bonos
        public async Task<IEnumerable<Bono>> GetAllAsync()
        {
            try
            {
                return await QueryAsync($"SELECT * FROM {TableName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener todos los bonos: {ex.Message}");
                throw;
            }
        }

        // Obtener un bono por su ID
        public async Task<Bono> GetByIdAsync(int idBono)
        {
            try
            {
                return await QueryFirstOrDefaultAsync($"SELECT * FROM {TableName} WHERE BonoId = @idBono", new { idBono });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener bono por ID {idBono}: {ex.Message}");
                throw;
            }
        }

        // Crear un nuevo bono
        public async Task<int> CreateAsync(Bono bono)
        {
            try
            {
                if (await ExisteBonoActivoAsync(bono.ClienteId, bono.ProcedimientoId) == false)
                {
                    bono.Usado = false;
                }
                else
                {
                    bono.Usado = true;
                }

                if (bono.Usado != true)
                {
                    var sql = $@"
                INSERT INTO {TableName} 
                (Codigo, ClienteId, ProcedimientoId, TipoDescuento, ValorDescuento, FechaCreacion, FechaExpiracion, Usado)
                OUTPUT INSERTED.BonoId
                VALUES 
                (@Codigo, @ClienteId, @ProcedimientoId, @TipoDescuento, @ValorDescuento, @FechaCreacion, @FechaExpiracion, @Usado)";

                    return await ExecuteScalarAsync<int>(sql, bono); // Esto ahora devolverá el ID generado
                }
                else
                {
                    return -1; // Bono ya existe
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al crear bono para cliente {bono.ClienteId} y procedimiento {bono.ProcedimientoId}: {ex.Message}");
                throw;
            }
        }


        // Verificar si existe un bono activo para un cliente y procedimiento
        public async Task<bool> ExisteBonoActivoAsync(int clienteId, int procedimientoId)
        {
            try
            {
                var sql = @"
                    SELECT 1 FROM Bonos
                    WHERE ClienteId = @clienteId
                      AND ProcedimientoId = @procedimientoId
                      AND Usado = 0
                      AND GETDATE() BETWEEN FechaCreacion AND FechaExpiracion";

                var result = await ExecuteScalarAsync<int>(sql, new { clienteId, procedimientoId });
                return result > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al verificar bono activo para cliente {clienteId} y procedimiento {procedimientoId}: {ex.Message}");
                throw;
            }
        }

        // Obtener un bono por su código
        public async Task<Bono> GetByCodigoAsync(string codigo)
        {
            try
            {
                var sql = @$"SELECT 
                b.*,
                c.ClienteId, c.Nombre, c.Apellido, c.Correo, c.Telefono, c.FechaRegistro, c.Activo,
                p.ProcedimientoId, p.Nombre, p.Descripcion, p.Precio, p.Duracion, p.Activo
            FROM {TableName} b
            INNER JOIN Clientes c ON c.ClienteId = b.ClienteId 
            INNER JOIN Procedimientos p ON p.ProcedimientoId = b.ProcedimientoId
            WHERE b.Codigo = @codigo";
        
                Dictionary<int, Bono> bonoDict = new Dictionary<int, Bono>();
        
                using (var connection = _dbConnection.CreateConnection())
                {
                    var bonos = await connection.QueryAsync<Bono, Cliente, Procedimiento, Bono>(
                        sql,
                        (bono, cliente, procedimiento) => {
                            if (!bonoDict.TryGetValue(bono.BonoId, out var bonoEntry))
                            {
                                bonoEntry = bono;
                                bonoEntry.Cliente = cliente;
                                bonoEntry.Procedimiento = procedimiento;
                                bonoDict.Add(bono.BonoId, bonoEntry);
                            }
                            return bonoEntry;
                        },
                        new { codigo },
                        splitOn: "ClienteId,ProcedimientoId"
                    );
                    
                    return bonos.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener bono por código {codigo}: {ex.Message}");
                throw;
            }
        }

        // Marcar un bono como usado
        public async Task<bool> MarcarComoUsadoAsync(int bonoId)
        {
            try
            {
                var sql = $"UPDATE {TableName} SET Usado = 1, FechaUso = GETDATE() WHERE {PrimaryKeyName} = @bonoId";
                var affectedRows = await ExecuteAsync(sql, new { bonoId });
                return affectedRows > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al marcar bono {bonoId} como usado: {ex.Message}");
                throw;
            }
        }

        // Revertir el uso de un bono
        public async Task<bool> RevertirUsoAsync(int bonoId)
        {
            try
            {
                var sql = $"UPDATE {TableName} SET Usado = 0, FechaUso = NULL WHERE {PrimaryKeyName} = @bonoId";
                var result = await ExecuteAsync(sql, new { bonoId });
                return result > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al revertir uso del bono {bonoId}: {ex.Message}");
                throw;
            }
        }

        // Obtener bonos activos de un cliente
        public async Task<IEnumerable<Bono>> GetBonosActivosClienteAsync(int clienteId)
        {
            try
            {
                var sql = @"
                    SELECT 
                            b.*,
                            c.ClienteId, c.Nombre, c.Apellido, c.Correo, c.Telefono, c.FechaRegistro, c.Activo,
                            p.ProcedimientoId, p.Nombre, p.Descripcion, p.Precio, p.Duracion, p.Activo
                    FROM Bonos b
                    INNER JOIN Clientes c ON c.ClienteId = b.ClienteId
                    INNER JOIN Procedimientos p ON p.ProcedimientoId = b.ProcedimientoId
                    WHERE b.ClienteId = @clienteId
                      AND b.Usado = 0
                      AND GETDATE() BETWEEN b.FechaCreacion AND b.FechaExpiracion";

                Dictionary<int, Bono> bonoDict = new Dictionary<int, Bono>();

                using (var connection = _dbConnection.CreateConnection())
                {
                    var bonos = await connection.QueryAsync<Bono, Cliente, Procedimiento, Bono>(
                        sql,
                        (bono, cliente, procedimiento) => {
                            if (!bonoDict.TryGetValue(bono.BonoId, out var bonoEntry))
                            {
                                bonoEntry = bono;
                                bonoEntry.Cliente = cliente;
                                bonoEntry.Procedimiento = procedimiento;
                                bonoDict.Add(bono.BonoId, bonoEntry);
                            }
                            return bonoEntry;
                        },
                        new { clienteId },
                        splitOn: "ClienteId,ProcedimientoId"
                    );

                    return bonos.Distinct();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener bonos activos del cliente {clienteId}: {ex.Message}");
                throw;
            }
        }
    }
}