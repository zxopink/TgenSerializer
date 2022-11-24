using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TgenSerializer;

namespace BenchMarkTests
{
    [MemoryDiagnoser]
    public class TupleAlloc
    {
        public (int,double, float, long, long) Tuple { get; set; }
        public Bytes TupleBytes { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            Tuple = (10, 20.5, 30.8f, 2400, 4200);
            ITuple elements = Tuple;
            Bytes buf = new();
            for (int i = 0; i < elements.Length; i++)
            {
                buf += Bytes.PrimitiveToByte(elements[i]);
            }
            TupleBytes = buf;
        }

        [Benchmark]
        public void SafeTuple()
        {
            (int, double, float, long, long) val =
                TupleBytes.GetTuple<(int, double, float, long, long)>();
        }
#if AllowUnsafe
        [Benchmark]
        public void UnsafeTuple()
        {

            (int, double, float, long, long) val =
                TupleBytes.GetTupleUnsafe<(int, double, float, long, long)>();
        }
#endif

        //|      Method |       Mean |     Error |    StdDev |   Gen0 | Allocated |
        //|------------ |-----------:|----------:|----------:|-------:|----------:|
        //|   SafeTuple | 847.801 ns | 5.6127 ns | 4.9755 ns | 0.0553 |     232 B |
        //| UnsafeTuple |   6.485 ns | 0.1271 ns | 0.1189 ns |      - |         - |
    }
}
