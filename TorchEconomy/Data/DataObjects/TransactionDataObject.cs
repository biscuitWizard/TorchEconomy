namespace TorchEconomy.Data.DataObjects
{
    public class TransactionDataObject
    {
        public long Id { get; set; }
        public long ToAccountId { get; set; }
        public long FromAccountId { get; set; }
        public decimal TransactionAmount { get; set; }
        public double TransactedOn { get; set; }
        public string Reason { get; set; }
    }
}
