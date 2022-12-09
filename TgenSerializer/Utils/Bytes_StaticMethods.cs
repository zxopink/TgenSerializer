using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;

namespace TgenSerializer
{
    public partial struct Bytes
    {
        public static Bytes Empty => Array.Empty<byte>();
        /// <summary>Converts a primitive (or string) object to an array of bytes</summary>
        //public static byte[] P2B(object obj) => PrimitiveToByte(obj);
        /// <summary>Converts a primitive (or string) object to an array of bytes</summary>
        public static byte[] P2B<T>(T obj) where T : unmanaged => PrimitiveToByte<T>(obj);
        public static byte[] PrimitiveToByte(object obj)
        {
            byte[] ret;
            if (obj is sbyte valSbyte)
            {
                string byteString = valSbyte.ToString("X2");
                return new byte[1] { byte.Parse(byteString, System.Globalization.NumberStyles.HexNumber) };
            }
            else if (obj is byte valByte)
            {
                return new byte[1] { valByte };
            }
            else if (obj is short valInt16)
            {
                ret = BitConverter.GetBytes(valInt16);
            }
            else if (obj is char valChar)
            {
                ret = BitConverter.GetBytes(valChar);
            }
            else if (obj is int valInt32)
            {
                ret = BitConverter.GetBytes(valInt32);
            }
            else if (obj is bool valBool)
            {
                ret = BitConverter.GetBytes(valBool);
            }
            else if (obj is long valInt64)
            {
                ret = BitConverter.GetBytes(valInt64);
            }
            else if (obj is float valFloat)
            {
                ret = BitConverter.GetBytes(valFloat);
            }
            else if (obj is double valDouble)
            {
                ret = BitConverter.GetBytes(valDouble);
            }
            else if (obj is ushort valUint16)
            {
                ret = BitConverter.GetBytes(valUint16);
            }
            else if (obj is uint valUint32)
            {
                ret = BitConverter.GetBytes(valUint32);
            }
            else if (obj is ulong valUint64)
            {
                ret = BitConverter.GetBytes(valUint64);
            }
            else if (obj is decimal deci)
            {
                //TODO
                int[] ints = Decimal.GetBits(deci); //Decimal is 16 bytes, 4 ints
                byte[] buf = new byte[ints.Length * sizeof(int)];
                System.Buffer.BlockCopy(ints, 0, buf, 0, buf.Length);
                ret = buf;
            }
            else
            {
                throw new SerializationException("Primitive object of type " + obj.GetType() + " cannot be converted into bytes");
            }
            if (BitConverter.IsLittleEndian)
                Array.Reverse(ret);
            return ret;

        } //Boxing (Garbage collection)
        public static byte[] PrimitiveToByte<T>(T obj) where T : unmanaged
        {
            byte[] ret;
            if (obj is sbyte valSbyte)
            {
                string byteString = valSbyte.ToString("X2");
                return new byte[1] { Byte.Parse(byteString, System.Globalization.NumberStyles.HexNumber) };
            }
            else if (obj is byte valByte)
            {
                return new byte[1] { valByte };
            }
            else if (obj is short valInt16)
            {
                ret = BitConverter.GetBytes(valInt16);
            }
            else if (obj is char valChar)
            {
                ret = BitConverter.GetBytes(valChar);
            }
            else if (obj is int valInt32)
            {
                ret = BitConverter.GetBytes(valInt32);
            }
            else if (obj is bool valBool)
            {
                ret = BitConverter.GetBytes(valBool);
            }
            else if (obj is long valInt64)
            {
                ret = BitConverter.GetBytes(valInt64);
            }
            else if (obj is float valFloat)
            {
                ret = BitConverter.GetBytes(valFloat);
            }
            else if (obj is double valDouble)
            {
                ret = BitConverter.GetBytes(valDouble);
            }
            else if (obj is ushort valUint16)
            {
                ret = BitConverter.GetBytes(valUint16);
            }
            else if (obj is uint valUint32)
            {
                ret = BitConverter.GetBytes(valUint32);
            }
            else if (obj is ulong valUint64)
            {
                ret = BitConverter.GetBytes(valUint64);
            }
            else if (obj is decimal deci)
            {
                //TODO
                int[] ints = Decimal.GetBits(deci); //Decimal is 16 bytes, 4 ints
                byte[] buf = new byte[ints.Length * sizeof(int)];
                System.Buffer.BlockCopy(ints, 0, buf, 0, buf.Length);
                ret = buf;
            }
            else
            {
                throw new SerializationException("Primitive object of type " + obj.GetType() + " cannot be converted into bytes");
            }
            if (BitConverter.IsLittleEndian)
                Array.Reverse(ret);
            return ret;
        } //No boxing

