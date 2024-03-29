﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;

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
        private const string serializerEntry = GlobalOperations.serializerEntry; //start of serializer object
        private const string serializerExit = GlobalOperations.serializerExit; //end of serializer object
        private const string typeEntry = GlobalOperations.typeEntry; //divides the name and type of an object
        private const string nullObj = GlobalOperations.nullObj; //sign for a nullObj (deprecated)

        private static BindingFlags bindingFlags = GlobalOperations.bindingFlags; //specifies to get both public and non public fields and properties
        #endregion

        public static object Construct(string objData)
        {
            int startingPoint = 0;
            startingPoint += startClass.Length;
            string strType = GetSection(ref objData, equals, ref startingPoint);
            Type typeOfObj = Type.GetType(strType, true);

            if (!typeOfObj.IsSerializable) //PROTECTION
                throw new SerializationException($"{typeOfObj} isn't a serializable type");

            return Construction(typeOfObj, ref objData, ref startingPoint); //starting point
        }

        /// <summary>
        /// In this method each class takes care of itself
        /// not a weird mix of confusing actions
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="mainType"></param>
        /// <returns></returns>
        private static object Construction(Type objType, ref string objData, ref int location)
        {
            if (objType.IsPrimitive || objType == typeof(string)) //primitive values
            {
                return GetValue(objType, ref objData, ref location);
            }

            if (typeof(ISerializable).IsAssignableFrom(objType))
            {
                Bytes valueStr = GetSection(ref objData, endClass, ref location);
                var reader = new DataReader(valueStr);
                var obj = ((ISerializable)Activator.CreateInstance(objType));
                obj.Deserialize(reader);
                return obj;
            }

            if (typeof(IEnumerable).IsAssignableFrom(objType)) //arrays/enumerators(sorta lists)/lists
            {
                if (objType.IsArray)
                    return ArrObjConstructor(objType, ref objData, ref location);
                return ListObjConstructor(objType, ref objData, ref location);
            }

            object instance = FormatterServices.GetUninitializedObject(objType);
            while (CheckHitOperator(objData, startClass, ref location))
            {
                MemberInfo fieldInfo = GetField(instance, ref objData, ref location); //detect the field inside obj

                //if the field isn't primitive, make a new instance of it
                //NOTE: strings must be initialized
                Type typeOfInstance = GetMemberType(fieldInfo);

                if (!typeOfInstance.IsSerializable) //PROTECTION
                    throw new SerializationException($"{typeOfInstance} isn't a serializable type");

                var obj = Construction(typeOfInstance, ref objData, ref location);
                SetValue(fieldInfo, instance, obj); //set the new field instance to the obj
            }
            location += endClass.Length;
            return instance;

            #region SpecialCases
            /*Special cases so far:
             * 1. Object is null (Done)
             * 2. Object points to itself in an infnite loop (Done)
             * 3. backingField (Done)
             * 4. Object is an enum/list/array (Done)
            */
            #endregion
        }

        private static object GetValue(Type objType, ref string dataInfo, ref int location)
        {
            string valueStr = GetSection(ref dataInfo, endClass, ref location);
            valueStr = valueStr == nullObj ? null : valueStr; //if the value is null, set it to null
            return Convert.ChangeType(valueStr, objType); //converts the string into T (T stands for one of the many primitive types)
        }

        private static object ArrObjConstructor(Type objType, ref string dataInfo, ref int location)
        {
            int length = int.Parse(GetSection(ref dataInfo, startEnum, ref location)); //removes the start "<" and gets the array's length
            var instance = (IList)Activator.CreateInstance(objType, new object[] { length });
            Type typeOfInstance = objType.GetElementType(); //Gets the type this array contrains, like the 'object' part of object[]
            for (int i = 0; !CheckHitOperator(dataInfo, endEnum, ref location); i++)
            {
                object item = Construction(typeOfInstance, ref dataInfo, ref location);
                instance[i] = item; //Can't use add!
                //Console.WriteLine(i + " / " + length);
            }
            location += endEnum.Length;
            location += endClass.Length;
            return instance;
        }

        private static object ListObjConstructor(Type objType, ref string dataInfo, ref int location)
        {
            IList instance = (IList)Activator.CreateInstance(objType);
            Type typeOfInstance = objType.GetGenericArguments()[0];
            location += startEnum.Length;
            while (!CheckHitOperator(dataInfo, endEnum, ref location))
            {
                object item = Construction(typeOfInstance, ref dataInfo, ref location);
                instance.Add(item);
            }
            location += endEnum.Length;
            location += endClass.Length;
            return instance;
        }

        /// <summary>
        /// This function was made to shorten my code, fieldinfo and property info both inherit from MemberInfo
        /// but to set the said field/property you must cast it back to field or property (depends on it's original type)
        /// </summary>
        /// <param name="objType">the info of the field/propery</param>
        /// <param name="obj">The value of the assigned member info</param>
        /// <param name="instance">Mother object of the member info </param>
        private static void SetValue(MemberInfo objType, object instance, object obj)
        {
            if (objType is FieldInfo)
                ((FieldInfo)objType).SetValue(instance, obj);
            else
                ((PropertyInfo)objType).GetSetMethod()?.Invoke(instance, new object[1] { obj }); //SetValue could be used but better not to
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
                return ((FieldInfo)objType).IsNotSerialized ? throw new SerializationException("Member isn't serilizable") : ((FieldInfo)objType).FieldType;
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
        private static MemberInfo GetField(object obj, ref string dataInfo, ref int location)
        {
            location += startClass.Length;
            string fieldName = GetSection(ref dataInfo, equals, ref location);
            return obj.GetType().GetMember(fieldName, bindingFlags)[0];
            //methods are also members
            //getMember returns array since one method could have multiple signatures
            //in our case we don't care since we look for a field/property which can only have one signature
        }


        /// <summary>
        /// This method will get the dataInfo and rescue the required section
        /// from the beginning of the dataInfo string until it encounters the given operation
        /// </summary>
        /// <param name="dataInfo">The operation</param>
        /// <param name="syntax">Object graph</param>
        /// <returns>The required section</returns>
        private static string GetSection(ref string dataInfo, string syntax, ref int location)
        {
            string sectionStr = "";
            for (int i = location; i < dataInfo.Length; i++)
            {
                if (CheckHitOperator(dataInfo, syntax, ref location)) //when the program gets to "=" it continues
                    break;
                sectionStr += dataInfo[i];
                location += 1;
            }
            location += syntax.Length;
            return sectionStr;
        }


        /// <summary>
        /// Gets the data of the serialized object
        /// and returns it's name and type
        /// </summary>
        /// <param name="dataInfo">Object data</param>
        /// <returns>Name and type of the serialized object</returns>
        private static KeyValuePair<string, Type> GetSerialiedName(ref string dataInfo, ref int location)
        {
            location += startClass.Length;
            string objName = GetSection(ref dataInfo, typeEntry, ref location);
            string objType = GetSection(ref dataInfo, equals, ref location);
            return new KeyValuePair<string, Type>(objName, Type.GetType(objType));
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
        private static bool CheckHitOperator(string data, string strOperator,ref int location)
        {
            //Could use string.substring instead
            for (int i = 0; i < strOperator.Length; i++)
                if (data[location + i] != strOperator[i])
                    return false;
            return true;
        }
    }
}
