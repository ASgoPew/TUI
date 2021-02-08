using Mono.Data.Sqlite;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using TerrariaUI;
using TShockAPI;
using TShockAPI.DB;

namespace TUIPlugin
{
    public static class Database
    {
        #region Data

        public const string UserTableName = "Users";
        public const string KeyValueTableName = "TUIKeyValue";
        public const string UserKeyValueTableName = "TUIUserKeyValue";
        public const string UserNumberTableName = "TUIUserNumber";
        public static bool IsMySql => db.GetSqlType() == SqlType.Mysql;

        public static IDbConnection db;

        #endregion

        #region ConnectDB

        /// <summary>
        /// Connects the mysql/sqlite database for the plugin, creating one if the database doesn't already exist.
        /// </summary>
        public static void ConnectDB()
        {
            Console.WriteLine($"TUI ========================================================================\n{TShock.Config.StorageType.ToLower()}");
            if (TShock.Config.StorageType.ToLower() == "sqlite")
                db = new SqliteConnection(string.Format("uri=file://{0},Version=3",
                    Path.Combine(TShock.SavePath, "tshock.sqlite")));
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
                    TUI.HandleException(e);
                    throw new Exception("MySQL not setup correctly.");
                }
            }
            else
                throw new Exception("Invalid storage type.");

