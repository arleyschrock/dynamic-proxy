using System;
using System.Collections.Generic;
using System.Text;

namespace DynamicProxy
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class ProxyForAttribute : Attribute
    {
        public Type Type { get; }

        public ProxyForAttribute(Type type)
        {
            Type = type;
        }
    }
}
