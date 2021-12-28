using System;
using System.Collections.Generic;
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

        [Obsolete]
        /// <summary>
        /// Could be slow or not even work in practice
        /// Test before publish
        /// </summary>
        /// <param name="index">Index of byte</param>
        /// <returns></returns>
        public int this[int index]
        {
            get
            {
                int counter = 0;
                for (int i = 0; i < Length; i++)
                {
                    if (list[i].Length + counter < index)
                        counter += list[i].Length;

                    else
                        return list[i][index - counter];
                }
                throw new StackOverflowException();
            }
            // get and set accessors
        }

        /// <summary>Amount of bytes stored in object</summary>
        public int Length { get; private set; }
        /*
        public int Length { get {
                int length = 0;
                foreach (var arr in list)
                    length += arr.Length;
                return length;
            } }
        */

        private int IndexToList(int index)
        {
            int count = 0, listIndex = 0;
            byte[] arr = list[listIndex];
            for (int i = 0; count < index; count++, i++)
                if (arr.Length == i)
                {
                    i = 0;
                    arr = list[++listIndex];
                }

            return listIndex;
        }

        #region Constructors
        public Bytes(byte b) : this(new byte[] { b }) { }
        public Bytes(byte[] arr) : this()
        {
            list.Add(arr);
            Length += arr.Length;
        }
        public Bytes(Bytes builder) : this() 
        {
            Append(builder); //New
            //list.AddRange(builder.list); //Old
        }
        public Bytes()
        {
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
            list.Add(obj);
            Length += obj.Length;
            return this;
        }

        public Bytes RemoveEndBytes(int amount) 
        {
            list.RemoveRange(Length - amount, amount);
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
            for (int i = 0; i < list.Count; i++)
            {
                list[i].CopyTo(arr, count);
                count += list[i].Length;
            }
            return arr;
        }

        /// <summary>Converts the bytes to T</summary>
        /// <param name="returnType">Type to cast to (must be a primitve or string type)</param>
        public object GetT(Type returnType) => ByteToPrimitive(returnType, this); //Implicit use of the function GetBytes()
        public object GetT(Type returnType, int startIndex) => ByteToPrimitive(returnType, this, startIndex);
        /// <summary>Converts the bytes to T</summary>
        /// <typeparam name="T">Type to cast to (must be a primitve or string type)</typeparam>
        public T GetT<T>() => ByteToPrimitive<T>(this); //Implicit use of the function GetBytes()
        public T GetT<T>(int startIndex) => ByteToPrimitive<T>(this, startIndex);

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
