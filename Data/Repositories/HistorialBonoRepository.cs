using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BonosEsteticaApi.Models;
using Dapper;
using System.Data;

namespace BonosEsteticaApi.Data.Repositories
{
    public class HistorialBonoRepository : BaseRepository<HistorialBono>
    {
        private string TableName;
        private string PrimaryKeyName;
        public HistorialBonoRepository(DatabaseConnection dbConnection) : base(dbConnection)
        {
            TableName = "HistorialBonos";
            PrimaryKeyName = "HistorialId";
        }

        // Registrar una acción en el historial
        public async Task<int> RegistrarAccionAsync(int bonoId, string accion, int usuarioId)
        {
            var historial = new HistorialBono
            {
                BonoId = bonoId,
                Accion = accion,
                Fecha = DateTime.Now,
                UsuarioId = usuarioId
            };

            var sql = $@"
                INSERT INTO {TableName} (BonoId, Accion, Fecha, UsuarioId)
                VALUES (@BonoId, @Accion, @Fecha, @UsuarioId)";

            return await ExecuteScalarAsync<int>(sql, historial);
        }

        // Obtener historial de un bono
        public async Task<IEnumerable<HistorialBono>> GetHistorialBonoAsync(int bonoId)
        {
            var sql = @"
                SELECT h.*, u.Nombre as UsuarioNombre
                FROM HistorialBonos h
                INNER JOIN Usuarios u ON h.UsuarioId = u.UsuarioId
                WHERE h.BonoId = @bonoId
                ORDER BY h.Fecha DESC";

            return await QueryAsync(sql, new { bonoId });
        }
    }
}