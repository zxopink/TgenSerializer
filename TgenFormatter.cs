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
    public static class TgenFormatter
    {
        public static void Serialize(Stream stream, object obj)
        {
            BinaryWriter writer = new BinaryWriter(stream);
            Console.WriteLine(obj.GetType());
            writer.Write(Deconstructor.Deconstruct(obj));
            Console.WriteLine("Done Writing");
            writer.Flush();
        }

        public static object Deserialize(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);
            Console.WriteLine("Start reading");
            string objGraphData = reader.ReadString();
            Console.WriteLine("Constructing");
            var obj = Constructor.Construct(objGraphData);
            Console.WriteLine("Done building obj");
            return obj;
        }

        public static string GetObjectGraph(object obj) => Deconstructor.Deconstruct(obj);

        public static object GetObjectFromGraph(string objData) => Constructor.Construct(objData);
    }
}
