using System;

namespace DynamicProxy
{
    public class InvalidAsyncException : Exception
    {
        public InvalidAsyncException()
        {
        }

        public InvalidAsyncException(string message) : base(message)
        {
        }

        public InvalidAsyncException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
