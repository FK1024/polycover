using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExampleCode6.Tests
{
    [TestClass()]
    public class ATests
    {
        [TestMethod()]
        public void NTest()
        {
            Assert.AreEqual(2, new A().N(true));
        }
    }
}
