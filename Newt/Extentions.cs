using System.Text;

namespace Newt
{
    internal static class Extentions
    {
        /// <summary>Returns true if the provided string has a value.</summary>
        public static bool HasValue(this string value)
        {
            return string.IsNullOrEmpty(value) == false;
        }

        /// <summary>
        /// Returns a 'snake_case' string value in 'ProperCase' format.
        /// The string is expected to contain words separated by underscores.
        /// This matches the naming conventionals in Postgres.
        /// </summary>
        /// <param name="value">Value containg mixed case and/or underscores.</param>
        /// <param name="displayable">
        /// If true then words will be separated by a space.
        /// Otherwise words will have no separators at all.
        /// </param>
        public static string SnakeToProper(this string value, bool displayable = false)
        {
            var s = new StringBuilder();
            var needsUpper = true;

            // Assume everything is lowercase.
            foreach (var ch in value.ToLowerInvariant())
            {
                // Underscores separate words, so the next should be uppercase.
                if (ch == '_') needsUpper = true;
                else
                {
                    // Not an underscore.
                    if (needsUpper)
                    {
                        // Separate words according to 'displayable'.
                        if (displayable) s.Append(' ');
                        s.Append(ch.ToString().ToUpperInvariant());
                    }
                    else s.Append(ch);
                    needsUpper = false;
                }
            }
            var result = s.ToString().Trim();

            // Treat IDs as special cases.
            if (result.ToLowerInvariant() == "id") return "Id";

            return result.Trim();
        }
    }
}