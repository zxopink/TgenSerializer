using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;

namespace TgenSerializer
{

    [Serializable]
    public class MySeriClass : ISerializable
    {
        //public int[] vs;
        public List<int> sake;

        public MySeriClass()
        {
            //vs = new int[500];
            sake = new List<int> { 57, 4 };
        }

        public MySeriClass(SerializationInfo info, StreamingContext context)
        {
            //vs = (int[])info.GetValue("arr", typeof(int[]));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            //info.AddValue("arr", vs);
            info.AddValue("list", sake);
        }
    }

    [Serializable]
    public class TestClass
    {
        public enum MyEnum
        {
            yes,
            no,
            meh
        }

        public static int num3;
        int numProp2 { get; set; }
        int numProp { get { return num1; } set { num1 = value; } }
        int num1;
        int num2;
        MyEnum myEnum;
        List<Btype> list1;
        Object failure;
        List<List<List<Btype>>> listFromTheUnspeakableHell;
        //TestClass thisone;
        public string str;
        Btype btyper;

        //public TestClass()
        //{ }

        public TestClass(int num1, int num2, string str) { this.num1 = num1; this.num2 = num2; this.str = str; btyper = new Btype(5);
            myEnum = MyEnum.meh;
            list1 = new List<Btype>();
            list1.Add(new Btype(7));
            list1.Add(new Btype(9));
            num3++;
            //failure = new object();
            //listFromTheUnspeakableHell = new List<List<List<Btype>>>();
        }
    }
    [Serializable]
    public class Btype
    {
        int a;
        int b;
        public Btype(int a) { this.a = a; }
    }

    [Serializable]
    public class CType
    {
        public int a;
        public CType(int a) { this.a = a; }
    }

    [Serializable]
    public class DType
    {
        public int b;
        public CType cType;
        public DType(int a, int b) { cType = new CType(a); this.b = b; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(typeof(int).IsValueType);

            TestClass test = new TestClass(5, 2, "ay");
            string target = "C:/Users/Yoav/Desktop/PonyDetails.txt";
            string digestionOutput = "C:/Users/Yoav/Desktop/resultOutput.txt";


            FileStream stream1 = new FileStream(target, FileMode.Create,FileAccess.ReadWrite);
            Formatter.Serialize(stream1, test, CompressionFormat.Json);
            stream1.Close();


            FileStream stream2 = new FileStream(target, FileMode.Open, FileAccess.ReadWrite);
            var a = (JsonElement)Formatter.Deserialize(stream2, CompressionFormat.Json);
            Console.WriteLine(a["num1"].Parse<string>());
            stream2.Close();

            FileStream stream3 = new FileStream(digestionOutput, FileMode.Create, FileAccess.ReadWrite);
            Formatter.Serialize(stream3, a, CompressionFormat.Json);
            stream3.Close();
            Console.WriteLine(a.Stringify());
            Console.WriteLine(a.Diagnose());

            Console.ReadLine();
        }

        /*
        private static byte[] Bitmappo()
        {
            Rectangle bounds = Screen.GetBounds(Point.Empty);
            Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height);
            Graphics g = Graphics.FromImage(bitmap);
            g.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size);
            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Low;
            g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;

            return ToByteArray(bitmap, System.Drawing.Imaging.ImageFormat.Jpeg);
        }

        private static byte[] ToByteArray(Image image, System.Drawing.Imaging.ImageFormat format)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, format);
                return ms.ToArray();
            }
        }

        private static Bitmap FromBytes(byte[] bytes)
        {
            Bitmap bmp;
            using (var ms = new MemoryStream(bytes))
            {
                bmp = new Bitmap(ms);
            }
            return bmp;
        }
                */

        /// <summary>
        /// Prints Console operations
        /// </summary>
        public static class CenterLog
        {
            /// <summary>
            /// print to console (NO COLOR)
            /// </summary>
            /// <param name="text">the text</param>
            public static void Print(string text)
            {

            }

            /// <summary>
            /// prints to console (WITH COLOR)
            /// </summary>
            /// <param name="text">the text</param>
            /// <param name="color">the color</param>
            public static void Print(string text, ConsoleColor color)
            {

            }
        }

        private static Type anothermath(Assembly arg1, string arg2, bool arg3)
        {
            throw new NotImplementedException();
        }

        private static Assembly mymath(AssemblyName arg)
        {
            throw new NotImplementedException();
        }
        /*
public static string DeconstructorStarter(object obj)
{
   StringBuilder objGraph = new StringBuilder();
   return ("[" + obj.GetType() + "=" + Deconstructor(obj) + "]");
   //must delcare the type at first so the constructor later on knows with what type it deals
   //the properties and fields can be aligned later on by using the first type, like a puzzle
   //the name of the object doesn't matter (therefore doesn't need to be saved) as well since the it will be changed anyways
}

public static string Deconstructor(object obj)
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
       objGraph.Append("[" + property.Name + "=" + Deconstructor(property.GetValue(obj)) + "]");

   foreach (var field in fields)
       objGraph.Append("[" + field.Name + "=" + Deconstructor(field.GetValue(obj)) + "]");
   return objGraph.ToString();

}
*/
    }
}
