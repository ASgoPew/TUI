using Mono.Data.Sqlite;
using MySql.Data.MySqlClient;
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
        public static string UserTableName = "TUIUserKeyValue";
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
                catch (MySqlException e)
                {
                    TUI.TUI.Hooks.Log.Invoke(new TUI.Hooks.Args.LogArgs(e.ToString(),
                        TUI.Hooks.Args.LogType.Error));
                    throw new Exception("MySQL not setup correctly.");
                }
            }
            else
                throw new Exception("Invalid storage type.");

            var sqlCreator = new SqlTableCreator(db,
                IsMySql
                    ? (IQueryBuilder)new MysqlQueryCreator()
                    : new SqliteQueryCreator());

            //sqlCreator.EnsureTableStructure(new SqlTable("TUIKeyValue",
            //    new SqlColumn("Key", MySqlDbType.TinyText) { Primary=true, Unique=true },
            //    new SqlColumn("Value", MySqlDbType.Text)));

            //sqlCreator.EnsureTableStructure(new SqlTable("TUIKeyValue",
                //new SqlColumn("Key", MySqlDbType.TinyText) { Primary = true, Unique = true },
                //new SqlColumn("Value", MySqlDbType.Binary)));

            Query($@"CREATE TABLE IF NOT EXISTS TUIKeyValue(
                        `Key` TEXT UNIQUE NOT NULL,
                        `Value` BINARY NOT NULL);
                     CREATE TABLE IF NOT EXISTS TUIUserKeyValue(
                        `User` INTEGER NOT NULL,
                        `Key` TEXT NOT NULL,
                        `Value` BINARY NOT NULL,
                        UNIQUE{(IsMySql ? " KEY" : "")} (`User`, `Key`))");

            /*sqlCreator.EnsureTableStructure(new SqlTable("TUIUserKeyValue",
                new SqlColumn("Key", MySqlDbType.TinyText) { Primary = true },
                new SqlColumn("Key", MySqlDbType.TinyText) { Primary = true },
                new SqlColumn("Value", MySqlDbType.Text)));*/
        }

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
            finally
            {
                db.Close();
            }

            return success;
        }

        public static byte[] GetData(string key)
        {
            db.Open();
            try
            {
                using (IDbCommand cmd = db.CreateCommand())
                {
                    cmd.CommandText = "SELECT Value FROM {0} WHERE Key='{1}'".SFormat(TableName, key);
                    using (IDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                            return (byte[])reader["Value"];
                        return null;
                    }
                }
            }
            catch (Exception e)
            {
                TUI.TUI.Hooks.Log.Invoke(new TUI.Hooks.Args.LogArgs(e.ToString(),
                    TUI.Hooks.Args.LogType.Error));
            }
            finally
            {
                db.Close();
            }

            return null;
        }

        public static void SetData(string key, byte[] data)
        {
            db.Open();
            try
            {
                using (var conn = db.CreateCommand())
                {
                    conn.CommandText = "REPLACE INTO {0} (Key, Value) VALUES ({1})".SFormat(TableName, $"'{key}', @data");
                    conn.AddParameter("@data", data);
                    conn.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                TUI.TUI.Hooks.Log.Invoke(new TUI.Hooks.Args.LogArgs(e.ToString(),
                    TUI.Hooks.Args.LogType.Error));
            }
            finally
            {
                db.Close();
            }
        }

        public static void RemoveKey(string key) =>
            Query("DELETE FROM {0} WHERE Key='{1}'".SFormat(TableName, key));

        public static byte[] GetData(int user, string key)
        {
            db.Open();
            try
            {
                using (IDbCommand cmd = db.CreateCommand())
                {
                    cmd.CommandText = "SELECT Value FROM {0} WHERE User={1} AND Key='{2}'".SFormat(UserTableName, user, key);
                    using (IDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                            return (byte[])reader["Value"];
                        return null;
                    }
                }
            }
            catch (Exception e)
            {
                TUI.TUI.Hooks.Log.Invoke(new TUI.Hooks.Args.LogArgs(e.ToString(),
                    TUI.Hooks.Args.LogType.Error));
            }
            finally
            {
                db.Close();
            }

            return null;
        }

        public static void SetData(int user, string key, byte[] data)
        {
            db.Open();
            try
            {
                using (var conn = db.CreateCommand())
                {
                    conn.CommandText = "REPLACE INTO {0} (User, Key, Value) VALUES ({1})".SFormat(UserTableName, $"{user}, '{key}', @data");
                    conn.AddParameter("@data", data);
                    conn.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                TUI.TUI.Hooks.Log.Invoke(new TUI.Hooks.Args.LogArgs(e.ToString(),
                    TUI.Hooks.Args.LogType.Error));
            }
            finally
            {
                db.Close();
            }
        }

        public static void RemoveKey(int user, string key) =>
            Query("DELETE FROM {0} WHERE User={1} AND Key='{2}'".SFormat(UserTableName, user, key));
    }
}
