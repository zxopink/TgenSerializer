using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TgenSerializer;

namespace BenchMarkTests
{
    [MemoryDiagnoser]
    public class Casting
    {
        public int GenericParam { get; set; } = 10;
        public object ObjectParam { get; set; } = 10;

        public byte[] ByteParam { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            ByteParam = Bytes.P2B(100);
        }

        [Benchmark]
        public byte[] GenericCastingToByte() => Bytes.PrimitiveToByte(GenericParam);
        [Benchmark]
        public byte[] ObjectCastingToByte() => Bytes.PrimitiveToByte(ObjectParam);

        [Benchmark]
        public int GenericCastingToValue() => Bytes.ByteToPrimitive<int>(ByteParam);
        [Benchmark]
        public object ObjectCastingToValue() => Bytes.ByteToPrimitive(typeof(int), ByteParam);

        /*
         *  |                Method |      Mean |     Error |    StdDev |   Gen0 | Allocated |
         *  |---------------------- |----------:|----------:|----------:|-------:|----------:|
         *  |  GenericCastingToByte |  5.511 ns | 0.0868 ns | 0.0725 ns | 0.0076 |      32 B |
         *  |   ObjectCastingToByte | 10.271 ns | 0.1376 ns | 0.1074 ns | 0.0076 |      32 B |
         *  | GenericCastingToValue | 17.898 ns | 0.3283 ns | 0.3071 ns |      - |         - |
         *  |  ObjectCastingToValue | 23.787 ns | 0.2689 ns | 0.2384 ns | 0.0057 |      24 B |
         */
    }
}
