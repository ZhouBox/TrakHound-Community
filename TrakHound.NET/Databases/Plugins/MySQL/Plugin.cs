﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;
using System.Data.SqlClient;

using TrakHound.Configurations;
using TrakHound.Databases;
using TrakHound;
using TrakHound.Logging;
using TrakHound.Tools;
using TrakHound.Plugins;
using TrakHound.Plugins.Database;

namespace TrakHound.Databases.Plugins.MySQL
{
    public partial class Plugin : IDatabasePlugin
    {

        public string Name { get { return "MySQL Database Plugin"; } }

        public string Type { get { return "MySQL"; } }

        string GetTableValue(string name, DataTable dt)
        {
            string result = null;

            DataRow[] rows = dt.Select("Name = '" + name + "'");
            if (rows.Length > 0)
            {
                result = rows[0]["Value"].ToString();
            }

            return result;
        }


        public void Initialize(Database_Configuration config)
        {
            var c = Configuration.ReadXML(config.Node);

            config.Configuration = c;

            Console.WriteLine(Type + " Successfully Initialized : " + c.Database + " @ " + c.Server + ":" + c.Port.ToString());
        }

        public bool Ping(object settings, out string msg)
        {
            bool result = false;

            msg = null;

            if (settings != null)
            {
                var config = Configuration.Get(settings);
                if (config != null)
                {
                    try
                    {
                        string connectionString = GetConnectionString(config);
                        if (connectionString != null)
                        {
                            using (SqlConnection con = new SqlConnection(connectionString))
                            {
                                con.Open();
                            }

                            msg = Type + " Successfully connected to : " + config.Database + " @ " + config.Server + ":" + config.Port;
                            result = true;
                        }
                    }
                    catch (SqlException sqex)
                    {
                        msg = Type + " Error connecting to : " + config.Database + " @ " + config.Server + ":" + config.Port + Environment.NewLine;
                        msg += sqex.Message;
                        result = false;
                    }
                    catch (Exception ex)
                    {
                        msg = Type + " Error connecting to : " + config.Database + " @ " + config.Server + ":" + config.Port + Environment.NewLine;
                        msg += ex.Message;
                        result = false;
                    }
                }
            }

            return result;
        }

        public bool CheckPermissions(object settings, Application_Type type)
        {
            bool Result = true;
            return Result;
        }



        static string GetConnectionString(Configuration config, bool setDatabase = true)
        {
            string result = null;

            string server = config.Server;
            if (config.Port > 0) server += ", " + config.Port.ToString();

            if (setDatabase) result = "server=" + server + "; " + "database=" + config.Database + "; " + "user id=" + config.Username + "; " + "password=" + config.Password + ";";
            else result = "server=" + server + "; " + "user id=" + config.Username + "; " + "password=" + config.Password + ";";


            return result;
        }

        static object ExecuteQuery<T>(Configuration config, string query, bool setDatabase = true)
        {
            object result = null;

            try
            {
                var connectionString = GetConnectionString(config, setDatabase);

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText = query;
                    command.ExecuteNonQuery();

                    result = ProcessResult<T>(command);

                    connection.Close();
                }
            }
            catch (SqlException sqex)
            {
                Logger.Log("SQL.Plugin.ExecuteQuery() :: SQLException :: " + sqex.Message);
                if (typeof(T) == typeof(bool)) result = false;
            }
            catch (Exception ex)
            {
                Logger.Log("SQL.Plugin.ExecuteQuery() :: Exception :: " + ex.Message);
                if (typeof(T) == typeof(bool)) result = false;
            }

            return result;
        }

        static object ProcessResult<T>(SqlCommand command)
        {
            object result = null;

            // Object
            if (typeof(T) == typeof(object))
            {
                result = command.ExecuteScalar();
            }

            // Boolean
            if (typeof(T) == typeof(bool))
            {
                // if it gets here without an throwing an exception then it was successful
                result = true;
            }

            // int
            if (typeof(T) == typeof(int))
            {
                object o = command.ExecuteScalar();
                var val = (int)(-1);
                if (o != null) int.TryParse(o.ToString(), out val);
                result = val;
            }