            try
            {
                db.Query(
                    $@"CREATE TABLE IF NOT EXISTS TUIKeyValue(
                        `Identifier` {(IsMySql ? "VARCHAR(256)" : "TEXT")} UNIQUE NOT NULL,
                        `Value` BLOB NOT NULL);
                    CREATE TABLE IF NOT EXISTS TUIUserNumber(
                        `User` INTEGER NOT NULL,
                        `Identifier` {(IsMySql ? "VARCHAR(256)" : "TEXT")} NOT NULL,
                        `Number` INTEGER NOT NULL,
                        UNIQUE{(IsMySql ? " KEY" : "")} (`User`, `Identifier`));
                    CREATE TABLE IF NOT EXISTS TUIUserKeyValue(
                        `User` INTEGER NOT NULL,
                        `Identifier` {(IsMySql ? "VARCHAR(256)" : "TEXT")} NOT NULL,
                        `Value` BLOB NOT NULL,
                        UNIQUE{(IsMySql ? " KEY" : "")} (`User`, `Identifier`))");
            }
            catch (Exception e)
            {
                TUI.HandleException(new Exception($"TUI.Database.ConnectDB()", e));
            }
        }

        #endregion

        #region GetData(string key)

        public static byte[] GetData(string key)
        {
            try
            {
                using (QueryResult reader = db.QueryReader($"SELECT Value FROM {KeyValueTableName} WHERE Identifier=@0", key))
                {
                    if (reader.Read())
                        return (byte[])reader.Reader.GetValue(0);
                }
            }
            catch (Exception e)
            {
                TUI.HandleException(new Exception($"TUI.Database.GetData() (key:{key})", e));
            }
            return null;
        }

        #endregion
        #region SetData(string key, byte[] data)

        public static void SetData(string key, byte[] data)
        {
            try
            {
                db.Query($"REPLACE INTO {KeyValueTableName} (Identifier, Value) VALUES (@0, @1)", key, data);
            }
            catch (Exception e)
            {
                TUI.HandleException(new Exception($"TUI.Database.SetData() (key:{key})", e));
            }
        }

        #endregion
        #region RemoveKey(string key)

        public static void RemoveData(string key)
        {
            try
            {
                db.Query($"DELETE FROM {KeyValueTableName} WHERE Identifier=@0", key);
            }
            catch (Exception e)
            {
                TUI.HandleException(new Exception($"TUI.Database.RemoveData() (key:{key})", e));
            }
        }

        #endregion

        #region GetData(int user, string key)

        public static byte[] GetData(int user, string key)
        {
            try
            {
                using (QueryResult reader = db.QueryReader($"SELECT Value FROM {UserKeyValueTableName} WHERE User=@0 AND Identifier=@1", user, key))
                {
                    if (reader.Read())
                        return (byte[])reader.Reader.GetValue(0);
                }
            }
            catch (Exception e)
            {
                TUI.HandleException(new Exception($"TUI.Database.GetData(int user) (key:{key})", e));
            }
            return null;
        }

        #endregion
        #region SetData(int user, string key, byte[] data)

        public static void SetData(int user, string key, byte[] data)
        {
            try
            {
                db.Query($"REPLACE INTO {UserKeyValueTableName} (User, Identifier, Value) VALUES (@0, @1, @2)", user, key, data);
            }
            catch (Exception e)
            {
                TUI.HandleException(new Exception($"TUI.Database.SetData(int user) (key:{key})", e));
            }
        }

        #endregion
        #region RemoveKey(int user, string key)

        public static void RemoveKey(int user, string key)
        {
            try
            {
                db.Query($"DELETE FROM {UserKeyValueTableName} WHERE User=@0 AND Identifier=@1", user, key);
            }
            catch (Exception e)
            {
                TUI.HandleException(new Exception($"TUI.Database.RemoveData(int user) (key:{key})", e));
            }
        }

        #endregion

        #region GetNumber

        public static int? GetNumber(int user, string key)
        {
            try
            {
                using (QueryResult reader = db.QueryReader($"SELECT Number FROM {UserNumberTableName} WHERE User=@0 AND Identifier=@1", user, key))
                {
                    if (reader.Read())
                        return reader.Get<int>("Number");
                }
            }
            catch (Exception e)
            {
                TUI.HandleException(new Exception($"TUI.Database.GetNumber() (key:{key})", e));
            }
            return null;
        }

        #endregion
        #region SetNumber

        public static void SetNumber(int user, string key, int number)
        {
            try
            {
                db.Query($"REPLACE INTO {UserNumberTableName} (User, Identifier, Number) VALUES (@0, @1, @2)", user, key, number);
            }
            catch (Exception e)
            {
                TUI.HandleException(new Exception($"TUI.Database.SetNumber() (key:{key})", e));
            }
        }

        #endregion
        #region RemoveNumber

        public static void RemoveNumber(int user, string key)
        {
            try
            {
                db.Query($"DELETE FROM {UserNumberTableName} WHERE User=@0 AND Identifier=@1", user, key);
            }
            catch (Exception e)
            {
                TUI.HandleException(new Exception($"TUI.Database.RemoveNumber() (key:{key})", e));
            }
        }

        #endregion
        #region SelectNumbers

        public static List<(int User, int Number, string Username)> SelectNumbers(string key, bool ascending, int count, int offset, bool requestNames)
        {
            List<(int, int, string)> result = new List<(int, int, string)>();
            try
            {
                string query = requestNames ?
                    $@"SELECT number.User, number.Number, user.Username
	                    FROM {UserNumberTableName} AS number
                        JOIN {UserTableName} as user ON number.User = user.ID
                        WHERE Identifier=@0
                        ORDER BY Number {(ascending ? "ASC" : "DESC")}
                        LIMIT @1
                        OFFSET @2"
                    : $@"SELECT User, Number
                        FROM {UserNumberTableName}
                        WHERE Identifier=@0
                        ORDER BY Number {(ascending ? "ASC" : "DESC")}
                        LIMIT @1
                        OFFSET @2";
                using (QueryResult reader = db.QueryReader(query, key, count, offset))
                {
                    while (reader.Read())
                    {
                        int user = reader.Get<int>("User");
                        int number = reader.Get<int>("Number");
                        string username = requestNames ? reader.Get<string>("Username") : null;
                        result.Add((user, number, username));
                    }
                }
            }
            catch (Exception e)
            {
                TUI.HandleException(new Exception($"TUI.Database.SelectNumbers() (key:{key})", e));
            }
            return result;
        }

        #endregion
    }
}
