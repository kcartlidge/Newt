using Newt.Models;
using Npgsql;
using System.Linq;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor.
#pragma warning disable CS8604 // Possible null reference argument.

namespace Newt.Postgres
{
    internal class PostgresScanner
    {
        private readonly string _connectionString;
        private readonly string _schema;

        public PostgresScanner(string connectionString, string schema)
        {
            this._connectionString = connectionString;
            this._schema = schema;
        }

        internal DBSchema Scan()
        {
            using var conn = new NpgsqlConnection(_connectionString);
            var db = new DBSchema { DatabaseName = conn.Database, Schema = _schema };
            ScanTables(conn, db);
            foreach (var table in db.Tables)
            {
                ScanColumns(conn, table);
                ScanConstraints(conn, table);
                ScanIndexes(conn, table);

                for (var i = 0; i < table.Columns.Count - 1; i++)
                {
                    table.Columns[i].IsPrimaryKey = table.Constraints
                        .Any(x => x.Column == table.Columns[i].Name && x.IsPrimaryKey);
                }
            }
            return db;
        }

        private static void ScanTables(NpgsqlConnection conn, DBSchema db)
        {
            conn.Open();
            var sql =
                $"SELECT table_name " +
                $"FROM   information_schema.tables " +
                $"WHERE  table_schema = '{db.Schema}'" +
                $"ORDER BY table_name;";
            using (var cmd = new NpgsqlCommand(sql, conn))
            {
                var rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    db.Tables.Add(new DBTable(conn.UserName, db.Schema, rdr.GetString(0)));
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
        private static void ScanConstraints(NpgsqlConnection conn, DBTable table)
        {
            conn.Open();
            var sql =
                $"SELECT tc.constraint_name, kc.column_name, tc.constraint_type, " +
                $"       cc.table_name as ref_table, cc.column_name as ref_column " +
                $"FROM   information_schema.table_constraints tc, information_schema.key_column_usage kc, " +
                $"       information_schema.constraint_column_usage cc " +
                $"WHERE  kc.table_name = tc.table_name " +
                $"AND    kc.table_schema = tc.table_schema " +
                $"AND    kc.constraint_name = tc.constraint_name " +
                $"AND    cc.constraint_name = tc.constraint_name " +
                $"AND    kc.table_schema = '{table.Schema}' " +
                $"AND    kc.table_name = '{table.Name}'";
            using (var cmd = new NpgsqlCommand(sql, conn))
            {
                var rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    var k = new DBConstraint(
                        rdr.GetString(0),
                        table.Schema,
                        table.Name,
                        rdr.GetString(1),
                        rdr.GetString(2)
                    );
                    if (k.IsForeignKey)
                    {
                        k.ForeignTable = rdr.GetString(3);
                        k.ForeignColumn = rdr.GetString(4);
                    }
                    table.Constraints.Add(k);
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
                    if (table.Constraints.Any(x => x.Name == name)) continue;
                    table.Indexes.Add(new DBIndex(
                        name,
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
