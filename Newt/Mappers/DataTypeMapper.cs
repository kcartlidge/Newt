using System;

namespace Newt.Mappers
{
    /// <summary>Data type mappers. Obviously.</summary>
    internal static class DataTypeMapper
    {
        /// <summary>Converts from a Postgres database type to a .Net one.</summary>
        public static string ToDotNetAndDbType(this string databaseType)
        {
            switch (databaseType.Trim().ToLowerInvariant())
            {
                case "smallint":
                case "smallserial":
                    return "short";
                case "integer":
                case "serial":
                    return "int";
                case "bigint":
                case "bigserial":
                    return "long";
                case "decimal":
                case "numeric":
                case "money":
                    return "decimal";
                case "real":
                    return "float";
                case "double precision":
                    return "double";

                case "bytea":
                    return "byte[]";

                case "character varying":
                case "varchar":
                case "character":
                case "char":
                case "text":
                    return "string";

                case "boolean":
                    return "bool";

                case "bit":
                    throw new Exception("Unsupported column type 'bit' - use 'boolean' instead.");

                case "timestamp":
                case "timestamptz":
                case "timestamp with time zone":
                case "timestamp without time zone":
                case "date":
                case "time":
                case "time with time zone":
                case "time without time zone":
                    return "DateTime";

                case "interval":
                    return "TimeSpan";

                case "uuid":
                    return "Guid";

                case "json":
                case "jsonb":
                    return "string";

                case "xml":
                    return "string";

                default:
                    return "object";
            }
        }

        /// <summary>Get CSS column width.</summary>
        public static string GetCssColWidth(
            this string dotnetType,
            bool useCapacity,
            int? capacity)
        {
            switch (dotnetType.Trim().ToLowerInvariant())
            {
                case "short":
                case "int":
                case "long":
                case "decimal":
                case "float":
                case "double":
                case "bool":
                    return "narrow";
                case "datetime":
                case "timespan":
                    return "medium";
                case "string":
                    if (useCapacity && capacity != null)
                    {
                        if (capacity > 20) return "wide";
                        if (capacity > 10) return "medium";
                        return "narrow";
                    }
                    return "wide";
                default:
                    return "wide";
            }
        }
    }
}
