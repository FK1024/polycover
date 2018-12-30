using Microsoft.VisualStudio.TestTools.UnitTesting;
using ExampleCode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
