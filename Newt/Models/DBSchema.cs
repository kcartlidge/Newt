using System.Collections.Generic;

namespace Newt.Models
{
    internal class DBSchema
    {
        public string Schema { get; internal init; }
        public string DatabaseName { get; init; }
        public List<DBTable> Tables { get; }

        private string FullName => $"{DatabaseName}.{Schema}";

        public DBSchema()
        {
            Schema = DatabaseName = string.Empty;
            Tables = new List<DBTable>();
        }

        public override string ToString()
        {
            return $"{FullName}, tables:{Tables.Count}";
        }
    }
}