using MySql.Data.MySqlClient;
using System.Data;

namespace TorchEconomySE.Data
{
    public class MysqlConnectionFactory : IConnectionFactory
    {
        public IDbConnection Open()
        {
            return new MySqlConnection("Server=localhost;Database=space_engineers;Uid=root;Pwd=password;");
        }
    }
}
