using System.Reflection;

namespace DynamicProxy
{
    public delegate bool ProxyPredicate<in T>(T target, MethodInfo method, PropertyInfo property);
}
