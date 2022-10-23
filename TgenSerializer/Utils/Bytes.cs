using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
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
    public partial class Bytes
    {
        List<byte[]> list;

        /// <summary>Amount of bytes stored in object</summary>
        public int Length { get; private set; }

        #region Constructors
        public Bytes(byte b) : this(new byte[] { b }) { }
        public Bytes(byte[] arr) : this()
        {
            Append(arr);
        }
        public Bytes(Bytes builder) : this() 
        {
            Append(builder); //New
            //list.AddRange(builder.list); //Old
        }
        public Bytes()
        {
            //Array.Empty<byte>(); //Free memory allocation, use this when switching to array only
            list = new List<byte[]>();
            Length = 0;
        }
        #endregion

        public Bytes Append(Bytes obj)
        {
            list.AddRange(obj.list);
            //obj.list.Count IS NOT obj.Length
            //One is the amount of byte arrays and the other is the amount of bytes stored
            //Length += obj.list.Count will break EVERYTHING
            Length += obj.Length;
            return this;
        }
        public Bytes Append(byte[] obj)
        {
            if (obj == null)
                return this;

                //Create new buffers
                //Use of old ones could break when manually cleaned by a bufferpool or such
                byte[] arr = new byte[obj.Length];
                Buffer.BlockCopy(obj, 0, arr, 0, arr.Length);

                list.Add(arr);
                Length += arr.Length;

            return this;
        }

        /*
        /// <summary>Converts the object to an array of bytes</summary>
        public byte[] GetBytes()
        {
            if (list.Count == 1)
                return list[0];

            byte[] arr = new byte[Length];
            {
                long i = 0;
                foreach (var byteArr in list)
                    for (int x = 0; x < byteArr.Length;i++, x++)
                        arr[i] = byteArr[x];
            }
            return arr;
        }
        */

        /// <summary>Converts the object to an array of bytes</summary>
        public byte[] GetBytes()
        {
            byte[] arr = new byte[Length];
            int count = 0;
            lock (list) //Necessary lock! otherwise two threads can change it's content
            {
                for (int i = 0; i < list.Count; i++)
                {
                    list[i].CopyTo(arr, count);
                    count += list[i].Length;
                }

                //RETHINK ABOUT THAT
                //Meaning the object will always change it's byte[] storage each GetByte calls, might not be very optimal
                list.Clear();
                list.Add(arr);
            }
            return arr;
        }

        public ReadOnlyCollection<byte> GetBytesReadOnly()
        {
            byte[] arr = new byte[Length];
            int count = 0;
            lock (list) //Necessary lock! otherwise two threads can change it's content
            {
                for (int i = 0; i < list.Count; i++)
                {
                    list[i].CopyTo(arr, count);
                    count += list[i].Length;
                }

                //RETHINK ABOUT THAT
                //Meaning the object will always change it's byte[] storage each GetByte calls, might not be very optimal
                list.Clear();
                list.Add(arr);
            }
            return Array.AsReadOnly(arr);
        }

        /// <summary>Converts the bytes to T</summary>
        /// <param name="returnType">Type to cast to (must be a primitve or string type)</param>
        public object GetT(Type returnType) => ByteToPrimitive(returnType, this); //Implicit use of the function GetBytes()
        public object GetT(Type returnType, int startIndex) => ByteToPrimitive(returnType, this, startIndex);
        /// <summary>Converts the bytes to T</summary>
        /// <typeparam name="T">Type to cast to (must be a primitve or string type)</typeparam>
        public T GetT<T>() where T : unmanaged => ByteToPrimitive<T>(this); //Implicit use of the function GetBytes()
        public T GetT<T>(int startIndex) where T : unmanaged => ByteToPrimitive<T>(this, startIndex);

        /// <summary>
        /// Converts bytes into a class
        /// only works if bytes represents an object graph
        /// </summary>
        public object ToMarshall() =>
            ByteToClass(this);

        /// <summary>
        /// Converts bytes into a marshall object
        /// only works if bytes represents an object graph
        /// </summary>
        public T ToMarshall<T>() =>
            (T)ByteToClass(this);

        //If b = byte[], the implicit operator will convert it to a BinaryBuilder
        public static Bytes operator +(Bytes a, Bytes b) 
        { return new Bytes(a).Append(b); }
        //Used to be 'new Bytes(a).Append(b);'


        //public static Bytes operator -(Bytes a, int amount)
        //{ return new Bytes(a).RemoveEndBytes(amount); }

    }
}
