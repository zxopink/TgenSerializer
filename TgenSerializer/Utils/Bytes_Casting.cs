using System;
using System.Collections.Generic;
using System.Text;

namespace TgenSerializer
{
    public partial struct Bytes
    {
        //explicit keyword means it requires a cast syntax
        //public static explicit operator byte(ByteBuilder obj) => obj.GetBytes()[0];
        //public static explicit operator ByteBuilder(byte b) => new ByteBuilder(b);

        //implicit keyword means it automatically converts the types (Implicit conversions don't require special syntax to be invoked)
        public static implicit operator byte[](Bytes builder) => builder.Buffer;
        public static implicit operator Bytes(byte[] b) => new Bytes(b);

        public static implicit operator Bytes(sbyte obj) => new Bytes(PrimitiveToByte(obj));
        public static implicit operator Bytes(byte obj) => new Bytes(PrimitiveToByte(obj));
        public static implicit operator Bytes(short obj) => new Bytes(PrimitiveToByte(obj));
        public static implicit operator Bytes(int obj) => new Bytes(PrimitiveToByte(obj));
        public static implicit operator Bytes(double obj) => new Bytes(PrimitiveToByte(obj));
        public static implicit operator Bytes(float obj) => new Bytes(PrimitiveToByte(obj));
        public static implicit operator Bytes(bool obj) => new Bytes(PrimitiveToByte(obj));
        public static implicit operator Bytes(long obj) => new Bytes(PrimitiveToByte(obj));
        public static implicit operator Bytes(ushort obj) => new Bytes(PrimitiveToByte(obj));
        public static implicit operator Bytes(uint obj) => new Bytes(PrimitiveToByte(obj));
        public static implicit operator Bytes(ulong obj) => new Bytes(PrimitiveToByte(obj));
        public static implicit operator Bytes(string str) => StrToBytes(str);

        public static implicit operator sbyte(Bytes obj) => B2P<sbyte>(obj);
        public static implicit operator byte(Bytes obj) => B2P<byte>(obj);
        public static implicit operator short(Bytes obj) => B2P<short>(obj);
        public static implicit operator int(Bytes obj) => B2P<int>(obj);
        public static implicit operator double(Bytes obj) => B2P<double>(obj);
        public static implicit operator float(Bytes obj) => B2P<float>(obj);
        public static implicit operator bool(Bytes obj) => B2P<bool>(obj);
        public static implicit operator long(Bytes obj) => B2P<long>(obj);
        public static implicit operator ushort(Bytes obj) => B2P<ushort>(obj);
        public static implicit operator uint(Bytes obj) => B2P<uint>(obj);
        public static implicit operator ulong(Bytes obj) => B2P<ulong>(obj);
        
        public static explicit operator string(Bytes str) => BytesToStr(str);

        /// <summary>Converts the bytes to a UTF8 string</summary>
        public override string ToString() => 
            BytesToStr(this);

        public string ToString(Encoding encoding) =>
            BytesToStr(this, encoding);
    }
}
