using TorchEconomy.Data.DataObjects;
using TorchEconomy.Data.Types;
using TorchEconomy.Markets.Data.DataObjects;
using TorchEconomy.Markets.Data.Models;

namespace TorchEconomy.Markets.Managers.Generators
{
	public interface IOrderGenerator
	{
		bool CanHandle(IndustryTypeEnum industryType);
		NPCMarketOrder[] GenerateOrders(IndustryTypeEnum industryType, NPCDataObject npc, MarketDataObject market);
	}
}