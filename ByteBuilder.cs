using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TgenSerializer
{
    class ByteBuilder
    {
        List<byte[]> list;

        public int Length { get {
                int length = 0;
                foreach (var arr in list)
                    length += arr.Length;
                return length;
            } }

        #region Constructors
        public ByteBuilder(byte[] arr)
        {
            list = new List<byte[]>();
            list.Add(arr);
        }
        public ByteBuilder(byte b)
        {
            list = new List<byte[]>();
            byte[] arr = new byte[1];
            arr[0] = b;
            list.Add(arr);
        }
        public ByteBuilder(ByteBuilder builder) {
            list = builder.list.ToList();
        }
        public ByteBuilder() { list = new List<byte[]>(); }
        #endregion

        public ByteBuilder Append(ByteBuilder obj)
        {
            list.AddRange(obj.list);
            return this;
        }

        private ByteBuilder AddByteArr(byte[] arr) { list.Add(arr); return this; }
        private ByteBuilder RemoveByteArr(byte[] arr) { list.Remove(arr); return this; }

        public byte[] GetBytes()
        {
            if (list.Count == 1)
                return list[0];

            long length = 0;
            foreach (var byteArr in list)
                length += byteArr.Length;

            byte[] arr = new byte[length];
            {
                long i = 0;
                foreach (var byteArr in list)
                    for (int x = 0; x < byteArr.Length;)
                    {
                        arr[i] = byteArr[x];
                        i++; x++;
                    }
            }
            return arr;
        }

        /// <summary>
        /// Turns a primitive object to byte[]
        /// (Taken from microsoft wiki) 
        /// </summary>
        /// <param name="obj">The primitive object</param>
        /// <returns>The byte[]</returns>
        public static byte[] PrimitiveToByte(object obj)
        {
            if (obj is sbyte)
            {
                string byteString = ((sbyte)obj).ToString("X2");
                return new byte[1] { Byte.Parse(byteString, System.Globalization.NumberStyles.HexNumber) };
            }
            else if (obj is byte)
            {
                return new byte[1] { (byte)obj };
            }
            else if (obj is short)
            {
                return BitConverter.GetBytes((short)obj);
            }
            else if (obj is int)
            {
                return BitConverter.GetBytes((int)obj);
            }
            else if (obj is string)
            {
                return StrToBytes((string)obj);
            }
            else if (obj is long)
            {
                return BitConverter.GetBytes((long)obj);
            }
            else if (obj is float)
            {
                return BitConverter.GetBytes((float)obj);
            }
            else if (obj is ushort)
            {
                return BitConverter.GetBytes((ushort)obj);
            }
            else if (obj is uint)
            {
                return BitConverter.GetBytes((uint)obj);
            }
            else if (obj is ulong)
            {
                return BitConverter.GetBytes((ulong)obj);
            }
            else
            {
                throw new Exception("Primitive object of type " + obj.GetType() + " cannot be converted into bytes (ByteBuilder casting exception)");
            }
        }
        public static object ByteToPrimitive(Type objType, byte[] objData, int startIndex = 0)
        {
            if (objType.Equals(typeof(sbyte)))
            {
                return (sbyte)BitConverter.ToChar(objData, startIndex);
            }
            else if (objType.Equals(typeof(byte)))
            {
                //needs to add to the start index?
                return objData[startIndex];
            }
            else if (objType.Equals(typeof(short)))
            {
                return BitConverter.ToInt16(objData, startIndex);
            }
            else if (objType.Equals(typeof(int)))
            {
                return BitConverter.ToInt32(objData, startIndex);
            }
            else if (objType.Equals(typeof(string)))
            {
                return BytesToStr(objData.Skip(startIndex).ToArray());
            }
            else if (objType.Equals(typeof(long)))
            {
                return BitConverter.ToInt64(objData, startIndex);
            }
            else if (objType.Equals(typeof(float)))
            {
                return BitConverter.ToSingle(objData, startIndex);
            }
            else if (objType.Equals(typeof(ushort)))
            {
                return BitConverter.ToUInt16(objData, startIndex);
            }
            else if (objType.Equals(typeof(uint)))
            {
                return BitConverter.ToUInt32(objData, startIndex);
            }
            else if (objType.Equals(typeof(ulong)))
            {
                return BitConverter.ToUInt64(objData, startIndex);
            }
            else
            {
                throw new Exception("Type " + objType + " cannot be converted into object from bytes (ByteBuilder casting exception)");
            }
        }

        public static byte[] StrToBytes(string str) => Encoding.ASCII.GetBytes(str);
        public static string BytesToStr(byte[] b) => Encoding.UTF8.GetString(b);

        //public static ByteBuilder operator +(ByteBuilder a, byte[] b)
        //{ a.AddByteArr(b); return a; }
        //public static ByteBuilder operator +(ByteBuilder a, string str)
        //{ byte[] b = StrToBytes(str); a.AddByteArr(b); return a; }
        //public static ByteBuilder operator +(ByteBuilder a, ByteBuilder b)
        //{ a.Append(b); return a; }

        //SHOULD LOOK AT THESE AGAIN, CALLING THE ByteBuilder CONSTRUCTOR THAT TAKES A ByteBuilder POINTS AT THE OLD LIST AND DOESN'T MAKE A NEW ONE
        //public static ByteBuilder operator +(ByteBuilder a, byte[] b)
        //{ return new ByteBuilder(a).Append(b); }
        //public static ByteBuilder operator +(ByteBuilder a, string str)
        //{ byte[] b = StrToBytes(str); return new ByteBuilder(a).Append(b); }
        public static ByteBuilder operator +(ByteBuilder a, ByteBuilder b)
        { return new ByteBuilder(a).Append(b); }


        public static ByteBuilder operator -(ByteBuilder a, byte[] b)
        { return a.RemoveByteArr(b); }

        //explicit keyword means it requires a cast syntax
        //public static explicit operator byte(ByteBuilder obj) => obj.GetBytes()[0];
        //public static explicit operator ByteBuilder(byte b) => new ByteBuilder(b);

        //implicit keyword means it automatically converts the types (Implicit conversions don't require special syntax to be invoked)
        public static implicit operator byte[](ByteBuilder builder) => builder.GetBytes();
        public static implicit operator ByteBuilder(byte[] b) => new ByteBuilder(b);
        public static implicit operator string(ByteBuilder builder) => BytesToStr(builder);

        public static implicit operator ByteBuilder(sbyte obj) => new ByteBuilder(PrimitiveToByte(obj));
        public static implicit operator ByteBuilder(byte obj) => new ByteBuilder(PrimitiveToByte(obj));
        public static implicit operator ByteBuilder(short obj) => new ByteBuilder(PrimitiveToByte(obj));
        public static implicit operator ByteBuilder(int obj) => new ByteBuilder(PrimitiveToByte(obj));
        public static implicit operator ByteBuilder(string str) => new ByteBuilder(StrToBytes(str));
        public static implicit operator ByteBuilder(long obj) => new ByteBuilder(PrimitiveToByte(obj));
        public static implicit operator ByteBuilder(ushort obj) => new ByteBuilder(PrimitiveToByte(obj));
        public static implicit operator ByteBuilder(uint obj) => new ByteBuilder(PrimitiveToByte(obj));
        public static implicit operator ByteBuilder(ulong obj) => new ByteBuilder(PrimitiveToByte(obj));

        public override string ToString()
        {
            return BytesToStr(this);
        }
    }
}
