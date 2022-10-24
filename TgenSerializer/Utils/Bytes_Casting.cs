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
        public static implicit operator byte[](Bytes builder) => builder.Array;
        public static implicit operator string(Bytes builder) => BytesToStr(builder);
        public static implicit operator Bytes(byte[] b) => new Bytes(b);

        public static implicit operator Bytes(sbyte obj) => new Bytes(PrimitiveToByte(obj));
        public static implicit operator Bytes(byte obj) => new Bytes(PrimitiveToByte(obj));
        public static implicit operator Bytes(short obj) => new Bytes(PrimitiveToByte(obj));
        public static implicit operator Bytes(int obj) => new Bytes(PrimitiveToByte(obj));
        public static implicit operator Bytes(double obj) => new Bytes(PrimitiveToByte(obj));
        public static implicit operator Bytes(float obj) => new Bytes(PrimitiveToByte(obj));
        public static implicit operator Bytes(bool obj) => new Bytes(PrimitiveToByte(obj));
        public static implicit operator Bytes(string str) => new Bytes(StrToBytes(str));
        public static implicit operator Bytes(long obj) => new Bytes(PrimitiveToByte(obj));
        public static implicit operator Bytes(ushort obj) => new Bytes(PrimitiveToByte(obj));
        public static implicit operator Bytes(uint obj) => new Bytes(PrimitiveToByte(obj));
        public static implicit operator Bytes(ulong obj) => new Bytes(PrimitiveToByte(obj));

        /// <summary>Converts the bytes to a UTF8 string</summary>
        public override string ToString() => 
            BytesToStr(this);
    }
}