            // Int64
            if (typeof(T) == typeof(Int64))
            {
                object o = command.ExecuteScalar();
                var val = (Int64)(-1);
                if (o != null) Int64.TryParse(o.ToString(), out val);
                result = val;
            }

            // string
            if (typeof(T) == typeof(string))
            {
                object o = command.ExecuteScalar();
                if (o != null) result = String_Functions.FromSpecial(o.ToString());
            }

            // DataTable
            if (typeof(T) == typeof(DataTable))
            {
                var dt = new DataTable();
                using (SqlDataAdapter a = new SqlDataAdapter(command))
                {
                    a.Fill(dt);
                }

                // Convert from ASCII Special Chars
                foreach (DataColumn column in dt.Columns)
                {
                    if (column.DataType == typeof(string))
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            row[column] = String_Functions.FromSpecial(row[column].ToString());
                        }
                    }
                }

                result = dt;
            }

            // DataRow
            if (typeof(T) == typeof(DataRow))
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        int columnCount = reader.FieldCount;

                        DataTable temp_dt = new DataTable();

                        for (int x = 0; x <= columnCount - 1; x++)
                        {
                            temp_dt.Columns.Add(reader.GetName(x));
                        }

                        DataRow row = temp_dt.NewRow();

                        object[] values = new object[columnCount];

                        while (reader.Read())
                        {
                            for (int j = 0; j <= values.Length - 1; j++)
                            {
                                object val = reader[j];

                                if (temp_dt.Columns[j].DataType == typeof(string)) val = String_Functions.FromSpecial(val.ToString());

                                values[j] = val;
                            }
                        }

                        row.ItemArray = values;

                        result = row;
                    }
                }
            }

            // string[]
            if (typeof(T) == typeof(string[]))
            {
                var tables = new List<string>();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read()) tables.Add(String_Functions.FromSpecial(reader[0].ToString()));
                    }
                }
                result = tables.ToArray();
            }

            // List<string>
            if (typeof(T) == typeof(List<string>))
            {
                var tables = new List<string>();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read()) tables.Add(String_Functions.FromSpecial(reader[0].ToString()));
                    }
                }
                result = tables;
            }

            return result;
        }


        public const string dateString = "yyyy-MM-dd H:mm:ss";

        public static string ConvertToSQLDateTime(string s)
        {
            string result = "null";

            DateTime TS;
            if (DateTime.TryParse(s, out TS)) result = TS.ToString(dateString);

            return result;
        }

        public static string COLUMN_NAME_START = "[";
        public static string COLUMN_NAME_END = "]";

        public static object[] ConvertColumnDefinitions(ColumnDefinition[] columns)
        {
            List<object> result = new List<object>();

            foreach (ColumnDefinition coldef in columns.ToList())
            {
                string def = ConvertColumnDefinition(coldef);
                if (coldef.NotNull) def += " NOT NULL";

                result.Add(def);
            }

            return result.ToArray();
        }

        public static string ConvertColumnDefinition(ColumnDefinition column)
        {
            return COLUMN_NAME_START + column.ColumnName + COLUMN_NAME_END + " " + ConvertColumnDataType(column.DataType);
        }

        public const string VarChar = "varchar(1000)";
        public const string BigInt = "bigint";
        public const string Double = "double precision";
        public const string Datetime = "datetime";
        public const string Bool = "boolean";

        public static string ConvertColumnDataType(DataType dataType)
        {
            if (dataType == DataType.Boolean) return Bool;

            if (dataType == DataType.Short) return BigInt;
            if (dataType == DataType.Long) return BigInt;
            if (dataType == DataType.Double) return Double;

            if (dataType == DataType.SmallText) return VarChar;
            if (dataType == DataType.MediumText) return VarChar;
            if (dataType == DataType.LargeText) return VarChar;

            if (dataType == DataType.DateTime) return Datetime;

            return null;
        }

        static string ConvertValue(object o)
        {
            if (o != null)
            {
                var val = o.ToString();
                if (o.GetType() == typeof(DateTime)) val = ConvertToSQLDateTime(val);
                if (o.GetType() == typeof(string)) val = "'" + String_Functions.ToSpecial(val) + "'";
                else val = "'" + val + "'";

                return val;
            }
            else
            {
                return "null";
            }
        }

    }
}
