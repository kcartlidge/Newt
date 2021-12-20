namespace Newt.Models
{
    internal class DBKey
    {
        public string Name { get; set; }
        public string Schema { get; set; }
        public string Table { get; set; }
        public string Column { get; set; }

        public string FullName => $"{Schema}.{Name}";

        public DBKey(string name, string schema, string table, string column)
        {
            Name = name;
            Schema = schema;
            Table = table;
            Column = column;
        }

        public override string ToString()
        {
            return $"{FullName} => {Schema}.{Table}.{Column}";
        }
    }
}