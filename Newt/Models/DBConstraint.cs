﻿using System;
using System.Text;

namespace Newt.Models
{
    /// <summary>Defines a database constraint.</summary>
    internal class DBConstraint
    {
        public string Name { get; set; }
        public DBTable Table { get; set; }
        public string Column { get; set; }
        public string ForeignTable { get; set; }
        public string ForeignColumn { get; set; }
        public bool IsPrimaryKey => ConstraintType?.ToLower() == "primary key";
        public bool IsForeignKey  => ConstraintType?.ToLower() == "foreign key";
        public bool IsUniqueKey  => ConstraintType?.ToLower() == "unique";

        private string ConstraintType { get; set; }
        private string Schema { get; set; }

        /// <summary>Dot-notated schema and name.</summary>
        private string FullName => $"{Schema}.{Name}";

        public DBConstraint(string name, string schema, DBTable table, string column, string constraintType)
        {
            Name = name;
            Schema = schema;
            Table = table;
            Column = column;
            ConstraintType = constraintType;
            ForeignTable = ForeignColumn = String.Empty;
        }

        /// <summary>Generate some SQL source for this constraint.</summary>
        public string GetDefinition(bool withTrailingComma)
        {
            var src = new StringBuilder("  CONSTRAINT ");
            src.Append($"{Name} {ConstraintType} ({Column})");
            if (IsForeignKey)
            {
                src.AppendLine();
                src.AppendLine($"    REFERENCES {Schema}.{ForeignTable} ({ForeignColumn}) MATCH SIMPLE ");
                src.Append("    ON UPDATE NO ACTION ON DELETE NO ACTION");
            }

            src.Append(withTrailingComma ? "," : "");
            return src.ToString();
        }

        public override string ToString()
        {
            return $"{ConstraintType} {FullName} => {Schema}.{Table}.{Column}";
        }
    }
}