using System;
using System.IO;
using System.Runtime.Serialization;

namespace TgenSerializer
{
    public class Formatter : IFormatter
    {
        private CompressionFormat compression;
        public CompressionFormat Compression { get => compression; set => compression = value; }

        public SerializationBinder Binder { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public StreamingContext Context { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public ISurrogateSelector SurrogateSelector { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        public Formatter(CompressionFormat compression = CompressionFormat.Json) =>
            this.compression = compression;

        public int maxSize = int.MaxValue;

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
            try
            {
                byte[] packet = BinaryDeconstructor.Deconstruct(obj);
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

        public static void JsonSerialize(Stream stream, object obj)
        {
            StreamWriter writer = new StreamWriter(stream);
            try
            {
                writer.Write(JsonDeconstructor.Deconstruct(obj));
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
        public static string ToJSON(object obj) =>
            JsonDeconstructor.Deconstruct(obj);
        public static void StringSerialize(Stream stream, object obj)
        {
            //Used to be BinaryWriter, keep eye on that
            StreamWriter writer = new StreamWriter(stream);
            try
            {
                writer.Write(Deconstructor.Deconstruct(obj));
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
        public static string ToString(object obj) =>
            Deconstructor.Deconstruct(obj);
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
                    return BinaryDeserialize(stream, maxSize);
                case CompressionFormat.Json:
                    return JsonDeserialize(stream);
                case CompressionFormat.String:
                    return StringDeserialize(stream);
                default:
                    throw new SerializationException("Please choose a format compression");
            }
        }
        public T Deserialize<T>(Stream stream) => (T)Deserialize(stream);

        public static object Deserialize(Stream stream, CompressionFormat compression, int maxSize = int.MaxValue)
        {
            switch (compression)
            {
                case CompressionFormat.Binary:
                    //int defaultTimeout = stream.ReadTimeout;
                    //stream.ReadTimeout = 10; //Like really, how long does the computer need to read a 4 byte signed integer? Change if needed
                    return BinaryDeserialize(stream, maxSize);
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
        public static object BinaryDeserialize(Stream stream, int maxSize = int.MaxValue)
        {
            BinaryReader reader = new BinaryReader(stream);
            int size = reader.ReadInt32();
            if (size > maxSize)
                throw new MarshalException(MarshalError.TooLarge, $"Stream info size ({size}) is bigger than max size ({maxSize})");
            else if(size < 0)
                throw new MarshalException(MarshalError.NegativeSize, $"Stream info size ({size}) is negative");
            
            byte[] packet = reader.ReadBytes(size);
            return BinaryConstructor.Construct(packet);
        }

        public static object FromBytes(byte[] data) =>
            BinaryConstructor.Construct(data);

        public static JsonElement JsonDeserialize(Stream stream)
        {
            StreamReader reader = new StreamReader(stream);
            string content = reader.ReadToEnd();
            return JsonConstructor.Construct(content);
        }
        public static JsonElement FromJSON(string data) =>
            JsonConstructor.Construct(data);

        public static object StringDeserialize(Stream stream)
        {
            //Used to be BinaryReader, keep eye on that
            StreamReader reader = new StreamReader(stream);
            string objGraphData = reader.ReadToEnd();
            return Constructor.Construct(objGraphData);
        }
        public static object FromString(string data) =>
            Constructor.Construct(data);

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