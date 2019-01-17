using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExampleCode4.Tests
{
    [TestClass()]
    public class ProgTests
    {
        [TestMethod()]
        public void m1Test()
        {
            Prog myProg = new Prog();
            C myParam = new C();
            Assert.AreEqual(3, myProg.m1(myParam));
        }
    }
}
