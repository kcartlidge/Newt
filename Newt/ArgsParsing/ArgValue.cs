namespace Newt.ArgsParsing
{
    internal enum ArgType
    {
        Unknown, Skip, Option, Parameter
    }

    internal class ArgValue
    {
        public int Sequence;
        public string Original;
        public ArgType ArgType;
        public string Name;
        public string Value;
        public bool HasDash;

        public ArgValue(int sequence, string original, ArgType argType)
        {
            Sequence = sequence;
            ArgType = argType;

            Original = original ?? "".Trim();
            if (Original.StartsWith("-"))
            {
                HasDash = true;
                Original = Original.TrimStart('-').Trim();
            }

            Name = Original.ToLowerInvariant();
            Value = string.Empty;
        }

        public override string ToString()
        {
            if (ArgType == ArgType.Parameter)
                return $"{Sequence} ({ArgType}) => {Original} = {Value}".Trim();
            else
                return $"{Sequence} ({ArgType}) => {Original}".Trim();
        }
    }
}