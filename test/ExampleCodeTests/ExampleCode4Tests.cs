using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExampleCode4.Tests
{
    [TestClass()]
    public class ProgTests
    {
        [TestMethod()]
        public void M1Test()
        {
            Prog myProg = new Prog();
            C myParam = new C();
            Assert.AreEqual(3, myProg.M1(myParam));
        }
    }
}
