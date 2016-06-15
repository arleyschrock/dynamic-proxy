﻿using System;
using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;

namespace DynamicProxy.Emit.Tests
{
    [TestFixture]
    public class GenericsTests 
    {
        [Test]
        public async void Get()
        {
            var proxy = Proxy.CreateProxy<ICrudApi<User>>(async invocation =>
            {
                await invocation.Proceed();
                var id = (int)invocation.Arguments[0];
                return new User { Id = id, FirstName = "John", LastName = "Doe" };
            });
            var user = await proxy.Get(5);
            Assert.AreEqual(5, user.Id);
            Assert.AreEqual("John", user.FirstName);
            Assert.AreEqual("Doe", user.LastName);
        }

        public class User
        {
            public int Id { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
        }

        public interface ICrudApi<T> 
        {
            Task<T> Get(int id);
            Task<T[]> GetAll();
        }
    }
}