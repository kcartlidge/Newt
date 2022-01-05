namespace Newt.Models
{
    internal class DBRelationship
    {
        public DBTable? Table { get; init; }
        public DBConstraint? Constraint { get; set; }
    }
}