using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet;
using BenchmarkDotNet.Attributes;
using TgenSerializer;
using Formatter = TgenSerializer.Formatter;

namespace BenchMarkTests
{
    [Serializable]
    public record TestClass(int num, string text, object[] arr);
    [MemoryDiagnoser]
    public class Serialization
    {
        public TestClass TestClass { get; set; }
        IFormatter Formatter { get; set; }
        public MemoryStream MemoryStream { get; set; }
        public Serialization()
        {
            TestClass = new(10, "Hello world", new object[] { 10, "yes", .5m });
        }
        [GlobalSetup(Targets = new[]
        { nameof(TgenSerialize), nameof(TgenDeserialize) })]
        public void TgenSetup()
        {
            Formatter = new Formatter(CompressionFormat.Binary);
        }
        [GlobalSetup(Targets = new[]
        { nameof(BinarySerialize), nameof(BinaryDeserialize) })]
        public void BinarySetup()
        {
            Formatter = new BinaryFormatter();
        }

        [IterationCleanup]
        public void IterationClean() =>
            MemoryStream.Dispose();

        [IterationSetup]
        public void IterationSetup()
        {
            MemoryStream = new();
            Formatter.Serialize(MemoryStream, TestClass);
            MemoryStream.Position = 0;
        }

        [Benchmark]
        public void TgenSerialize()
        {
            Formatter.Serialize(MemoryStream, TestClass);
        }

        [Benchmark]
        public void TgenDeserialize()
        {
            Formatter.Deserialize(MemoryStream);
        }

        [Benchmark]
        public void BinarySerialize()
        {
            Formatter.Serialize(MemoryStream, TestClass);
        }

        [Benchmark]
        public void BinaryDeserialize()
        {
            Formatter.Deserialize(MemoryStream);
        }
        //|            Method |      Mean |    Error |   StdDev |    Median | Allocated |
        //|------------------ |----------:|---------:|---------:|----------:|----------:|
        //|     TgenSerialize | 126.30 us | 6.667 us | 18.48 us | 120.50 us |  16.97 KB |
        //|   TgenDeserialize |  95.43 us | 7.142 us | 20.83 us |  89.60 us |   3.82 KB |
        //|   BinarySerialize |  58.88 us | 5.125 us | 14.79 us |  55.25 us |    4.9 KB |
        //| BinaryDeserialize |  57.35 us | 4.707 us | 13.51 us |  54.00 us |   7.13 KB |
    }
}
