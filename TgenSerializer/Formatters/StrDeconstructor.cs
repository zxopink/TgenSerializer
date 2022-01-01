using System;
using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;

namespace TgenSerializer
{
    public static class Deconstructor
    {
        //These fields are shared both by the constructor and constructor
        //NOTE: BetweenEnum and EndClass must have the same lenght since the serlizer treats them as the end of a class
        #region Global Fields
        private const string startClass = GlobalOperations.startClass; //sign for the start of a class
        private const string equals = GlobalOperations.equals; //sign for equals 
        private const string endClass = GlobalOperations.endClass; //sign for the end of a class
        private const string startEnum = GlobalOperations.startEnum; //start of array (enumer is sort of a collection like array and list, I like to call it array at time)
        private const string betweenEnum = GlobalOperations.betweenEnum; //spaces between items/members in the array
        private const string endEnum = GlobalOperations.endEnum; //end of array
        private const string serializerEntry = GlobalOperations.serializerEntry; //start of serializer object
        private const string serializerExit = GlobalOperations.serializerExit; //end of serializer object
        private const string typeEntry = GlobalOperations.typeEntry; //divides the name and type of an object
        private const string nullObj = GlobalOperations.nullObj; //sign for a nullObj (deprecated)

        private static BindingFlags bindingFlags = GlobalOperations.bindingFlags; //specifies to get both public and non public fields and properties
        #endregion

        public static string Deconstruct(object obj)
        {
            StringBuilder objGraph = new StringBuilder();
            string result = (startClass + obj.GetType().AssemblyQualifiedName + equals + Deconstruction(obj) + endClass);
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

            if (obj.GetType().IsPrimitive || obj is string)
                return obj.ToString();

            if (obj is ISerializable)
            {
                var writer = new DataWriter();
                ((ISerializable)obj).Serialize(writer);
                return writer.data;
            }

            if (!obj.GetType().IsSerializable) //PROTECTION
                return string.Empty; //don't touch the field, CONSIDER: throwing an error

            if (obj is IList) //string is also an enum but will never reach here thanks to the primitive check
            {
                return ListObjDeconstructor((IList)obj);
            }

            var fields = obj.GetType().GetFields(bindingFlags);
            StringBuilder objGraph = new StringBuilder();
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
                    continue; //Don't touch the object, was no meant to serialized

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
            return objGraph.ToString();
        }

        private static string GetNameOfBackingField(string backingField)
        {
            //backing field follows by the pattern: "<'name'>k__BackingField"
            StringBuilder name = new StringBuilder();
            name.Append(backingField);
            name.Remove(0, 1); //cuts the field's '<' at the start (NOT AN ENUM!)
            name.Remove(backingField.Length - 17, 16); //cuts the '>k__BackingField' at the end
            return name.ToString();
        }

        private static string ListObjDeconstructor(IList list)
        {
            StringBuilder objGraph = new StringBuilder(); //TODO: ADD A WAY TO COUNT MEMEBERS AND AVOID NULL sends
            if (list.GetType().IsArray)
            {
                //if (list is byte[])
                    //return Encoding.UTF8.GetString((byte[])list); //these two lines are good but implemented to the binary formatter
                objGraph.Append(list.Count);
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
            return objGraph.ToString();
        }
    }
}
