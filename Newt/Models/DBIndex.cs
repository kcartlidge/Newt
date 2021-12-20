namespace Newt.Models
{
    internal class DBIndex
    {
        public string Name { get; set; }
        public string Schema { get; set; }
        public string Table { get; set; }
        public string Definition { get; set; }

        public string FullName => $"{Schema}.{Name}";

        public DBIndex(string name, string schema, string table, string column)
        {
            Name = name;
            Schema = schema;
            Table = table;
            Definition = column;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}