using System.Data;
using MySql.Data.MySqlClient;

namespace TorchEconomy.Data
{
    public class MysqlConnectionFactory : IConnectionFactory
    {
        public IDbConnection Open()
        {
            return new MySqlConnection("Server=localhost;Database=space_engineers;Uid=root;Pwd=password;");
        }
    }
}
