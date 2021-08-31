using System;
using System.IO;
using System.Runtime.Serialization;
using static TgenSerializer.TgenFormatterSettings;

namespace TgenSerializer
{
    public static class TgenFormatter
    {
        #region Serialization
        public static void Serialize(Stream stream, object obj)
        {
            BinaryWriter writer = new BinaryWriter(stream);
            switch (Compression)
            {
                case FormatCompression.Binary:
                    byte[] packet = BinaryDeconstructor.Deconstruct(obj);
                    writer.Write(packet.Length);
                    writer.Write(packet);
                    break;
                case FormatCompression.Json:
                    writer.Write(JsonDeconstructor.Deconstruct(obj));
                    break;
                case FormatCompression.String:
                    writer.Write(Deconstructor.Deconstruct(obj));
                    break;
                default:
                    throw new SerializationException("Please choose a format compression");
            }
            writer.Flush();
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
        private static void BinarySerialize(Stream stream, object obj)
        {
            BinaryWriter writer = new BinaryWriter(stream);
            byte[] packet = BinaryDeconstructor.Deconstruct(obj);
            writer.Write(packet.Length);
            writer.Write(packet);
            writer.Flush();
        }
        private static void JsonSerialize(Stream stream, object obj)
        {
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(JsonDeconstructor.Deconstruct(obj));
            writer.Flush();
        }
        private static void StringSerialize(Stream stream, object obj)
        {
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write(Deconstructor.Deconstruct(obj));
            writer.Flush();
        }
        #endregion

        public static string Serialize(object obj) => Deconstructor.Deconstruct(obj);
        #endregion

        #region Deserialization
        #region Standard Deserialization
        public static object Deserialize(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);
            switch (Compression)
            {
                case FormatCompression.Binary:
                    //int defaultTimeout = stream.ReadTimeout;
                    //stream.ReadTimeout = 10; //Like really, how long does the computer need to read a 4 byte signed integer? Change if needed
                    byte[] packet = reader.ReadBytes(reader.ReadInt32());
                    object obj = BinaryConstructor.Construct(packet);
                    return obj;
                case FormatCompression.String:
                    string objGraphData = reader.ReadString();
                    return Constructor.Construct(objGraphData);
                default:
                    throw new SerializationException("Please choose a format compression");
            }
        }
        public static T Deserialize<T>(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);
            switch (Compression)
            {
                case FormatCompression.Binary:
                    //int defaultTimeout = stream.ReadTimeout;
                    //stream.ReadTimeout = 10; //Like really, how long does the computer need to read a 4 byte signed integer? Change if needed
                    byte[] packet = reader.ReadBytes(reader.ReadInt32());
                    T obj = (T)BinaryConstructor.Construct(packet);
                    return obj;
                case FormatCompression.String:
                    string objGraphData = reader.ReadString();
                    return (T)Constructor.Construct(objGraphData);
                default:
                    throw new SerializationException("Please choose a format compression");
            }
        }
        #endregion

        #region Format Deserialization
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

        public static T Deserialize<T>(Stream stream, FormatCompression compression)
        {
            BinaryReader reader = new BinaryReader(stream);
            switch (compression)
            {
                case FormatCompression.Binary:
                    //int defaultTimeout = stream.ReadTimeout;
                    //stream.ReadTimeout = 10; //Like really, how long does the computer need to read a 4 byte signed integer? Change if needed
                    byte[] packet = reader.ReadBytes(reader.ReadInt32());
                    T obj = (T)BinaryConstructor.Construct(packet);
                    return obj;
                case FormatCompression.String:
                    string objGraphData = reader.ReadString();
                    return (T)Constructor.Construct(objGraphData);
                default:
                    throw new SerializationException("Please choose a format compression");
            }
        }
        #endregion

        private static object BinaryDeserialize(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);
            byte[] packet = reader.ReadBytes(reader.ReadInt32());
            return BinaryConstructor.Construct(packet);
        }
        private static JsonElement JsonDeserialize(Stream stream)
        {
            StreamReader reader = new StreamReader(stream);
            string content = reader.ReadToEnd();
            return JsonConstructor.Construct(content);
        }
        private static object StringDeserialize(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);
            string objGraphData = reader.ReadString();
            return Constructor.Construct(objGraphData);
        }

        #region String Deserialization 
        public static object Deserialize(string objGraphData) => Constructor.Construct(objGraphData);
        public static T Deserialize<T>(string objGraphData) => (T)Constructor.Construct(objGraphData);
        #endregion

        #region Binary Deserialization 
        public static object Deserialize(byte[] objBinaryData) => BinaryConstructor.Construct(objBinaryData);
        public static T Deserialize<T>(byte[] objBinaryData) => (T)BinaryConstructor.Construct(objBinaryData);
        #endregion
        #endregion

        public static byte[] GetBinaryGraph(object obj) => BinaryDeconstructor.Deconstruct(obj);
    }

    public static class TgenFormatterSettings
    {
        private static FormatCompression compression = FormatCompression.Binary;
        /// <summary>
        /// Compression settings
        /// </summary>
        public static FormatCompression Compression { get { return compression; } set { compression = value; } }
    }
    public enum FormatCompression
    {
        /// <summary>
        /// More compact but hard to read (similar to BinaryFormatter)
        /// Ideal when using Network packets or for files that shouldn't be edited
        /// </summary>
        Binary,

        /// <summary>
        /// Json serialization
        /// </summary>
        Json,

        /// <summary>
        /// Less compact yet easy to modify using either string or in a text file (similar to JSON)
        /// Ideal when printing to a text file or when the packet needs to be modified before casted into a runtime object
        /// </summary>
        String
    }
}
