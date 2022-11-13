using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace TgenSerializer
{
    public static class BinaryConstructor
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

        public static object Construct(byte[] objData)
        {
            var constructor = new ConstructionGraph(0, objData);
            Type typeOfObj = constructor.GraphType();

            if (!typeOfObj.IsSerializable) //PROTECTION
                throw new MarshalException(MarshalError.NonSerializable, $"{typeOfObj} isn't a serializable type");

            object result = constructor.Start(typeOfObj);
            //object result = Construction(typeOfObj, ref objData, ref startingPoint); //starting point
            return result;
        }

        private struct ConstructionGraph
        {
            int Location { get; set; }
            byte[] Graph { get; set; }
            public ConstructionGraph(int location, byte[] graph)
            {
                Location = location;
                Graph = graph;
            }
            public Type GraphType()
            {
                Location += startClass.Length;
                string strType = GetSection(equals);
                return Type.GetType(strType, true);
            }
            public object Start(Type objType)
            {
                return Construct(objType);
            }

            private object Construct(Type objType)
            {
                if (objType.IsPrimitive) //Primitive values
                    return GetValue(objType);

                if (objType == typeof(string))
                    return GetString();


                if (typeof(ISerializable).IsAssignableFrom(objType))
                {
                    Bytes valueStr = GetSection(endClass);
                    var reader = new DataReader(valueStr);
                    var obj = ((ISerializable)Activator.CreateInstance(objType));
                    obj.Deserialize(reader);
                    return obj;
                }

                //if (objType.IsValueType) //obj is struct
                //{
                //    
                //}

                if (typeof(IEnumerable).IsAssignableFrom(objType)) //arrays/enumerators(sorta lists)/lists
                {
                    if (objType.IsArray)
                        return ArrObjConstructor(objType);
                    return ListObjConstructor(objType);
                }

                object instance = FormatterServices.GetUninitializedObject(objType);
                while (CheckHitOperator(startClass, Location))
                {
                    FieldInfo fieldInfo = GetField(instance); //detect the field inside obj

                    //if the field isn't primitive, make a new instance of it
                    //NOTE: strings must be initialized
                    Type typeOfInstance = GetMemberType(fieldInfo);

                    if (fieldInfo.IsNotSerialized)
                        continue;

                    if (!typeOfInstance.IsSerializable) //PROTECTION
                        throw new MarshalException(MarshalError.NonSerializable, $"{typeOfInstance} isn't a serializable type");

                    var obj = Construct(typeOfInstance);
                    fieldInfo.SetValue(instance, obj); //Will always be a field, at some cases a backingField
                                                       //SetValue(fieldInfo, instance, obj); //set the new field instance to the obj
                }
                Location += endClass.Length;
                return instance;
            }

            private object GetValue(Type objType)
            {
                //Only applied on primitive types
                int size = Marshal.SizeOf(objType);
                object value = Bytes.ByteToPrimitive(objType, Graph, Location);
                Location += size;
                Location += endClass.Length;
                return value;
            }
            private string GetString()
            {
                int length = Bytes.B2P<int>(Graph, Location);
                Location += sizeof(int);

                if (!CheckHitOperator(endClass, Location + length))
                    throw new SerializationException("size for string is mismatched");
                byte[] strData = new byte[length];
                Buffer.BlockCopy(Graph, Location, strData, 0, length);
                Location += length;
                Location += endClass.Length;
                return Bytes.BytesToStr(strData);
            }

            private object ArrObjConstructor(Type objType)
            {
                //int length = int.Parse(GetSection(ref dataInfo, startEnum, ref location)); //removes the start "<" and gets the array's length
                int length = Bytes.B2P<int>(Graph, Location);
                Location += sizeof(int);
                Location += startEnum.Length;

                if (objType.IsArray && objType.Equals(typeof(byte[])))
                {
                    if (!CheckHitOperator(endClass, Location + length))
                        throw new SerializationException("size for array is mismatched");
                    byte[] byteArr = new byte[length];

                    Buffer.BlockCopy(Graph, Location, byteArr, 0, length);

                    Location += byteArr.Length;
                    Location += endEnum.Length;
                    Location += endClass.Length;
                    return byteArr;
                }

                var instance = (IList)Activator.CreateInstance(objType, new object[] { length });
                Type typeOfInstance = objType.GetElementType(); //Gets the type this array contrains, like the 'object' part of object[]
                for (int i = 0; i < length; i++)
                {
                    object item = Construct(typeOfInstance);
                    instance[i] = item; //Can't use add!
                }
                Location += endEnum.Length;
                Location += endClass.Length;
                return instance;
            }

            private object ListObjConstructor(Type objType)
            {
                IList instance = (IList)Activator.CreateInstance(objType);
                Type typeOfInstance = objType.GetGenericArguments()[0];
                Location += startEnum.Length;
                while (!CheckHitOperator(endEnum, Location))
                {
                    object item = Construct(typeOfInstance);
                    instance.Add(item);
                }
                Location += endEnum.Length;
                Location += endClass.Length;
                return instance;
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
                    return ((FieldInfo)objType).IsNotSerialized ? throw new MarshalException(MarshalError.NonSerializable, "Member isn't serilizable") : ((FieldInfo)objType).FieldType;
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
            private FieldInfo GetField(object obj)
            {
                Location += startClass.Length;
                string fieldName = GetSection(equals);
                Type objType = obj.GetType();
                return GetFieldInfosIncludingBaseClasses(objType, fieldName);
                //methods are also members
                //getMember returns array since one method could have multiple signatures
                //in our case we don't care since we look for a field/property which can only have one signature
            }

            public static FieldInfo GetFieldInfosIncludingBaseClasses(Type type, string name)
            {
                // If this class doesn't have a base, don't waste any time
                if (type.BaseType == typeof(object))
                    return GetField(type, name);
                else
                {   // Otherwise, collect all types up to the furthest base class
                    var currentType = type;
                    FieldInfo field = null;
                    while (field == null && currentType != typeof(object))
                    {
                        field = GetField(currentType, name);
                        currentType = currentType.BaseType;
                    }
                    return field;
                }
            }

            //Get fields and backingFields
            public static FieldInfo GetField(Type type, string name) =>
                type.GetField(name, bindingFlags) ?? type.GetField($"<{name}>k__BackingField", bindingFlags);

            /// <summary>
            /// This method will get the dataInfo and rescue the required section
            /// from the beginning of the dataInfo string until it encounters the given operation
            /// </summary>
            /// <param name="dataInfo">The operation</param>
            /// <param name="syntax">Object graph</param>
            /// <returns>The required section</returns>
            private Bytes GetSection(byte[] syntax)
            {
                int size;
                for (size = 0; !CheckHitOperator(syntax, Location); size++, Location++) ;

                byte[] section = new byte[size];
                Buffer.BlockCopy(Graph, Location - size, section, 0, size);
                Location += syntax.Length;

                return section;
            }

            /// <summary>
            /// Checks if the next part of the data in the string is an operator (operator can be more than one letter)
            /// </summary>
            /// <param name="data"></param>
            /// <param name="strOperator"></param>
            /// <returns></returns>
            private bool CheckHitOperator(byte[] byteOperator, int location)
            {
                try
                {
                    for (int i = 0; i < byteOperator.Length; i++)
                        if (Graph[location + i] != byteOperator[i])
                            return false;
                    return true;
                }
                catch (StackOverflowException)
                {
                    throw new MarshalException(MarshalError.SyntaxError);
                }
            }
        }
    }
}
