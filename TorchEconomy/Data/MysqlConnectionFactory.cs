using System.Data;
using MySql.Data.MySqlClient;

namespace TorchEconomy.Data
{
    public class MysqlConnectionFactory : IConnectionFactory
    {
        public IDbConnection Open()
        {
            var connection = new MySqlConnection(EconomyPlugin.Instance.Config.ConnectionString);
            connection.Open();
            
            return connection;
        }
    }
}
