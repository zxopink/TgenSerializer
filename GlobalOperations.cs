using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TgenSerializer
{
    public static class GlobalOperations
    {
        //These fields are shared both by the constructor and constructor
        //NOTE: BetweenEnum and EndClass must have the same lenght since the serlizer treats them as the end of a class
        #region Global Fields
        public const string startClass = "[";
        public const string equals = "==";
        public const string endClass = "]";
        public const string startEnum = "<"; //start of array (enumer is sort of a collection like array and list, I like to call it array at time)
        public const string betweenEnum = ","; //spaces between items/members in the array
        public const string endEnum = ">"; //end of array
        public const string serializerEntry = "!"; //INCOMPLETE
        public const string serializerExit = "~"; //INCOMPLETE
        public const string typeEntry = "/"; //INCOMPLETE
        public const string nullObj = "";

        public const BindingFlags bindingFlags = BindingFlags.Instance |
       BindingFlags.NonPublic |
       BindingFlags.Public; //specifies to get both public and non public fields and properties
        #endregion
    }
}
