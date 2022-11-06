using System;
using System.Data;
using System.Xml;
using System.Collections;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

using System.Reflection;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;
using FluentAssertions;

namespace LoyaltyAutoTest.BusinessLogic.System
{
    public static class JuixExtensions
    {
        #region Extensions For JObject

        public static void ShouldBeNegative(this JObject Obj, string Path, string because = "", params object[] becauseArgs)
        {
            ShouldBeInNumberRange(Obj, Path, int.MinValue, 0, because, becauseArgs);
        }

        public static void ShouldBeInNumberRange(this JObject Obj, string Path, double MinimumNumber, double MaximumNumber, string because = "", params object[] becauseArgs)
        {
            XElement xPointer = GetElementFromJson(Obj, Path, because, becauseArgs);
            double Value = xPointer.Value.ToDouble();
            Value.Should().BeInRange(MinimumNumber, MaximumNumber, because, becauseArgs);

        }

        public static void ShouldExists(this JObject Obj, string Path, string because = "", params object[] becauseArgs)
        {
            ShouldHave(Obj, Path, null, because, becauseArgs);
        }

        public static void ShouldHave(this JObject Obj, string Path, object ExpectedValue, string because = "", params object[] becauseArgs)
        {
            XElement xPointer = GetElementFromJson(Obj, Path, because, becauseArgs);

            if (ExpectedValue != null)
            {

                string ExpectedValueFormatted;
                ExpectedValueFormatted = ExpectedValue.ToString();
                if (ExpectedValue is string) ExpectedValueFormatted = ExpectedValue.ToString();
                if (ExpectedValue is decimal) ExpectedValueFormatted = ((decimal)ExpectedValue).ToString("0.##");
                if (ExpectedValue is bool) ExpectedValueFormatted = ExpectedValue.ToString().ToLower();

                xPointer.Should().HaveValue(ExpectedValueFormatted.ToString(), because, becauseArgs);
            }


        }


        public static XElement GetElementFromJson(this JObject Obj, string Path)
        {
            return GetElementFromJson(Obj, Path, "");
        }

        public static XElement GetElementFromJson(this JObject Obj, string Path, string because, params object[] becauseArgs)
        {
            XDocument xRes = (XDocument)JsonConvert.DeserializeXNode(Obj.ToString(), "Document");
            XElement xRoot = xRes.Descendants("ResponseData").First();

            XElement xPointer = xRoot;

            foreach (string PathElement in Path.Split("/"))
            {
                xPointer.Should().HaveElement(PathElement, because, becauseArgs);
                xPointer = xPointer.Descendants(PathElement).First();
            }

            return xPointer;
        }


        /// <summary>
        /// This function convert json object to xDocument
        /// </summary>
        /// <param name="Obj">Json object</param>
        /// <returns></returns>
        public static XDocument ToXDocument(this JObject Obj)
        {
            XDocument xRes = (XDocument)JsonConvert.DeserializeXNode(Obj.ToString(), "Document");
            return xRes;
        }


        /// <summary>
        /// This function convert json object to xml
        /// </summary>
        /// <param name="Obj">Json object</param>
        /// <returns></returns>
        public static XmlDocument ToXml(this JObject Obj)
        {
            //var entireJson = JToken.Parse(Obj.ToString());
            XmlDocument doc = new XmlDocument();
            XmlElement root = doc.CreateElement("root");
            foreach (var property in Obj)
            {
                XmlAttribute attr = doc.CreateAttribute(property.Key);
                attr.Value = property.Value.ToString();
                root.Attributes.Append(attr);
            }
            doc.AppendChild(root);

            return doc;
        }

        /// <summary>
        /// This function convert xml  to json object
        /// </summary>
        /// <param name="Obj">Json object</param>
        /// <returns></returns>
        public static JObject ToJObject(this XmlDocument Xml)
        {
            string json = JsonConvert.SerializeObject(Xml);
            json = json.Replace("@", string.Empty);
            return JObject.Parse(json);
        }

        /// <summary>
        /// This function convert DataTable to list of object json
        /// </summary>
        /// <param name="Obj">Json object</param>
        /// <returns></returns>
        public static List<JObject> ConvertToJObjectList(this DataTable dataTable)
        {
            if (dataTable != null & dataTable.Rows.Count > 0)
            {
                var list = new List<JObject>();

                foreach (DataRow row in dataTable.Rows)
                {
                    JObject item = ConvertDataRowToJObject(row);

                    list.Add(item);
                }

                return list;
            }

            return null;
        }

