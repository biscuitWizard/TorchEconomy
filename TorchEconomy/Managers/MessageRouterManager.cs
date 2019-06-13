using System;
using System.Collections.Generic;
using NLog;
using Sandbox.Definitions;
using Sandbox.ModAPI;
using TorchEconomy.Data;
using TorchEconomy.Messages;
using TorchEconomy.Messages.Accounts;
using VRage.Network;

namespace TorchEconomy.Managers
{
	public class MessageRouterManager : BaseManager
	{
		private static readonly Logger Log = LogManager.GetLogger("Economy.Managers.Messages");
		private readonly Dictionary<Type, List<Action<BaseMessage>>> _subscriptions 
			= new Dictionary<Type, List<Action<BaseMessage>>>();
		private readonly Dictionary<EndpointId, ulong> _steamEndpointMapping = new Dictionary<EndpointId, ulong>();
		
		public MessageRouterManager(IConnectionFactory connectionFactory) : base(connectionFactory)
		{
		}

		public override void Start()
		{
			base.Start();
			
			MyAPIGateway.Multiplayer.RegisterMessageHandler(EconomyConfig.ModChannelId, HandleMessage);
		}

		public override void Stop()
		{
			base.Stop();
			
			MyAPIGateway.Multiplayer.UnregisterMessageHandler(EconomyConfig.ModChannelId, HandleMessage);
			_subscriptions.Clear();
		}

		public void Subscribe<TMessage>(Action<TMessage> callback) where TMessage : BaseMessage
		{
			var type = typeof(TMessage);
			if (!_subscriptions.TryGetValue(type, out var subscriptions))
			{
				subscriptions = new List<Action<BaseMessage>>();
				_subscriptions.Add(type, subscriptions);
			}

			subscriptions.Add((message) => callback(message as TMessage));
		}

		public void Publish<TMessage>(TMessage message) where TMessage : BaseMessage
		{
			var type = typeof(TMessage);
			if (!_subscriptions.TryGetValue(type, out var subscriptions))
				return; // No subscriptions.

			foreach (var sub in subscriptions)
			{
				sub(message);
			}
		}

		public void SendResponse<TResponse>(TResponse message) where TResponse : BaseResponseMessage
		{
			var recipient = message.OriginalMessage.SenderId;
			var channel = message.OriginalMessage.ResponseChannelOverride ?? EconomyConfig.ModChannelId;
			var bytes = message.ToBytes();

			MyAPIGateway.Multiplayer.SendMessageTo(channel, bytes, recipient, true);
		}
		
		/// <summary>
		/// Incoming messages from the network are processed through HandleMessage().
		///
		/// This really should never be called from inside of TorchEconomy.
		/// </summary>
		/// <param name="data"></param>
		private void HandleMessage(byte[] data)
		{
			try
			{
				var baseMessage = MyAPIGateway.Utilities.SerializeFromBinary<BaseMessage>(data);

				var senderEndpoint = MyEventContext.Current.Sender;
				baseMessage.SenderId = senderEndpoint.Value;
				
				if (Config.ForceTransactionCheck)
				{
					if (baseMessage.TransactionKey != Config.TransactionKey)
					{
						// Access denied.
						Log.Error("Access denied on API Call. Reason: Transaction Key Mismatch");
						return;
					}
				}

				Publish(baseMessage);

//				switch (baseMessage.MessageType)
//				{
//					case MessageTypeEnum.GetConfig:
//						Publish(baseMessage as GetConfigMessage);
//						break;
//					case MessageTypeEnum.GetAccounts:
//						Publish(baseMessage as GetAccountsMessage);
//						break;
//					case MessageTypeEnum.AdjustBalance:
//						Publish(baseMessage as AdjustBalanceMessage);
//						break;
//					case MessageTypeEnum.CreateAccount:
//						Publish(baseMessage as CreateAccountMessage);
//						break;
//				}
			}
			catch (Exception e)
			{
				Log.Error($"Unable to deserialize message with length: {data.Length}", e);
			}
		}
	}
}