# TgenSerializer

TgenSerializer is a serializer for the .Net (.Net core) framework.

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

## BinaryBuilder
The binary builder is a tool class that works just like `StringBuilder` but for bytes.
It's used to make the work with arrays of bytes much easier and simple. (Like pythons')

With the BinaryBuilder you can use mathematical operations (`+` & `-`) to connect arrays of bytes or remove them.
It also includes implicit cast operations to easily cast primitive objects into bytes.  
Example:
```cs
    BinaryBuilder hello = new BinaryBuilder("Hello ");
    byte[] world = BinaryBuilder.StrToBytes("world");
    byte[] helloWorld = hello + world;

    BinaryBuilder result = hello World;
    Console.WriteLine(result); //Hello world
```