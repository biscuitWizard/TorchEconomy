using System.Data;
using System.IO;
using Mono.Data.Sqlite;

namespace TorchEconomy.Data
{
	public class SqliteConnectionFactory : IConnectionFactory
	{
		private const string DatabaseName = "torch_economy.sqlite";

		public static string DbPath => Path.Combine(EconomyPlugin.Instance.StoragePath, DatabaseName);

		public IDbConnection Open()
		{
			if (!File.Exists(DbPath))
			{
				SqliteConnection.CreateFile(DbPath);
			}
			
			var connection = new SqliteConnection("Data Source=" + DbPath);
			connection.Open();
			
			return connection;
		}
	}
}