        public static string ConvertToHTML(this DataTable dataTable)
        {
            return ConvertToHTML(dataTable, string.Empty, false);
        }
        public static string ConvertToHTML(this DataTable dataTable, string CSS, bool IsAddRowCounter)
        {
            string html = @"<style>" + CSS + "</style>";

            html += "<table border='1'>";
            //add header row
            html += "<thead><tr>";
            if (IsAddRowCounter)
            {
                html += "<th>#</th>";
            }
            for (int i = 0; i < dataTable.Columns.Count; i++)
                html += "<th>" + dataTable.Columns[i].ColumnName + "</th>";
            html += "</tr>";
            html += "</thead><tbody>";
            //add rows
            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                html += "<tr>";
                if (IsAddRowCounter)
                {
                    html += "<td>" + (i + 1) + "</td>";
                }
                for (int j = 0; j < dataTable.Columns.Count; j++)
                    html += "<td>" + dataTable.Rows[i][j].ToString() + "</td>";
                html += "</tr>";
            }
            html += "</tbody>";
            html += "</table>";
            return html;
        }


        /// <summary>
        /// This function convert DataTable to object json
        /// </summary>
        /// <param name="Obj">Json object</param>
        /// <returns></returns>
        public static JObject ConvertToJObject(this DataTable dataTable)
        {
            if (dataTable != null & dataTable.Rows.Count > 0)
            {
                DataRow row = dataTable.Rows[0];
                return ConvertDataRowToJObject(row);
            }

            return null;
        }

        /// <summary>
        /// This function convert datarow to object json
        /// </summary>
        /// <param name="row">datarow object</param>
        /// <returns></returns>
        public static JObject ConvertToJObject(this DataRow row)
        {
            return ConvertDataRowToJObject(row);
        }


        /// <summary>
        /// This function convert DataRow to object json
        /// </summary>
        /// <param name="Obj">Json object</param>
        /// <returns></returns>
        private static JObject ConvertDataRowToJObject(DataRow row)
        {
            var item = new JObject();

            foreach (DataColumn column in row.Table.Columns)
            {
                // if it is date object we convert to utc 
                if (column.DataType == Type.GetType("System.DateTime"))
                {
                    if (row[column.ColumnName] is null)
                    {
                        item.Add(column.ColumnName, "");
                    }
                    else
                    {
                        var obj = row[column.ColumnName].ToDateTime().ToUniversalTime();
                        item.Add(column.ColumnName, JToken.FromObject(obj));
                    }

                }
                else
                {
                    item.Add(column.ColumnName, JToken.FromObject(row[column.ColumnName]));
                }
            }

            return item;
        }

        public static int ToInt32(this JToken Obj)
        {
            string sValue = Obj.ToString();
            if (string.IsNullOrEmpty(sValue)) return 0;
            int iValue = Convert.ToInt32(sValue);
            return iValue;

        }

        public static bool TryParse<T>(this JObject jObject, out T result) where T : new()
        {
            try
            {
                result = JsonConvert.DeserializeObject<T>(jObject.ToString());
                return true;
            }
            catch
            {
                result = default(T);
                return false;
            }
        }
        #endregion

        #region Extensions For XmlElement

        #region GetAttribute
        public static int GetAttributeInt(this XmlElement xElement, string AttributeName)
        {
            string sValue = xElement.GetAttribute(AttributeName);
            if (string.IsNullOrEmpty(sValue)) return 0;

            return Convert.ToInt32(sValue);
        }
        public static T GetElementsByTagName<T>(this XmlElement xElement, string AttributeName)
        {
            var element = xElement.GetElementsByTagName(AttributeName)[0];
            if (element == null)
            {
                return default;
            }
            return (T)Convert.ChangeType(element.InnerText, typeof(T));

        }

        /// <summary>
        /// Returns null if the attribute is explicitly saved as null
        /// </summary>
        /// <param name="xElement"></param>
        /// <param name="AttributeName"></param>
        /// <returns></returns>
        public static string GetAttributeNullable(this XmlElement xElement, string AttributeName)
        {
            string sValue = xElement.GetAttribute(AttributeName);
            if (string.IsNullOrEmpty(sValue) || sValue == "null") return null;

            return sValue;
        }

        /// <summary>
        /// returns the value as INT32 if no value found will return NULL
        /// </summary>
        /// <param name="xNode"></param>
        /// <param name="AttributeName"></param>
        /// <returns></returns>
        public static int? GetAttributeNullableInt(this XmlElement xElement, string AttributeName)
        {
            string sValue = xElement.GetAttribute(AttributeName);
            if (string.IsNullOrEmpty(sValue)) return null;

            return Convert.ToInt32(sValue);
        }


        /// <summary>
        /// returns the value as double if no value found will return NULL
        /// </summary>
        /// <param name="xNode"></param>
        /// <param name="AttributeName"></param>
        /// <returns></returns>
        public static double? GetAttributeNullableDouble(this XmlElement xElement, string AttributeName)
        {
            string sValue = xElement.GetAttribute(AttributeName);
            if (string.IsNullOrEmpty(sValue)) return null;

            return Convert.ToDouble(sValue);
        }


