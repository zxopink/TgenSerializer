using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TgenSerializer
{
    public static class Constructor
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

        public static object Construct(string objData)
        {
            objData = objData.Remove(0, startClass.Length); //remove the '[' at the start
            string strType = "";
            foreach (var letter in objData)
            {
                if (CheckHitOperator(objData, equals)) //when the program gets to "=" it continues
                    break;
                strType += letter;
                objData = objData.Remove(0, 1); //remove the letters since they won't be used later
            }
            objData = objData.Remove(0, equals.Length); //remove the '=' at the end
            //Console.WriteLine(strType);
            Type typeOfObj = Type.GetType(strType);
            //var instance = Activator.CreateInstance(typeOfObj); //creates an instance of object with it's constructor
            var instance = FormatterServices.GetUninitializedObject(typeOfObj); //creates an instance of object without calling it's constructor (default deserialization)
            Construction(ref instance, ref objData);
            return instance;
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

            if (obj is IEnumerable)
            {
                obj = GetValue(obj.GetType(), ref objData);
                return;
            }
            while (CheckHitOperator(objData, startClass))
            {
                MemberInfo fieldInfo = GetField(obj, ref objData); //detect the field inside obj

                //if the field isn't primitive, make a new instance of it
                //NOTE: strings must be initialized
                Type typeOfInstance = GetMemberType(fieldInfo);
                //I have no clue how to initialize a string object, this is my way to do it
                object instance = typeOfInstance != typeof(string) ? FormatterServices.GetUninitializedObject(typeOfInstance) : string.Empty;
                Construction(ref instance, ref objData); //proceed to construct the new field instance
                SetValue(fieldInfo, obj, instance); //set the new field instance to the obj
            }
            if (CheckHitOperator(objData, betweenEnum))
                objData = objData.Remove(0, betweenEnum.Length);
            else
                objData = objData.Remove(0, endClass.Length); //remove the ']' at the end of the class
            return;

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
            string valueStr = "";
            foreach (var letter in dataInfo)
            {
                if (CheckHitOperator(dataInfo, startEnum))
                {
                    dataInfo = dataInfo.Remove(0, startEnum.Length); //remove the start "<"
                    IList instance = (IList)Activator.CreateInstance(objType);
                    while (!CheckHitOperator(dataInfo, endEnum))
                    {
                        //objType is a list of the real type
                        //makes a new object by the type of the given list
                        object item = FormatterServices.GetUninitializedObject(objType.GetGenericArguments()[0]);
                        Construction(ref item, ref dataInfo);
                        instance.Add(item);
                        //dataInfo = dataInfo.Remove(0, 1); //remove the "," between items
                    }
                    dataInfo = dataInfo.Remove(0, endEnum.Length); //remove the end ">"
                    dataInfo = dataInfo.Remove(0, endClass.Length); //remove the end class "]"
                    return instance;
                }

                if (CheckHitOperator(dataInfo, endClass)) //when the program gets to "]" it continues
                    break;
                valueStr += letter;
                dataInfo = dataInfo.Remove(0, 1); //remove the letters since they won't be used later
            }
            dataInfo = dataInfo.Remove(0, endClass.Length); //remove the ']' at the end
            Console.WriteLine(valueStr + "   " + nullObj + " YO " + objType);
            if (valueStr == nullObj)
                Console.WriteLine("NULL" + Convert.ChangeType(null, objType) + objType);
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

        /// <summary>
        /// checks if the given MemberInfo is a field or a property
        /// (MemberInfo can only be a field, property and a method by COM)
        /// since a serilizer doesn't check for methods, we only check if the MemberInfo is a field or a property
        /// </summary>
        /// <param name="objType"></param>
        /// <returns></returns>
        private static Type GetMemberType(MemberInfo objType)
        {
            if (objType is FieldInfo)
                return ((FieldInfo)objType).FieldType;
            else
                return ((PropertyInfo)objType).PropertyType;
        }

        /// <summary>
        /// Gets a string which includes the name of the field/property
        /// and returns the field/property itself
        /// </summary>
        /// <param name="obj">Class of the field</param>
        /// <param name="dataInfo">The list of strings</param>
        /// <returns>Field inside the obj type</returns>
        private static MemberInfo GetField(object obj, ref string dataInfo)
        {
            dataInfo = dataInfo.Remove(0, startClass.Length); //remove the '[' at the start
            string fieldName = "";
            foreach (var letter in dataInfo)
            {
                if (CheckHitOperator(dataInfo, equals)) //when the program gets to "=" it continues
                    break;
                fieldName += letter;
                dataInfo = dataInfo.Remove(0, 1); //remove the letters since they won't be used later
            }
            dataInfo = dataInfo.Remove(0, equals.Length); //remove the '=' at the end
            return obj.GetType().GetMember(fieldName, bindingFlags)[0];
            //methods are also members
            //getMember returns array since one method could have multiple signatures
            //in our case we don't care since we look for a field/property which can only have one signature
        }

        /// <summary>
        /// Checks if the next part of the data in the string is an operator (operator can be more than one letter)
        /// </summary>
        /// <param name="data"></param>
        /// <param name="strOperator"></param>
        /// <returns></returns>
        private static bool CheckHitOperator(string data, string strOperator)
        {
            for (int i = 0; i < strOperator.Length; i++)
                if (data[i] != strOperator[i])
                    return false;

            Console.WriteLine(strOperator + "  " + data.Substring(0, strOperator.Length));
            return true;
        }
    }
}
