using System.Data;

namespace TorchEconomy.Data
{
    public interface IConnectionFactory
    {
        void Setup();
        IDbConnection Open();
    }
}
