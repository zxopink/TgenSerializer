using System;
using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;

namespace TgenSerializer
{
    public static class JsonDeconstructor
    {
        //These fields are shared both by the constructor and constructor
        //NOTE: BetweenEnum and EndClass must have the same lenght since the serlizer treats them as the end of a class
        #region Global Fields
        private const string startClass = JsonGlobalOperations.startClass; //sign for the start of a class
        private const string equals = JsonGlobalOperations.equals; //sign for equals 
        private const string endClass = JsonGlobalOperations.endClass; //sign for the end of a class
        private const string startVal = JsonGlobalOperations.startVal; //sign for the start of a value
        private const string endVal = JsonGlobalOperations.endVal; //sign for the end of a value
        private const string startEnum = JsonGlobalOperations.startEnum; //start of array (enumer is sort of a collection like array and list, I like to call it array at time)
        private const string betweenEnum = JsonGlobalOperations.betweenEnum; //spaces between items/members in the array
        private const string endEnum = JsonGlobalOperations.endEnum; //end of array
        private const string serializerEntry = JsonGlobalOperations.serializerEntry; //start of serializer object
        private const string serializerExit = JsonGlobalOperations.serializerExit; //end of serializer object
        private const string typeEntry = JsonGlobalOperations.typeEntry; //divides the name and type of an object
        private const string nullObj = JsonGlobalOperations.nullObj; //sign for a nullObj (deprecated)

        private static BindingFlags bindingFlags = GlobalOperations.bindingFlags; //specifies to get both public and non public fields and properties
        #endregion

        public static string Deconstruct(object obj)
        {
            StringBuilder objGraph = new StringBuilder();
            string result = Deconstruction(obj);
            //Console.WriteLine("String Formatter Decompression: " + ByteBuilder.StrToBytes(result).Length);
            return result;
            //must delcare the type at first so the constructor later on knows with what type it deals
            //the properties and fields can be aligned later on by using the first type, like a puzzle
            //the name of the object doesn't matter (therefore doesn't need to be saved) as well since the it will be changed anyways
        }

        private static string Deconstruction(object obj)
        {
            if (obj == null)
                return nullObj;

            {//Primitive values
                if (obj.GetType().IsPrimitive)
                    return obj.ToString();

                if(obj is string)
                    return startVal + obj.ToString() + endVal;
            }

            if (!obj.GetType().IsSerializable) //PROTECTION
                return string.Empty; //don't touch the field, CONSIDER: throwing an error

            //else if (obj is ISerializable)
            //{
            //    return SeriObjDeconstructor((ISerializable)obj);
            //}

            if (obj is IList) //string is also an enum but will never reach here thanks to the primitive check
            {
                return ListObjDeconstructor((IList)obj);
            }

            #region SpecialCases
            /*Special cases so far:
             * 1. Object is null (Done)
             * 2. Object points to itself in an infnite loop (Done)
             * 3. backingField (Done)
             * 4. Object is an enum/list/array (Done)
            */
            #endregion

            var fields = obj.GetType().GetFields(bindingFlags);
            StringBuilder objGraph = new StringBuilder();
            objGraph.Append(startClass);
            foreach (var field in fields)
            {
                //the field is a field class, the fieldValue is the value of this field (the actual object)
                //for examle field is "int num = 5" and the field value is the 5
                if (field.IsNotSerialized) //PROTECTION
                    continue; //Don't touch the object, was not meant to be serialized

                object fieldValue = field.GetValue(obj);

                if (fieldValue == null) //No sending nulls
                    continue; //Null object, ignore. spares the text in the writing

                if (fieldValue.GetType() == typeof(object))
                    continue; //Means the object is literally 'new object();' an empty object

                if (fieldValue == obj)
                    throw new StackOverflowException("An object points to itself"); //Will cause an infinite loop, so just throw it

                if (Attribute.GetCustomAttribute(field, typeof(CompilerGeneratedAttribute)) == null) //this line checks for backing field
                    objGraph.Append(startVal + field.Name + endVal + equals + Deconstruction(fieldValue) + betweenEnum);
            }
            objGraph.Remove(objGraph.Length - betweenEnum.Length, betweenEnum.Length); //Remove the ',' at each class ending
            objGraph.Append(endClass);
            return objGraph.ToString();
        }

        /// <summary>
        /// deconstructs objects that inhert ISerializable
        /// </summary>
        /// <returns></returns>
        private static string SeriObjDeconstructor(ISerializable obj)
        {
            SerializationInfo info = new SerializationInfo(obj.GetType(), new FormatterConverter());
            StreamingContext context = new StreamingContext(StreamingContextStates.All);
            obj.GetObjectData(info, context);
            var node = info.GetEnumerator();
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(serializerEntry);

            ///Object Type Change
            stringBuilder.Append(info.ObjectType.AssemblyQualifiedName);
            stringBuilder.Append(equals);
            ///Object Type Change

            while (node.MoveNext())
            {
                stringBuilder.Append(startClass + node.Name + typeEntry + node.ObjectType + equals + Deconstruction(node.Value) + endClass);
                //stringBuilder.Append(startClass + node.Name + typeEntry + node.ObjectType + equals + Deconstruct(node.Value) + endClass);
            }
            stringBuilder.Append(serializerExit);
            return stringBuilder.ToString();
        }

        private static string ListObjDeconstructor(IList list)
        {
            StringBuilder objGraph = new StringBuilder(); //TODO: ADD A WAY TO COUNT MEMEBERS AND AVOID NULL sends
            objGraph.Append(startEnum);
            foreach (var member in list)
            {
                if (member == null) //Don't add to the list,
                {
                    //objGraph.Append(nullObj + betweenEnum); //No sending nulls
                    continue;
                }
                objGraph.Append(Deconstruction(member) + betweenEnum); //between Enum is like endclass
            }
            objGraph.Remove(objGraph.Length - betweenEnum.Length, betweenEnum.Length); //Remove the ',' at each class ending
            objGraph.Append(endEnum);
            return objGraph.ToString();
        }
    }
}