        /// <summary>
        /// this method will convert the 1 to true and 0 to false
        /// also the value "True" to true
        /// exception/null will become false
        /// </summary>
        /// <param name="xElement"></param>
        /// <param name="AttributeName"></param>
        /// <returns></returns>
        public static bool GetAttributeBoolean(this XmlElement xElement, string AttributeName)
        {
            string sValue = xElement.GetAttribute(AttributeName).ToLower();
            if (sValue == "true") return true;
            if (sValue == "1") return true;
            return false;
        }

        public static double GetAttributeDouble(this XmlElement xElement, string AttributeName)
        {
            string sValue = xElement.GetAttribute(AttributeName);
            if (string.IsNullOrEmpty(sValue)) return 0;

            return Convert.ToDouble(sValue);
        }



        #endregion

        #region SetAttribute

        public static void SetAttribute(this XmlElement xElement, string name, string value, string prefix, string prefixURI)
        {
            XmlAttribute xAtt = xElement.OwnerDocument.CreateAttribute(prefix, name, prefixURI);
            xAtt.InnerText = value;
            xElement.Attributes.Append(xAtt);

        }

        public static void SetAttribute(this XmlElement xElement, string name, int? value)
        {
            if (value == null)
            {
                xElement.SetAttribute(name, "");
            }
            else
            {
                xElement.SetAttribute(name, value.Value);
            }

        }

        public static void SetAttribute(this XmlElement xElement, string name, double? value)
        {
            if (value == null)
            {
                xElement.SetAttribute(name, "");
            }
            else
            {
                xElement.SetAttribute(name, value.Value);
            }

        }


        public static void SetAttribute(this XmlElement xElement, string name, int value)
        {
            xElement.SetAttribute(name, value.ToString());

        }

        public static void SetAttribute(this XmlElement xElement, string name, double value)
        {
            xElement.SetAttribute(name, value.ToString());
        }

        /// <summary>
        /// will convert boolean "True" value to "1" and Boolean "False" value to "0"
        /// </summary>
        /// <param name="xElement"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public static void SetAttribute(this XmlElement xElement, string name, bool value)
        {
            xElement.SetAttribute(name, (value) ? "1" : "0");
        }


        public static void SetAttribute(this XmlElement xElement, string name, DateTime value)
        {
            xElement.SetAttribute(name, value.ToString());

        }

        public static void SetAttribute(this XmlElement xElement, string name, DateTime? value)
        {
            if (value == null)
            {
                xElement.SetAttribute(name, "");
            }
            else
            {
                xElement.SetAttribute(name, value.Value);
            }

        }


        /// <summary>
        /// this method will append a new child to the an ELEMENT (shortcut for AppendChild)
        /// </summary>
        /// <param name="xElement"></param>
        /// <param name="NodeName">the name of the new node </param>
        public static XmlElement AddNode(this XmlElement xElement, string NodeName)
        {
            XmlElement xNewNode = xElement.OwnerDocument.CreateElement(NodeName);
            xElement.AppendChild(xNewNode);
            return xNewNode;
        }

        /// <summary>
        /// this method will wrap text into CDATA injected to an element.
        /// </summary>
        /// <param name="xElement"></param>
        /// <param name="Text">the text to be wrapped in CDATA section</param>
        /// <param name="WrapNodeName">the name of the node to wrap the CDATA section</param>
        public static void WrapTextInCDATA(this XmlElement xElement, string Text, string WrapNodeName)
        {
            XmlElement xWrapNode = xElement.OwnerDocument.CreateElement(WrapNodeName);
            xWrapNode.AppendChild(xElement.OwnerDocument.CreateCDataSection(Text));
            xElement.AppendChild(xWrapNode);
        }





        /// <summary>
        /// this method will wrap text into an element.
        /// input : 
        /// text : "myText"
        /// WrapNodeName : "Node"
        /// result will be a new node with the name of "Node" contains text of "MyText"
        /// </summary>
        /// <param name="xElement"></param>
        /// <param name="Text">the text to be wrapped in CDATA section</param>
        /// <param name="WrapNodeName">the name of the node to wrap the CDATA section</param>
        public static void WrapTextInNode(this XmlElement xElement, string Text, string WrapNodeName)
        {
            XmlElement xWrapNode = xElement.OwnerDocument.CreateElement(WrapNodeName);
            xWrapNode.InnerText = Text;
            xElement.AppendChild(xWrapNode);
        }

        #endregion




        #endregion

        #region Extensions For XmlNode

        /// <summary>
        /// this method will cast xmlnode to XmlElement
        /// </summary>
        /// <param name="xNode"></param>
        /// <returns></returns>
        private static XmlElement GetXmlElement(XmlNode xNode)
        {
            if (xNode is XmlDocument)
            {
                return (xNode as XmlDocument).DocumentElement;
            }
            else
            {
                return xNode as XmlElement;
            }
        }

