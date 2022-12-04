using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace TgenSerializer
{
    internal static class BinaryConstructor
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
        
        public static object Construct(byte[] objData, IList<TgenConverter> converters)
        {
            try
            {
                var constructor = new ConstructionGraph(0, objData);
                Type typeOfObj = constructor.GraphType(converters);

                if (!typeOfObj.IsSerializable) //PROTECTION
                    throw new MarshalException(MarshalError.NonSerializable, $"{typeOfObj} isn't a serializable type");

                object result = constructor.Start(typeOfObj);
                //object result = Construction(typeOfObj, ref objData, ref startingPoint); //starting point
                return result;
            }
            catch (SerializationException)
            { throw; }
            catch (Exception e)
            { throw new MarshalException(e); }
        }

        private struct ConstructionGraph
        {
            int MaxSize { get; set; }
            int _location;
            int Location
            {
                get => _location;
                set 
                {
                    _location = value; //Add check if hit max value
                    if (Location > MaxSize)
                        throw new MarshalException(MarshalError.TooLarge, $"Tried to reach location {Location}");
                }

            }
            byte[] Graph { get; set; }
            public ConstructionGraph(int location, byte[] graph, int maxSize = int.MaxValue)
            {
                MaxSize = maxSize;
                _location = location;
                Graph = graph;
            }
            public Type GraphType()
            {
                string strType = GetSection(equals).ToString();
                return Type.GetType(strType, true);
            }
            //TODO
            public Type GraphType(IList<TgenConverter> converters)
            {
                Bytes section = GetSection(equals);
                if (section.Length == sizeof(uint)) //Must be an id
                {
                    uint id = section.Get<uint>();
                    Type t = converters?.FirstOrDefault(conv => conv.Id == id)?.Type;
                    if (t != null) //Had id
                        return t;
                }

                string strType = section.ToString();
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
                    return GetSerializable(objType);

                if (typeof(IList).IsAssignableFrom(objType)) //arrays/ILists
                {
                    if (objType.IsArray)
                        return ArrConstruction(objType);
                    return ListConstruction(objType);
                }

                object instance = FormatterServices.GetUninitializedObject(objType);
                while (CheckHitOperator(startClass, Location))
                {
                    Location += startClass.Length;
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
                    Location += endClass.Length;
                }
                return instance;
            }

            private object GetValue(Type objType)
            {
                //Only applied on primitive types
                int size = Marshal.SizeOf(objType);
                object value = Bytes.ByteToPrimitive(objType, Graph, Location);
                Location += size;
                return value;
            }

            private ISerializable GetSerializable(Type type)
            {
                int length = Bytes.B2P<int>(Graph, Location);
                Location += sizeof(int);

                if (!CheckHitOperator(endClass, Location + length))
                    throw new SerializationException($"size for ISerializeable ({type}) is mismatched");

                int startIndex = Location;
                Location += length; //Append location before allocation, it might be too big

                byte[] seriData = new byte[length];
                Buffer.BlockCopy(Graph, startIndex, seriData, 0, length);

                ISerializable obj = (ISerializable)Activator.CreateInstance(type);
                obj.Deserialize((Bytes)seriData);
                return obj;
            }

            private string GetString()
            {
                int length = Bytes.B2P<int>(Graph, Location);
                Location += sizeof(int);

                if (!CheckHitOperator(endClass, Location + length))
                    throw new SerializationException("size for string is mismatched");

                int startIndex = Location;
                Location += length; //Append location before allocation, it might be too big

                byte[] strData = new byte[length];
                Buffer.BlockCopy(Graph, startIndex, strData, 0, length);
                return Bytes.BytesToStr(strData);
            }

            private object ArrConstruction(Type objType) //Type is an array
            {
                Type elementType = objType.GetElementType();
                int length = Bytes.B2P<int>(Graph, Location);
                Location += sizeof(int);
                Location += startEnum.Length;

                if (elementType.IsPrimitive) //Primitive array constructions
                {
                    if (!CheckHitOperator(endEnum, Location + length))
                        throw new SerializationException("size for array is mismatched");

                    int startIndex = Location;
                    Location += length; //Append location before allocation, it might be too big

                    Array primArr = Array.CreateInstance(elementType, length / Marshal.SizeOf(elementType));
                    Buffer.BlockCopy(Graph, startIndex, primArr, 0, length);

                    Location += endEnum.Length;
                    return primArr;
                }

                //Managed array types constructions
                var instance = Array.CreateInstance(elementType, length);
                for (int i = 0; i < length; i++)
                {
                    object item = Construct(elementType);
                    instance.SetValue(item, i);
                    Location += endClass.Length;
                }
                Location += endEnum.Length;
                return instance;
            }

            private object ListConstruction(Type objType)
            {
                IList instance = (IList)Activator.CreateInstance(objType);
                Type typeOfInstance = objType.GetGenericArguments()[0];
                Location += startEnum.Length;
                while (!CheckHitOperator(endEnum, Location))
                {
                    object item = Construct(typeOfInstance);
                    instance.Add(item);
                    Location += endClass.Length;
                }
                Location += endEnum.Length;
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
                string fieldName = GetSection(equals).ToString();
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
                catch (SystemException ex) when (ex is IndexOutOfRangeException ||
                                                 ex is StackOverflowException)
                {
                    throw new MarshalException(MarshalError.SyntaxError, "Operator failiure", ex);
                }
            }
        }
    }
}
