using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExampleCode.Tests
{
    [TestClass()]
    public class C1Tests
    {
        [TestMethod()]
        public void M1Test()
        {
            C1 myc1 = new C1();
            Assert.AreEqual(1, myc1.M1(1));
        }
    }
}
