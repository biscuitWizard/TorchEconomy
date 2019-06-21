using TorchEconomy.Data;
using TorchEconomy.Data.Schema;

namespace TorchEconomy.Markets.Data.DataObjects
{
	public class MarketOrderFluctuationDataObject : IDataObject
	{
		[PrimaryKey]
		public long Id { get; set; }
		
		public long MarketId { get; set; }
		
		[Required] 
		[StringLength(128)]
		public string DefinitionId { get; set; }
		
		[Required]
		public decimal PriceAdjustment { get; set; }
		
		[Required]
		public int LastUpdatedOn { get; set; }
		
		[Required]
		[DefaultValue(false)]
		public bool IsDeleted { get; set; }
	}
}