        #region GetAttribute
        //public static string GetAttribute(this XmlNode xNode, string AttributeName)
        //{
        //    XmlElement _xNode = GetXmlElement(xNode);

        //    return _xNode.GetAttribute(AttributeName);
        //}

        public static int GetAttributeInt(this XmlNode xNode, string AttributeName)
        {
            XmlElement _xNode = GetXmlElement(xNode);

            return _xNode.GetAttributeInt(AttributeName);
        }

        public static string GetAttribute(this XmlNode xNode, string AttributeName)
        {
            XmlElement _xNode = GetXmlElement(xNode);

            return _xNode.GetAttribute(AttributeName);
        }

        /// <summary>
        /// returns the value as INT32 if no value found will return NULL
        /// </summary>
        /// <param name="xNode"></param>
        /// <param name="AttributeName"></param>
        /// <returns></returns>
        public static int? GetAttributeNullableInt(this XmlNode xNode, string AttributeName)
        {
            XmlElement _xNode = GetXmlElement(xNode);

            return _xNode.GetAttributeNullableInt(AttributeName);
        }

        /// <summary>
        /// returns the value as double if no value found will return NULL
        /// </summary>
        /// <param name="xNode"></param>
        /// <param name="AttributeName"></param>
        /// <returns></returns>
        public static double? GetAttributeNullableDouble(this XmlNode xNode, string AttributeName)
        {
            XmlElement _xNode = GetXmlElement(xNode);

            return _xNode.GetAttributeNullableDouble(AttributeName);
        }


        public static double GetAttributeDouble(this XmlNode xNode, string AttributeName)
        {
            XmlElement _xNode = GetXmlElement(xNode);

            return _xNode.GetAttributeDouble(AttributeName);
        }

        /// <summary>
        /// this method will convert the 1 to true and 0 to false
        /// exception/null will become false
        /// </summary>
        /// <param name="xElement"></param>
        /// <param name="AttributeName"></param>
        /// <returns></returns>
        public static bool GetAttributeBoolean(this XmlNode xNode, string AttributeName)
        {
            XmlElement _xNode = GetXmlElement(xNode);

            return _xNode.GetAttributeBoolean(AttributeName);
        }


        #endregion

        #region SetAttribute
        public static void SetAttribute(this XmlNode xNode, string AttributeName, string Value)
        {
            XmlElement _xNode = GetXmlElement(xNode);

            _xNode.SetAttribute(AttributeName, Value);
        }

        public static void SetAttribute(this XmlNode xNode, string AttributeName, double Value)
        {
            XmlElement _xNode = GetXmlElement(xNode);

            _xNode.SetAttribute(AttributeName, Value);
        }

        public static void SetAttribute(this XmlNode xNode, string AttributeName, double? Value)
        {
            XmlElement _xNode = GetXmlElement(xNode);

            _xNode.SetAttribute(AttributeName, Value);
        }

        public static void SetAttribute(this XmlNode xNode, string AttributeName, int Value)
        {
            XmlElement _xNode = GetXmlElement(xNode);

            _xNode.SetAttribute(AttributeName, Value);
        }

        public static void SetAttribute(this XmlNode xNode, string AttributeName, int? Value)
        {
            XmlElement _xNode = GetXmlElement(xNode);

            _xNode.SetAttribute(AttributeName, Value);
        }

        public static void SetAttribute(this XmlNode xNode, string AttributeName, bool Value)
        {
            XmlElement _xNode = GetXmlElement(xNode);

            _xNode.SetAttribute(AttributeName, Value);
        }

        public static void SetAttribute(this XmlNode xNode, string AttributeName, DateTime Value)
        {
            XmlElement _xNode = GetXmlElement(xNode);

            _xNode.SetAttribute(AttributeName, Value);
        }


        #endregion




        #endregion

        #region Extensions For Object

        /// <summary>
        ///Extension for .net Object Class
        ///this method will convert the .ToString() value to boolean
        ///{true,1} will be True, {empty and all the rest} will be false
        /// </summary>
        /// <param name="Obj"></param>
        /// <returns></returns>
        public static bool ToBoolean(this Object Obj)
        {
            string sValue = Obj.ToString();
            if (string.IsNullOrEmpty(sValue)) return false;

            sValue = sValue.ToLower();
            if (sValue == "1" || sValue == "true") return true;

            return false;


        }


        /// <summary>
        ///Extension for .net Object Class
        ///this method will convert the .ToString() value to Int32
        /// </summary>
        /// <param name="Obj"></param>
        /// <returns></returns>
        public static int ToInt32(this Object Obj)
        {
            string sValue = Obj.ToString();
            if (string.IsNullOrEmpty(sValue)) return 0;
            int iValue = Convert.ToInt32(sValue);
            return iValue;

        }
        /// <summary>
        ///Extension for .net Object Class
        ///this method will convert the .ToString() value to Nullable Int32
        ///if the value is NULL it will be returns as NULL
        /// </summary>
        /// <param name="Obj"></param>
        /// <returns></returns>
        public static int? ToNullableInt32(this Object Obj)
        {

            if (Obj == null)
            {
                return null;
            }
            else
            {
                string sValue = Obj.ToString();
                if (string.IsNullOrEmpty(sValue))
                {
                    return null;
                }
                else
                {
                    int iValue = Convert.ToInt32(sValue);
                    return iValue;
                }
            }

        }

