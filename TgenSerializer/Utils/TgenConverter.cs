using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace TgenSerializer.Utils
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
            List<Type> types = assmblies.SelectMany(assm => assm.GetTypes())
                .Where(t => t.IsSerializable).ToList();

            //Sort them all, not sure if `GetTypes()` is CLR dependant
            types.Sort((t1, t2) => t1.ToString().CompareTo(t2.ToString()));

            List<TgenConverter> converters = new List<TgenConverter>();
            uint counter = 0;
            foreach (var type in types)
            {
                if (!type.IsSerializable) continue;
                TgenConverter conv = new TgenConverter(type, counter++);
                converters.Add(conv);
            }
            return converters;
        }
    }
}
