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
        #region Global Fields
        private static string startClass = "[";
        private static string equals = "=";
        private static string endClass = "]";
        private static string startEnum = "<"; //start of array (enumer is sort of a collection like array and list, I like to call it array at time)
        private static string betweenEnum = ","; //spaces between items/members in the array
        private static string endEnum = ">"; //end of array
        private static string nullObj = string.Empty;

        private static BindingFlags bindingFlags = BindingFlags.Instance |
       BindingFlags.NonPublic |
       BindingFlags.Public; //specifies to get both public and non public fields and properties
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
                //the field is a field class, the fieldValue is the value of this vield
                //for examle field is "int num = 5" and the field value is the 5
                object fieldValue = field.GetValue(obj); 
                
                if (fieldValue == null)
                {
                    objGraph.Append(startClass + field.Name + equals + nullObj + endClass);
                    continue;
                }
                if (fieldValue == obj)
                    throw new StackOverflowException("An object points to itself");
                if (fieldValue.GetType().GetInterfaces().Contains(typeof(IEnumerable)) && !(fieldValue is string)) //string is a special type of enum (list of chars)
                {
                    objGraph.Append(startClass + field.Name + equals + startEnum);
                    foreach (var member in fieldValue as IEnumerable)
                    {
                        objGraph.Append(Deconstruction(member) + betweenEnum);
                    }
                    objGraph.Remove(objGraph.Length - 1, 1); //remove the last "," (TEST IT)
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
            name.Remove(0, 1); //cuts the '<' at the start
            name.Remove(backingField.Length - 17, 16); //cuts the '>k__BackingField' at the end
            return name.ToString();
        }

        private static string MemberDeconstructor(MemberInfo member)
        {
            return "[" + member.Name + "=" + "]"; //Deconstruction(member.GetValue(obj)) + "]";
        }

        public static object Construct(string objData)
        {
            objData = objData.Remove(0, 1); //remove the '[' at the start
            string strType = "";
            foreach (var letter in objData)
            {
                if (letter == equals[0]) //when the program gets to "=" it continues
                    break;
                strType += letter;
                objData = objData.Remove(0,1); //remove the letters since they won't be used later
            }
            objData = objData.Remove(0, 1); //remove the '=' at the end
            //Console.WriteLine(strType);
            Type typeOfObj = Type.GetType(strType);
            //var instance = Activator.CreateInstance(typeOfObj); //creates an instance of object with it's constructor
            var instance = FormatterServices.GetUninitializedObject(typeOfObj); //creates an instance of object without calling it's constructor (default deserialization)
            Construction(ref instance, ref objData);
            return instance;

            return instance;

            var bindingFlags = BindingFlags.Instance |
       BindingFlags.NonPublic |
       BindingFlags.Public; //specifies to get both public and non public fields and properties
            var fields = objData.GetType().GetFields(bindingFlags);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="mainType"></param>
        /// <returns></returns>
        private static void Construction(ref object obj, ref string objData)
        {
            if (obj.GetType().IsPrimitive || obj is string)
            {
                obj = GetValue(obj.GetType(), ref objData);
                return;
            }
            if (obj.GetType().IsArray)
            {
                obj = GetValue(obj.GetType(), ref objData);
                return;
            }
            while (objData[0] == startClass[0])
            {
                MemberInfo fieldInfo = GetField(obj, ref objData); //detect the field inside obj
                Console.WriteLine(fieldInfo + " YOO");
                Console.WriteLine(GetMemberType(fieldInfo));
                object instance = FormatterServices.GetUninitializedObject(GetMemberType(fieldInfo)); //if the field isn't primitive, make a new instance of it
                Construction(ref instance, ref objData); //proceed to construct the new field instance
                SetValue(fieldInfo, obj, instance); //set the new field instance to the obj
            }
            objData = objData.Remove(0, 1); //remove the ']' at the end of the class
            return;

            //throw new NotImplementedException(); //SHOULD NEVER REACH HERE

            #region SpecialCases
            /*Special cases so far:
             * 1. Object is null (Done)
             * 2. Object points to itself in an infnite loop (Done)
             * 3. backingField (Done)
             * 4. Object is an enum/list/array (Done)
            */
            #endregion
        }

        private static object GetValue(Type objType, ref string dataInfo)
        {
            //dataInfo = dataInfo.Remove(0, 1); //remove the '=' at the start
            string valueStr = "";
            foreach (var letter in dataInfo)
            {
                if (letter == startEnum[0])
                {
                    dataInfo = dataInfo.Remove(0, 1); //remove the start "<"
                    object instance = FormatterServices.GetUninitializedObject(objType);
                    foreach (var item in (IEnumerable)instance)
                    {
                        var field = item;
                        Construction(ref field, ref dataInfo);
                        dataInfo = dataInfo.Remove(0, 1); //remove the "," between items
                    }
                    dataInfo = dataInfo.Remove(0, 1); //remove the start ">"
                    return instance;
                }
                    //throw new NotImplementedException(); //HANDLE ENUMS

                if (letter == endClass[0]) //when the program gets to "]" it continues
                    break;
                valueStr += letter;
                dataInfo = dataInfo.Remove(0, 1); //remove the letters since they won't be used later
            }
            dataInfo = dataInfo.Remove(0, 1); //remove the ']' at the end

            valueStr = valueStr == nullObj ? null : valueStr; //if the value is null, set it to null
            //WARNING UNTESTED, CONVERTING NULL MIGHT THROW AN ERROR, TEST SO
            return Convert.ChangeType(valueStr, objType); //converts the string into T (T stands for one of the many primitive types)
        }

        /// <summary>
        /// This function was made to shorten my code, fieldinfo and property info both inherit from MemberInfo
        /// but to set the said field/property you must cast it back to field or property (depends on it's original type)
        /// </summary>
        /// <param name="objType">the info of the field/propery</param>
        /// <param name="obj">Mother object of the member info</param>
        /// <param name="instance">The value of the assigned member info</param>
        private static void SetValue(MemberInfo objType, object obj, object instance)
        {
            if (objType is FieldInfo)
                ((FieldInfo)objType).SetValue(obj, instance);
            else
                ((PropertyInfo)objType).SetValue(obj, instance);
        }

        private static Type GetMemberType(MemberInfo objType)
        {
            if (objType is FieldInfo)
                return ((FieldInfo)objType).FieldType;
            else
                return  ((PropertyInfo)objType).PropertyType;
        }

        private static MemberInfo GetField(object obj, ref string dataInfo)
        {
            dataInfo = dataInfo.Remove(0, 1); //remove the '[' at the start
            string fieldName = "";
            foreach (var letter in dataInfo)
            {
                if (letter == equals[0]) //when the program gets to "=" it continues
                    break;
                fieldName += letter;
                dataInfo = dataInfo.Remove(0, 1); //remove the letters since they won't be used later
            }
            dataInfo = dataInfo.Remove(0, 1); //remove the '=' at the end

            return obj.GetType().GetMember(fieldName, bindingFlags)[0];
            //methods are also members
            //getMember returns array since one method could have multiple signatures
            //in our case we don't care since we look for a field/property which can only have one signature
        }
    }
}
