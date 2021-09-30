using System;
using System.IO;
using System.Runtime.Serialization;

namespace TgenSerializer
{
    public class Formatter
    {
        private CompressionFormat compression;
        public CompressionFormat Compression { get => compression; set => compression = value; }

        public Formatter(CompressionFormat compression = CompressionFormat.Json) =>
            this.compression = compression;

        #region Serialization
        public void Serialize(Stream stream, object obj)
        {
            switch (compression)
            {
                case CompressionFormat.Binary:
                    BinarySerialize(stream, obj);
                    break;
                case CompressionFormat.Json:
                    JsonSerialize(stream, obj);
                    break;
                case CompressionFormat.String:
                    StringSerialize(stream, obj);
                    break;
                default:
                    throw new SerializationException("Please choose a format compression");
            }
        }

        public static void Serialize(Stream stream, object obj, CompressionFormat compression)
        {
            switch (compression)
            {
                case CompressionFormat.Binary:
                    BinarySerialize(stream, obj);
                    break;
                case CompressionFormat.Json:
                    JsonSerialize(stream, obj);
                    break;
                case CompressionFormat.String:
                    StringSerialize(stream, obj);
                    break;
                default:
                    throw new SerializationException("Please choose a format compression");
            }
        }
        #region Formats
        public static void BinarySerialize(Stream stream, object obj)
        {
            BinaryWriter writer = new BinaryWriter(stream);
            byte[] packet = BinaryDeconstructor.Deconstruct(obj);
            writer.Write(packet.Length);
            writer.Write(packet);
            writer.Flush();
        }
        public static void JsonSerialize(Stream stream, object obj)
        {
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(JsonDeconstructor.Deconstruct(obj));
            writer.Flush();
        }
        public static void StringSerialize(Stream stream, object obj)
        {
            //Used to be BinaryWriter, keep eye on that
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(Deconstructor.Deconstruct(obj));
            writer.Flush();
        }
        #endregion
        #endregion

        #region Deserialization
        public object Deserialize(Stream stream)
        {
            switch (compression)
            {
                case CompressionFormat.Binary:
                    //int defaultTimeout = stream.ReadTimeout;
                    //stream.ReadTimeout = 10; //Like really, how long does the computer need to read a 4 byte signed integer? Change if needed
                    return BinaryDeserialize(stream);
                case CompressionFormat.Json:
                    return JsonDeserialize(stream);
                case CompressionFormat.String:
                    return StringDeserialize(stream);
                default:
                    throw new SerializationException("Please choose a format compression");
            }
        }
        public T Deserialize<T>(Stream stream) => (T)Deserialize(stream);

        public static object Deserialize(Stream stream, CompressionFormat compression)
        {
            switch (compression)
            {
                case CompressionFormat.Binary:
                    //int defaultTimeout = stream.ReadTimeout;
                    //stream.ReadTimeout = 10; //Like really, how long does the computer need to read a 4 byte signed integer? Change if needed
                    return BinaryDeserialize(stream);
                case CompressionFormat.Json:
                    return JsonDeserialize(stream);
                case CompressionFormat.String:
                    return StringDeserialize(stream);
                default:
                    throw new SerializationException("Please choose a format compression");
            }
        }
        public static T Deserialize<T>(Stream stream, CompressionFormat compression) => 
            (T)Deserialize(stream, compression);

        #region Formats
        public static object BinaryDeserialize(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);
            byte[] packet = reader.ReadBytes(reader.ReadInt32());
            return BinaryConstructor.Construct(packet);
        }
        public static JsonElement JsonDeserialize(Stream stream)
        {
            StreamReader reader = new StreamReader(stream);
            string content = reader.ReadToEnd();
            return JsonConstructor.Construct(content);
        }
        public static object StringDeserialize(Stream stream)
        {
            //Used to be BinaryReader, keep eye on that
            StreamReader reader = new StreamReader(stream);
            string objGraphData = reader.ReadToEnd();
            return Constructor.Construct(objGraphData);
        }
        #endregion
        #endregion
    }
    public enum CompressionFormat
    {
        /// <summary>
        /// Buffer, More compact but hard to read (similar to BinaryFormatter)
        /// Ideal when using Network packets or for files that shouldn't be modified
        /// </summary>
        Binary,

        /// <summary>
        /// Json serialization
        /// </summary>
        Json,

        /// <summary>
        /// Text, Less compact yet easy to modify using either string or in a text file (similar to JSON)
        /// Ideal when printing to a text file or when the packet needs to be modified before casted into a runtime object
        /// </summary>
        String
    }
}