using Dapper;
using Microsoft.Data.SqlClient;
using Sepes.Infrastructure.Service.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sepes.Infrastructure.Service.DataModelService
{
    public class DapperQueryService : IDapperQueryService
    {       
        readonly string _dbConnectionString;

        public DapperQueryService(IDatabaseConnectionStringProvider databaseConnectionStringProvider)          
        {           
            _dbConnectionString = databaseConnectionStringProvider.GetConnectionString();
        }

        public async Task<IEnumerable<T>> RunDapperQueryMultiple<T>(string query, object parameters = null)
        {
            using (var connection = new SqlConnection(_dbConnectionString))
            {
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    await connection.OpenAsync();
                }

                return await connection.QueryAsync<T>(query, parameters);
            }
        }

        public async Task<T> RunDapperQuerySingleAsync<T>(string query, object parameters = null)
        {
            using (var connection = new SqlConnection(_dbConnectionString))
            {
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    await connection.OpenAsync();
                }

                return await connection.QuerySingleOrDefaultAsync<T>(query, parameters);
            }
        }

        public async Task ExecuteAsync(string statement, object parameters = null)
        {
            using (var connection = new SqlConnection(_dbConnectionString))
            {
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    await connection.OpenAsync();
                }

                await connection.ExecuteAsync(statement, parameters);
            }
        }       
    }
}

