using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BonosEsteticaApi.Models;

namespace BonosEsteticaApi.Data.Repositories
{
    public class UsuarioRepository : BaseRepository<Usuario>
    {
        public UsuarioRepository(DatabaseConnection dbConnection) : base(dbConnection)
        {
        }

        public async Task<Usuario> GetByEmailAsync(string correo)
        {
            return await QueryFirstOrDefaultAsync("SELECT * FROM Usuarios WHERE Correo = @Correo", new { Correo = correo });
        }

        public async Task<Usuario> GetByIdAsync(int id)
        {
            return await QueryFirstOrDefaultAsync("SELECT * FROM Usuarios WHERE UsuarioId = @Id", new { Id = id });
        }

        public async Task<IEnumerable<Usuario>> GetAllAsync()
        {
            return await QueryAsync("SELECT * FROM Usuarios");
        }

        public async Task<int> CreateAsync(Usuario usuario)
        {
            var sql = @"
                INSERT INTO Usuarios (Nombre, Correo, ContraseñaHash,ContraseñaSalt, Rol, Activo)
                VALUES (@Nombre, @Correo, @ContraseñaHash,@ContraseñaSalt, @Rol, @Activo);
                SELECT CAST(SCOPE_IDENTITY() as int)";

            return await ExecuteScalarAsync<int>(sql, usuario);
        }

        public async Task<bool> UpdateAsync(Usuario usuario)
        {
            var sql = @"
                UPDATE Usuarios
                SET Nombre = @Nombre,
                    Correo = @Correo,
                    ContraseñaHash = @ContraseñaHash,
                    Rol = @Rol,
                    Activo = @Activo
                WHERE UsuarioId = @UsuarioId";

            var result = await ExecuteAsync(sql, usuario);
            return result > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var sql = "UPDATE Usuarios SET Activo = 0 WHERE UsuarioId = @Id";
            var result = await ExecuteAsync(sql, new { Id = id });
            return result > 0;
        }
    }
}