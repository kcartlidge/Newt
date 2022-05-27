namespace Newt.ArgsParsing
{
    internal enum EntryKinds
    {
        Unknown, Skip, Option, Parameter
    }

    /// <summary>Holds details of a matched command argument.</summary>
    internal class MatchedValue
    {
        public int Sequence;
        public string Original;
        public EntryKinds KindOfEntry;
        public string Name;
        public string Value;
        public bool HasDash;

        /// <summary>Create a record of a matched command argument.</summary>
        /// <param name="sequence">The argument position.</param>
        /// <param name="original">The original value.</param>
        /// <param name="kindOfEntry">The type of argument.</param>
        public MatchedValue(int sequence, string original, EntryKinds kindOfEntry)
        {
            Sequence = sequence;
            KindOfEntry = kindOfEntry;

            // Note, then remove, the leading prefix.
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
            if (KindOfEntry == EntryKinds.Parameter)
                return $"{Sequence} ({KindOfEntry}) => {Original} = {Value}".Trim();
            else
                return $"{Sequence} ({KindOfEntry}) => {Original}".Trim();
        }
    }
}