using System.Data;

namespace TorchEconomySE.Data
{
    public interface IConnectionFactory
    {
        IDbConnection Open();
    }
}
