using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace System
{
    [TestClass]
    public class SingletonFixure
    {
        [TestMethod]
        public void SingletonUniqueSync()
        {
            Implementation sint1 = Singleton<Implementation>.Instance;
            Implementation sint2 = Singleton<Implementation>.Instance;

            Assert.ReferenceEquals(sint1, sint2);
        }

        [TestMethod]
        public async Task SingletonUniqueAsync()
        {
            Func<Implementation> factory = () => Singleton<Implementation>.Instance;

            List<Task<Implementation>> tasks = new List<Task<Implementation>>();
            int count = 1000;
            for (int i = 0; i < count; i++)
                tasks.Add(Task.Factory.StartNew<Implementation>(factory));

            var instances = await Task.WhenAll<Implementation>(tasks);

            for (int i = 0; i < count; i++)
                for (int j = 0; j < count; j++)
                {
                    Assert.ReferenceEquals(instances[i], instances[j]);
                    Assert.AreEqual(instances[i], instances[j]);
                }
        }
    }
}
