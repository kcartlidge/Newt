using System.Collections.Generic;
using System.Linq;

namespace Newt.Models
{
    internal class DBSchema
    {
        public string Schema { get; internal set; }
        public string DatabaseName { get; set; }
        public List<DBTable> Tables { get; set; }

        public string FullName => $"{DatabaseName}.{Schema}";
        public int TableNameWidth => Tables.Max(x => x.Name.Length);
        public int TableFullNameWidth => Tables.Max(x => x.FullName.Length);
        public int TableClassNameWidth => Tables.Max(x => x.ClassName.Length);

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