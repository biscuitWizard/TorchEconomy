using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using TorchEconomy.Data;

namespace TorchEconomy.Managers
{
	/// <summary>
	/// Takes actions runs them on the main thread over time.
	///
	/// This is important for Keen-code that cannot be run on my sweet promises and threads.
	/// </summary>
	public class ThreadSynchronizationDispatcher : BaseManager
	{
		private class QueuedAction
		{
			public Action Action { get; set; }
			public Action Callback { get; set; }
		}

		private static ConcurrentQueue<QueuedAction> _actionQueue 
			= new ConcurrentQueue<QueuedAction>();

		/// <summary>
		/// How many milliseconds the update loop can consume before we need to let the
		/// thread continue whatever the fuck it was doing.
		/// </summary>
		public static long UpdateMillisecondsLimit = 10;
		
		public ThreadSynchronizationDispatcher(IConnectionFactory connectionFactory) : base(connectionFactory)
		{}

		public static void Enqueue(Action action, Action callback = null)
		{
			_actionQueue.Enqueue(new QueuedAction
			{
				Action = action,
				Callback = callback
			});
		}

		public override void Update()
		{
			long elapsedMilliseconds = 0;
			while (!_actionQueue.IsEmpty
			       && elapsedMilliseconds < UpdateMillisecondsLimit)
			{
				if (!_actionQueue.TryDequeue(out var queuedAction))
					break; // ???? Well, fuck it.
				
				var stopwatch = Stopwatch.StartNew();
				queuedAction.Action();
				queuedAction.Callback?.Invoke();
				stopwatch.Stop();

				elapsedMilliseconds += stopwatch.ElapsedMilliseconds;
			}
		}

		public override void Stop()
		{
			_actionQueue = new ConcurrentQueue<QueuedAction>();
		}
	}
}