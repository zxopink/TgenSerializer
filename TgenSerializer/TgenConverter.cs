using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace TgenSerializer
{
    /// <summary>
    /// Converts type to number and back
    /// </summary>
    public class TgenConverter
    {
        public Type Type { get; }
        public uint Id { get; }

        public TgenConverter(Type type, uint id)
        {
            Type = type;
            Id = id;
        }

        public static List<TgenConverter> CreateConvertersFromAssembly(params Assembly[] assmblies)
        {
            Type[] types = assmblies.SelectMany(assm => assm.GetTypes()).ToArray();
            List<TgenConverter> converters = new List<TgenConverter>();
            uint counter = 0;
            foreach (var type in types)
            {
                if (!type.IsSerializable) continue;
                TgenConverter conv = new TgenConverter(type, counter++);
                converters.Add(conv);
            }
            
            //Sort them all, not sure if `GetTypes()` is CLR dependant
            converters.Sort((conv1, conv2) => conv1.Type.ToString().CompareTo(conv2.Type.ToString()));
            return converters;
        }
    }
}
