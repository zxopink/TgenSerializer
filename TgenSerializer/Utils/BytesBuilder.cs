using System;
using System.Collections.Generic;
using System.Text;

namespace TgenSerializer.Utils
{
    public class BytesBuilder
    {
        List<byte[]> BytesList { get; set; }
        public BytesBuilder()
        {
            BytesList = new List<byte[]>();
        }

        public BytesBuilder(Bytes value) : this()
        {
            
        }

        public void Append()
        {
            
        }
    }
}
