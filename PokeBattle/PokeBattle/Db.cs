using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;

namespace PokeBattle
{
    class Db
    {
        SQLiteConnection _connection;
        public Db(string name)
        {
            _connection = new SQLiteConnection("Data Source=" + name + ";Version=3;");
            _connection.Open();
        }

        public void Close()
        {
            _connection.Close();
        }

        public DataTable ReadDataTable(string sql)
        {
            var adapter = new SQLiteDataAdapter(sql, _connection);
            var dt = new DataTable();
            adapter.Fill(dt);
            return dt;
        }

        public T ReadValue<T>(string sql)
        {
            var command = new SQLiteCommand(sql, _connection);
            using (var reader = command.ExecuteReader())
            {
                reader.Read();
                return (T)(reader.IsDBNull(0) ? null : (dynamic)reader.GetValue(0));
            }
        }

        public IEnumerable<T> ReadColumn<T>(string sql)
        {
            var command = new SQLiteCommand(sql, _connection);
            using (var reader = command.ExecuteReader())
                while (reader.Read())
                    yield return (T)(reader.IsDBNull(0) ? null : (dynamic)reader.GetValue(0));
        }
    }

    static class DbExtensions
    {
        // this is needed because sqlite integer types are 64b and c# doesn't allow to directly unbox to a different type, so it unboxes to dynamic before
        public static T GetValue<T>(this DataRow dr, string col) => (T)(dr.IsNull(col) ? null : (dynamic)dr[col]);
    }
}
