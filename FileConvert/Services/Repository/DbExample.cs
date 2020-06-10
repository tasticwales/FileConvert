using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace FileConvert.Services
{
    public interface IDbExample
    {
        Task<string[]> GetFromDB(string connectionString, string readCommand);
    }


    public class DbExample : IDbExample
    {

        private IConfiguration _config { get; set; }

        public DbExample(IConfiguration config)
        {
            _config = config;
        }

        public async Task<string[]> GetFromDB(string connectionString, string readCommand)
        {
            List<string> sourceToConvert = new List<string>();

            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand
                {
                    CommandType = System.Data.CommandType.StoredProcedure,
                    CommandText = readCommand,
                    Connection = new SqlConnection(connectionString)
                };

                await connection.OpenAsync();

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        sourceToConvert.Add(reader["field"].ToString());
                    }
                }
            }

            return sourceToConvert.ToArray();
        }
    }
}
