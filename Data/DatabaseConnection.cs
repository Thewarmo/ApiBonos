using System;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace BonosEsteticaApi.Data
{
    public class DatabaseConnection
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public DatabaseConnection(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
        }

        public IDbConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}