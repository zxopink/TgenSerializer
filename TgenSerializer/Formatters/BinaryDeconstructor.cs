using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;

namespace TgenSerializer
{
    public class BinaryDeconstructor
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
        [Obsolete]
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

        public static byte[] Deconstruct(object obj)
        {
            byte[] result = (startClass + obj.GetType().AssemblyQualifiedName + equals + Deconstruction(obj) + endClass);
            //Console.WriteLine("Binary Formatter Decompression: " + result.Length);
            return result;
            //must delcare the type at first so the constructor later on knows with what type it deals
            //the properties and fields can be aligned later on by using the first type, like a puzzle
            //the name of the object doesn't matter (therefore doesn't need to be saved) as well since the it will be changed anyways
        }

        private static Bytes Deconstruction(object obj)
        {
            if (obj == null)
                return nullObj;

            Type type = obj.GetType();

            if (type.IsPrimitive)
                return Bytes.PrimitiveToByte(obj);

            if (obj is string str)
                return Bytes.ToBytes(str.Length, Bytes.StrToBytes(str));

            if (obj is ISerializable)
            {
                var writer = new DataWriter();
                ((ISerializable)obj).Serialize(writer);
                return writer.data;
            }

            if (!type.IsSerializable) //PROTECTION
                return Bytes.Empty; //don't touch the field, CONSIDER: throwing an error

            if (obj is IList) //string is also an enum but will never reach here thanks to the primitive check
            {
                return ListObjDeconstructor((IList)obj);
            }

            var fields = GetFieldInfosIncludingBaseClasses(type, bindingFlags);//type.GetFields(bindingFlags);
            Bytes objGraph = new Bytes();
            #region SpecialCases
            /*Special cases so far:
             * 1. Object is null (Done)
             * 2. Object points to itself in an infnite loop (Done)
             * 3. backingField (Done)
             * 4. Object is an enum/list/array (Done)
            */
            #endregion
            foreach (var field in fields)
            {
                //the field is a field class, the fieldValue is the value of this field (the actual object)
                //for examle field is "int num = 5" and the field value is the 5
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
                    objGraph.Append(startClass + field.Name + equals + Deconstruction(fieldValue) + endClass);
                else
                    objGraph.Append(startClass + GetNameOfBackingField(field.Name) + equals + Deconstruction(fieldValue) + endClass);
            }
            return objGraph;
        }

        //TODO:
        //If B inherits from A and they both have a private field with the same name
        //The serializer might screw up during construction, make them different by calling them B.x and A.x
        //Or 1.x and 2.x based on the hierarchy level
        public static FieldInfo[] GetFieldInfosIncludingBaseClasses(Type type, BindingFlags bindingFlags)
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

        private static Bytes ListObjDeconstructor(IList list)
        {
            Bytes objGraph = new Bytes(); //TODO: ADD A WAY TO COUNT MEMEBERS AND AVOID NULL sends
            //THIS IS A BIG NONO (spent too much on to find these issue)
            //IF ANOTHER UNSOLVALBE ISSUE RAISES LISTS ARE YOUR FIRST WARNING
            if (list.GetType().IsArray)
            {
                //if (list is List<byte>)
                //    return ((List<byte>)list).ToArray();
                objGraph.Append(list.Count.ToString());
                if (list is byte[] byteArr)
                {
                    objGraph.Append(startEnum);
                    objGraph.Append(byteArr);
                    objGraph.Append(endEnum);
                    return objGraph;
                }
            }
            objGraph.Append(startEnum);
            foreach (var member in list)
            {
                if (member == null) //Don't add to the list,
                {
                    //objGraph.Append(nullObj + betweenEnum); //No sending nulls
                    continue;
                }
                objGraph.Append(Deconstruction(member) + endClass); //between Enum is like endclass
            }
            objGraph.Append(endEnum);
            return objGraph;
        }
    }
}
