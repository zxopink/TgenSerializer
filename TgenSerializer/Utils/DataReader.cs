using System;
using System.Collections.Generic;
using System.Text;

namespace TgenSerializer
{
    public class DataReader
    {
        byte[] Data { get; set; }
        int Index { get; set; }

        public DataReader(byte[] data)
        {
            Data = data;
            Index = 0;
        }

        public bool GetBoolean() { var value = Bytes.ToBoolean(Data, Index); Index += sizeof(bool); return value; }
        public char GetChar() { var value = Bytes.ToChar(Data, Index); Index += sizeof(char); return value; }
        public double GetDouble() { var value = Bytes.ToDouble(Data, Index); Index += sizeof(double); return value; }
        public short GetInt16() { var value = Bytes.ToInt16(Data, Index); Index += sizeof(short); return value; }
        public int GetInt32() { var value = Bytes.ToInt32(Data, Index); Index += sizeof(int); return value; }
        public long GetInt64() { var value = Bytes.ToInt64(Data, Index); Index += sizeof(long); return value; }
        public float GetSingle() { var value = Bytes.ToSingle(Data, Index); Index += sizeof(float); return value; }
        public string GetString() { return Bytes.BytesToStr(GetBytes()); }
        public ushort GetUInt16() { var value = Bytes.ToUInt16(Data, Index); Index += sizeof(ushort); return value; }
        public uint GetUInt32() { var value = Bytes.ToUInt32(Data, Index); Index += sizeof(uint); return value; }
        public ulong GetUInt64() { var value = Bytes.ToUInt64(Data, Index); Index += sizeof(ulong); return value; }
        public byte GetByte() { var value = Data[Index]; Index += sizeof(byte); return value; }
        public byte[] GetBytes() 
        {
            var size = Bytes.ToInt32(Data, Index);
            byte[] content = new byte[size];
            Array.Copy(Data, Index + sizeof(int), content, 0, size);

            Index += sizeof(int) + size;
            return content;
        }

        /// <returns>The type of the object or null if there isn't a type</returns>
        public Type TryGetType()
        {
            try
            {
                var size = Bytes.ToInt32(Data, Index);
                byte[] content = new byte[size];
                Array.Copy(Data, Index + sizeof(int), content, 0, size);
                string typeStr = Bytes.BytesToStr(content);
                Type type = Type.GetType(typeStr, true);

                Index += sizeof(int) + size;
                return type;
            }
            catch (Exception)
            {
            }
            return null;
        }
    }
}
