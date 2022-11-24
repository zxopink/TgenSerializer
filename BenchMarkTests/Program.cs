using BenchmarkDotNet;
using BenchmarkDotNet.Running;
using BenchMarkTests;
using System.Dynamic;
using System.Runtime.CompilerServices;
using TgenSerializer;

//var t = new TestClass(10, "Hello world", new object[] { 10, "yes", .5m });
//MemoryStream stm = new();
//Formatter f = new Formatter(CompressionFormat.Binary);
//f.Serialize(stm, t);
//stm.Position = 0;
//f.Deserialize(stm);

//(int, int, int, int) Tuple = (5,5,10,10);
//ITuple elements = Tuple;
//Bytes buf = new();
//for (int i = 0; i < elements.Length; i++)
//{
//    buf += Bytes.PrimitiveToByte(elements[i]);
//}
//Bytes TupleBytes = buf;
//var t = TupleBytes.GetTupleUnsafe<(long, long, long, long, long, long, long, long)>();

var summery = BenchmarkRunner.Run<TupleAlloc>();