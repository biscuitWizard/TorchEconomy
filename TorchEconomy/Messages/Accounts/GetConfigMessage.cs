using ProtoBuf;

namespace TorchEconomy.Messages.Accounts
{
	[ProtoContract]
	public class GetConfigMessage : BaseMessage
	{
		public override MessageTypeEnum MessageType => MessageTypeEnum.GetConfig;
	}

	[ProtoContract]
	public class GetConfigResponseMessage : BaseResponseMessage
	{
		public override MessageTypeEnum MessageType => MessageTypeEnum.GetConfig;
		
		[ProtoMember(201)]
		public EconomyConfig Config { get; set; }
	}
}