using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TgenSerializer
{
    class Serializer
    {
        public static string Deconstruct(object obj)
        {
            StringBuilder objGraph = new StringBuilder();
            return ("[" + obj.GetType() + "=" + Deconstruction(obj) + "]");
            //must delcare the type at first so the constructor later on knows with what type it deals
            //the properties and fields can be aligned later on by using the first type, like a puzzle
            //the name of the object doesn't matter (therefore doesn't need to be saved) as well since the it will be changed anyways
        }

        private static string Deconstruction(object obj)
        {
            if (obj.GetType().IsPrimitive || obj is string)
                return obj.ToString();
            var bindingFlags = BindingFlags.Instance |
                   BindingFlags.NonPublic |
                   BindingFlags.Public; //specifies to get both public and non public fields and properties
            var properties = obj.GetType().GetProperties(bindingFlags);
            var fields = obj.GetType().GetFields(bindingFlags);
            StringBuilder objGraph = new StringBuilder();
            foreach (var property in properties)
                objGraph.Append("[" + property.Name + "=" + Deconstruction(property.GetValue(obj)) + "]");
            foreach (var field in fields)
            {
                if (Attribute.GetCustomAttribute(field, typeof(CompilerGeneratedAttribute)) == null)
                    Console.WriteLine("BACKING FIELD");
                    objGraph.Append("[" + field.Name + "=" + Deconstruction(field.GetValue(obj)) + "]");
            }
            return objGraph.ToString();

        }
    }
}
