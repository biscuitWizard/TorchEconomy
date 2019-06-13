using ProtoBuf;

namespace TorchEconomy.Messages.Accounts
{
	[ProtoContract]
	public class AdjustBalanceMessage : BaseMessage
	{
		public override MessageTypeEnum MessageType => MessageTypeEnum.AdjustBalance;
		
		/// <summary>
		/// Target account ID to modify.
		/// </summary>
		public ulong AccountId { get; set; }
		/// <summary>
		/// The amount to adjust by.
		/// </summary>
		public decimal Amount { get; set; }
		/// <summary>
		/// Reason for adjusting balance.
		/// </summary>
		public string Reason { get; set; }
		
		/// <summary>
		/// Optional field. If specified, will also deduct from the specified accountID and
		/// add their ID to the transaction log.
		/// </summary>
		public ulong? FromAccountId { get; set; }
	}

	[ProtoContract]
	public class AdjustBalanceResponseMessage : BaseResponseMessage
	{
		public override MessageTypeEnum MessageType => MessageTypeEnum.AdjustBalance;
	}
}