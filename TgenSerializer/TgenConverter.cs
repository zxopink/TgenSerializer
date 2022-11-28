using System;
using System.Collections.Generic;
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
    }
}
