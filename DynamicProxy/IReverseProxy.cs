namespace DynamicProxy
{
    public interface IReverseProxy
    {
        InvocationHandler InvocationHandler { get; }
    }
}