using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestUnit
{
    [Serializable]
    record A(int numA);

    [Serializable]
    record B(int numA, int numB) : A(numA);

    [Serializable]
    record C(int numA, int numB, int numC) : B(numA, numB);

    [Serializable]
    record TestClass();


    [TestFixture]
    internal class BinaryFormatterTest
    {
        [SetUp]
        public void Setup()
        {
            TestClass = new TestClass();
            InheritanceTest = new(10, 2, 5);
            DowngradeTest = new(12, -8,645);
        }

        private static TestClass TestClass;
        [Test]
        public void Basic()
        {
            TestClass test = TestClass;
            byte[] bytes = Serialize(test);
            var result = Deserialize<TestClass>(bytes);
            Assert.That(test, Is.EqualTo(result));
        }

        private static C InheritanceTest;
        [Test]
        public void Inheritance()
        {
            C test = InheritanceTest;
            byte[] bytes = Serialize(test);
            var result = Deserialize<C>(bytes);
            Assert.That(test, Is.EqualTo(result));
        }

        private static C DowngradeTest;
        [Test]
        public void Downgrade()
        {
            C test = DowngradeTest;
            byte[] bytes = Serialize(test);
            var result = Deserialize<A>(bytes);
            Assert.That(test, Is.EqualTo(result));
        }

        private byte[] Serialize(object obj) => BinaryDeconstructor.Deconstruct(obj);
        private T Deserialize<T>(byte[] graph) => (T)BinaryConstructor.Construct(graph);
    }
}
