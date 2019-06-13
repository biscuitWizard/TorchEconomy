using System.Data;

namespace TorchEconomy.Data
{
    public interface IConnectionFactory
    {
        IDbConnection Open();
    }
}