        /// <summary>
        ///Extension for .net Object Class
        ///this method will convert the .ToString() value to Nullable Int32
        ///if the value is NULL it will be returns as NULL
        /// </summary>
        /// <param name="Obj"></param>
        /// <returns></returns>
        public static double? ToNullableDouble(this Object Obj)
        {

            if (Obj == null)
            {
                return null;
            }
            else
            {
                string sValue = Obj.ToString();
                if (string.IsNullOrEmpty(sValue))
                {
                    return null;
                }
                else
                {
                    double iValue = Convert.ToDouble(sValue);
                    return iValue;
                }
            }

        }

        /// <summary>
        ///Extension for .net Object Class
        ///this method will convert the .ToString() value to Double
        /// </summary>
        /// <param name="Obj"></param>
        /// <returns></returns>
        public static double ToDouble(this Object Obj)
        {
            string sValue = Obj.ToString();
            if (string.IsNullOrEmpty(sValue)) return 0;

            double iValue = Convert.ToDouble(sValue);
            return iValue;

        }

        /// <summary>
        ///Extension for .net Object Class
        ///this method will convert the .ToString() value to Double
        /// </summary>
        /// <param name="Obj"></param>
        /// <returns></returns>
        public static decimal ToDecimal(this Object Obj)
        {
            string sValue = Obj.ToString();
            if (string.IsNullOrEmpty(sValue)) return 0;

            decimal iValue = Convert.ToDecimal(sValue);
            return iValue;

        }

        /// <summary>
        ///Extension for .net Object Class
        ///this method will convert the .ToString() value to DateTime
        /// </summary>
        /// <param name="Obj"></param>
        /// <returns></returns>
        public static DateTime ToDateTime(this Object Obj)
        {
            string sValue = Obj.ToString();
            DateTime iValue = Convert.ToDateTime(sValue);
            return iValue;

        }

        public static DateTime ToUniversalDateTime(this Object Obj)
        {
            string sValue = Obj.ToString();
            DateTime iValue = Convert.ToDateTime(sValue).ToUniversalTime();
            return iValue;

        }

        /// <summary>
        ///Extension for .net Object Class
        ///this method will convert the .ToString() value to DateTime
        ///if string is null or empty null value will be set
        /// </summary>
        /// <param name="Obj"></param>
        /// <returns></returns>
        public static DateTime? ToNullableDateTime(this Object Obj)
        {
            if (Obj is null) return null;

            if (String.IsNullOrEmpty(Obj.ToString()))
            {
                return null;
            }
            else
            {
                return Obj.ToDateTime();
            }

        }

        public static DateTime? ToNullableUniversalDateTime(this Object Obj)
        {
            if (String.IsNullOrEmpty(Obj.ToString()))
            {
                return null;
            }
            else
            {
                return Obj.ToUniversalDateTime();
            }
        }


        /// <summary>
        /// this methos will convert the null value to DBNull.Value
        /// use when assigning the parameter in an sql parameter
        /// </summary>
        /// <param name="Obj"></param>
        /// <returns></returns>
        public static object ToDbNull(this double? Obj)
        {
            if (Obj.HasValue) return Obj.Value;
            else return DBNull.Value;
        }


        #region toString extension

        /// <summary>
        ///Extension for .net Object Class
        ///this method will convert the string to various formats
        /// </summary>
        /// <param name="Obj"></param>
        /// <returns></returns>
        public static string ToString(this Object Obj, FormatType Format)
        {
            switch (Format)
            {
                case FormatType.Numbers:
                    int iNum;
                    bool res = Int32.TryParse(Obj.ToString(), out iNum);
                    if (res)
                    {
                        return iNum.ToString("0,0");
                    }
                    else
                    {
                        //decimal
                        return Obj.ToDouble().ToString("0.##");
                    }
                    break;

                case FormatType.Percent:
                    return Obj.ToString() + "%";

                case FormatType.Decimal:
                    return Obj.ToDouble().ToString("0.##");

                case FormatType.DecimalMoney:
                    return Obj.ToDouble().ToString("0.## ₪");


                //return Obj.ToDouble().ToString("#0,0.##");


                case FormatType.Money:
                    return Obj.ToDouble().ToString("0,0 ₪");
                //return Obj.ToInt32().ToString("0,0 ₪");

                default:
                    return Obj.ToString();
            }
        }


