using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace TgenSerializer
{
    internal class StandardAssemblyConverter
    {
        public static IList<TgenConverter> CreateConvertersFromAssembly(Assembly assm)
        {
            Type[] types = assm.GetTypes();
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
