namespace TorchEconomy.Data.DataObjects
{
    public class TransactionDataObject
    {
        public ulong Id { get; set; }
        public ulong ToAccountId { get; set; }
        public ulong FromAccountId { get; set; }
        public decimal TransactionAmount { get; set; }
        public double TransactedOn { get; set; }
        public string Reason { get; set; }
    }
}
