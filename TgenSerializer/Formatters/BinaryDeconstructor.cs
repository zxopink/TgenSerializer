using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using TgenSerializer.Utils;

namespace TgenSerializer
{
    internal class BinaryDeconstructor
    {
        //These fields are shared both by the constructor and constructor
        //NOTE: BetweenEnum and EndClass must have the same lenght since the serlizer treats them as the end of a class
        #region Global Fields
        private static Bytes startClass = BinaryGlobalOperations.startClass; //sign for the start of a class
        private static Bytes equals = BinaryGlobalOperations.equals; //sign for equals 
        private static Bytes endClass = BinaryGlobalOperations.endClass; //sign for the end of a class
        private static Bytes startEnum = BinaryGlobalOperations.startEnum; //start of array (enumer is sort of a collection like array and list, I like to call it array at time)
        private static Bytes betweenEnum = BinaryGlobalOperations.betweenEnum; //spaces between items/members in the array
        private static Bytes endEnum = BinaryGlobalOperations.endEnum; //end of array
        private static Bytes serializerEntry = BinaryGlobalOperations.serializerEntry; //start of serializer object
        private static Bytes serializerExit = BinaryGlobalOperations.serializerExit; //end of serializer object
        private static Bytes typeEntry = BinaryGlobalOperations.typeEntry; //divides the name and type of an object]
        private static Bytes nullObj = BinaryGlobalOperations.nullObj; //sign for a nullObj (deprecated)

        private const BindingFlags bindingFlags = GlobalOperations.bindingFlags; //specifies to get both public and non public fields and properties
        #endregion

