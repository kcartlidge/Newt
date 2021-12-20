using System;
using Newt.Models;
using Npgsql;
using System.Linq;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor.
#pragma warning disable CS8604 // Possible null reference argument.

namespace Newt.Postgres
{
    internal class PostgresScanner
    {
        private readonly string connectionString;
        private readonly string schema;

        public PostgresScanner(string connectionString, string schema)
        {
            this.connectionString = connectionString;
            this.schema = schema;
        }

        internal DBSchema Scan()
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                var DB = new DBSchema { DatabaseName = conn.Database, Schema = schema };
                ScanTables(conn, DB);
                foreach (var table in DB.Tables)
                {
                    ScanColumns(conn, table);
                    ScanPrimaryKeys(conn, table);

                    if (table.Keys.Count > 1)
                    {
                        // Multiple fields *are* actually supported structurally and in most code,
                        // but will cause issues in some areas such as Controller route parameters.
                        throw new Exception($"Only single-field primary keys are supported ({table.Name}).");
                    }

                    ScanIndexes(conn, table);

                    for (int i = 0; i < table.Columns.Count - 1; i++)
                    {
                        table.Columns[i].IsKey = table.Keys.Any(x => x.Column == table.Columns[i].Name);
                    }
                }
                return DB;
            }
        }

        private static void ScanTables(NpgsqlConnection conn, DBSchema DB)
        {
            conn.Open();
            var sql =
                $"SELECT table_name " +
                $"FROM   information_schema.tables " +
                $"WHERE  table_schema = '{DB.Schema}'" +
                $"ORDER BY table_name;";
            using (var cmd = new NpgsqlCommand(sql, conn))
            {
                var rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    DB.Tables.Add(new DBTable(conn.UserName, DB.Schema, rdr.GetString(0)));
            }
            conn.Close();
        }
        private static void ScanColumns(NpgsqlConnection conn, DBTable table)
        {
            conn.Open();
            var sql =
                $"SELECT ordinal_position, column_name, is_nullable, data_type, character_maximum_length " +
                $"FROM   information_schema.columns " +
                $"WHERE  table_schema = '{table.Schema}' " +
                $"AND    table_name = '{table.Name}';";
            using (var cmd = new NpgsqlCommand(sql, conn))
            {
                var rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    int? maxLen = rdr.IsDBNull(4) ? null : rdr.GetInt32(4);
                    table.Columns.Add(new DBColumn(
                        rdr.GetInt32(0),
                        table.Schema,
                        table.Name,
                        rdr.GetString(1),
                        rdr.GetString(2).ToUpperInvariant() == "YES",
                        rdr.GetString(3),
                        maxLen));
                }
            }
            conn.Close();
        }
        private static void ScanPrimaryKeys(NpgsqlConnection conn, DBTable table)
        {
            conn.Open();
            var sql =
                $"SELECT tc.constraint_name, kc.column_name " +
                $"FROM   information_schema.table_constraints tc, information_schema.key_column_usage kc " +
                $"WHERE  kc.table_name = tc.table_name " +
                $"AND    kc.table_schema = tc.table_schema " +
                $"AND    kc.constraint_name = tc.constraint_name " +
                $"AND    kc.table_schema = '{table.Schema}' " +
                $"AND    kc.table_name = '{table.Name}'" +
                $"AND    tc.constraint_type = 'PRIMARY KEY'";
            using (var cmd = new NpgsqlCommand(sql, conn))
            {
                var rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    table.Keys.Add(new DBKey(
                        rdr.GetString(0),
                        table.Schema,
                        table.Name,
                        rdr.GetString(1)
                    ));
                }
            }
            conn.Close();
        }
        private static void ScanIndexes(NpgsqlConnection conn, DBTable table)
        {
            conn.Open();
            var sql =
                $"SELECT indexname, indexdef " +
                $"FROM   pg_indexes " +
                $"WHERE  schemaname = '{table.Schema}' " +
                $"AND    tablename = '{table.Name}'";
            using (var cmd = new NpgsqlCommand(sql, conn))
            {
                var rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    var name = rdr.GetString(0);
                    if (table.Keys.Any(x => x.Name == name)) continue;
                    table.Indexes.Add(new DBIndex(
                        name,
                        table.Schema,
                        table.Name,
                        rdr.GetString(1)
                    ));
                }
            }
            conn.Close();
        }
    }
}

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor.
#pragma warning restore CS8604 // Possible null reference argument.
