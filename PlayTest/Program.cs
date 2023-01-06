using TgenSerializer;
using TgenSerializer.Utils;

Bytes vals = Bytes.GetBytes(10, 20, 40, 80);
(int n10, int n20, int n40, int n80) = vals.GetTuple<(int, int, int, int)>();
Console.WriteLine();

[Serializable]
class A { public int a; public int b; public string name; }
//var valT = bytes.GetTupleUnsafe<(int,int,int)>();
//Console.WriteLine(valT); //(10, 20)