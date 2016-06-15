using System;

namespace DynamicProxy
{
    [AttributeUsage(AttributeTargets.Method)]
    public class OriginalMethodAttribute : Attribute
    {
        public string Name { get; }

        public OriginalMethodAttribute(string name)
        {
            Name = name;
        }
    }
}
