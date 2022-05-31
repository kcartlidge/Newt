using System.Linq;
using Newt.Mappers;
using System.Text;

namespace Newt.Models
{
    /// <summary>Defines a database column.</summary>
    internal class DBColumn
    {
        public int Sequence { get; set; }
        public string Schema { get; set; }
        public string Table { get; set; }
        public string Name { get; set; }
        public string Comment { get; set; }
        public string Datatype { get; set; }
        public string DefaultValue { get; set; }
        public int? Capacity { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool IsNullable { get; set; }

        /// <summary>The ProperCase name of the mapped property.</summary>
        public string PropertyName => Name.SnakeToProper();

        /// <summary>The mapped property's .Net data type.</summary>
        public string PropertyType => Datatype.ToDotNetAndDbType();

        /// <summary>Dot-notated schema, table, and name.</summary>
        public string FullName => $"{Schema}.{Table}.{Name}";

        /// <summary>True if Capacity holds a usable value.</summary>
        public bool UseCapacity => Capacity.HasValue && PropertyType != "bool";

        /// <summary>True if the column has a default value.</summary>
        public bool HasDefault => DefaultValue.HasValue();

        public DBColumn(
            int sequence,
            string schema,
            string table,
            string name,
            string comment,
            bool isNullable,
            string datatype,
            string defaultValue,
            int? capacity)
        {
            Sequence = sequence;
            Schema = schema;
            Table = table;
            Name = name;
            Comment = comment ?? "";
            IsNullable = isNullable;
            Datatype = datatype;
            DefaultValue = defaultValue;
            Capacity = capacity;
        }

        /// <summary>Does this property map to a whole number type?</summary>
        public bool IsCardinal
        {
            get
            {
                switch (Datatype.ToLowerInvariant())
                {
                    case "smallint":
                    case "integer":
                    case "bigint":
                    case "serial":
                    case "bigserial":
                        return true;
                    default:
                        return false;
                }
            }
        }

        /// <summary>Generate the .Net property source for this column.</summary>
        public string AsCode()
        {
            var src = new StringBuilder();
            src.AppendLine($"        /// <summary>");
            if (Comment.HasValue()) src.AppendLine($"        /// {Comment}");
            src.AppendLine($"        /// Database column `{Name}` of type `{Datatype}`.");
            if (UseCapacity) src.AppendLine($"        /// The capacity is {Capacity}.");
            src.AppendLine($"        /// </summary>");
            if (UseCapacity) src.AppendLine($"        [MaxLength({Capacity})]");
            if (IsPrimaryKey) src.AppendLine($"        [Key]");
            if (!IsNullable) src.AppendLine($"        [Required]");
            src.AppendLine($"        [Column(\"{Name}\")]");
            src.AppendLine($"        [DisplayName(\"{Name.SnakeToProper(true)}\")]");
            var nullableProp = (IsNullable ? "?" : "");
            src.AppendLine($"        public {PropertyType}{nullableProp} {PropertyName} {{ get; set; }}");
            return src.ToString();
        }

        public override string ToString()
        {
            var nul = IsNullable ? string.Empty : " not null";
            var cap = UseCapacity ? $"({Capacity})" : string.Empty;
            return $"{Sequence}. {FullName} ({PropertyName}) - {Datatype}{cap}{nul}";
        }
    }
}