        /// <summary>Converts an array of bytes to a specified primitive (or string) object</summary>
        //public static object B2P(Type objType, byte[] objData) => ByteToPrimitive(objType, objData);
        /// <summary>Converts an array of bytes to a specified primitive (or string) object</summary>
        public static T B2P<T>(byte[] objData) where T : unmanaged => ByteToPrimitive<T>(objData);
        public static T B2P<T>(byte[] objData, int startIndex) where T : unmanaged => ByteToPrimitive<T>(objData, startIndex);

        public static object ByteToPrimitive(Type objType, byte[] objData, int startIndex = 0)
        {
            if (BitConverter.IsLittleEndian)
                Array.Reverse(objData);
            object val;
            if (objType.Equals(typeof(sbyte)))
            {
                val = BitConverter.ToChar(objData, startIndex);
            }
            else if (objType.Equals(typeof(byte)))
            {
                val = objData[startIndex];
            }
            else if (objType.Equals(typeof(short)))
            {
                val = BitConverter.ToInt16(objData, startIndex);
            }
            else if (objType.Equals(typeof(int)))
            {
                val = BitConverter.ToInt32(objData, startIndex);
            }
            else if (objType.Equals(typeof(bool)))
            {
                val = BitConverter.ToBoolean(objData, startIndex);
            }
            else if (objType.Equals(typeof(long)))
            {
                val = BitConverter.ToInt64(objData, startIndex);
            }
            else if (objType.Equals(typeof(float)))
            {
                val = BitConverter.ToSingle(objData, startIndex);
            }
            else if (objType.Equals(typeof(double)))
            {
                val = BitConverter.ToDouble(objData, startIndex);
            }
            else if (objType.Equals(typeof(ushort)))
            {
                val = BitConverter.ToUInt16(objData, startIndex);
            }
            else if (objType.Equals(typeof(uint)))
            {
                val = BitConverter.ToUInt32(objData, startIndex);
            }
            else if (objType.Equals(typeof(ulong)))
            {
                val = BitConverter.ToUInt64(objData, startIndex);
            }
            else if (objType.Equals(typeof(decimal)))
            {
                //TODO
                throw new NotSupportedException("Does not support decimal numbers yet");
            }
            else
            {
                throw new SerializationException("Type " + objType + " cannot be converted into bytes");
            }
            if (BitConverter.IsLittleEndian)
                Array.Reverse(objData);
            return val;
        } //Boxing (Garbage collection)
        /// <summary>
        /// Turns a byte[] into a primitive object
        /// </summary>
        /// <param name="objType">Type of object</param>
        /// <param name="objData">The object in byte[]</param>
        /// <param name="startIndex">optional, the startIndex in the byte[]</param>
        /// <returns>A primitive object</returns>
        public static T ByteToPrimitive<T>(byte[] objData, int startIndex = 0) where T : unmanaged
        {
            if (BitConverter.IsLittleEndian)
                Array.Reverse(objData);
            T val;
            if (typeof(T).Equals(typeof(sbyte)))
            {
                var obj = BitConverter.ToChar(objData, startIndex);
                val = obj is T ret ? ret : default;
            }
            else if (typeof(T).Equals(typeof(byte)))
            {
                var obj = objData[startIndex];
                val = obj is T ret ? ret : default;
            }
            else if (typeof(T).Equals(typeof(short)))
            {
                var obj = BitConverter.ToInt16(objData, startIndex);
                val = obj is T ret ? ret : default;
            }
            else if (typeof(T).Equals(typeof(int)))
            {
                var obj = BitConverter.ToInt32(objData, startIndex);
                val = obj is T ret ? ret : default;
            }
            else if (typeof(T).Equals(typeof(bool)))
            {
                var obj = BitConverter.ToBoolean(objData, startIndex);
                val = obj is T ret ? ret : default;
            }
            else if (typeof(T).Equals(typeof(long)))
            {
                var obj = BitConverter.ToInt64(objData, startIndex);
                val = obj is T ret ? ret : default;
            }
            else if (typeof(T).Equals(typeof(float)))
            {
                var obj = BitConverter.ToSingle(objData, startIndex);
                val = obj is T ret ? ret : default;
            }
            else if (typeof(T).Equals(typeof(double)))
            {
                var obj = BitConverter.ToDouble(objData, startIndex);
                val = obj is T ret ? ret : default;
            }
            else if (typeof(T).Equals(typeof(ushort)))
            {
                var obj = BitConverter.ToUInt16(objData, startIndex);
                val = obj is T ret ? ret : default;
            }
            else if (typeof(T).Equals(typeof(uint)))
            {
                var obj = BitConverter.ToUInt32(objData, startIndex);
                val = obj is T ret ? ret : default;
            }
            else if (typeof(T).Equals(typeof(ulong)))
            {
                var obj = BitConverter.ToUInt64(objData, startIndex);
                val = obj is T ret ? ret : default;
            }
            else if (typeof(T).Equals(typeof(decimal)))
            {
                //TODO
                throw new NotSupportedException("Does not support decimal numbers yet");
            }
            else
            {
                throw new SerializationException("Type " + typeof(T) + " cannot be converted into bytes");
            }
            if (BitConverter.IsLittleEndian)
                Array.Reverse(objData);
            return val;
        } //No boxing

