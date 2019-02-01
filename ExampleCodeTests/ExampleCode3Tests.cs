using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExampleCode3.Tests
{
    [TestClass()]
    public class ProgTests
    {
        [TestMethod()]
        public void MTest1()
        {
            IA myClass = new A();
            IA myParam = new B();
            Assert.AreEqual(2, myClass.M(myParam));
        }

        [TestMethod()]
        public void MTest2()
        {
            IA myClass = new A();
            IA myParam = new D();
            Assert.AreEqual(4, myClass.M(myParam));
        }
    }
}