        #region PrimitiveToByte
        /// <summary>
        /// Turns a primitive object to byte[]
        /// (Taken from microsoft wiki) 
        /// </summary>
        /// <param name="obj">The primitive object</param>
        /// <returns>The byte[]</returns>
        private static byte[] GetPrimitiveByte(object obj)
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
                return Encoding.ASCII.GetBytes((string)obj);
            }
            else if (obj is long)
            {
                return BitConverter.GetBytes((long)obj);
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
        #endregion

        public static byte[] Deconstruct(object obj, IList<TgenConverter> converters)
        {
            try
            {
                Type objType = obj.GetType();
                uint? id = converters?.FirstOrDefault(conv => conv.Type == objType)?.Id;

                BytesBuilder builder = new BytesBuilder();
                if (id.HasValue)
                    builder.Append(id.Value);
                else
                    builder.Append(objType.AssemblyQualifiedName);
                builder.Append(equals);

                var destructor = new DeconstructionGraph(builder);
                destructor.Start(obj);

                builder.Append(endClass);
                return builder.ToBytes();
            }
            catch (SerializationException)
            { throw; }
            catch (Exception e)
            { throw new MarshalException(e); }
        }

        private struct DeconstructionGraph
        {
            private BytesBuilder Graph { get; set; }
            public DeconstructionGraph(BytesBuilder builder)
            {
                Graph = builder;
            }
            public byte[] Start(object obj)
            {
                Destruct(obj);
                return Graph.ToBytes();
            }
            private void Destruct(object obj)
            {
                Type type = obj.GetType();

                if (obj == null)
                {
                    Graph.Append(nullObj);
                    return;
                }

                else if (type.IsPrimitive)
                {
                    Graph.Append(Bytes.PrimitiveToByte(obj));
                    return;
                }

                else if (obj is string str)
                {
                    Bytes data = Bytes.StrToBytes(str);
                    Graph.Append(data.Length, data);
                    return;
                }

                else if (obj is ISerializable serializableObj)
                {
                    Bytes data = serializableObj.Serialize();
                    int size = data.Length;
                    Graph.Append(size);
                    Graph.Append(data);
                    return;
                }

                else if (!type.IsSerializable) //PROTECTION
                {
                    throw new SerializationException($"{type} is missing a {nameof(SerializableAttribute)}");
                }

                else if (obj is IList list) //Breaks down ILists with optimization for primitive arrays
                {
                    ListObjDeconstructor(list);
                    return;
                }

                FieldInfo[] fields = GetFieldsIncludingBaseClasses(type, bindingFlags);//type.GetFields(bindingFlags);
                foreach (FieldInfo field in fields)
                {
                    if (field.IsNotSerialized)
                        continue; //Don't touch the object, was meant to not be serialized

                    object fieldValue = field.GetValue(obj);

                    if (fieldValue == null) //No sending nulls
                        continue; //Null object, ignore. spares the text in the writing

                    if (fieldValue == obj)
                        throw new StackOverflowException("An object points to itself"); //Will cause an infinite loop, so just throw it

                    //BACKING FIELDS ARE IGNORED BECAUSE THE PROPERTIES LINE SAVES THEM INSTEAD
                    //one of the few compiler generated attributes is backing fields
                    //backing field is a proprty with a get and set only, which has a hidden field behind it
                    //instead we save this field in the properties
                    if (Attribute.GetCustomAttribute(field, typeof(CompilerGeneratedAttribute)) == null) //this line checks for backing field
                    {
                        Graph.Append(startClass, field.Name, equals);
                        Destruct(fieldValue);
                        Graph.Append(endClass);
                    }
                    else
                    {
                        Graph.Append(startClass, GetNameOfBackingField(field.Name), equals);
                        Destruct(fieldValue);
                        Graph.Append(endClass);
                    }
                }
            }

            //TODO:
            //If B inherits from A and they both have a private field with the same name
            //The serializer might screw up during construction, make them different by calling them B.x and A.x
            //Or 1.x and 2.x based on the hierarchy level
            public static FieldInfo[] GetFieldsIncludingBaseClasses(Type type, BindingFlags bindingFlags)
            {
                FieldInfo[] fieldInfos = type.GetFields(bindingFlags);

                // If this class doesn't have a base, don't waste any time
                if (type.BaseType == typeof(object))
                {
                    return fieldInfos;
                }
                else
                {   // Otherwise, collect all types up to the furthest base class
                    var currentType = type;
                    var fieldComparer = new FieldInfoComparer();
                    var fieldInfoList = new HashSet<FieldInfo>(fieldInfos, fieldComparer);
                    while (currentType != typeof(object))
                    {
                        fieldInfos = currentType.GetFields(bindingFlags);
                        fieldInfoList.UnionWith(fieldInfos);
                        currentType = currentType.BaseType;
                    }
                    return fieldInfoList.ToArray();
                }
            }
            private class FieldInfoComparer : IEqualityComparer<FieldInfo>
            {
                public bool Equals(FieldInfo x, FieldInfo y)
                {
                    return x.DeclaringType == y.DeclaringType && x.Name == y.Name;
                }

                public int GetHashCode(FieldInfo obj)
                {
                    return obj.Name.GetHashCode() ^ obj.DeclaringType.GetHashCode();
                }
            }

            private static Bytes GetNameOfBackingField(string backingField)
            {
                //backing field follows by the pattern: "<'name'>k__BackingField"
                //cut the field's '<' at the start (NOT AN ENUM!)
                //cut the '>k__BackingField' at the end

                //Get the name's length
                int count = backingField.IndexOf('>') - 1; //Minus the '<'
                return backingField.Substring(1, count);
            }

            private void ListObjDeconstructor(IList list)
            {
                Type type = list.GetType();
                if (type.IsArray && type.GetElementType().IsPrimitive)
                {
                    Array primArray = (Array)list;
                    int len = Buffer.ByteLength(primArray);

                    byte[] data;
                    if (list is byte[] byteArr)
                        data = byteArr;
                    else
                    {
                        data = new byte[len];
                        Buffer.BlockCopy(primArray, 0, data, 0, len);
                    }
                    Graph.Append(len);
                    Graph.Append(startEnum);
                    Graph.Append(data);
                    Graph.Append(endEnum);
                    return;
                }

                if(type.IsArray)
                    Graph.Append(list.Count);

                Graph.Append(startEnum);
                foreach (var member in list)
                {
                    if (member == null) continue;
                    Destruct(member);
                    Graph.Append(endClass); //between Enum is like endclass
                }
                Graph.Append(endEnum);
                return;
            }
        }
    }
}
