using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;

namespace TgenSerializer
{
    /// <summary>A utility class for an array of bytes</summary>
    public partial struct Bytes
    {
        private byte[] _buffer { get; set; }
        public byte[] Buffer => _buffer ?? Empty;

        /// <summary>Amount of bytes stored in object</summary>
        public int Length => Buffer.Length;

        public Bytes(byte b) : this(new byte[] { b }) { }
        public Bytes(byte[] arr) =>
            _buffer = arr;
        public Bytes(Bytes builder) =>
            _buffer = builder.Buffer;

        public bool IsEmptyOrNull() =>
            Buffer == null || Length == 0;

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
        /// <typeparam name="TTuple">A ValueTuple for example <c>(int, double)</c></typeparam>
        /// <returns>A tuple from the given array</returns>
        /// 
        public TTuple GetTuple<TTuple>() where TTuple : struct //maybe use struct constraints instead
        {
            TTuple tuple = new TTuple();

            //struct must be boxed since structs are value types.
            //Otherwise `SetValue` makes a new value then loses it
            object boxed = tuple;

            var fields = typeof(TTuple).GetFields(); //REFLECTION
            int index = 0;
            for (int i = 0; i < fields.Length; i++)
            {
                Type t = fields[i].FieldType;
                object val = Bytes.ByteToPrimitive(t, Buffer, index);
                fields[i].SetValue(boxed, val);
                index += Marshal.SizeOf(t);
            }
            return (TTuple)boxed;
        }

#if AllowUnsafe
        /// <summary>Blazingly faster as it requires no reflection or memory allocations although marked <c>unsafe</c></summary>
        public unsafe TTuple GetTupleUnsafe<TTuple>() where TTuple : struct
        {
            TTuple tuple = new TTuple();

            TTuple* ptr = &tuple;
            int size = sizeof(TTuple);
            try
            {
                fixed (byte* arrPtr = &Buffer[0])
                    System.Buffer.MemoryCopy(arrPtr, ptr, sizeof(TTuple), size);
            }
            catch (Exception) { throw; }


            return tuple;
        }
#endif

        public byte[] SubBytes(int start) =>
            SubBytes(start, Length);

        public byte[] SubBytes(int start, int end)
        {
            int size = end - start;
            byte[] sub = new byte[size];
            System.Buffer.BlockCopy(Buffer, start, sub, 0, size);
            return sub;
        }

        /// <summary>Like <c>Get</c> but for multiple values at once</summary>
        /// <returns>Array with the given primitive types</returns>
        public object[] GetValues(params Type[] types)
        {
            List<object> objs = new List<object>();
            int index = 0;
            foreach (Type t in types)
            {
                object val = Bytes.ByteToPrimitive(t, Buffer, index);
                objs.Add(val);
                index += Marshal.SizeOf(t);
            }
            return objs.ToArray();
        }

        //If b = byte[], the implicit operator will convert it to Bytes
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
                object val = Bytes.ByteToPrimitive(t, Buffer, index);
                fields[i].SetValue(boxed, val);
                index += Marshal.SizeOf(t);
            }
            return (T)boxed;
        }
#endif
    }
}
