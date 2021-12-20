using Pluralize.NET;
using System.Collections.Generic;
using System.Linq;

namespace Newt.Models
{
    internal class DBTable
    {
        private readonly Pluralizer _plurals;

        public string Owner { get; }
        public string Schema { get; }
        public string Name { get; }
        public List<DBColumn> Columns { get; }
        public List<DBConstraint> Constraints { get; }
        public List<DBIndex> Indexes { get; }

        public string ClassName => Name.ToProper();
        public string ClassNamePlural => _plurals.Pluralize(ClassName);
        public int ColumnNameWidth => Columns.Max(x => x.Name.Length);

        private string FullName => $"{Schema}.{Name}";

        public DBTable(string owner, string schema, string name)
        {
            _plurals = new Pluralizer();
            Owner = owner;
            Schema = schema;
            Name = name;
            Columns = new List<DBColumn>();
            Constraints = new List<DBConstraint>();
            Indexes = new List<DBIndex>();
        }
        
        public override string ToString()
        {
            return $"{FullName}, columns: {Columns.Count}";
        }
    }
}