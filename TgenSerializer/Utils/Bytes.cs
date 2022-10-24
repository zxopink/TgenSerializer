using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;

namespace TgenSerializer
{
    /// <summary>A utility class for an array of bytes</summary>
    public partial struct Bytes
    {
        private byte[] array { get; set; }
        public byte[] Array => array ?? Empty;

        /// <summary>Amount of bytes stored in object</summary>
        public int Length => Array.Length;

        public Bytes(byte b) : this(new byte[] { b }) { }
        public Bytes(byte[] arr) =>
            array = arr;
        public Bytes(Bytes builder) =>
            array = builder.Array;

        public bool IsEmptyOrNull() =>
            Array == null || Length == 0;

        /// <summary>Converts the bytes to T</summary>
        /// <param name="returnType">must be a primitive</param>
        public object Get(Type returnType) => ByteToPrimitive(returnType, this); //Implicit use of the function GetBytes()
        /// <summary>Converts the bytes to T</summary>
        /// <typeparam name="T">must be a primitive or string type</typeparam>
        public T Get<T>() where T : unmanaged => ByteToPrimitive<T>(this); //Implicit use of the function GetBytes()

        //If b = byte[], the implicit operator will convert it to a BinaryBuilder
        public static Bytes operator +(Bytes a, Bytes b) 
        { return Bytes.Concat(a, b); }
        //Used to be 'new Bytes(a).Append(b);'

    }
}
