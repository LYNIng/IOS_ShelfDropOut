using System;

namespace GenerateCode
{
    [AttributeUsage(AttributeTargets.Class)]
    public class GenerateCustomDataCodeClass : Attribute
    {
        public string ClassName { get; private set; }
        public GenerateCustomDataCodeClass(string className)
        {
            ClassName = className ?? string.Empty;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class GenerateCustomValue : Attribute
    {

    }

    
}