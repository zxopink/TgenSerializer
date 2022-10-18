using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace TgenSerializer
{
    public partial class Bytes
    {
        /// <summary>Converts a primitive (or string) object to an array of bytes</summary>
        public static byte[] P2B(object obj) => PrimitiveToByte(obj);
        /// <summary>Converts a primitive (or string) object to an array of bytes</summary>
        public static byte[] P2B<T>(T obj) => PrimitiveToByte(obj);

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

        /// <summary>Converts an array of bytes to a specified primitive (or string) object</summary>
        public static object B2P(Type objType, byte[] objData) => ByteToPrimitive(objType, objData);
        /// <summary>Converts an array of bytes to a specified primitive (or string) object</summary>
        public static T B2P<T>(byte[] objData) => ByteToPrimitive<T>(objData);

        /// <summary>
        /// Turns a byte[] into a primitive object
        /// </summary>
        /// <param name="objType">Type of object</param>
        /// <param name="objData">The object in byte[]</param>
        /// <param name="startIndex">optional, the startIndex in the byte[]</param>
        /// <returns>A primitive object</returns>
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
                if (startIndex == 0) //Minor performance improvement
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

        public static byte[] GetBytes(bool value) => BitConverter.GetBytes(value);
        public static byte[] GetBytes(char value) => BitConverter.GetBytes(value);
        public static byte[] GetBytes(short value) => BitConverter.GetBytes(value);
        public static byte[] GetBytes(int value) => BitConverter.GetBytes(value);
        public static byte[] GetBytes(long value) => BitConverter.GetBytes(value);
        public static byte[] GetBytes(ushort value) => BitConverter.GetBytes(value);
        public static byte[] GetBytes(uint value) => BitConverter.GetBytes(value);
        public static byte[] GetBytes(ulong value) => BitConverter.GetBytes(value);
        public static byte[] GetBytes(float value) => BitConverter.GetBytes(value);
        public static byte[] GetBytes(double value) => BitConverter.GetBytes(value);
        public static byte[] GetBytes(string value) => StrToBytes(value);

        public static byte[] ToBytes(params object[] objects)
        {
            List<byte[]> list = new List<byte[]>();
            int length = 0;
            foreach (var item in objects)
            {
                byte[] arr = PrimitiveToByte(item);
                length += arr.Length;
                list.Add(arr);
            }
            byte[] finalArr = new byte[length];
            int countArr = 0;
            for (int i = 0; i < length; i += list[countArr].Length, countArr++)
                Buffer.BlockCopy(list[countArr], 0, finalArr, i, list[countArr].Length);

            return finalArr;
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

        /// <summary>
        /// Converts the object to object graph of bytes
        /// </summary>
        /// <param name="obj">A non primitive serializable object</param>
        /// <returns>Object graph</returns>
        public static byte[] ClassToByte(object obj) =>
            BinaryDeconstructor.Deconstruct(obj);

        /// <summary>
        /// Converts an object graph to marshall object
        /// </summary>
        /// <param name="obj">An object graph</param>
        /// <returns>marshalled object</returns>
        public static object ByteToClass(byte[] objGraph) =>
            BinaryConstructor.Construct(objGraph);

        public static T ByteToPrimitive<T>(byte[] objData, int startIndex = 0) => (T)ByteToPrimitive(typeof(T), objData, startIndex);

        /// <summary>A UTF8 encryption</summary>
        public static byte[] StrToBytes(string str) => Encoding.UTF8.GetBytes(str);
        public static byte[] StrToBytes(string str, Encoding encoder) => encoder.GetBytes(str);

        /// <summary>A UTF8 encryption</summary>
        public static string BytesToStr(byte[] b) => Encoding.UTF8.GetString(b);
        public static string BytesToStr(byte[] b, Encoding encoder) => encoder.GetString(b);
    }
}
