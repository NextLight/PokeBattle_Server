using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SQLite;
using System.Data;

namespace PokeBattle
{
    class Db
    {
        SQLiteConnection connection;
        public Db(string dbName)
        {
            connection = new SQLiteConnection("Data Source=" + dbName + ";Version=3;");
            connection.Open();
        }

        public void Close()
        {
            connection.Close();
        }

        public DataTable ReadDataTable(string sql)
        {
            var adapter = new SQLiteDataAdapter(sql, connection);
            var dt = new DataTable();
            adapter.Fill(dt);
            return dt;
        }

        public T ReadValue<T>(string sql)
        {
            var command = new SQLiteCommand(sql, connection);
            using (var reader = command.ExecuteReader())
            {
                //TODO: throw exception if (!reader.HasRows) 
                reader.Read();
                return (T)(reader.IsDBNull(0) ? null : (dynamic)reader.GetValue(0));
            }
        }

        public IEnumerable<T> ReadColumn<T>(string sql)
        {
            var command = new SQLiteCommand(sql, connection);
            using (var reader = command.ExecuteReader())
            {
                //TODO: throw exception if (reader.FieldCount == 0)
                while (reader.Read())
                    yield return (T)(reader.IsDBNull(0) ? null : (dynamic)reader.GetValue(0));
            }
        }
    }

    static class DbUtils
    {
        // this is needed because sqlite integer types are 64b and c# doesn't allow to directly unbox to a different type, so it unboxes to dynamic before
        public static T GetValue<T>(this DataRow dr, string col)
        {
            return (T)(dr.IsNull(col) ? null : (dynamic)dr[col]);
        }

        public static IEnumerable<T> GetColumn<T>(this DataTable dt, int col)
        {
            //TODO: throw exception if (col >= dt.Columns.Count)
            foreach (DataRow dr in dt.AsEnumerable())
                yield return dr.Field<T>(col);
        }
    }
}
