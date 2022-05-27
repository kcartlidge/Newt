namespace Newt.Models
{
    /// <summary>Defines a database index.</summary>
    internal class DBIndex
    {
        public string Definition { get; set; }

        private string Name { get; }

        public DBIndex(string name, string definition)
        {
            Name = name;
            Definition = definition;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}