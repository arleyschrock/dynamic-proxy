using System;

namespace DynamicProxy
{
    public interface IProxyTypeFactory
    {
        Type CreateProxyType(Type sourceType);
    }
}
