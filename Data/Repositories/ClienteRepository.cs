using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BonosEsteticaApi.Models;
using Dapper;

namespace BonosEsteticaApi.Data.Repositories
{
    public class ClienteRepository : BaseRepository<Cliente>
    {
        public ClienteRepository(DatabaseConnection dbConnection) : base(dbConnection)
        {
        }

        public async Task<Cliente> GetByEmailAsync(string correo)
        {
            return await QueryFirstOrDefaultAsync("SELECT * FROM Clientes WHERE Correo = @Correo", new { Correo = correo });
        }

        public async Task<Cliente> GetByTelefonoAsync(string telefono)
        {
            return await QueryFirstOrDefaultAsync("SELECT * FROM Clientes WHERE Telefono = @Telefono", new { Telefono = telefono });
        }

        public async Task<Cliente> GetByIdAsync(int id)
        {
            return await QueryFirstOrDefaultAsync("SELECT * FROM Clientes WHERE ClienteId = @Id", new { Id = id });
        }

        public async Task<IEnumerable<Cliente>> GetAllAsync()
        {
            return await QueryAsync("SELECT * FROM Clientes");
        }

        public async Task<int> CreateAsync(Cliente cliente)
        {
            var sql = @"
                INSERT INTO Clientes (Nombre, Apellido, Correo, Telefono, FechaRegistro,Activo)
                VALUES (@Nombre, @Apellido, @Correo, @Telefono, @FechaRegistro, 1);
                SELECT CAST(SCOPE_IDENTITY() as int)";

            return await ExecuteScalarAsync<int>(sql, cliente);
        }

        public async Task<bool> UpdateAsync(Cliente cliente)
        {
            var sql = @"
                UPDATE Clientes 
                SET Nombre = @Nombre, 
                    Apellido = @Apellido, 
                    Correo = @Correo, 
                    Telefono = @Telefono, 
                    FechaRegistro = @FechaRegistro, 
                    Activo = @Activo
                WHERE ClienteId = @ClienteId";

            var affectedRows = await ExecuteAsync(sql, cliente);
            return affectedRows > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var sql = "UPDATE Clientes SET Activo = 0 WHERE ClienteId = @Id";
            var affectedRows = await ExecuteAsync(sql, new { Id = id });
            return affectedRows > 0;
        }

        public async Task<bool> ActivateAsync(int id)
        {
            var sql = "UPDATE Clientes SET Activo = 1 WHERE ClienteId = @Id";
            var affectedRows = await ExecuteAsync(sql, new { Id = id });
            return affectedRows > 0;
        }
    }
}