        public enum FormatType
        {
            Numbers = 1,
            Percent = 2,
            Percent2Digits = 3,
            Decimal = 4,
            Money = 5,
            DecimalMoney = 6,
        }
        #endregion





        #endregion

        #region Extension For DateTime

        /// <summary>
        /// this will return the date foramt as you declared in appconfiguration 
        /// under the property : SqlServerDateInsertFormat
        /// use it when you want to insert data into database with full time and date
        /// 
        /// 08/12/19 Benny
        /// i have changed it to return utc time 
        /// </summary>
        /// <param name="Obj"></param>
        /// <returns></returns>
        public static string ToStringForSqlServer(this DateTime Obj)
        {
            return Obj.ToUniversalTime().ToString("MM/dd/yyyy HH:mm:ss.fff");
            //return Obj.ToUniversalTime().ToString();
            //string sValue = Obj.ToString(AppConfiguration.SqlServerDateInsertFormat);
            //return sValue;

        }

        /// <summary>
        /// Converts DateTime to date-only string
        /// </summary>
        /// <param name="Obj"></param>
        /// <returns></returns>
        public static string ToStringDateOnly(this DateTime Obj)
        {
            return Obj.ToString("MM/dd/yyyy");
        }

        /// <summary>
        ///this method will return the current Quarter of the object
        /// </summary>
        /// <param name="Obj"></param>
        /// <returns></returns>
        public static int GetQuarter(this DateTime Obj)
        {
            List<int> Q1List = new List<int>() { 1, 2, 3 };
            List<int> Q2List = new List<int>() { 4, 5, 6 };
            List<int> Q3List = new List<int>() { 7, 8, 9 };
            List<int> Q4List = new List<int>() { 10, 11, 12 };


            if (Q1List.Contains(Obj.Month))
            {
                return 1;
            }

            if (Q2List.Contains(Obj.Month))
            {
                return 2;
            }

            if (Q3List.Contains(Obj.Month))
            {
                return 3;
            }


            //else
            return 4;
        }



        public static DateTime StartOfDay(this DateTime date)
        {
            return date.Date;
        }

