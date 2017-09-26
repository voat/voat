using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Voat.Caching;

namespace Voat.Tests.Infrastructure
{
    public abstract class CacheUnitTest : BaseUnitTest
    {
        public ICacheHandler handler = null;
        public string name = null;

        public CacheUnitTest(ICacheHandler handler, string name)
        {
            this.handler = handler;
            this.name = name;

        }
        public void VerifyValidHandler()
        {
            if (handler == null)
            {
                Assert.Inconclusive($"Handler type: {name} is not instantiated");
            }
        }
        public async Task VerifyValidHandlerAsync()
        {
            await Task.Run(() =>
            {
                if (handler == null)
                {
                    Assert.Inconclusive($"Handler type: {name} is not instantiated");
                }
            });
        }
    }
}
