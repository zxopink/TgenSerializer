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
        private const string serializerEntry = GlobalOperations.serializerEntry; //start of serializer object
        private const string serializerExit = GlobalOperations.serializerExit; //end of serializer object
        private const string typeEntry = GlobalOperations.typeEntry; //divides the name and type of an object
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
            Type typeOfObj = Type.GetType(strType, true);

            if (!typeOfObj.IsSerializable) //PROTECTION
                throw new SerializationException("The given object isn't serializable");

            return Construction(typeOfObj, ref objData);

            //else if(typeOfObj is ISerializable)
            //{
            //    SeriObjConstructor(typeOfObj, ref objData);
            //}

            //var instance = Activator.CreateInstance(typeOfObj); //creates an instance of object with it's constructor
            //object instance = typeOfObj != typeof(string) ? FormatterServices.GetUninitializedObject(typeOfObj) : string.Empty; //creates an instance of object without calling it's constructor (default deserialization)
            //Can't make an Uninitialized object of type string
            //var instance = FormatterServices.GetUninitializedObject(typeOfObj); //creates an instance of object without calling it's constructor (default deserialization)
            //Construction(ref instance, ref objData);
            //return instance;
        }

        /// <summary>
        /// In this method each class takes care of itself
        /// not a weird mix of confusing actions
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="mainType"></param>
        /// <returns></returns>
        private static object Construction(Type objType, ref string objData)
        {
            //if (CheckHitOperator(objData, nullObj)) //hit null obj
                //return null;

            if (objType.IsPrimitive || objType == typeof(string)) //primitive values
            {
                return GetValue(objType, ref objData);
            }

            if (typeof(ISerializable).IsAssignableFrom(objType))
            {
                return SeriObjConstructor(objType, ref objData);
            }

            if (typeof(IEnumerable).IsAssignableFrom(objType)) //arrays/enumerators(sorta lists)/lists
            {
                if (objType.IsArray)
                    return ArrObjConstructor(objType, ref objData);
                return ListObjConstructor(objType, ref objData);
            }

            object instance = FormatterServices.GetUninitializedObject(objType);
            while (CheckHitOperator(objData, startClass))
            {
                MemberInfo fieldInfo = GetField(instance, ref objData); //detect the field inside obj

                //if the field isn't primitive, make a new instance of it
                //NOTE: strings must be initialized
                Type typeOfInstance = GetMemberType(fieldInfo);

                if (!typeOfInstance.IsSerializable) //PROTECTION
                    throw new SerializationException("The given object isn't serializable");

                var obj = Construction(typeOfInstance, ref objData);
                SetValue(fieldInfo, instance, obj); //set the new field instance to the obj
            }
            if (CheckHitOperator(objData, betweenEnum))
                objData = objData.Remove(0, betweenEnum.Length);
            else
                objData = objData.Remove(0, endClass.Length); //remove the ']' at the end of the class
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

        private static object GetValue(Type objType, ref string dataInfo)
        {
            string valueStr = "";
            foreach (var letter in dataInfo)
            {
                if (CheckHitOperator(dataInfo, endClass) || CheckHitOperator(dataInfo, betweenEnum)) //when the program gets to "]" or "," (happens with lists that have primitve types) it continues
                    break;
                valueStr += letter;
                dataInfo = dataInfo.Remove(0, 1); //remove the letters since they won't be used later
            }
            dataInfo = dataInfo.Remove(0, endClass.Length); //remove the ']' at the end
            valueStr = valueStr == nullObj ? null : valueStr; //if the value is null, set it to null
            //WARNING UNTESTED, CONVERTING NULL MIGHT THROW AN ERROR, TEST SO
            return Convert.ChangeType(valueStr, objType); //converts the string into T (T stands for one of the many primitive types)
        }

        private static object ArrObjConstructor(Type objType, ref string dataInfo)
        {
            int length = int.Parse(GetSection(ref dataInfo, startEnum)); //removes the start "<" and gets the array's length
            var instance = (IList)Activator.CreateInstance(objType, new object[] { length });
            Type typeOfInstance = objType.GetElementType(); //Gets the type this array contrains, like the 'object' part of object[]
            for (int i = 0; !CheckHitOperator(dataInfo, endEnum); i++)
            {
                object item = Construction(typeOfInstance, ref dataInfo);
                instance[i] = item; //Can't use add!
                Console.WriteLine(i + " / " + length);
            }
            dataInfo = dataInfo.Remove(0, endEnum.Length); //remove the end ">"
            dataInfo = dataInfo.Remove(0, endClass.Length); //remove the end class "]"
            return instance;
        }

        private static object ListObjConstructor(Type objType, ref string dataInfo)
        {
            IList instance = (IList)Activator.CreateInstance(objType);
            Type typeOfInstance = objType.GetGenericArguments()[0];
            dataInfo = dataInfo.Remove(0, startEnum.Length); //remove the start "<"
            while (!CheckHitOperator(dataInfo, endEnum))
            {
                //Type typeOfInstance = objType.GetGenericArguments()[0];
                object item = Construction(typeOfInstance, ref dataInfo);
                instance.Add(item);
                //dataInfo = dataInfo.Remove(0, 1); //remove the "," between items //CONSIDER USING THAT
            }
            dataInfo = dataInfo.Remove(0, endEnum.Length); //remove the end ">"
            dataInfo = dataInfo.Remove(0, endClass.Length); //remove the end class "]"
            return instance;
        }

        private static object SeriObjConstructor(Type objType, ref string dataInfo)
        {
            SerializationInfo info = new SerializationInfo(objType, new FormatterConverter());
            StreamingContext context = new StreamingContext(StreamingContextStates.All);
            dataInfo = dataInfo.Remove(0, serializerEntry.Length); //remove the start "!"
            while (!CheckHitOperator(dataInfo, serializerExit))
            {
                KeyValuePair<string, Type> serializedObj = GetSerialiedName(ref dataInfo); //<name, type>
                info.AddValue(serializedObj.Key, Construction(serializedObj.Value, ref dataInfo));
            }
            dataInfo = dataInfo.Remove(0, serializerExit.Length); //remove the end "~"
            dataInfo = dataInfo.Remove(0, endClass.Length); //remove the end class "]"
            var instance = Activator.CreateInstance(objType, info, context); //constructor of a serializable object looks like: `public MyItemType(SerializationInfo info, StreamingContext context)`
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
                ((PropertyInfo)objType).SetValue(instance, obj);
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
            //return ((FieldInfo)objType).FieldType;
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
        /// This method will get the dataInfo and rescue the required section
        /// from the beginning of the dataInfo string until it encounters the given operation
        /// </summary>
        /// <param name="dataInfo">The operation</param>
        /// <param name="syntax">Object graph</param>
        /// <returns>The required section</returns>
        private static string GetSection(ref string dataInfo, string syntax)
        {
            string sectionStr = "";
            foreach (var letter in dataInfo)
            {
                if (CheckHitOperator(dataInfo, syntax)) //when the program gets to "=" it continues
                    break;
                sectionStr += letter;
                dataInfo = dataInfo.Remove(0, 1); //remove the letters since they won't be used later
            }
            dataInfo = dataInfo.Remove(0, syntax.Length);
            return sectionStr;
        }


        /// <summary>
        /// Gets the data of the serialized object
        /// and returns it's name and type
        /// </summary>
        /// <param name="dataInfo">Object data</param>
        /// <returns>Name and type of the serialized object</returns>
        private static KeyValuePair<string, Type> GetSerialiedName(ref string dataInfo)
        {
            dataInfo = dataInfo.Remove(0, startClass.Length); //remove the '[' at the start
            string objName = "";
            foreach (var letter in dataInfo)
            {
                if (CheckHitOperator(dataInfo, typeEntry)) //when the program gets to "/" it continues
                    break;
                objName += letter;
                dataInfo = dataInfo.Remove(0, 1); //remove the letters since they won't be used later
            }
            dataInfo = dataInfo.Remove(0, typeEntry.Length); //remove the '/' at the end

            string objType = "";
            foreach (var letter in dataInfo)
            {
                if (CheckHitOperator(dataInfo, equals)) //when the program gets to "=" it continues
                    break;
                objType += letter;
                dataInfo = dataInfo.Remove(0, 1); //remove the letters since they won't be used later
            }
            dataInfo = dataInfo.Remove(0, equals.Length); //remove the '=' at the end

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
        private static bool CheckHitOperator(string data, string strOperator)
        {
            //Could use string.substring instead
            for (int i = 0; i < strOperator.Length; i++)
                if (data[i] != strOperator[i])
                    return false;
            return true;
        }
    }
}
