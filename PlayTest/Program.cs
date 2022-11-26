using TgenSerializer;

Bytes bytes = 10;
bytes += 20;
bytes += 50;

var valT = bytes.GetTupleUnsafe<(int,int,int)>();
Console.WriteLine(valT); //(10, 20)