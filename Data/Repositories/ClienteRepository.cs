using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BonosEsteticaApi.Models;

namespace BonosEsteticaApi.Data.Repositories
{
    public class ClienteRepository : BaseRepository<Cliente>
    {
        public ClienteRepository(DatabaseConnection dbConnection) : base(dbConnection)
        {
        }

        public async Task<IEnumerable<Cliente>> GetAllAsync()
        {
            return await QueryAsync("SELECT * FROM Clientes");
        }

        public async Task<Cliente> GetByIdAsync(int id)
        {
            return await QueryFirstOrDefaultAsync("SELECT * FROM Clientes WHERE ClienteId = @Id", new { Id = id });
        }

        public async Task<int> CreateAsync(Cliente cliente)
        {
            var sql = @"
                INSERT INTO Clientes (Nombre, Apellido, Correo, Telefono, FechaRegistro)
                VALUES (@Nombre, @Apellido, @Correo, @Telefono, @FechaRegistro);
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
                    Telefono = @Telefono
                WHERE ClienteId = @ClienteId";

            var result = await ExecuteAsync(sql, cliente);
            return result > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var sql = "DELETE FROM Clientes WHERE ClienteId = @Id";
            var result = await ExecuteAsync(sql, new { Id = id });
            return result > 0;
        }
    }
}