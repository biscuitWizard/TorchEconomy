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
		[ProtoMember(201)]
		public long AccountId { get; set; }
		/// <summary>
		/// The amount to adjust by.
		/// </summary>
		[ProtoMember(202)]
		public decimal Amount { get; set; }
		/// <summary>
		/// Reason for adjusting balance.
		/// </summary>
		[ProtoMember(203)]
		public string Reason { get; set; }
		
		/// <summary>
		/// Optional field. If specified, will also deduct from the specified accountID and
		/// add their ID to the transaction log.
		/// </summary>
		[ProtoMember(204)]
		public long? FromAccountId { get; set; }
	}

	[ProtoContract]
	public class AdjustBalanceResponseMessage : BaseResponseMessage
	{
		public override MessageTypeEnum MessageType => MessageTypeEnum.AdjustBalance;
	}
}