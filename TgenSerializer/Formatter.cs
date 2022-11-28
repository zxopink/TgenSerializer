using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;

namespace TgenSerializer
{
    public class Formatter : IFormatter
    {

        public SerializationBinder Binder { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public StreamingContext Context { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public ISurrogateSelector SurrogateSelector { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        public IList<TgenConverter> Converters { get; set; }

        public Formatter()
        {
            
        }

        public int maxSize = int.MaxValue;

        public void Serialize(Stream stream, object obj)
        {
            BinarySerialize(stream, obj, Converters);
        }
        public static void BinarySerialize(Stream stream, object obj, IList<TgenConverter> converters)
        {
            BinaryWriter writer = new BinaryWriter(stream);
            try
            {
                byte[] packet = BinaryDeconstructor.Deconstruct(obj, converters);
                writer.Write(packet.Length);
                writer.Write(packet);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                writer.Flush();
            }
        }
        public static byte[] ToBytes(object obj) =>
             BinaryDeconstructor.Deconstruct(obj);


        public object Deserialize(Stream stream)
        {
            return BinaryDeserialize(stream, Converters, maxSize);
        }
        public T Deserialize<T>(Stream stream) => (T)Deserialize(stream);

        public static object BinaryDeserialize(Stream stream, IList<TgenConverter> converters, int maxSize = int.MaxValue)
        {
            BinaryReader reader = new BinaryReader(stream);
            int size = reader.ReadInt32();
            if (size > maxSize)
                throw new MarshalException(MarshalError.TooLarge, $"Stream info size ({size}) is bigger than max size ({maxSize})");
            else if(size < 0)
                throw new MarshalException(MarshalError.NegativeSize, $"Stream info size ({size}) is negative");
            
            byte[] packet = reader.ReadBytes(size);
            return BinaryConstructor.Construct(packet, converters);
        }

        public static object FromBytes(byte[] data) =>
            BinaryConstructor.Construct(data);
    }
}