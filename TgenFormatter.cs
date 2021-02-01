using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using static TgenSerializer.TgenFormatterSettings;

namespace TgenSerializer
{
    public static class TgenFormatter
    {
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
                case FormatCompression.String:
                    writer.Write(Deconstructor.Deconstruct(obj));
                    break;
                default:
                    throw new SerializationException("Please choose a format compression");
            }
            writer.Flush();
        }

        public static object Deserialize(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);
            switch (Compression)
            {
                case FormatCompression.Binary:
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

        public static byte[] ReadAllBytes(this BinaryReader reader)
        {
            const int bufferSize = 4096;
            using (var ms = new MemoryStream())
            {
                byte[] buffer = new byte[bufferSize];
                int count;
                while ((count = reader.Read(buffer, 0, buffer.Length)) != 0)
                    ms.Write(buffer, 0, count);
                return ms.ToArray();
            }

        }

        public static string GetObjectGraph(object obj) => Deconstructor.Deconstruct(obj);

        public static object GetObjectFromGraph(string objData) => Constructor.Construct(objData);
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
        /// Less compact yet easy to modify using either string or in a text file (similar to JSON)
        /// Ideal when printing to a text file or when the packet needs to be modified before casted into a runtime object
        /// </summary>
        String
    }
}
