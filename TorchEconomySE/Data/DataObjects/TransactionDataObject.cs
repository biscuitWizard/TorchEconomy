namespace TorchEconomySE.Models
{
    public class TransactionDataObject
    {
        public ulong Id { get; set; }
        public ulong FromPlayerIdentity { get; set; }
        public ulong ToPlayerIdentity { get; set; }
        public decimal TransactionAmount { get; set; }
        public double TransactedOn { get; set; }
    }
}
