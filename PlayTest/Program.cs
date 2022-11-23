using TgenSerializer;

Bytes bytes = 10;
bytes += 20;

var valT = bytes.GetTuple<(int, int)>();
Console.WriteLine(valT); //(10, 20)