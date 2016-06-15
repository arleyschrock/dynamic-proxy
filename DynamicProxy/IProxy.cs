namespace DynamicProxy
{
    public interface IProxy
    {
        InvocationHandler InvocationHandler { get; }
    }
}
