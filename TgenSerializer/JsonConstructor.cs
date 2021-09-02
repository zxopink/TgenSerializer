using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;

namespace TgenSerializer
{
    public static class JsonConstructor
    {
        //These fields are shared both by the constructor and constructor
        //NOTE: BetweenEnum and EndClass must have the same lenght since the serlizer treats them as the end of a class
        #region Global Fields
        private const string startClass = JsonGlobalOperations.startClass; //sign for the start of a class
        private const string equals = JsonGlobalOperations.equals; //sign for equals 
        private const string endClass = JsonGlobalOperations.endClass; //sign for the end of a class
        private const string startVal = JsonGlobalOperations.startVal; //sign for the start of a value
        private const string endVal = JsonGlobalOperations.endVal; //sign for the end of a value
        private const string startEnum = JsonGlobalOperations.startEnum; //start of array (enumer is sort of a collection like array and list, I like to call it array at time)
        private const string betweenEnum = JsonGlobalOperations.betweenEnum; //spaces between items/members in the array
        private const string endEnum = JsonGlobalOperations.endEnum; //end of array
        private const string nullObj = JsonGlobalOperations.nullObj; //sign for a nullObj (deprecated)

        private static BindingFlags bindingFlags = JsonGlobalOperations.bindingFlags; //specifies to get both public and non public fields and properties
        #endregion

        public static JsonElement Construct(string objData)
        {
            objData = Regex.Replace(objData, @"\t|\n|\r", ""); //Remove newlines, tabs and such

            int startingPoint = 0;
            object content = Construction(ref objData, ref startingPoint);
            JsonElement element = new JsonElement("Object", content);

            return element; //starting point
        }

        /// <summary>
        /// In this method each class takes care of itself
        /// not a weird mix of confusing actions
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="mainType"></param>
        /// <returns></returns>
        private static object Construction(ref string objData, ref int location)
        {
            string currentOperator = GetNextOperator(ref objData, ref location); //Might mess up with spaces, check that out
            JumpOperator(ref objData, currentOperator, ref location);

            if (IsValue(currentOperator)) //primitive values
            {
                /*In JSON, values must be one of the following data types:

                    a string: {"name":"John"}
                    a number: {"age":30}
                    an object (JSON object): { "employee":{"name":"John", "age":30, "city":"New York"} }
                    an array: { "employees":["John", "Anna", "Peter"] }
                    a boolean: {"sale":true}
                    null: {"middlename":null}
                */
                return GetValue(ref objData, currentOperator, ref location);
            }

            if (currentOperator == startEnum) //arrays/enumerators(sorta lists)/lists
                return ListObjConstructor(ref objData, ref location);

            List<JsonElement> elements = new List<JsonElement>();
            while(currentOperator == startClass || currentOperator == betweenEnum)
            {
                JumpOperator(ref objData, startVal, ref location);
                string objName = GetSection(ref objData, endVal, ref location);
                JumpOperator(ref objData, equals, ref location);

                var objContent = Construction(ref objData, ref location);
                JsonElement element = new JsonElement(objName, objContent);
                elements.Add(element);

                currentOperator = GetNextOperator(ref objData, ref location);
            }
            JumpOperator(ref objData, currentOperator, ref location);
            return elements;

            throw new Exception("Json constructor failure, could not match operator");

            #region SpecialCases
            /*Special cases so far:
             * 1. Object is null (Done)
             * 2. Object points to itself in an infnite loop (Done)
             * 3. backingField (Done)
             * 4. Object is an enum/list/array (Done)
            */
            #endregion
        }

        private static bool IsValue(string key)
        {
            return (key == startVal || //string value
                int.TryParse(key, out _) || key == "-" || key == "." || //number value, or minus (-5)
                key.Equals("t", StringComparison.CurrentCultureIgnoreCase) || //true value
                key.Equals("f", StringComparison.CurrentCultureIgnoreCase) || //false value
                key.Equals("n", StringComparison.CurrentCultureIgnoreCase)); //null value
        }

        private static object GetValue(ref string objData, string key, ref int location)
        {
            if (key == startVal) //string value
            {
                string value = GetSection(ref objData, endVal, ref location);
                return value;
            }
            else if (int.TryParse(key, out _) || key == "-" || key == ".") //-5 or .8
            {
                string num = key;
                while (char.IsDigit(objData[location]) || objData[location] == '.') //5.3
                {
                    num += objData[location];
                    location++;
                }
                return double.Parse(num);
            }
            else if (key.Equals("t", StringComparison.CurrentCultureIgnoreCase))
            {
                location += "rue".Length;
                return true;
            }
            else if (key.Equals("f", StringComparison.CurrentCultureIgnoreCase))
            {
                location += "alse".Length;
                return false;
            }
            else //(key == "n")
            {
                location += "ull".Length;
                return null;
            }


        }

        private static object ListObjConstructor(ref string dataInfo, ref int location)
        {
            //Constructed objects can be lists,values and objects, therefore we use a list of objects to include them all and not JsonElement
            IList instance = new List<object>(); 
            while (!CheckHitOperator(ref dataInfo, endEnum, ref location))
            {
                object item = Construction(ref dataInfo, ref location);
                instance.Add(item);

                if (GetNextOperator(ref dataInfo, ref location) == betweenEnum)
                    JumpOperator(ref dataInfo, betweenEnum, ref location);
            }
            JumpOperator(ref dataInfo, endEnum, ref location);
            return instance;
        }


        /// <summary>
        /// This method will get the dataInfo and rescue the required section
        /// from the beginning of the dataInfo string until it encounters the given operation
        /// </summary>
        /// <param name="dataInfo">The operation</param>
        /// <param name="syntax">Object graph</param>
        /// <returns>The required section</returns>
        private static string GetSection(ref string dataInfo, string syntax, ref int location)
        {
            string sectionStr = "";
            for (int i = location; i < dataInfo.Length; i++)
            {
                if (CheckHitOperator(ref dataInfo, syntax, ref location)) //when the program gets to "=" it continues
                    break;
                sectionStr += dataInfo[i];
                location += 1;
            }
            location += syntax.Length;
            return sectionStr;
        }

        private static string GetNextOperator(ref string data, ref int location)
        {
            for (int i = 0; i < data.Length; i++)
                if (data[location + i] != ' ' )
                    return data[location + i].ToString();
            throw new Exception("Could not find the next operator");
        }

        private static void JumpOperator(ref string data, string strOperator, ref int location)
        {
            GetToOperator(ref data, strOperator, ref location);
            location += strOperator.Length;
        }

        /// <summary>
        /// Jump the location in the string until we get to the desired operator
        /// </summary>
        /// <param name="data"></param>
        /// <param name="strOperator"></param>
        /// <param name="location"></param>
        private static void GetToOperator(ref string data, string strOperator, ref int location)
        {
            while (!CheckHitOperator(ref data, strOperator, ref location))
                location++;
        }

        /// <summary>
        /// Checks if the next part of the data in the string is an operator (operator can be more than one letter)
        /// </summary>
        /// <param name="data"></param>
        /// <param name="strOperator"></param>
        /// <returns></returns>
        private static bool CheckHitOperator(ref string data, string strOperator,ref int location)
        {
            //Could use string.substring instead
            for (int i = 0; i < strOperator.Length; i++)
                if (data[location + i] != strOperator[i])
                    return false;
            return true;
        }
    }
}
