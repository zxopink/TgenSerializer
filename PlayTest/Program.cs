using TgenSerializer;

Bytes bytes = 10;
bytes += 20;

var valT = bytes.GetTupleUnsafe<(int, int)>();
Console.WriteLine(valT); //(10, 20)