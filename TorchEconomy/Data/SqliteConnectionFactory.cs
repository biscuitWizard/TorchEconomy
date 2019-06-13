using System.Data;
using System.IO;
using Mono.Data.Sqlite;

namespace TorchEconomy.Data
{
	public class SqliteConnectionFactory : IConnectionFactory
	{
		private const string DbPath = "torch_economy.sqlite";
		
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