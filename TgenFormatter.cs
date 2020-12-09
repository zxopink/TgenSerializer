using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.CompilerServices;
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

        public static object Deserilize(Stream stream, object obj)
        {
            using (BinaryReader reader = new BinaryReader(stream, Encoding.ASCII))
                return Constructor.Construct(reader.ReadString());
        }
    }
}
