using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace TgenSerializer
{
    public class JsonElement
    {
        public readonly string Name;
        private object content;

        /// <summary>
        /// A list(array)
        /// </summary>
        public readonly bool IsList;
        /// <summary>
        /// A non primitive object
        /// </summary>
        public readonly bool IsObject;
        /// <summary>
        /// A primitive value
        /// </summary>
        public readonly bool IsValue;

        /// <summary>
        /// Returns 0 if not a list
        /// </summary>
        public int Count { get {
                if (!IsList)
                    return 0;

                return ((List<object>)content).Count;
            } }

        public JsonElement this[string key]
        {
            get => GetElement(key);
            set => SetElement(key, value);
        }

        public JsonElement this[int key]
        {
            get => GetIndex(key);
            set => SetIndex(key, value);
        }

        private JsonElement GetElement(string key)
        {
            if (!IsObject)
                throw new Exception("Element is not an object, can't accept element names");

            List<JsonElement> elements = (List<JsonElement>)content;
            foreach (var element in elements)
            {
                if (element.Name == key)
                    return element;
            }
            return null;
            //throw new Exception("Element does not exist");
        }

        private void SetElement(string key, JsonElement value)
        {
            if (!IsObject)
                throw new Exception("Element is not an object, can't accept element names");

            List<JsonElement> elements = (List<JsonElement>)content;
            for (int i = 0; i < elements.Count; i++)
            {
                if (elements[i].Name == key)
                    elements[i].content = value.content;
            }
            elements.Add(new JsonElement(key, value.content)); //Element doesn't exist, add it
        }

        private JsonElement GetIndex(int key)
        {
            if (!IsList)
                throw new Exception("Element is not a list, can't accept integer index");

            List<object> elements = (List<object>)content;
            return new JsonElement(key.ToString(), elements[key]);
        }

        private void SetIndex(int key, JsonElement value)
        {
            if (!IsList)
                throw new Exception("Element is not a list, can't accept integer index");

            List<object> elements = (List<object>)content;
            elements[key] = value.content;
        }

        public List<JsonElement> GetElements()
        { 
            if(!IsObject)
                throw new Exception("Element is not an object, doesn't contain elements");

            return (List<JsonElement>)content;
        }

        public JsonElement(string name, object obj)
        {
            this.Name = name;
            this.content = obj;

            Type contentType = obj?.GetType();
            IsList = contentType == typeof(List<object>);
            IsObject = contentType == typeof(List<JsonElement>);
            IsValue = contentType == typeof(string) || contentType.IsValueType;
        }

        public override string ToString() => content.ToString();

        public T Parse<T>() => (T)Parse(typeof(T));
        public object Parse(Type type)
        {
            object result = null;

            if(IsValue) //For simple primitive values
                result = Convert.ChangeType(content, type);

            else if(IsObject) //For non primitive values
                result = ConstructType(type, this);

            else if (IsList) //Runtime list casting, not ideal but works :)
            {
                IList instance = (IList)Activator.CreateInstance(type);
                Type typeOfInstance = type.GetGenericArguments()[0];

                for (int i = 0; i < this.Count; i++)
                    instance.Add(ConstructType(typeOfInstance, this[i]));

                result = instance;
            }
            return result; //Take care of custom types
        }

        private object ConstructType(Type objType, JsonElement content)
        {
            object obj = FormatterServices.GetUninitializedObject(objType);
            var fields = objType.GetFields(JsonGlobalOperations.bindingFlags);
            foreach (var field in fields)
            {
                if (Attribute.GetCustomAttribute(field, typeof(CompilerGeneratedAttribute)) != null) //this line checks for backing field
                    continue;

                object value = content[field.Name]?.Parse(field.FieldType);
                field.SetValue(obj, value);
            }
            return obj;
        }

        /// <summary>
        /// Prints out the elements
        /// </summary>
        /// <returns></returns>
        public string Stringify()
        {
            string value = $"{Name}: ";
            if (IsValue)
            {
                if (content is string)
                    value += $"\"{ToString()}\"";
                else
                    value += $"{ToString()}";
            }
            if (IsObject)
            {
                value += "{\n\t";
                List<JsonElement> list = (List<JsonElement>)content;
                for (int i = 0; i < list.Count; i++)
                {
                    value += $"{list[i].Stringify()}";

                    if(i != list.Count - 1)
                        value +=",\n\t";
                }
                value += "\n\t}";
            }

            if (IsList)
            {
                value += "[\n\t";
                for (int i = 0; i < Count; i++)
                {
                    value += $"{this[i].Stringify()}";

                    if (i != Count - 1)
                        value += ",\n\t";
                }
                value += "\n\t]";
            }

            return value;
        }

        public static implicit operator sbyte(JsonElement obj) => obj.Parse<sbyte>();
        public static implicit operator byte(JsonElement obj) => obj.Parse<byte>();
        public static implicit operator short(JsonElement obj) => obj.Parse<short>();
        public static implicit operator int(JsonElement obj) => obj.Parse<int>();
        public static implicit operator double(JsonElement obj) => obj.Parse<double>();
        public static implicit operator float(JsonElement obj) => obj.Parse<float>();
        public static implicit operator bool(JsonElement obj) => obj.Parse<bool>();
        public static implicit operator string(JsonElement str) => str.Parse<string>();
        public static implicit operator long(JsonElement obj) => obj.Parse<long>();
        public static implicit operator ushort(JsonElement obj) => obj.Parse<ushort>();
        public static implicit operator uint(JsonElement obj) => obj.Parse<uint>();
        public static implicit operator ulong(JsonElement obj) => obj.Parse<ulong>();

        public static implicit operator JsonElement(sbyte obj) => new JsonElement(string.Empty, obj);
        public static implicit operator JsonElement(byte obj) => new JsonElement(string.Empty, obj);
        public static implicit operator JsonElement(short obj) => new JsonElement(string.Empty, obj);
        public static implicit operator JsonElement(int obj) => new JsonElement(string.Empty, obj);
        public static implicit operator JsonElement(double obj) => new JsonElement(string.Empty, obj);
        public static implicit operator JsonElement(float obj) => new JsonElement(string.Empty, obj);
        public static implicit operator JsonElement(bool obj) => new JsonElement(string.Empty, obj);
        public static implicit operator JsonElement(string str) => new JsonElement(string.Empty, str);
        public static implicit operator JsonElement(long obj) => new JsonElement(string.Empty, obj);
        public static implicit operator JsonElement(ushort obj) => new JsonElement(string.Empty, obj);
        public static implicit operator JsonElement(uint obj) => new JsonElement(string.Empty, obj);
        public static implicit operator JsonElement(ulong obj) => new JsonElement(string.Empty, obj);

    }
}
