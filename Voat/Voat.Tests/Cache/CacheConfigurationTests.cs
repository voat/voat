using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Voat.Tests.Cache
{
    [TestClass]
    public class CacheConfigurationTests : BaseUnitTest
    {
        [TestMethod]
        public void Can_Read_Config()
        {
            var section = Voat.Caching.CacheHandlerSection.Instance;
            Assert.IsNotNull(section, "Caching Section is null");
            Assert.AreEqual(3, section.Handlers.Length);
            Assert.IsNotNull(section.Handler, "Enabled handler null");
        }
    }
}
