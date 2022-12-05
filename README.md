# TgenSerializer
[![NuGet Package][NuGet]][NuGet-url]

TgenSerializer is a simple serializer for CSharp (dotnet standard 2.0).
The library also includes a few utility classes for work with byte array (`byte[]`)

It makes an object graph of serializeable objects then returns the result as Bytes or into a stream.
## Serialize/Deserialize
```cs
A a = new A()
{
    a = 5,
    b = 10,
    name = "foo"
};
TgenFormatter formatter = new TgenFormatter();
Bytes b = formatter.Serialize(a);
byte[] data = b.Buffer; //Can be converted to byte[]
A ret = formatter.Deserialize<A>(data);

[Serializable]
class A { public int a; public int b; public string name; }
```
*Custom types must be marked using the `[Serializable]` attribute or implement `ISerializable`

## Bytes
The `Bytes` class is a tool that works much like `string` but for byte arrays.
It's used to make the work with arrays of byte arrays easier and simpler. (Like pythons')

With the `Bytes` class you can use addition (`+`) to concat arrays of bytes.
It also includes implicit cast operations to easily cast primitive objects into bytes and back.  
Example:
```cs
    Bytes hello = "Hello ";
    byte[] world = (Bytes)"world";
    byte[] helloWorld = hello + world;

    Bytes result = helloWorld;
    Console.WriteLine(result.ToString()); //'Hello world' casted back to string
```

## BytesBuilder
Much like StringBuilder but for bytes
```cs
BytesBuilder b = new BytesBuilder();
b.Append(5, 10, 20);
b.Append(30);
byte[] nums = b.ToBytes();
```

## ValueTuples/Tuples
Bytes has helper methods to easily convert into groups of primitive values and back
```cs
Bytes vals = Bytes.GetBytes(10, 20, 40, 80);
(int n10, int n20, int n40, int n80) = vals.GetTuple<(int, int, int, int)>();
```

[NuGet]: https://img.shields.io/nuget/v/TgenSerializer?color=blue
[NuGet-url]: https://www.nuget.org/packages/TgenSerializer
