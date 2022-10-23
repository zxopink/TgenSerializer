using BenchmarkDotNet;
using BenchmarkDotNet.Running;
using BenchMarkTests;
using TgenSerializer;


[Serializable]
record AClass(string yep);
