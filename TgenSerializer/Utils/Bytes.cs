using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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

        /// <summary>
        /// Gets a <c>ValueTuple</c> type with primitive item types and converts the Bytes to the given type.
        /// 
        /// <example>
        /// Example:
        ///    <code>
        ///    Bytes val = 10;
        ///    val += 20;
        ///    
        ///    (int, int) ret = val.GetTuple&lt;(int, int)&gt;();
        ///    Console.WriteLine(ret); //(10, 20)
        ///    </code>
        /// </example>
        /// </summary>
        /// <typeparam name="T">A ValueTuple for example <c>(int, double)</c></typeparam>
        /// <returns>A tuple from the given array</returns>
        /// 
        public T GetTuple<T>() where T : new() //maybe use struct constraints instead
        {
            T tuple = new T();

            //struct must be boxed since structs are value types.
            //Otherwise `SetValue` makes a new value then loses it
            object boxed = tuple;

            var fields = typeof(T).GetFields(); //REFLECTION
            int index = 0;
            for (int i = 0; i < fields.Length; i++)
            {
                Type t = fields[i].FieldType;
                object val = Bytes.ByteToPrimitive(t, Array, index);
                fields[i].SetValue(boxed, val);
                index += Marshal.SizeOf(t);
            }
            return (T)boxed;
        }

        /// <summary>
        /// Gets a ValueTuple type with primitive items and converts the Bytes to the given type.
        /// </summary>
        public object[] GetTypes(params Type[] types)
        {
            List<object> objs = new List<object>();
            int index = 0;
            foreach (Type t in types)
            {
                object val = Bytes.ByteToPrimitive(t, Array, index);
                objs.Add(val);
                index += Marshal.SizeOf(t);
            }
            return objs.ToArray();
        }

        //If b = byte[], the implicit operator will convert it to a BinaryBuilder
        public static Bytes operator +(Bytes a, Bytes b) 
        { return Bytes.Concat(a, b); }
        //Used to be 'new Bytes(a).Append(b);'


#if NETSTANDARD2_1
        /// <summary>
        /// Gets a <c>ValueTuple</c> type with primitive item types and converts the Bytes to the given type.
        /// 
        /// <example>
        /// Example:
        ///      <code>
        ///      Bytes val = 10;
        ///      val += 20;
        ///      
        ///      (int, int) ret = val.GetTuple&lt;(int, int)&gt;();
        ///      Console.WriteLine(ret); //(10, 20)
        ///      </code>
        /// </example>
        /// </summary>
        /// <typeparam name="T">A ValueTuple for example <c>(int, double)</c></typeparam>
        /// <returns>A tuple from the given array</returns>
        /// 
        public T GetTuple<T>() where T : ITuple, new()
        {
            T tuple = new T();

            //struct must be boxed since structs are value types.
            //Otherwise `SetValue` makes a new value then loses it
            object boxed = tuple; 

            var fields = typeof(T).GetFields(); //REFLECTION
            int index = 0;
            for (int i = 0; i < tuple.Length; i++)
            {
                Type t = tuple[i].GetType();
                object val = Bytes.ByteToPrimitive(t, Array, index);
                fields[i].SetValue(boxed, val);
                index += Marshal.SizeOf(t);
            }
            return (T)boxed;
        }
#endif
    }
}
