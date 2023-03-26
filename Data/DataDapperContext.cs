using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
namespace DotnetAPI.Data
{
    class DataContextDapper
    {
        private readonly IConfiguration _configuration;

        public DataContextDapper(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        private System.Data.IDbConnection Connection()
        {
            IDbConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            return connection;
        }
        public IEnumerable<T> LoadData<T>(string sqlCommand)
        {
            IDbConnection connection = Connection();
            return connection.Query<T>(sqlCommand);
        }
        public T LoadSingleData<T>(string sqlCommand)
        {
            IDbConnection connection = Connection();
            return connection.QuerySingle<T>(sqlCommand);
        }
        public bool ExecuteSqlWithParameters(string sqlCommand, DynamicParameters parameters)
        {
            IDbConnection connection = Connection();
            return connection.Execute(sqlCommand, parameters) > 0;
        }
        public IEnumerable<T> LoadDataWithParameters<T>(string sqlCommand, DynamicParameters parameters)
        {
            IDbConnection connection = Connection();
            return connection.Query<T>(sqlCommand, parameters);
        }

        public T LoadDataSingleWithParameters<T>(string sqlCommand, DynamicParameters parameters)
        {
            IDbConnection connection = Connection();
            return connection.QuerySingle<T>(sqlCommand, parameters);
        }

    }
}