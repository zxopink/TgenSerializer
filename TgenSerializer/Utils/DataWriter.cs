using System;
using System.Collections.Generic;
using System.Text;
using TgenSerializer.Utils;

namespace TgenSerializer
{
    public class DataWriter
    {
        public ByteBuilder data { get; private set; }

        public DataWriter(object obj) : this(obj.GetType()) {}

        /// <summary>
        /// Writes down the object's info
        /// </summary>
        /// <param name="objType">Object's data</param>
        public DataWriter(Type objType) : this()
        {
            data.Append(objType.FullName);
        }
        public DataWriter()
        {
            data = new ByteBuilder();
        }

        public void WriteBytes(Type type) { WriteBytes(type.FullName); }
        public void WriteBytes(bool value) { data.Append(Bytes.GetBytes(value)); }
        public void WriteBytes(char value) { data.Append(Bytes.GetBytes(value)); }
        public void WriteBytes(short value) { data.Append(Bytes.GetBytes(value)); }
        public void WriteBytes(int value) { data.Append(Bytes.GetBytes(value)); }
        public void WriteBytes(long value) { data.Append(Bytes.GetBytes(value)); }
        public void WriteBytes(ushort value) { data.Append(Bytes.GetBytes(value)); }
        public void WriteBytes(uint value) { data.Append(Bytes.GetBytes(value)); }
        public void WriteBytes(ulong value) { data.Append(Bytes.GetBytes(value)); }
        public void WriteBytes(float value) { data.Append(Bytes.GetBytes(value)); }
        public void WriteBytes(double value) { data.Append(Bytes.GetBytes(value)); }
        public void WriteBytes(string value) { WriteBytes(Bytes.StrToBytes(value)); }
        public void WriteBytes(byte value) { data.Append(value); }
        public void WriteBytes(byte[] value)
        {
            data.Append(value.Length);
            data.Append(value);
        }

        public byte[] GetData() => data.ToBytes();
    }
}
