using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace System.Threading.Tasks
{
    [TestClass]
    public class AsyncLazyFixture
    {
        [TestMethod]
        public void AsyncLazy_ValueFactory_NeverUsed()
        {
            var lazy = new AsyncLazy<int>(() =>
            {
                Assert.Fail();
                return 100;
            });

            Assert.IsFalse(lazy.IsValueCreated);
            Assert.IsFalse(lazy.IsSuccessfullyCompleted);
        }

        [TestMethod]
        public void AsyncLazy_ValueFactory_DefaultValue()
        {
            Implementation defaultImpl = new Implementation(Guid.Empty);
            AsyncLazy<Implementation> lazy = new AsyncLazy<Implementation>(
                () => new Implementation(Guid.NewGuid()),
                defaultImpl);

            Assert.IsFalse(lazy.IsValueCreated);
            Assert.IsFalse(lazy.IsSuccessfullyCompleted);

            Assert.AreEqual(defaultImpl, lazy.Value);
        }

        [TestMethod]
        public async Task AsyncLazy_FuncFactory_DefaultValue()
        {
            Implementation defaultImpl = new Implementation(Guid.Empty);
            AsyncLazy<Implementation> lazy = new AsyncLazy<Implementation>(
                () => Task.Factory.StartNew(() => new Implementation(Guid.NewGuid())),
                defaultImpl);

            Assert.IsFalse(lazy.IsValueCreated);
            Assert.IsFalse(lazy.IsSuccessfullyCompleted);

            Assert.AreEqual(defaultImpl, lazy.Value);

            var result = await lazy;
            Assert.AreEqual(result, lazy.Value);
            Assert.AreNotEqual(defaultImpl, result);
        }

        [TestMethod]
        public async Task AsyncLazy_MultipleAwaits()
        {
            int count = 0;
            var mre = new ManualResetEvent(false);
            Implementation defaultImpl = new Implementation(Guid.Empty);
            AsyncLazy<Implementation> lazy = new AsyncLazy<Implementation>(
                () =>
                {
                    Interlocked.Increment(ref count);
                    mre.WaitOne();
                    return new Implementation(Guid.NewGuid());
                },
                defaultImpl);

            Assert.IsFalse(lazy.IsValueCreated);
            Assert.IsFalse(lazy.IsSuccessfullyCompleted);

            var tasks = new List<Task<Implementation>>();
            for (int i = 0; i < 100; i++)
            {
                var task = Task.Factory.StartNew(async () => await lazy).Result;
                tasks.Add(task);

                Assert.IsFalse(task.IsCompleted);
            }

            mre.Set();

            var results = await Task.WhenAll(tasks);

            Assert.AreEqual(count, 1);
        }
    }
}
