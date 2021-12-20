using System.Text;

namespace Newt
{
    internal static class Extentions
    {
        public static bool HasValue(this string value)
        {
            return string.IsNullOrEmpty(value) == false;
        }

        public static string ToProper(this string value, bool displayable = false)
        {
            var s = new StringBuilder();

            var needsUpper = true;
            foreach (var ch in value.ToLowerInvariant())
            {
                if (ch == '_') needsUpper = true;
                else
                {
                    if (needsUpper)
                    {
                        if (displayable) s.Append(' ');
                        s.Append(ch.ToString().ToUpperInvariant());
                    }
                    else s.Append(ch);
                    needsUpper = false;
                }
            }
            var result = s.ToString().Trim();
            if (result.ToLowerInvariant() == "id") return "Id";
            return result.Trim();
        }
    }
}