﻿using System.Threading.Tasks;

namespace DynamicProxy.Fody.Tests
{
    public class HandWrittenProxy : IHandWritten
    {
        private IHandWritten target;
        private InvocationHandler invocationHandler;

        public HandWrittenProxy(IHandWritten target, InvocationHandler invocationHandler)
        {
            this.target = target;
            this.invocationHandler = invocationHandler;
        }

        public Task<string> GetStringAsync()
        {
            var method = typeof(IHandWritten).GetMethod("GetStringAsync");
            var arguments = new object[0];
            var invocation = new AsyncInvocationT<string>(this, invocationHandler, method, null, arguments, args => target.GetStringAsync());
            return invocationHandler.AsyncInvokeT(invocation);
        }

        public Task DoSomethingAsync()
        {
            var method = typeof(IHandWritten).GetMethod("DoSomethingAsync");
            var arguments = new object[0];
            var invocation = new VoidAsyncInvocation(this, invocationHandler, method, null, arguments, args => target.DoSomethingAsync());
            return invocationHandler.VoidAsyncInvoke(invocation);
        }

        public void DoSomething()
        {
            var method = typeof(IHandWritten).GetMethod("DoSomething");
            var arguments = new object[0];
            var invocation = new VoidInvocation(this, invocationHandler, method, null, arguments, args => target.DoSomething());
            invocationHandler.VoidInvoke(invocation);
        }

        public string GetString()
        {
            var method = typeof(IHandWritten).GetMethod("GetString");
            var arguments = new object[0];
            var invocation = new InvocationT<string>(this, invocationHandler, method, null, arguments, args => target.GetString());
            return invocationHandler.InvokeT(invocation);
        }

        public int Sum(int first, int second)
        {
            var method = typeof(IHandWritten).GetMethod("Sum");
            var arguments = new object[] { first, second };
            var invocation = new InvocationT<int>(this, invocationHandler, method, null, arguments, args => target.Sum((int)args.Arguments[0], (int)args.Arguments[1]));
            return invocationHandler.InvokeT(invocation);
        }

        public Task<int> SumAsync(int first, int second)
        {
            var method = typeof(IHandWritten).GetMethod("SumAsync");
            var arguments = new object[] { first, second };
            var invocation = new AsyncInvocationT<int>(this, invocationHandler, method, null, arguments, args => target.SumAsync((int)args.Arguments[0], (int)args.Arguments[1]));
            return invocationHandler.AsyncInvokeT(invocation);
        }

        public string StringProperty
        {
            get
            {
                var property = typeof(IHandWritten).GetProperty("StringProperty");
                var method = property.GetMethod;
                var arguments = new object[0];
                var invocation = new InvocationT<string>(this, invocationHandler, method, property, arguments, args => target.StringProperty);
                return invocationHandler.InvokeT(invocation);
            }
            set
            {
                var property = typeof(IHandWritten).GetProperty("StringProperty");
                var method = property.GetMethod;
                var arguments = new object[] { value };
                var invocation = new VoidInvocation(this, invocationHandler, method, property, arguments, args => target.StringProperty = (string)args.Arguments[0]);
                invocationHandler.VoidInvoke(invocation);                
            }
        }
    }
}