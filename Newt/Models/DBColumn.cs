using System.Linq;
using Newt.Mappers;
using System.Text;

namespace Newt.Models
{
    internal class DBColumn
    {
        public int Sequence { get; set; }
        public string Schema { get; set; }
        public string Table { get; set; }
        public string Name { get; set; }
        public string Datatype { get; set; }
        public int? Capacity { get; set; }
        public bool IsKey { get; set; }
        public bool IsNullable { get; set; }

        public string PropertyName => Name.ToProper();
        public string PropertyNameCamelCase => $"{PropertyName.ToLower().First()}{PropertyName.Remove(0, 1)}";
        public string PropertyType => Datatype.ToDotNetAndDbType();
        public string DbType => Datatype.ToDotNetAndDbType();
        public string FullName => $"{Schema}.{Table}.{Name}";

        public DBColumn(
            int sequence,
            string schema,
            string table,
            string name,
            bool isNullable,
            string datatype,
            int? capacity)
        {
            Sequence = sequence;
            Schema = schema;
            Table = table;
            Name = name;
            IsNullable = isNullable;
            Datatype = datatype;
            Capacity = capacity;
        }

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

        public string AsCode(bool lowLevel = true)
        {
            var src = new StringBuilder();
            if (lowLevel) src.AppendLine($"        /// <summary>Column `{Name}` of type `{Datatype}`</summary>");
            if (Capacity.HasValue && PropertyType != "bool")
            {
                if (lowLevel) src.AppendLine($"        /// <remarks>This column has a capacity of {Capacity}.</remarks>");
                src.AppendLine($"        [MaxLength({Capacity})]");
            }
            if (lowLevel && IsKey) src.AppendLine($"        [Key]");
            if (!IsNullable) src.AppendLine($"        [Required]");
            if (lowLevel) src.AppendLine($"        [Column(\"{Name}\")]");
            src.AppendLine($"        [DisplayName(\"{Name.ToProper(true)}\")]");
            var nullableProp = (IsNullable ? "?" : "");
            src.AppendLine($"        public {PropertyType}{nullableProp} {PropertyName} {{ get; set; }}");
            return src.ToString();
        }

        public override string ToString()
        {
            var nul = IsNullable ? string.Empty : " not null";
            var cap = Capacity.HasValue ? $"({Capacity})" : string.Empty;
            return $"{Sequence}. {FullName} ({PropertyName}) - {Datatype}{cap}{nul}";
        }
    }
}