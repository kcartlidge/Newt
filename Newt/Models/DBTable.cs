using Pluralize.NET;
using System.Collections.Generic;
using System.Linq;

namespace Newt.Models
{
    /// <summary>Defines a database table and it's columns, indexes, etc.</summary>
    internal class DBTable
    {
        private readonly Pluralizer _plurals;

        public string Owner { get; }
        public string Schema { get; }
        public string Name { get; }
        public List<DBColumn> Columns { get; }
        public List<DBConstraint> Constraints { get; }
        public List<DBIndex> Indexes { get; }
        public List<DBRelationship> NavigationProperties { get; }

        /// <summary>The ProperCase name of the mapped class.</summary>
        public string ClassName => Name.SnakeToProper();

        /// <summary>The ProperCase name of the mapped property, pluralised.</summary>
        public string ClassNamePlural => _plurals.Pluralize(ClassName);

        /// <summary>The width needed to allow for all column names.</summary>
        public int ColumnNameWidth => Columns.Max(x => x.Name.Length);

        /// <summary>Dot-notated schema and name.</summary>
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
            NavigationProperties = new List<DBRelationship>();
        }
        
        public override string ToString()
        {
            return $"{FullName}, columns: {Columns.Count}";
        }
    }
}