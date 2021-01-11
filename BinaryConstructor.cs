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
    public static class BinaryConstructor
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

        public static object Construct(byte[] objData)
        {
            int startingPoint = 0;
            startingPoint += startClass.Length;
            string strType = GetSection(ref objData, equals, ref startingPoint);
            Type typeOfObj = Type.GetType(strType, true);

            if (!typeOfObj.IsSerializable) //PROTECTION
                throw new SerializationException("The given object isn't serializable");
            object result = Construction(typeOfObj, ref objData, ref startingPoint); //starting point
            return result;
        }

        /// <summary>
        /// In this method each class takes care of itself
        /// not a weird mix of confusing actions
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="mainType"></param>
        /// <returns></returns>
        private static object Construction(Type objType, ref byte[] objData, ref int location)
        {
            if (objType.IsPrimitive || objType == typeof(string)) //Primitive values
            {
                return GetValue(objType, ref objData, ref location);
            }

            if (typeof(ISerializable).IsAssignableFrom(objType)) //Serializable objects
            {
                return SeriObjConstructor(objType, ref objData, ref location);
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
                    throw new SerializationException("The given object isn't serializable");

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

        private static object GetValue(Type objType, ref byte[] dataInfo, ref int location)
        {
            BinaryBuilder valueStr = GetSection(ref dataInfo, endClass, ref location);
            valueStr = valueStr == nullObj ? null : valueStr; //if the value is null, set it to null
            return BinaryBuilder.ByteToPrimitive(objType, valueStr);
        }

        private static object ArrObjConstructor(Type objType, ref byte[] dataInfo, ref int location)
        {
            int length = int.Parse(GetSection(ref dataInfo, startEnum, ref location)); //removes the start "<" and gets the array's length
            //ON THIN ICE
            if (objType.IsArray && objType.Equals(typeof(byte[])))
            {
                byte[] byteArr = dataInfo.Skip(location).Take(length).ToArray();
                location += byteArr.Length;
                location += endEnum.Length;
                location += endClass.Length;
                return byteArr;
            }

            var instance = (IList)Activator.CreateInstance(objType, new object[] { length });
            Type typeOfInstance = objType.GetElementType(); //Gets the type this array contrains, like the 'object' part of object[]
            for (int i = 0; !CheckHitOperator(dataInfo, endEnum, ref location); i++)
            {
                object item = Construction(typeOfInstance, ref dataInfo, ref location);
                instance[i] = item; //Can't use add!
            }
            location += endEnum.Length;
            location += endClass.Length;
            return instance;
        }

        private static object ListObjConstructor(Type objType, ref byte[] dataInfo, ref int location)
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

        private static object SeriObjConstructor(Type objType, ref byte[] dataInfo, ref int location)
        {
            SerializationInfo info = new SerializationInfo(objType, new FormatterConverter());
            StreamingContext context = new StreamingContext(StreamingContextStates.All);
            location += serializerEntry.Length;

            ///Object Type Change
            string possibleType = GetSection(ref dataInfo, equals, ref location);
            objType = Type.GetType(possibleType, true);
            ///Object Type Change

            while (!CheckHitOperator(dataInfo, serializerExit, ref location))
            {
                KeyValuePair<string, Type> serializedObj = GetSerialiedName(ref dataInfo, ref location); //<name, type>
                info.AddValue(serializedObj.Key, Construction(serializedObj.Value, ref dataInfo, ref location));
            }
            location += serializerExit.Length;
            location += endClass.Length;

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
        private static MemberInfo GetField(object obj, ref byte[] dataInfo, ref int location)
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
        private static BinaryBuilder GetSection(ref byte[] dataInfo, byte[] syntax, ref int location)
        {
            BinaryBuilder sectionByte = new BinaryBuilder();
            for (int i = location; i < dataInfo.Length; i++)
            {
                if (CheckHitOperator(dataInfo, syntax, ref location)) //when the program gets to "=" it continues
                    break;
                sectionByte.Append(dataInfo[i]);
                location += 1;
            }
            location += syntax.Length;
            return sectionByte;
        }


        /// <summary>
        /// Gets the data of the serialized object
        /// and returns it's name and type
        /// </summary>
        /// <param name="dataInfo">Object data</param>
        /// <returns>Name and type of the serialized object</returns>
        private static KeyValuePair<string, Type> GetSerialiedName(ref byte[] dataInfo, ref int location)
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
        private static bool CheckHitOperator(byte[] data, byte[] byteOperator, ref int location)
        {
            //Could use string.substring instead
            //Console.WriteLine(new ByteBuilder(byteOperator).ToString());
            //Console.WriteLine(data.Length + " /  " + location);
            //Console.WriteLine(new ByteBuilder(data.ToList().GetRange(location, data.Length - location).ToArray()).ToString());
            for (int i = 0; i < byteOperator.Length; i++)
                if (data[location + i] != byteOperator[i])
                    return false;
            return true;
        }
    }
}
