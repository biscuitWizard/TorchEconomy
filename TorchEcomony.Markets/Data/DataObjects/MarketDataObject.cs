namespace TorchEconomy.Markets.Data.DataObjects
{
    public class MarketDataObject
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public float Range { get; set; }
        public long ParentGridId { get; set; }
        public long? AccountId { get; set; }
        public bool IsDeleted { get; set; }
    }
}
