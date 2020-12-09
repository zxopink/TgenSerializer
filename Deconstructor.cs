using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TgenSerializer
{
    static class Deconstructor
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
        private const string nullObj = GlobalOperations.nullObj; //sign for a nullObj (deprecated)

        private static BindingFlags bindingFlags = GlobalOperations.bindingFlags; //specifies to get both public and non public fields and properties
        #endregion

        public static string Deconstruct(object obj)
        {
            StringBuilder objGraph = new StringBuilder();
            return (startClass + obj.GetType() + equals + Deconstruction(obj) + endClass);
            //must delcare the type at first so the constructor later on knows with what type it deals
            //the properties and fields can be aligned later on by using the first type, like a puzzle
            //the name of the object doesn't matter (therefore doesn't need to be saved) as well since the it will be changed anyways
        }

        private static string Deconstruction(object obj)
        {
            if (!obj.GetType().IsSerializable) //PROTECTION
                return string.Empty; //don't touch the field, CONSIDER: throwing an error

            if (obj.GetType().IsPrimitive || obj is string)
                return obj.ToString();

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
                    continue; //doesn't touch the object

                object fieldValue = field.GetValue(obj);
                
                if (fieldValue == null)
                {
                    //This line can be removed to not include null refrences
                    //The constructor knows how to deal with it
                    //objGraph.Append(string.Empty);
                    //objGraph.Append(startClass + field.Name + equals + nullObj + endClass);
                    continue;
                }
                if (fieldValue == obj)
                    throw new StackOverflowException("An object points to itself");
                if (fieldValue.GetType().GetInterfaces().Contains(typeof(IEnumerable)) && !(fieldValue is string)) //string is a special type of enum (list of chars)
                {
                    objGraph.Append(startClass + field.Name + equals + startEnum);
                    foreach (var member in fieldValue as IEnumerable)
                    {
                        objGraph.Append(Deconstruction(member) + betweenEnum); //between Enum is like endclass
                    }
                    //objGraph.Remove(objGraph.Length - 1, 1); //remove the last "," (TEST IT)
                    objGraph.Append(endEnum + endClass);
                    continue; //if you don't use continue the enumer will procceed and print it's settings (lenght, item, size, version...)
                }

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
    }
}
