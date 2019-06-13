using ProtoBuf;
using Sandbox.ModAPI;
using TorchEconomy.Messages.Accounts;

namespace TorchEconomy.Messages
{
	[ProtoContract]
	[ProtoInclude(1, typeof(GetAccountsMessage))]
	[ProtoInclude(2, typeof(AdjustBalanceMessage))]
	[ProtoInclude(3, typeof(CreateAccountMessage))]
	[ProtoInclude(4, typeof(GetConfigMessage))]
	public abstract class BaseMessage
	{
		[ProtoMember(101)]
		public abstract MessageTypeEnum MessageType { get; }
		
		/// <summary>
		/// Messages sent to TorchEconomy signed with this message will be authenticated.
		/// </summary>
		[ProtoMember(102)]
		public string TransactionKey { get; set; }
		
		/// <summary>
		/// Can be used to tell Torch Economy to respond to this message
		/// on a different channel than normal.
		///
		/// If this isn't specified, Torch Econ will NOT send a response!!
		///
		/// This is because I cannot tell where a message is from, so it has to be broadcast.
		/// </summary>
		[ProtoMember(103)]
		public ushort? ResponseChannelOverride { get; set; }

		public byte[] ToBytes()
		{
			return MyAPIGateway.Utilities.SerializeToBinary(this);
		}
	}
}