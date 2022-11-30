using TgenSerializer;

Bytes bytes = 10;
bytes += 20;
bytes += 50;

int[] arr = new int[10];
arr[4] = 5;
byte[] bt = new byte[40];
int len = Buffer.ByteLength(arr);
Console.WriteLine();
//var valT = bytes.GetTupleUnsafe<(int,int,int)>();
//Console.WriteLine(valT); //(10, 20)