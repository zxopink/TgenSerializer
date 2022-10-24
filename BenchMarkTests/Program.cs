using BenchmarkDotNet;
using BenchmarkDotNet.Running;
using BenchMarkTests;
using TgenSerializer;

//var t = new TestClass(10, "Hello world", new object[] { 10, "yes", .5m });
//MemoryStream stm = new();
//Formatter f = new Formatter(CompressionFormat.Binary);
//f.Serialize(stm, t);
//stm.Position = 0;
//f.Deserialize(stm);

var summery = BenchmarkRunner.Run<Serialization>();