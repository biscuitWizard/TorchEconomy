using ProtoBuf;
using TorchEconomy.Data.DataObjects;

namespace TorchEconomy.Messages.Accounts
{
	[ProtoContract]
	public class CreateAccountMessage : BaseMessage
	{
		public override MessageTypeEnum MessageType => MessageTypeEnum.CreateAccount;
		
		[ProtoMember(201)]
		public ulong ForPlayerId { get; set; }
		
		[ProtoMember(202)]
		public decimal InitialBalance { get; set; }
		
		[ProtoMember(203)]
		public bool IsNPC { get; set; }
	}

	[ProtoContract]
	public class CreateAccountResponseMessage : BaseResponseMessage
	{
		public override MessageTypeEnum MessageType => MessageTypeEnum.CreateAccount;
		
		[ProtoMember(201)]
		public AccountDataObject Account { get; set; }
	}
}