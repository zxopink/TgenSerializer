# TgenSerializer
[![NuGet Package][NuGet]][NuGet-url]

TgenSerializer is a serializer for Dotnet.

It makes an object graph of serializeable objects using a recursive method
then prints out the result into a stream.
The serializer has two format options
*String (readable and modifiable). User settings for example
*Bytes (Fast and compressed). Network packets for example

## Serialize
The Serialize is a static function that takes a serializeable object (Object with a `[Serializeable]` attribute) and a stream.  
The function then writes the object's properties into the given stream (can be any type of stream, NetworkStream, FileStream, etc...)

## Deserialize
The Deserialize is a static function that takes a stream and returns an uninitialized object*.
The returned object has all the properties of the serialized one.

* Uninitialized object is an object that was created without a constructor call.

## Bytes
The `Bytes` class is a tool that works just like `StringBuilder` but for byte arrays.
It's used to make the work with arrays of byte arrays easier and simpler. (Like pythons')

With the `Bytes` class you can use mathematical operations (`+` & `-`) to concat arrays of bytes or remove them.
It also includes implicit cast operations to easily cast primitive objects into bytes.  
Example:
```cs
    Bytes hello = "Hello ";
    byte[] world = (Bytes)"world";
    byte[] helloWorld = hello + world;

    Bytes result = helloWorld;
    Console.WriteLine(result); //'Hello world' casted back to string
```

[NuGet]: https://img.shields.io/nuget/v/TgenSerializer?color=blue
[NuGet-url]: https://www.nuget.org/packages/TgenSerializer