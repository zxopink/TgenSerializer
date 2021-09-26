using System;
using System.IO;
using System.Runtime.Serialization;

namespace TgenSerializer
{
    public class Formatter
    {
        private FormatCompression compression;
        public FormatCompression Compression { get => compression; set => compression = value; }

        public Formatter(FormatCompression compression = FormatCompression.Json) =>
            this.compression = compression;

        #region Serialization
        public void Serialize(Stream stream, object obj)
        {
            switch (compression)
            {
                case FormatCompression.Binary:
                    BinarySerialize(stream, obj);
                    break;
                case FormatCompression.Json:
                    JsonSerialize(stream, obj);
                    break;
                case FormatCompression.String:
                    StringSerialize(stream, obj);
                    break;
                default:
                    throw new SerializationException("Please choose a format compression");
            }
        }

        public static void Serialize(Stream stream, object obj, FormatCompression compression)
        {
            switch (compression)
            {
                case FormatCompression.Binary:
                    BinarySerialize(stream, obj);
                    break;
                case FormatCompression.Json:
                    JsonSerialize(stream, obj);
                    break;
                case FormatCompression.String:
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
                case FormatCompression.Binary:
                    //int defaultTimeout = stream.ReadTimeout;
                    //stream.ReadTimeout = 10; //Like really, how long does the computer need to read a 4 byte signed integer? Change if needed
                    return BinaryDeserialize(stream);
                case FormatCompression.Json:
                    return JsonDeserialize(stream);
                case FormatCompression.String:
                    return StringDeserialize(stream);
                default:
                    throw new SerializationException("Please choose a format compression");
            }
        }
        public T Deserialize<T>(Stream stream) => (T)Deserialize(stream);

        public static object Deserialize(Stream stream, FormatCompression compression)
        {
            switch (compression)
            {
                case FormatCompression.Binary:
                    //int defaultTimeout = stream.ReadTimeout;
                    //stream.ReadTimeout = 10; //Like really, how long does the computer need to read a 4 byte signed integer? Change if needed
                    return BinaryDeserialize(stream);
                case FormatCompression.Json:
                    return JsonDeserialize(stream);
                case FormatCompression.String:
                    return StringDeserialize(stream);
                default:
                    throw new SerializationException("Please choose a format compression");
            }
        }
        public static T Deserialize<T>(Stream stream, FormatCompression compression) => 
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
    public enum FormatCompression
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