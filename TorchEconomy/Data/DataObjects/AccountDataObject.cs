namespace TorchEconomy.Data.DataObjects
{
    public class AccountDataObject
    {
        public ulong Id { get; set; }
        public ulong PlayerIdentity { get; set; }
        public decimal Balance { get; set; }
        public bool IsNPC { get; set; }
        public bool IsPrimary { get; set; }
    }
}