        public static Bytes GetBytes(bool value) => (Bytes)BitConverter.GetBytes(value);
        public static Bytes GetBytes(char value) => (Bytes)BitConverter.GetBytes(value);
        public static Bytes GetBytes(short value) => (Bytes)BitConverter.GetBytes(value);
        public static Bytes GetBytes(int value) => (Bytes)BitConverter.GetBytes(value);
        public static Bytes GetBytes(long value) => (Bytes)BitConverter.GetBytes(value);
        public static Bytes GetBytes(ushort value) => (Bytes)BitConverter.GetBytes(value);
        public static Bytes GetBytes(uint value) => (Bytes)BitConverter.GetBytes(value);
        public static Bytes GetBytes(ulong value) => (Bytes)BitConverter.GetBytes(value);
        public static Bytes GetBytes(float value) => (Bytes)BitConverter.GetBytes(value);
        public static Bytes GetBytes(double value) => (Bytes)BitConverter.GetBytes(value);
        public static Bytes GetBytes(string value) => (Bytes)StrToBytes(value);
        public static Bytes GetBytes(params object[] objects)
        {
            byte[][] list = new byte[objects.Length][];
            for (int i = 0; i < list.Length; i++)
            {
                var item = objects[i];
                if (item is byte[] byteArr)
                {
                    list[i] = byteArr;
                    continue;
                }
                list[i] = PrimitiveToByte(item);
            }
            return (Bytes)Concat(list);
        }

        public static byte[] Concat(params byte[][] bytes)
        {
            int size = bytes.Sum(arr => arr.Length);
            byte[] ret = new byte[size];
            int index = 0;
            for (int i = 0; i < bytes.Length; index += bytes[i].Length, i++)
                System.Buffer.BlockCopy(bytes[i], 0, ret, index, bytes[i].Length);

            return ret;
        }

        public static byte[] Concat(IEnumerable<byte[]> bytes)
        {
            int size = bytes.Sum(arr => arr.Length);
            byte[] ret = new byte[size];
            int index = 0;
            foreach (var byteGroup in bytes)
            {
                System.Buffer.BlockCopy(byteGroup, 0, ret, index, byteGroup.Length);
                index += byteGroup.Length;
            }
            return ret;
        }

        public static bool ToBoolean(byte[] value, int startIndex) => BitConverter.ToBoolean(value, startIndex);
        public static char ToChar(byte[] value, int startIndex) => BitConverter.ToChar(value, startIndex);
        public static double ToDouble(byte[] value, int startIndex) => BitConverter.ToDouble(value, startIndex);
        public static short ToInt16(byte[] value, int startIndex) => BitConverter.ToInt16(value, startIndex);
        public static int ToInt32(byte[] value, int startIndex) => BitConverter.ToInt32(value, startIndex);
        public static long ToInt64(byte[] value, int startIndex) => BitConverter.ToInt64(value, startIndex);
        public static float ToSingle(byte[] value, int startIndex) => BitConverter.ToSingle(value, startIndex);
        public static string ToString(byte[] value) => BytesToStr(value);
        public static ushort ToUInt16(byte[] value, int startIndex) => BitConverter.ToUInt16(value, startIndex);
        public static uint ToUInt32(byte[] value, int startIndex) => BitConverter.ToUInt32(value, startIndex);
        public static ulong ToUInt64(byte[] value, int startIndex) => BitConverter.ToUInt64(value, startIndex);

        /// <summary>A UTF8 encryption</summary>
        public static byte[] StrToBytes(string str) => Encoding.UTF8.GetBytes(str);
        public static byte[] StrToBytes(string str, Encoding encoder) => encoder.GetBytes(str);

        /// <summary>A UTF8 encryption</summary>
        public static string BytesToStr(byte[] b) => Encoding.UTF8.GetString(b);
        public static string BytesToStr(byte[] b, Encoding encoder) => encoder.GetString(b);
    }
}
