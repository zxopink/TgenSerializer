using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestUnit
{
    [TestFixture]
    internal class TuplesTest
    {
        [Test]
        public void ValueTuple()
        {
            (int x, int y) val = (10, 15);
            Bytes ret = BinaryDeconstructor.Deconstruct(val);
            (int x, int y) reted = ((int x, int y))BinaryConstructor.Construct(ret);
            Assert.That(val, Is.EqualTo(reted));
        }

        [Test]
        public void Tuple()
        {
            Tuple<int, int> val = new Tuple<int, int>(10, 10);
            Bytes ret = BinaryDeconstructor.Deconstruct(val);
            Tuple<int, int> reted = (Tuple<int, int>)BinaryConstructor.Construct(ret);
            Assert.That(val, Is.EqualTo(reted));
        }
    }
}
