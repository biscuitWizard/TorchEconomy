using System.Data;
using System.IO;
using System.Reflection;
using Dapper;
using MySql.Data.MySqlClient;

namespace TorchEconomy.Data
{
    public class MysqlConnectionFactory : IConnectionFactory
    {
        public void Setup()
        {
            using (var connection = Open())
            {
                using (var stream = Assembly
                    .GetAssembly(typeof(EconomyPlugin))
                    .GetManifestResourceStream("TorchEconomy.Data.CreateTables.sql"))
                {
                    using (var reader = new StreamReader(stream))
                    {
                        string createTablesSql = reader.ReadToEnd();
                        connection.Execute(createTablesSql);
                    }
                }
            }
        }

        public IDbConnection Open()
        {
            var connection = new MySqlConnection(EconomyPlugin.Instance.Config.ConnectionString);
            connection.Open();
            
            return connection;
        }
    }
}
