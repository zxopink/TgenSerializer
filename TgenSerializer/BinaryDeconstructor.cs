using System;
using System.Collections;
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
        private static BinaryBuilder startClass = BinaryGlobalOperations.startClass; //sign for the start of a class
        private static BinaryBuilder equals = BinaryGlobalOperations.equals; //sign for equals 
        private static BinaryBuilder endClass = BinaryGlobalOperations.endClass; //sign for the end of a class
        private static BinaryBuilder startEnum = BinaryGlobalOperations.startEnum; //start of array (enumer is sort of a collection like array and list, I like to call it array at time)
        private static BinaryBuilder betweenEnum = BinaryGlobalOperations.betweenEnum; //spaces between items/members in the array
        private static BinaryBuilder endEnum = BinaryGlobalOperations.endEnum; //end of array
        private static BinaryBuilder serializerEntry = BinaryGlobalOperations.serializerEntry; //start of serializer object
        private static BinaryBuilder serializerExit = BinaryGlobalOperations.serializerExit; //end of serializer object
        private static BinaryBuilder typeEntry = BinaryGlobalOperations.typeEntry; //divides the name and type of an object]
        [Obsolete]
        private static BinaryBuilder nullObj = BinaryGlobalOperations.nullObj; //sign for a nullObj (deprecated)

        private const BindingFlags bindingFlags = GlobalOperations.bindingFlags; //specifies to get both public and non public fields and properties
        #endregion

        private static BinaryBuilder StrToByte(string str) => Encoding.ASCII.GetBytes(str);

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
            BinaryBuilder builder = new BinaryBuilder();
            byte[] result = (startClass + obj.GetType().AssemblyQualifiedName + equals + Deconstruction(obj) + endClass);
            //Console.WriteLine("Binary Formatter Decompression: " + result.Length);
            return result;
            //must delcare the type at first so the constructor later on knows with what type it deals
            //the properties and fields can be aligned later on by using the first type, like a puzzle
            //the name of the object doesn't matter (therefore doesn't need to be saved) as well since the it will be changed anyways
        }

        private static BinaryBuilder Deconstruction(object obj)
        {
            if (obj == null)
                return nullObj;

            if (obj.GetType().IsPrimitive || obj is string)
                return BinaryBuilder.PrimitiveToByte(obj);

            if (!obj.GetType().IsSerializable) //PROTECTION
                return new byte[0]; //don't touch the field, CONSIDER: throwing an error

            if (obj is IList) //string is also an enum but will never reach here thanks to the primitive check
            {
                return ListObjDeconstructor((IList)obj);
            }

            var fields = obj.GetType().GetFields(bindingFlags);
            BinaryBuilder objGraph = new BinaryBuilder();
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
                if (field.IsNotSerialized) //PROTECTION
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

        private static BinaryBuilder GetNameOfBackingField(string backingField)
        {
            //backing field follows by the pattern: "<'name'>k__BackingField"
            StringBuilder name = new StringBuilder();
            name.Append(backingField);
            name.Remove(0, 1); //cuts the field's '<' at the start (NOT AN ENUM!)
            name.Remove(backingField.Length - 17, 16); //cuts the '>k__BackingField' at the end
            return name.ToString();
        }

        private static BinaryBuilder ListObjDeconstructor(IList list)
        {
            BinaryBuilder objGraph = new BinaryBuilder(); //TODO: ADD A WAY TO COUNT MEMEBERS AND AVOID NULL sends
            //THIS IS A BIG NONO (spent too much on to find these issue)
            //IF ANOTHER UNSOLVALBE ISSUE RAISES LISTS ARE YOUR FIRST WARNING
            if (list.GetType().IsArray)
            {
                //if (list is List<byte>)
                //    return ((List<byte>)list).ToArray();
                objGraph.Append(list.Count.ToString());
                if (list is byte[])
                {
                    objGraph.Append(startEnum);
                    objGraph.Append((byte[])list);
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
