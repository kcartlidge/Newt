using System;

namespace Newt.ArgsParsing
{
    /// <summary>Specifies a single command argument expectation/requirement.</summary>
    internal class ArgumentDefinition
    {
        public readonly Type DotNetType;
        public readonly bool IsRequired;
        public readonly string Info;
        public readonly object? DefaultValue;

        /// <summary>Create a new argument definition.</summary>
        /// <param name="type">The .Net type it maps to.</param>
        public ArgumentDefinition(Type type, bool isRequired, string info, object? defaultValue)
        {
            this.DotNetType = type;
            this.IsRequired = isRequired;
            this.Info = info;
            this.DefaultValue = defaultValue;
        }
    }
}