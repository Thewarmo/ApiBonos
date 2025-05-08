using System.Collections.Generic;
using System.Threading.Tasks;
using BonosEsteticaApi.Models;
using Dapper;

namespace BonosEsteticaApi.Data.Repositories
{
    public class ProcedimientoRepository : BaseRepository<Procedimiento>
    {
        public ProcedimientoRepository(DatabaseConnection dbConnection) : base(dbConnection)
        {
        }

        public async Task<Procedimiento> GetByNombreAsync(string nombre)
        {
            return await QueryFirstOrDefaultAsync("SELECT * FROM Procedimientos WHERE Nombre = @Nombre", new { Nombre = nombre });
        }

        public async Task<Procedimiento> GetByIdAsync(int id)
        {
            return await QueryFirstOrDefaultAsync("SELECT * FROM Procedimientos WHERE ProcedimientoId = @Id", new { Id = id });
        }

        public async Task<IEnumerable<Procedimiento>> GetAllAsync()
        {
            return await QueryAsync("SELECT * FROM Procedimientos");
        }

        public async Task<int> CreateAsync(Procedimiento procedimiento)
        {
            var sql = @"
                INSERT INTO Procedimientos (Nombre, Descripcion, Precio, Duracion, Activo)
                VALUES (@Nombre, @Descripcion, @Precio, @Duracion, @Activo);
                SELECT CAST(SCOPE_IDENTITY() as int)";

            return await ExecuteScalarAsync<int>(sql, procedimiento);
        }

        public async Task<bool> UpdateAsync(Procedimiento procedimiento)
        {
            var sql = @"
                UPDATE Procedimientos 
                SET Nombre = @Nombre, 
                    Descripcion = @Descripcion, 
                    Precio = @Precio, 
                    Duracion = @Duracion, 
                    Activo = @Activo
                WHERE ProcedimientoId = @ProcedimientoId";

            var affectedRows = await ExecuteAsync(sql, procedimiento);
            return affectedRows > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var sql = "UPDATE Procedimientos SET Activo = 0 WHERE ProcedimientoId = @Id";
            var affectedRows = await ExecuteAsync(sql, new { Id = id });
            return affectedRows > 0;
        }

        public async Task<bool> ActivarAsync(int id)
        {
            var sql = "UPDATE Procedimientos SET Activo = 1 WHERE ProcedimientoId = @Id";
            var affectedRows = await ExecuteAsync(sql, new { Id = id });
            return affectedRows > 0;
        }
    }
}