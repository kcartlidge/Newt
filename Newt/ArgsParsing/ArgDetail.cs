using System;

namespace Newt.ArgsParsing
{
    public class ArgDetail
    {
        public readonly Type ArgType;
        public readonly bool IsRequired;
        public readonly string Info;
        public readonly object? DefaultValue;
        
        public ArgDetail(Type type, bool isRequired, string info, object? defaultValue)
        {
            this.ArgType = type;
            this.IsRequired = isRequired;
            this.Info = info;
            this.DefaultValue = defaultValue;
        }
    }
}