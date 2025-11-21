using Microsoft.Data.SqlClient;
using System.Data;

namespace BestieToy.Data
{
    public class DBContext
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public DBContext(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("BestieToyConnection") ??
                              "Server=localhost;Database=BestieToy;Trusted_Connection=true;TrustServerCertificate=true;";
        }

        public IDbConnection CreateConnection() => new SqlConnection(_connectionString);
    }
}