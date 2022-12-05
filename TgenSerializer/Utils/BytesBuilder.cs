using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace TgenSerializer.Utils
{
    /// <summary>
    /// Similar class to StringBuilder.
    /// It makes the concept of byte array appending much easier and way less memory consuming.
    /// So the program can append many byte arrays in a significant speed and while maintaining high preformance.
    /// Note: extreme use of this class should require basic understanding of memory management in object oriented languages.
    /// </summary>
    public class BytesBuilder
    {
        List<byte[]> BytesList { get; set; }
        public int Length { get; private set; }
        public BytesBuilder()
        {
            BytesList = new List<byte[]>();
            Length = 0;
        }

        public BytesBuilder(byte[] value) : this() =>
            Append(value);
        public BytesBuilder(byte[][] value) : this() =>
            Append(value);

        public void Append(byte[] bytes)
        {
            BytesList.Add(bytes);
            Length += bytes.Length;
        }
        public void Append(Bytes bytes)
        {
            BytesList.Add(bytes);
            Length += bytes.Length;
        }
        public void Append(params byte[][] bytes)
        {
            BytesList.AddRange(bytes);
            Length += bytes.Sum(arr => arr.Length);
        }
        public void Append(params Bytes[] bytes)
        {
            foreach (var byteGroup in bytes)
                BytesList.Add(byteGroup);
            Length += bytes.Sum(arr => arr.Length);
        }

        public byte[] ToBytes()
        {
            if (BytesList.Count == 1)
                return BytesList[0];
            var concat = Bytes.Concat(BytesList);
            BytesList.Clear();
            BytesList.Add(concat);
            Length = concat.Length;
            return concat;
        }

        public void Clear()
        {
            BytesList.Clear();
            Length = 0;
        }

        public override string ToString()
        {
            return Bytes.BytesToStr(ToBytes());
        }
    }
}