        public static DateTime EndOfDay(this DateTime date)
        {
            return date.Date.AddDays(1).AddTicks(-1);
        }
        /// <summary>
        /// returns the first day of week
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="startOfWeek"></param>
        /// <returns></returns>
        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = dt.DayOfWeek - startOfWeek;
            if (diff < 0)
            {
                diff += 7;
            }
            return dt.AddDays(-1 * diff).Date;
        }

        /// <summary>
        /// returns the first day of month
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="startOfWeek"></param>
        /// <returns></returns>
        public static DateTime StartOfMonth(this DateTime dt)
        {
            DateTime dt2 = new DateTime(dt.Year, dt.Month, 1);
            return dt2;
        }

        /// <summary>
        /// returns the first day of year
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="startOfWeek"></param>
        /// <returns></returns>
        public static DateTime StartOfYear(this DateTime dt)
        {
            DateTime dt2 = new DateTime(dt.Year, 1, 1);
            return dt2;
        }



        #endregion

        #region Extension For String

        /// <summary>
        /// Extension for string
        /// will eliminate all chars excepts ascii chars
        /// </summary>
        /// <param name="input"></param>
        /// <param name="includeExtendedAscii"></param>
        /// <returns></returns>
        public static string JsonClear(this string input)
        {

            input = input.Replace("\'", "");
            input = input.Replace("\"", "");
            input = input.Replace("\\", "");
            input = input.Replace("{", "");
            input = input.Replace("}", "");
            input = input.Replace("[", "");
            input = input.Replace("]", "");
            return input;
        }

        /// <summary>
        ///Extension for .net String Class
        ///this method will split the string by the comma char ','
        /// </summary>
        /// <param name="Obj"></param>
        /// <returns></returns>
        public static string[] SplitByComma(this String Obj)
        {
            string[] Res = Obj.Split(',');
            return Res;
        }

        /// <summary>
        ///Extension for .net String Class
        ///this method will split the string by dynamic char sent as parameter to function
        ///this method remove white spaces 
        /// </summary>
        /// <param name="Obj"></param>
        /// <returns></returns>
        //public static string[] DynamicSplit(this String Obj, char [] SplitBy)
        //{
        //    return Obj.Split(SplitBy, System.StringSplitOptions.RemoveEmptyEntries);
        //}

        public static string[] DynamicSplit(this String Obj, char SplitBy)
        {
            return Obj.Split(new[] { SplitBy }, StringSplitOptions.RemoveEmptyEntries);
        }



        /// <summary>
        ///Extension for .net String Class
        ///this method will split the string by the comma char ',' and return as List of type int
        /// </summary>
        /// <param name="Obj"></param>
        /// <returns></returns>
        public static List<int> SplitByCommaInt(this String Obj)
        {
            string[] Res = Obj.Split(',');
            //int[] myInts = Array.ConvertAll(Res, s => s.ToInt32());
            var newList = Res.Select(i => i.ToInt32()).ToList();

            return newList;

        }


        /// <summary>
        ///Extension for .net String Class
        ///this method will Reverse the string chars
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string Reverse(this String input)
        {
            char[] inputarray = input.ToCharArray();
            Array.Reverse(inputarray);
            string output = new string(inputarray);

            return output;
        }

        /// <summary>
        /// remove the last chart
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string RemoveLastChar(this String input)
        {
            string output = input;
            if (output.Length > 0) output = output.Substring(0, output.Length - 1);

            return output;
        }

        /// <summary>
        /// remove the first char
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string RemoveFirstChar(this String input)
        {
            string output = input;
            if (output.Length > 0) output = output.Substring(1, output.Length - 1);

            return output;
        }





        /// <summary>
        /// Word wraps the given text to fit within the specified width.
        /// </summary>
        // <param name="text">Text to be word wrapped</param>
        /// <param name="width">Width, in characters, to which the text
        /// should be word wrapped</param>
        /// <returns>The modified text</returns>
        /// <see cref="http://www.softcircuits.com/Blog/post/2010/01/10/Implementing-Word-Wrap-in-C.aspx"/>
        public static string WordWrap(this string the_string, int width, string NewLineString)
        {
            int pos, next;
            StringBuilder sb = new StringBuilder();

            // Lucidity check
            if (width < 1)
                return the_string;

            // Parse each line of text
            for (pos = 0; pos < the_string.Length; pos = next)
            {
                // Find end of line
                int eol = the_string.IndexOf(NewLineString, pos);

                if (eol == -1)
                    next = eol = the_string.Length;
                else
                    next = eol + NewLineString.Length;

                // Copy this line of text, breaking into smaller lines as needed
                if (eol > pos)
                {
                    do
                    {
                        int len = eol - pos;

                        if (len > width)
                            len = BreakLine(the_string, pos, width);

                        sb.Append(the_string, pos, len);
                        sb.Append(NewLineString);

                        // Trim whitespace following break
                        pos += len;

                        while (pos < eol && Char.IsWhiteSpace(the_string[pos]))
                            pos++;

                    } while (eol > pos);
                }
                else sb.Append(NewLineString); // Empty line
            }

            return sb.ToString();
        }

        /// <summary>
        /// Locates position to break the given line so as to avoid
        /// breaking words.
        /// </summary>
        /// <param name="text">String that contains line of text</param>
        /// <param name="pos">Index where line of text starts</param>
        /// <param name="max">Maximum line length</param>
        /// <returns>The modified line length</returns>
        public static int BreakLine(string text, int pos, int max)
        {
            // Find last whitespace in line
            int i = max - 1;

            while (i >= 0 && !Char.IsWhiteSpace(text[pos + i]))
                i--;

            if (i < 0)
                return max; // No whitespace found; break at maximum length

            // Find start of whitespace
            while (i >= 0 && Char.IsWhiteSpace(text[pos + i]))
                i--;

            // Return length of text before whitespace
            return i + 1;
        }



        /// <summary>
        /// this methos will convert the null value to DBNull.Value
        /// use when assigning the parameter in an sql parameter
        /// </summary>
        /// <param name="Obj"></param>
        /// <returns></returns>
        public static object ToDbNull(this string Obj)
        {
            if (!string.IsNullOrEmpty(Obj)) return Obj.ToString();
            else return DBNull.Value;
        }


        #endregion

        #region Extension For Boolean

        /// <summary>
        ///Extension for .net Boolean Class
        ///this method will return the opposite boolean value
        ///true -> false, false->true
        /// </summary>
        /// <param name="Obj"></param>
        /// <returns></returns>
        public static bool Opposite(this bool Obj)
        {
            if (Obj == true) return false;
            else return true;

        }

        /// <summary>
        ///Extension for .net Boolean Class
        ///this method will return 1 for true and 0 for false 
        /// </summary>
        /// <param name="Obj"></param>
        /// <returns></returns>
        public static int ToInt32(this bool Obj)
        {
            if (Obj == true) return 1;
            else return 0;

        }


        #endregion

        #region Extension For INT

        /// <summary>
        /// this methos will convert the null value to DBNull.Value
        /// use when assigning the parameter in an sql parameter
        /// </summary>
        /// <param name="Obj"></param>
        /// <returns></returns>
        public static object ToDbNull(this int? Obj)
        {
            if (Obj.HasValue) return Obj.Value;
            else return DBNull.Value;
        }

        /// <summary>
        ///Extension for .net INT32 Class
        ///this method will return the  boolean value
        ///1 -> true, otherwise-->false
        /// </summary>
        /// <param name="Obj"></param>
        /// <returns></returns>
        public static bool ToBoolean(this int Obj)
        {
            if (Obj == 1) return true;
            else return false;

        }

        #endregion

        #region Extension for double

        /// <summary>
        /// this methos will return the number formatted as currency
        /// 1000 --> 1,000 NIS
        /// </summary>
        /// <param name="Obj"></param>
        /// <returns></returns>
        public static string ToCurrencyString(this double Obj)
        {
            return Obj.ToString("C2", CultureInfo.CurrentCulture);
        }

        #endregion

        #region Extenstions for collections

        /// <summary>
        /// Adding dictonary keys and values for exist dictonary
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="S"></typeparam>
        /// <param name="source"></param>
        /// <param name="collection"></param>
        public static void AddRange<T, S>(this Dictionary<T, S> source, Dictionary<T, S> collection)
        {
            if (collection == null)
            {
                return;
            }

            foreach (var item in collection)
            {
                if (!source.ContainsKey(item.Key))
                {
                    source.Add(item.Key, item.Value);
                }
                else
                {
                    source[item.Key] = item.Value;
                }
            }
        }


        /// <summary>
        /// convert an array of string to array list
        /// </summary>
        /// <param name="Obj"></param>
        /// <returns></returns>
        public static ArrayList ToArrayList(this string[] Obj)
        {
            ArrayList myArrayList = new ArrayList();
            myArrayList.AddRange(Obj);
            return myArrayList;
        }

        /// <summary>
        /// converts the string list to string CSV
        /// </summary>
        /// <param name="Obj"></param>
        /// <returns></returns>
        public static string ToCSV(this List<string> Obj)
        {
            return String.Join(",", Obj.Select(x => x.ToString()).ToArray());

        }





        /// <summary>
        /// converts the int list to string CSV
        /// </summary>
        /// <param name="Obj"></param>
        /// <returns></returns>
        public static string ToCSV(this List<int> Obj)
        {
            if (Obj.Count == 0) return "";
            return String.Join(",", Obj.Select(x => x.ToString()).ToArray());

        }



        /// <summary>
        /// convert hashtable to CSV string based on the KEYS
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ToCSV(this Hashtable input)
        {
            return string.Join(",", (from string name in input.Keys select name).ToArray());
        }


        #endregion

        #region Extentions for DataTable



        /// <summary>
        /// this method will convert the datarow[] to list of int
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="ColumnName"></param>
        /// <returns></returns>
        public static List<int> ToListOfInt(this DataTable dt, string ColumnName)
        {
            List<int> Ids = new List<int>();
            foreach (DataRow dr in dt.Rows)
            {
                Ids.Add(dr[ColumnName].ToInt32());
            }
            return Ids;
        }


        /// <summary>
        /// this method will convert the datarow[] to list of int
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="ColumnName"></param>
        /// <returns></returns>
        public static List<int> ToListOfInt(this DataTable dt)
        {
            List<int> Ids = new List<int>();
            foreach (DataRow dr in dt.Rows)
            {
                Ids.Add(dr[0].ToInt32());
            }
            return Ids;
        }

        /// <summary>
        /// convert datatable to hashtable with key/value configuration
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="KeyColumn"></param>
        /// <param name="ValueColumn"></param>
        /// <returns></returns>
        public static Hashtable ToHashTable(this DataTable dt, string KeyColumn, string ValueColumn)
        {
            Hashtable res = new Hashtable();

            foreach (DataRow dr in dt.Rows)
            {
                if (res.ContainsKey(dr[KeyColumn])) continue;

                res.Add(dr[KeyColumn], dr[ValueColumn]);
            }
            return res;
        }

        #endregion

        #region Extensions for DataRow
        public static T ToObject<T>(this DataRow dataRow)
        where T : new()
        {
            T item = new T();

            foreach (DataColumn column in dataRow.Table.Columns)
            {
                PropertyInfo property = GetProperty(typeof(T), column.ColumnName);

                if (property != null && dataRow[column] != DBNull.Value && dataRow[column].ToString() != "NULL")
                {
                    property.SetValue(item, ChangeType(dataRow[column], property.PropertyType), null);
                }
            }

            return item;
        }

        private static PropertyInfo GetProperty(Type type, string attributeName)
        {
            PropertyInfo property = type.GetProperty(attributeName);

            if (property != null)
            {
                return property;
            }

            return type.GetProperties()
                 .Where(p => p.IsDefined(typeof(DisplayAttribute), false) && p.GetCustomAttributes(typeof(DisplayAttribute), false).Cast<DisplayAttribute>().Single().Name == attributeName)
                 .FirstOrDefault();
        }

        public static object ChangeType(object value, Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                if (value == null)
                {
                    return null;
                }

                return Convert.ChangeType(value, Nullable.GetUnderlyingType(type));
            }

            if (type.IsEnum)
            {
                return Enum.ToObject(type, value);
            }
            else
            {
                return Convert.ChangeType(value, type);
            }
        }
        #endregion
    }


}