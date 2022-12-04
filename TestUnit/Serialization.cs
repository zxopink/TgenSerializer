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
    public enum days : byte
    {
        sunday,
        monday,
        third
    }
    [Serializable]
    public record TestComplexClass(int num, days days, string text, string[] arr, byte val, string text2, byte[] byt, int[] ints, List<double> doubles);

    [TestFixture]
    internal class Serialization
    {
        public TestComplexClass TestClass { get; set; }
        Formatter Formatter { get; set; }
        public MemoryStream MemoryStream { get; set; }

        [SetUp]
        public void Setup()
        {
            TestClass = new(10, days.monday, "Hello world",
                new string[] { "no", "yes", "what.." },
                250,
                "second text",
                new byte[50],
                new int[80],
                new List<double>());
            Formatter = new Formatter();
            MemoryStream = new();
        }

        [TestCase]
        public void TestStreamComplexFormatting()
        {
            Formatter.Serialize(MemoryStream, TestClass);
            MemoryStream.Position = 0;
            TestComplexClass obj = (TestComplexClass)Formatter.Deserialize(MemoryStream);
            MemoryStream.Dispose();

            bool arrsEqual = TestClass.arr.SequenceEqual(obj.arr);
            bool arrsEqual2 = TestClass.ints.SequenceEqual(obj.ints);
            bool arrsEqual3 = TestClass.byt.SequenceEqual(obj.byt);
            bool arrsEqual4 = TestClass.doubles.SequenceEqual(obj.doubles);
            bool properties = TestClass.days == obj.days && TestClass.val == obj.val && TestClass.text == obj.text && TestClass.text2 == obj.text2 && TestClass.num == obj.num;
            Assert.IsTrue(arrsEqual && arrsEqual2 && arrsEqual3 && arrsEqual4 && properties, "Not equals", TestClass, obj);

        }
        [TestCase]
        public void TestComplexFormatting()
        {
            Bytes seri = Formatter.Serialize(TestClass);
            TestComplexClass obj = (TestComplexClass)Formatter.Deserialize(seri);


            bool arrsEqual = TestClass.arr.SequenceEqual(obj.arr);
            bool arrsEqual2 = TestClass.ints.SequenceEqual(obj.ints);
            bool arrsEqual3 = TestClass.byt.SequenceEqual(obj.byt);
            bool arrsEqual4 = TestClass.doubles.SequenceEqual(obj.doubles);
            bool properties = TestClass.days == obj.days && TestClass.val == obj.val && TestClass.text == obj.text && TestClass.text2 == obj.text2 && TestClass.num == obj.num;
            Assert.IsTrue(arrsEqual && arrsEqual2 && arrsEqual3 && arrsEqual4 && properties, "Not equals", TestClass, obj);
        }

    }
}
