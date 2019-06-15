using System;
using System.Data;
using System.IO;
using System.Reflection;
using Dapper;
using Mono.Data.Sqlite;
using NLog;
using TorchEconomy.Data.Schema;

namespace TorchEconomy.Data
{
	public class SqliteConnectionFactory : IConnectionFactory
	{
		private static readonly Logger Log = LogManager.GetLogger("Economy.SqliteConnectionFactory");
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
			var connection = new SqliteConnection(EconomyPlugin.Instance.Config.SQLiteConnectionString);
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
			try
			{
				using (var connection = Open())
				{
					var schemaGenerator = new SQLiteSchemaGenerator();
					var sql = schemaGenerator.Generate();
					Log.Info(sql);
					connection.Execute(sql);
				}
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}
	}
}