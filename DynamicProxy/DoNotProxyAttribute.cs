using System;

namespace DynamicProxy
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
    public class DoNotProxyAttribute : Attribute
    {
    }
}
