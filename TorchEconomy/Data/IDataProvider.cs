namespace TorchEconomy.Data
{
	/// <summary>
	/// Data providers are built in the IoC as singleton systems that
	/// are initialized and used to hold/access data through
	/// program/session lifetime.
	/// </summary>
	public interface IDataProvider
	{
		void OnStart();
		void OnSessionLoaded();
	}
}