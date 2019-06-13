using ProtoBuf;
using TorchEconomy.Data.DataObjects;

namespace TorchEconomy.Messages.Accounts
{
	[ProtoContract]
	public class GetAccountsMessage : BaseMessage
	{
		public override MessageTypeEnum MessageType => MessageTypeEnum.GetAccounts;
		
		/// <summary>
		/// The player to get accounts for.
		/// </summary>
		[ProtoMember(201)]
		public ulong PlayerId { get; set; }
	}

	[ProtoContract]
	public class GetAccountsResponseMessage : BaseResponseMessage
	{
		public override MessageTypeEnum MessageType => MessageTypeEnum.GetAccounts;
		
		[ProtoMember(201)]
		public AccountDataObject[] Accounts { get; set; }
	}
}