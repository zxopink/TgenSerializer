using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestUnit
{
    [TestFixture]
    internal class BytesTest
    {
        [SetUp]
        public void Setup()
        {

        }

        [TestCase(5)]
        public void TestInt(int number)
        {
            byte[] bytes = Bytes.P2B(number);
            int after = Bytes.B2P<int>(bytes);
            Assert.That(after, Is.EqualTo(number));
        }
    }
}
