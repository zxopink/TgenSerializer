using System;
using System.Reflection;
using System.Text;

namespace TgenSerializer
{
    /// Reasons I decided to NOT use microsoft's binary formatter: (Thanks to Marc Gravell)
    /// https://github.com/protobuf-net/protobuf-net.Grpc/blob/main/tests/protobuf-net.Grpc.Test.Integration/CustomMarshaller.cs
    /// 
    /// -Binary formatter isn't safe, it can run any given type tossed by the client / server with given parameters
    /// such cases could cause an RCE (Remote Code Execution) exploits, it's enough to send a payload with the type System.Diagnostics.Process with 'shutdown' command
    /// as one of it's constructor parameters to run a command line process
    /// !-this formatter only serializes and deserializes [serializable] classes, and only works with 'simple' (not RefObj/Marshel inheritance) data objects
    /// Plus, this formatter doesn't mess with Marshel types, it's meant for data types so a type such as System.Diagnostics.Process can't be exploited
    /// 
    /// -Another dangrous case is to pass a type with a field that's being reached on disposal.
    /// the object will throw an exception due to failed cast, waiting to be collected by the GC (garbage collector) since there are no refrences pointing at it
    /// but once the GC calls it's dispose function the dispose function will reach the malicious field and potentially cause an RCE exploit.
    /// so system constructors aren't the only thing to fear here
    /// !- this formatter is careful with what it constructs and deconstructs
    /// 
    /// -Binary formatter takes much more space specifying types for each payload which is expensive at fast phased networking environments
    /// !-this formatter assumes both sides are using the same type (The same binary file, DLL) && (if not it throws an exception) and fill gaps thanks to a clever recursion management

    public static class GlobalOperations
    {   
        //These fields are shared both by the Constructor and Deconstructor
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

    public static class JsonGlobalOperations
    {
        //These fields are shared both by the Constructor and Deconstructor
        #region Global Fields
        public const string startClass = "{";
        public const string equals = ":";
        public const string endClass = "}";
        public const string startVal = "\u0022"; //Stands for "
        public const string endVal = "\u0022"; //Stands for "
        public const string startEnum = "["; //start of array (enumer is sort of a collection like array and list, I like to call it array at time)
        public const string betweenEnum = ","; //spaces between items/members in the array
        public const string endEnum = "]"; //end of array
        public const string nullObj = "null";

        public const BindingFlags bindingFlags = BindingFlags.Instance |
       BindingFlags.NonPublic |
       BindingFlags.Public; //specifies to get both public and non public fields and properties
        #endregion
    }

    public static class BinaryGlobalOperations
    {
        //These fields are shared both by the Constructor and Deconstructor
        //NOTE: BetweenEnum and EndClass must have the same lenght since the serlizer treats them as the end of a class
        #region Global Binary Fields
        public static byte[] startClass = Encoding.UTF8.GetBytes("[[[");
        public static byte[] equals = Encoding.UTF8.GetBytes("===");
        public static byte[] endClass = Encoding.UTF8.GetBytes("]]]");
        public static byte[] startEnum = Encoding.UTF8.GetBytes("<<<"); //start of array (enumer is sort of a collection like array and list, I like to call it array at time)
        [Obsolete]
        public static byte[] betweenEnum = Encoding.UTF8.GetBytes(","); //spaces between items/members in the array (Obsolete)
        public static byte[] endEnum = Encoding.UTF8.GetBytes(">>>"); //end of array
        public static byte[] serializerEntry = Encoding.UTF8.GetBytes("!!!");
        public static byte[] serializerExit = Encoding.UTF8.GetBytes("~~~");
        public static byte[] typeEntry = Encoding.UTF8.GetBytes("///");
        [Obsolete]
        public static byte[] nullObj = Encoding.UTF8.GetBytes("");
        #endregion
    }
}
