using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace TgenSerializer
{
    class TgenFormatter
    {
        public static void Serilize(Stream stream, object obj)
        {
            using (BinaryWriter writer = new BinaryWriter(stream, Encoding.ASCII))
                writer.Write(Deconstructor.Deconstruct(obj));
        }

        public static object Deserilize(Stream stream)
        {
            using (BinaryReader reader = new BinaryReader(stream, Encoding.ASCII))
                return Constructor.Construct(reader.ReadString());
        }

        public static string GetObjectGraph(object obj) => Deconstructor.Deconstruct(obj);

        public static object GetObjectFromGraph(string objData) => Constructor.Construct(objData);
    }
}
