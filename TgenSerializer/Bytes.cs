using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace TgenSerializer
{
    /// <summary>
    /// Similar class to StringBuilder.
    /// It makes the concept of byte array appending much easier and way less memory consuming.
    /// So the program can append many byte arrays in a significant speed and while maintaining high preformance.
    /// Note: extreme use of this class should require basic understanding of memory management in object oriented languages.
    /// </summary>
    public class Bytes
    {
        List<byte[]> list;

        public int Length { get {
                int length = 0;
                foreach (var arr in list)
                    length += arr.Length;
                return length;
            } }

        #region Constructors
        public Bytes(byte[] arr)
        {
            list = new List<byte[]>();
            list.Add(arr);
        }
        public Bytes(byte b)
        {
            list = new List<byte[]>();
            byte[] arr = new byte[1];
            arr[0] = b;
            list.Add(arr);
        }
        public Bytes(Bytes builder) {
            list = new List<byte[]>();
            list.AddRange(builder.list);
        }
        public Bytes() { list = new List<byte[]>(); }
        #endregion

        public Bytes Append(Bytes obj)
        {
            list.AddRange(obj.list);
            return this;
        }

        private Bytes AddByteArr(byte[] arr) { list.Add(arr); return this; }
        private Bytes RemoveByteArr(byte[] arr) { list.Remove(arr); return this; }

        /// <summary>Converts the object to an array of bytes</summary>
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

        /// <summary>Converts the bytes to T</summary>
        /// <param name="returnType">Type to cast to (must be a primitve or string type)</param>
        public object GetT(Type returnType) => ByteToPrimitive(returnType, this); //Implicit use of the function GetBytes()
        /// <summary>Converts the bytes to T</summary>
        /// <typeparam name="T">Type to cast to (must be a primitve or string type)</typeparam>
        public T GetT<T>() => ByteToPrimitive<T>(this); //Implicit use of the function GetBytes()

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
            else if (obj is bool)
            {
                return BitConverter.GetBytes((bool)obj);
            }
            else if (obj is long)
            {
                return BitConverter.GetBytes((long)obj);
            }
            else if (obj is float)
            {
                return BitConverter.GetBytes((float)obj);
            }
            else if (obj is double)
            {
                return BitConverter.GetBytes((double)obj);
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
                throw new SerializationException("Primitive object of type " + obj.GetType() + " cannot be converted into bytes");
            }
        }
        /// <summary>Converts a primitive (or string) object to an array of bytes</summary>
        public static byte[] P2B(object obj) => PrimitiveToByte(obj);
        /// <summary>Converts a primitive (or string) object to an array of bytes</summary>
        public static byte[] P2B<T>(T obj) => PrimitiveToByte(obj);

        public static object ByteToPrimitive(Type objType, byte[] objData, int startIndex = 0)
        {
            if (objType.Equals(typeof(sbyte)))
            {
                return (sbyte)BitConverter.ToChar(objData, startIndex);
            }
            else if (objType.Equals(typeof(byte)))
            {
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
                if(startIndex == 0) //Minor performance improvement
                    return BytesToStr(objData);

                byte[] byteArr = new byte[objData.Length - startIndex];
                for (int i = 0; i < byteArr.Length; i++)
                    byteArr[i] = objData[startIndex + i];
                return BytesToStr(byteArr);
            }
            else if (objType.Equals(typeof(bool)))
            {
                return BitConverter.ToBoolean(objData, startIndex);
            }
            else if (objType.Equals(typeof(long)))
            {
                return BitConverter.ToInt64(objData, startIndex);
            }
            else if (objType.Equals(typeof(float)))
            {
                return BitConverter.ToSingle(objData, startIndex);
            }
            else if (objType.Equals(typeof(double)))
            {
                return BitConverter.ToDouble(objData, startIndex);
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
                throw new SerializationException("Type " + objType + " cannot be converted into object from bytes");
            }
        }
        public static T ByteToPrimitive<T>(byte[] objData, int startIndex = 0) => (T)ByteToPrimitive(typeof(T), objData, startIndex);

        /// <summary>Converts an array of bytes to a specified primitive (or string) object</summary>
        public static object B2P(Type objType, byte[] objData) => ByteToPrimitive(objType, objData);
        /// <summary>Converts an array of bytes to a specified primitive (or string) object</summary>
        public static T B2P<T>(byte[] objData) => ByteToPrimitive<T>(objData);

        /// <summary>A UTF8 encryption</summary>
        public static byte[] StrToBytes(string str) => Encoding.UTF8.GetBytes(str);
        public static byte[] StrToBytes(string str, Encoding encoder) => encoder.GetBytes(str); //THIS LINE USED TO BE ASCII, COULD BREAK EVERYTHING

        /// <summary>A UTF8 encryption</summary>
        public static string BytesToStr(byte[] b) => Encoding.UTF8.GetString(b);
        public static string BytesToStr(byte[] b, Encoding encoder) => encoder.GetString(b);

        //If b = byte[], the implicit operator will convert it to a BinaryBuilder
        public static Bytes operator +(Bytes a, Bytes b) 
        { return new Bytes(a).Append(b); }


        public static Bytes operator -(Bytes a, byte[] b)
        { return a.RemoveByteArr(b); }

        //explicit keyword means it requires a cast syntax
        //public static explicit operator byte(ByteBuilder obj) => obj.GetBytes()[0];
        //public static explicit operator ByteBuilder(byte b) => new ByteBuilder(b);

        //implicit keyword means it automatically converts the types (Implicit conversions don't require special syntax to be invoked)
        public static implicit operator byte[](Bytes builder) => builder.GetBytes();
        public static implicit operator Bytes(byte[] b) => new Bytes(b);
        public static implicit operator string(Bytes builder) => BytesToStr(builder);

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

        //Add implicit ByteToPrimitive operations

        public override string ToString()
        {
            return BytesToStr(this);
        }
    }
}
