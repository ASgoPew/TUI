using Mono.Data.Sqlite;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Data;
using System.IO;
using TShockAPI;
using TShockAPI.DB;

namespace TUIPlugin
{
    public static class Database
    {
        public static string TableName = "TUIKeyValue";
        public static bool IsMySql => db.GetSqlType() == SqlType.Mysql;

        public static IDbConnection db;

        /// <summary>
        /// Connects the mysql/sqlite database for the plugin, creating one if the database doesn't already exist.
        /// </summary>
        public static void ConnectDB()
        {
            if (TShock.Config.StorageType.ToLower() == "sqlite")
                db = new SqliteConnection(string.Format("uri=file://{0},Version=3",
                    Path.Combine(TShock.SavePath, "TUIKeyValue.sqlite")));
            else if (TShock.Config.StorageType.ToLower() == "mysql")
            {
                try
                {
                    var host = TShock.Config.MySqlHost.Split(':');
                    db = new MySqlConnection
                    {
                        ConnectionString = string.Format("Server={0}; Port={1}; Database={2}; Uid={3}; Pwd={4}",
                            host[0],
                            host.Length == 1 ? "3306" : host[1],
                            TShock.Config.MySqlDbName,
                            TShock.Config.MySqlUsername,
                            TShock.Config.MySqlPassword)
                    };
                }
                catch (MySqlException x)
                {
                    TShock.Log.Error(x.ToString());
                    throw new Exception("MySQL not setup correctly.");
                }
            }
            else
                throw new Exception("Invalid storage type.");

            var sqlCreator = new SqlTableCreator(db,
                IsMySql
                    ? (IQueryBuilder)new MysqlQueryCreator()
                    : new SqliteQueryCreator());

            sqlCreator.EnsureTableStructure(new SqlTable("TUIKeyValue",
                new SqlColumn("Key", MySqlDbType.TinyText) { Primary=true, Unique=true },
                new SqlColumn("Value", MySqlDbType.Text)));
            /*sqlCreator.EnsureTableStructure(new SqlTable("TUIUserKeyValue",
                new SqlColumn("Key", MySqlDbType.TinyText) { Primary = true },
                new SqlColumn("Key", MySqlDbType.TinyText) { Primary = true },
                new SqlColumn("Value", MySqlDbType.Text)));*/
        }

        public static QueryResult QueryReader(string query) => db.QueryReader(query);

        /// <summary>
        /// Performs an SQL query
        /// </summary>
        /// <param name="query">The SQL statement to be ran.</param>
        /// <returns>
        /// Returns true if the statement was successful.
        /// Returns false if the statement failed.
        /// </returns>
        public static bool Query(string query)
        {
            bool success = true;
            db.Open();
            try
            {
                using (var conn = db.CreateCommand())
                {
                    conn.CommandText = query;
                    conn.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                TUI.TUI.Hooks.Log.Invoke(new TUI.Hooks.Args.LogArgs(e.ToString(),
                    TUI.Hooks.Args.LogType.Error));
                success = false;
            }

            db.Close();
            return success;
        }

        public static object GetData(string key, Type type)
        {
            string query = "SELECT Value FROM {0} WHERE Key='{1}'".SFormat(TableName, key);
#if DEBUG
            Console.WriteLine(query);
#endif
            using (QueryResult result = QueryReader(query))
                if (result.Read())
                    return JsonConvert.DeserializeObject(result.Get<string>("Value"), type);
            return null;
        }

        public static void SetData(string key, object data)
        {
            string query = "REPLACE INTO {0} (Key, Value) VALUES ({1})".SFormat(TableName, $"'{key}', '{JsonConvert.SerializeObject(data)}'");
#if DEBUG
            Console.WriteLine(query);
#endif
            Query(query);
        }

        public static void RemoveKey(string key) =>
            Query("DELETE FROM {0} WHERE Key='{1}'".SFormat(TableName, key));
    }
}
