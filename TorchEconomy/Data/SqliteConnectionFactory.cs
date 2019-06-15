using System.Data;
using System.IO;
using System.Reflection;
using Dapper;
using Mono.Data.Sqlite;

namespace TorchEconomy.Data
{
	public class SqliteConnectionFactory : IConnectionFactory
	{
		private const string DatabaseName = "torch_economy.sqlite";

		public static string DbPath => Path.Combine(EconomyPlugin.Instance.StoragePath, DatabaseName);

		public void Setup()
		{
			if (!File.Exists(DbPath))
			{
				CreateDatabase();
			}
		}

		public IDbConnection Open()
		{
			var connection = new SqliteConnection(EconomyPlugin.Instance.Config.ConnectionString);
			connection.Open();
			
			return connection;
		}

		private void CreateDatabase()
		{
			SqliteConnection.CreateFile(DbPath);

			InitializeTables();
		}

		private void InitializeTables()
		{
			using (var connection = Open())
			{
				using (Stream stream = Assembly
					.GetAssembly(typeof(EconomyPlugin))
					.GetManifestResourceStream("TorchEconomy.Data.CreateTables.sql"))
				{
					using (StreamReader reader = new StreamReader(stream))
					{
						string createTablesSql = reader.ReadToEnd();
						connection.Execute(createTablesSql);
					}
				}
			}
		}
	}
}