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
        public TgenFormatter Formatter = new();
        [Test]
        public void ValueTuple()
        {
            (int x, int y) val = (10, 15);
            Bytes ret = Formatter.Serialize(val);
            (int x, int y) reted = Formatter.Deserialize<(int x, int y)>(ret);
            Assert.That(val, Is.EqualTo(reted));
        }

        [Test]
        public void Tuple()
        {
            Tuple<int, int> val = new Tuple<int, int>(10, 10);
            Bytes ret = Formatter.Serialize(val);
            Tuple<int, int> reted = Formatter.Deserialize<Tuple<int, int>> (ret);
            Assert.That(val, Is.EqualTo(reted));
        }
    }
}
