using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExampleCode5.Tests
{
    [TestClass()]
    public class ProgTests
    {
        [TestMethod()]
        public void MTest1()
        {
            A a = new A();
            Assert.AreEqual(1, a.M());
        }

        [TestMethod()]
        public void MTest2()
        {
            B b = new B();
            Assert.AreEqual(2, b.M());
        }
    }
}
