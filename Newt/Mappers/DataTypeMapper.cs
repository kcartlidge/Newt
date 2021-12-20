using System;

namespace Newt.Mappers
{
    internal static class DataTypeMapper
    {
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
                    throw new Exception("Unsupported column type 'bit' - use 'voolean' instead.");

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
    }
}
