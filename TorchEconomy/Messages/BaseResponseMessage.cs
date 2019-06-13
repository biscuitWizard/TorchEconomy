using ProtoBuf;
using Sandbox.ModAPI;
using TorchEconomy.Messages.Accounts;

namespace TorchEconomy.Messages
{
	[ProtoContract]
	[ProtoInclude(1, typeof(GetAccountsResponseMessage))]
	[ProtoInclude(2, typeof(AdjustBalanceResponseMessage))]
	[ProtoInclude(3, typeof(CreateAccountResponseMessage))]
	[ProtoInclude(4, typeof(GetConfigResponseMessage))]
	public abstract class BaseResponseMessage
	{
		/// <summary>
		/// This is not serialized. Just here for convenience.
		/// </summary>
		public BaseMessage OriginalMessage { get; set; }
		
		[ProtoMember(101)]
		public abstract MessageTypeEnum MessageType { get; }
		
		[ProtoMember(102)]
		public bool Success { get; set; }

		public BaseResponseMessage()
		{
			Success = true;
		}
		
		public byte[] ToBytes()
		{
			return MyAPIGateway.Utilities.SerializeToBinary(this);
		}
	}
}