using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;

namespace BonosEsteticaApi.Data.Repositories
{
    public abstract class BaseRepository<T> where T : class
    {
        protected readonly DatabaseConnection _dbConnection;

        protected BaseRepository(DatabaseConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        protected async Task<IEnumerable<T>> QueryAsync(string sql, object param = null)
        {
            using (var connection = _dbConnection.CreateConnection())
            {
                return await connection.QueryAsync<T>(sql, param);
            }
        }

        protected async Task<T> QueryFirstOrDefaultAsync(string sql, object param = null)
        {
            using (var connection = _dbConnection.CreateConnection())
            {
                return await connection.QueryFirstOrDefaultAsync<T>(sql, param);
            }
        }


        protected async Task<int> ExecuteAsync(string sql, object param = null)
        {
            using (var connection = _dbConnection.CreateConnection())
            {
                return await connection.ExecuteAsync(sql, param);
            }
        }

        protected async Task<T> ExecuteScalarAsync<T>(string sql, object param = null)
        {
            using (var connection = _dbConnection.CreateConnection())
            {
                return await connection.ExecuteScalarAsync<T>(sql, param);
            }
        }
    }
}