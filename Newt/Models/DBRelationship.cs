namespace Newt.Models
{
    /// <summary>Defines a database relationship.</summary>
    internal class DBRelationship
    {
        public DBTable? Table { get; init; }
        public DBConstraint? Constraint { get; set; }
    }
}