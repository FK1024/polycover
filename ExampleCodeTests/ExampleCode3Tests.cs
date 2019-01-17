using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExampleCode3.Tests
{
    [TestClass()]
    public class ProgTests
    {
        [TestMethod()]
        public void m1Test()
        {
            IA myClass = new A();
            IA myParam = new B();
            Assert.AreEqual(2, myClass.m(myParam));
        }
    }
}
