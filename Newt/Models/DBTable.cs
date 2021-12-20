using Pluralize.NET;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Newt.Models
{
    internal class DBTable
    {
        private Pluralizer plurals;

        public string Owner { get; set; }
        public string Schema { get; set; }
        public string Name { get; set; }
        public List<DBColumn> Columns { get; set; }
        public List<DBKey> Keys { get; set; }
        public List<DBIndex> Indexes { get; set; }

        public string ClassName => Name.ToProper();
        public string ClassNamePlural => plurals.Pluralize(ClassName);
        public string ClassNamePluralLower => ClassNamePlural.ToLowerInvariant();
        public string FullName => $"{Schema}.{Name}";
        public int ColumnNameWidth => Columns.Max(x => x.Name.Length);

        public DBTable(string owner, string schema, string name)
        {
            plurals = new Pluralizer();
            Owner = owner;
            Schema = schema;
            Name = name;
            Columns = new List<DBColumn>();
            Keys = new List<DBKey>();
            Indexes = new List<DBIndex>();
        }

        public string KeysAsParameters()
        {
            var result = new StringBuilder();
            foreach (var key in Keys)
            {
                var keyColumn = Columns.FirstOrDefault(x => x.Name == key.Column);
                if (keyColumn == null) continue;

                if (result.Length > 0) result.Append(", ");
                result.Append($"{keyColumn.PropertyType} {keyColumn.PropertyNameCamelCase}");
            }
            return result.ToString();
        }

        public string NonKeysAsParameters()
        {
            var result = new StringBuilder();
            foreach (var column in Columns)
            {
                if (column.IsKey) continue;

                if (result.Length > 0) result.Append(", ");
                var paramType = column.IsNullable ? $"{column.PropertyType}?" : column.PropertyType;
                result.Append($"{paramType} {column.PropertyNameCamelCase}");
            }
            return result.ToString();
        }

        /// <summary>This depends upon a SINGLE primary key!</summary>
        /// <remarks>
        /// Actually *will generate* with multiples, but the Controller routes will be strange.
        /// </remarks>
        public string KeysAsRouteParameters()
        {
            var result = new StringBuilder();
            foreach (var key in Keys)
            {
                var keyColumn = Columns.FirstOrDefault(x => x.Name == key.Column);
                if (keyColumn == null) continue;

                if (result.Length > 0) result.Append("/"); else result.Append("(\"");
                result.Append($"{{{keyColumn.PropertyNameCamelCase}:{keyColumn.PropertyType}}}");
            }
            if (result.Length > 0) result.Append("\")");
            return result.ToString();
        }

        public string KeysAsLinqClause()
        {
            var result = new StringBuilder();
            foreach (var key in Keys)
            {
                var keyColumn = Columns.FirstOrDefault(x => x.Name == key.Column);
                if (keyColumn == null) continue;

                if (result.Length > 0) result.Append(", "); else result.Append("x => ");
                result.Append($"x.{keyColumn.PropertyName} == {keyColumn.PropertyNameCamelCase}");
            }
            return result.ToString();
        }

        public override string ToString()
        {
            return $"{FullName}, columns: {Columns.Count}";
        }
    }
}