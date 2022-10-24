using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Formatter = TgenSerializer.Formatter;

namespace TestUnit
{
    [Serializable]
    public record TestComplexClass(int num, string text, string[] arr);

    [TestFixture]
    internal class Serialization
    {
        public TestComplexClass TestClass { get; set; }
        Formatter Formatter { get; set; }
        public MemoryStream MemoryStream { get; set; }

        [SetUp]
        public void Setup()
        {
            TestClass = new(10, "Hello world", new string[] { "no", "yes", "what.." });
            Formatter = new Formatter(CompressionFormat.Binary);
            MemoryStream = new();
        }

        [TestCase]
        public void TestComplexFormatting()
        {
            Formatter.Serialize(MemoryStream, TestClass);
            MemoryStream.Position = 0;
            TestComplexClass obj = (TestComplexClass)Formatter.Deserialize(MemoryStream);
            MemoryStream.Dispose();

            bool arrsEqual = TestClass.arr.SequenceEqual(obj.arr);
            bool properties = TestClass.text == obj.text && TestClass.num == obj.num;
            Assert.IsTrue(arrsEqual && properties, "Not equals",TestClass, obj);
        }

    }
}
