#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor.
#pragma warning disable CS8604 // Possible null reference argument.

using Newt.Models;
using Npgsql;
using System.Linq;

namespace Newt.Postgres
{
    /// <summary>Database structure scanner.</summary>
    internal class PostgresScanner
    {
        private readonly string _connectionString;
        private readonly string _schema;

        public PostgresScanner(string connectionString, string schema)
        {
            this._connectionString = connectionString;
            this._schema = schema;
        }

        /// <summary>
        /// Scan the database, returning details of the schema contents.
        /// </summary>
        internal DBSchema Scan()
        {
            using var conn = new NpgsqlConnection(_connectionString);
            var db = new DBSchema { DatabaseName = conn.Database, Schema = _schema };

            ScanTables(conn, db);
            foreach (var table in db.Tables)
            {
                // Get the basics.
                ScanColumns(conn, table);
                ScanConstraints(conn, table);
                ScanIndexes(conn, table);

                // Isolate and flag primary key columns.
                for (var i = 0; i < table.Columns.Count - 1; i++)
                {
                    table.Columns[i].IsPrimaryKey = table.Constraints
                        .Any(x => x.Column == table.Columns[i].Name && x.IsPrimaryKey);
                }
                
                // Create relationships from foreign keys.
                foreach (var constraint in table.Constraints.Where(x => x.IsForeignKey))
                {
                    var targetTable = db.Tables.FirstOrDefault(x => x.Name == constraint.ForeignTable);
                    targetTable?.NavigationProperties.Add(new DBRelationship
                    {
                        Constraint = constraint,
                        Table = table,
                    });
                }
            }
            return db;
        }

        /// <summary>Scan all the tables.</summary>
        private static void ScanTables(NpgsqlConnection conn, DBSchema db)
        {
            conn.Open();
            var sql =
                $"SELECT table_name, pg_catalog.obj_description(pgc.oid, 'pg_class') as table_description " +
                $"FROM   information_schema.tables, pg_catalog.pg_class pgc " +
                $"WHERE  table_name = pgc.relname " +
                $"AND    table_type='BASE TABLE' " +
                $"AND    table_schema = '{db.Schema}'" +
                $"ORDER BY table_name;";
            using (var cmd = new NpgsqlCommand(sql, conn))
            {
                var rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    string name = rdr.GetString(0);
                    string comment = rdr.IsDBNull(1) ? "" : rdr.GetString(1);
                    db.Tables.Add(new DBTable(conn.UserName, db.Schema, name, comment));
                }
            }
            conn.Close();
        }

        /// <summary>Scan all a table's column details.</summary>
        private static void ScanColumns(NpgsqlConnection conn, DBTable table)
        {
            conn.Open();
            var sql =
                $"SELECT ordinal_position, column_name, is_nullable, data_type, character_maximum_length, column_default, " +
                $"       pg_catalog.col_description(format('%s.%s',table_schema,table_name)::regclass::oid,ordinal_position) as column_description " +
                $"FROM   information_schema.columns " +
                $"WHERE  table_schema = '{table.Schema}' " +
                $"AND    table_name = '{table.Name}';";
            using (var cmd = new NpgsqlCommand(sql, conn))
            {
                var rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    int? maxLen = rdr.IsDBNull(4) ? null : rdr.GetInt32(4);
                    string defaultValue = rdr.IsDBNull(5) ? "" : rdr.GetString(5);
                    string comment = rdr.IsDBNull(6) ? "" : rdr.GetString(6);
                    if (comment.HasValue() && comment.EndsWith(".") == false)
                    {
                        comment = $"{comment.Trim()}.";
                    }

                    // Deliberately omit sequence nextvals.
                    // Sequences aren't scripted so no point (and 'serial' covers it anyway).
                    if (defaultValue.HasValue() &&
                        defaultValue.ToLowerInvariant().StartsWith("nextval("))
                    {
                        defaultValue = "";
                    }

                    table.Columns.Add(new DBColumn(
                        rdr.GetInt32(0),
                        table.Schema,
                        table.Name,
                        rdr.GetString(1),
                        comment,
                        rdr.GetString(2).ToUpperInvariant() == "YES",
                        rdr.GetString(3),
                        defaultValue,
                        maxLen));
                }
            }
            conn.Close();
        }

        /// <summary>Scan all a table's constraint details.</summary>
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
                        table,
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

        /// <summary>Scan all a table's indexes.</summary>
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
