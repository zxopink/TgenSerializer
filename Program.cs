using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TgenSerializer
{
    [Serializable]
    public class TestClass
    {
        public static int num3;
        int numProp2 { get; set; }
        int numProp { get { return num1; } set { num1 = value; } }
        int num1;
        int num2;
        List<Btype> list1;
        Object failure;
        List<List<List<Btype>>> listFromTheUnspeakableHell;
        TestClass thisone;
        public string str;
        Btype btyper;

        //public TestClass()
        //{ }

        public TestClass(int num1, int num2, string str) { this.num1 = num1; this.num2 = num2; this.str = str; btyper = new Btype(5);
            list1 = new List<Btype>();
            list1.Add(new Btype(7));
            list1.Add(new Btype(9));
            num3++;
            failure = new object();
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
            StringBuilder objGraph = new StringBuilder();
            TestClass test = new TestClass(5, 2, "ay");
            //Console.WriteLine(TestClass.num3);
            var bindingFlags = BindingFlags.Instance |
                   BindingFlags.NonPublic |
                   BindingFlags.Public;
            var properties = test.GetType().GetProperties(bindingFlags);
            var fields = test.GetType().GetFields(bindingFlags);

            /*
            BinaryFormatter formatter = new BinaryFormatter();
            string filePath = "C:/Users/yoavh/OneDrive/Desktop/C# Seril/seri.txt";
            if (File.Exists(filePath))
                File.Delete(filePath);
            FileStream strm = new FileStream(filePath, FileMode.CreateNew);
            formatter.Serialize(strm, test);
            strm.Position = 0;
            TestClass deseri = (TestClass)formatter.Deserialize(strm);
            Console.WriteLine(TestClass.num3);
            //FileStream a = File.Exists(filePath) ? new FileStream(filePath, FileMode.CreateNew) : null; //this line was made for fun
            */

            DType constructTest = new DType(5, 8);
            //Console.WriteLine(properties.Length);
            //Console.WriteLine(fields.Length);
            //Console.WriteLine(Deconstructor.Deconstruct(test));
            string myObj = Deconstructor.Deconstruct(test);
            Console.WriteLine(myObj);
            TestClass constructed = (TestClass)Constructor.Construct(myObj);
            Console.WriteLine(constructed.str);
            Console.WriteLine(Deconstructor.Deconstruct(test));
            //Console.WriteLine(constructed.cType.a + " AND " + constructed.b);
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
