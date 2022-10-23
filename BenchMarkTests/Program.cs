using BenchmarkDotNet;
using BenchmarkDotNet.Running;
using BenchMarkTests;

var summary = BenchmarkRunner.Run<Casting>();
Console.WriteLine(summary);