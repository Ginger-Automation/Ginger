#region License
/*
Copyright © 2014-2019 European Support Limited

Licensed under the Apache License, Version 2.0 (the "License")
you may not use this file except in compliance with the License.
You may obtain a copy of the License at 

http://www.apache.org/licenses/LICENSE-2.0 

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS, 
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
See the License for the specific language governing permissions and 
limitations under the License. 
*/
#endregion

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Repository;
using Amdocs.Ginger.Repository;
using Ginger.Reports;
using GingerCore.Actions;
using GingerCore.Activities;
using GingerCore.DataSource;
using GingerCore.GeneralLib;
using GingerCore.Variables;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace GingerCore.Repository
{
    // OLD OLD OLD OLD OLD OLD OLD OLD OLD OLD OLD OLD OLD OLD OLD OLD OLD OLD OLD OLD OLD OLD OLD OLD


    // This class is for storing RepositoryItem on disk, it needs to be serialized to XML
    // reason for not using some of the existing options:
    // Binary - makes it difficult to compare version/history in CC + some say it is slower!?
    // XML formatter - the default have some challenges and with some NG objects    
    // Pros - With our own serialization we can solve the problem of copy vs link of Action, during load/save we can take the items from repo
    // We can have several style of serialization - 1 store to repo - not all attrs are save, 2 store local save most attrs
    // It should work faster - to be tested and optimized
    // + We can keep backward compatibility much easier
    // + It solve the copy/link to other repo item during serialization/de-serialization
    // It will also solve problems with older agents - no need to update all agents, since sending xml and parsing with defaults
    // we can also decide on ad hoc serialization based on the target: if we send it to agent, save to disk or other
    // We can also decide on ignore error and get partial object - to be fixed- but maybe better than nothing
    // We cam also read partial files - i.e: if we just need the Business flow name for list, no need to read all file
    // We can add custom attr at the top

    public class RepositorySerializer : IRepositorySerializer
    {
        // static string mGingerVersion;
        // static long mGingerVersionAsLong = 0;        

        public void SaveToFile(RepositoryItemBase ri, string FileName)
        {
            string txt = SerializeToString(ri);
            File.WriteAllText(FileName, txt);
        }

        public static bool isCopy = false;
        public string SerializeToString(RepositoryItemBase ri)
        {
            if (ri != null)
            {
                using (MemoryStream output = new MemoryStream())
                {
                    using (XmlTextWriter xml = new XmlTextWriter(output, Encoding.UTF8))
                    {
                        xml.WriteStartDocument();

                        xml.WriteWhitespace("\n");
                        xml.WriteComment("Ginger Repository Item created with version: " + Amdocs.Ginger.Common.GeneralLib.ApplicationInfo.ApplicationMajorVersion);
                        xml.WriteWhitespace("\n");

                        xmlwriteObject(xml, ri);

                        xml.WriteEndDocument();
                    }
                    string result = Encoding.UTF8.GetString(output.ToArray());
                    return result;
                }
            }
            else
                return string.Empty;
        }

       

        //TODO: later on get back this function it is more organize, but causing saving problems  -to be fixed later
        private void xmlwriteObject(XmlTextWriter xml, RepositoryItemBase ri)
        {
            xml.WriteStartElement(ri.GetType().ToString());
            WriteRepoItemProperties(xml, ri);
            WriteRepoItemFields(xml, ri);
            xml.WriteEndElement();
        }

        private void WriteRepoItemProperties(XmlTextWriter xml, RepositoryItemBase ri)
        {
            //TODO: handle all the same like fields and make it shared functions

            // Get the properties - need to be ordered so compare/isDirty can work faster
            var properties = ri.GetType().GetMembers().Where(x => x.MemberType == MemberTypes.Property).OrderBy(x => x.Name);
            foreach (MemberInfo mi in properties)
            {
                dynamic v = null;
                IsSerializedForLocalRepositoryAttribute token = Attribute.GetCustomAttribute(mi, typeof(IsSerializedForLocalRepositoryAttribute), false) as IsSerializedForLocalRepositoryAttribute;
                if (token == null) continue;

                //Get tha attr value
                v = ri.GetType().GetProperty(mi.Name).GetValue(ri);
                // Enum might be unknown = not set - so no need to write to xml, like null for object                        
                if (ri.GetType().GetProperty(mi.Name).PropertyType.IsEnum)
                {
                    string vs = v.ToString();
                    // No need to write enum unknown = null
                    if (vs != "Unknown")
                    {
                        xmlwriteatrr(xml, mi.Name, vs);
                    }
                }
                else
                {
                    if (v != null)
                    {
                        xmlwriteatrr(xml, mi.Name, v.ToString());
                    }
                }
            }
        }

        private void WriteRepoItemFields(XmlTextWriter xml, RepositoryItemBase ri)
        {
            var Fields = ri.GetType().GetMembers().Where(x => x.MemberType == MemberTypes.Field).OrderBy(x => x.Name);

            foreach (MemberInfo fi in Fields)
            {
                dynamic v = null;
                IsSerializedForLocalRepositoryAttribute token = Attribute.GetCustomAttribute(fi, typeof(IsSerializedForLocalRepositoryAttribute), false) as IsSerializedForLocalRepositoryAttribute;
                if (token == null) continue;

                
                if (IsObseravbleListLazyLoad(fi.Name))
                {
                    bool b = ((IObservableList)(ri.GetType().GetField(fi.Name).GetValue(ri))).LazyLoad;
                    if (b)
                    {
                        string s = ((IObservableList)(ri.GetType().GetField(fi.Name).GetValue(ri))).StringData;
                        xml.WriteStartElement("Activities");
                        xml.WriteString(s);
                        xml.WriteEndElement();
                        return;
                    }
                }
                

                v = ri.GetType().GetField(fi.Name).GetValue(ri);
                if (v != null)
                {
                    if (v is IObservableList)
                    {
                        xmlwriteObservableList(xml, fi.Name, (IObservableList)v);
                    }
                    else
                    {
                        if (v is List<string>)
                        {
                            xmlwriteStringList(xml, fi.Name, (List<string>)v);
                        }
                        else if (v is RepositoryItemBase)
                        {
                            xmlwriteSingleObjectField(xml, fi.Name, v);
                        }
                        else
                        {
                            xml.WriteComment(">>>>>>>>>>>>>>>>> Unknown Field type to serialize - " + fi.Name + " - " + v.ToString());
                        }
                    }
                }
            }
        }

        private void xmlwriteSingleObjectField(XmlTextWriter xml, string Name, Object obj)
        {
            xml.WriteWhitespace("\n");
            xml.WriteStartElement(Name);
            xml.WriteWhitespace("\n");
            xmlwriteObject(xml, (RepositoryItemBase)obj);

            xml.WriteWhitespace("\n");
            xml.WriteEndElement();
            xml.WriteWhitespace("\n");
        }

        private void xmlwriteStringList(XmlTextWriter xml, string Name, List<string> list)
        {
            xml.WriteWhitespace("\n");
            xml.WriteStartElement(Name);
            foreach (string s in list)
            {
                xml.WriteWhitespace("\n");
                xml.WriteElementString("string", s);
            }
            xml.WriteWhitespace("\n");
            xml.WriteEndElement();
            xml.WriteWhitespace("\n");
        }


        private void xmlwriteObservableList(XmlTextWriter xml, string Name, IObservableList list)
        {
            //check if the list is of Repo item or native - got a small diff when writing
            xml.WriteWhitespace("\n");
            xml.WriteStartElement(Name);
            foreach (var v in list)
            {
                xml.WriteWhitespace("\n");
                if (v is RepositoryItemBase)
                {
                    if (!((RepositoryItemBase)v).IsTempItem) // Ignore temp items like dynamic activities or some output values if marked as temp
                    {
                        xmlwriteObject(xml, (RepositoryItemBase)v);
                    }
                }
                else
                {
                    //TODO: use generic type write
                    xml.WriteElementString(v.GetType().FullName, v.ToString());
                }
            }
            xml.WriteWhitespace("\n");
            xml.WriteEndElement();
            xml.WriteWhitespace("\n");
        }

        private void xmlwriteatrr(XmlTextWriter xml, string Name, string Value)
        {
            xml.WriteStartAttribute(Name);

            //TODO: it is big waste to check for string for all writes... perf issue FIXME
            if (string.Compare(Name, "GUID", true) == 0 && isCopy)
                xml.WriteString(Guid.NewGuid().ToString());
            else
                xml.WriteString(Value);
            xml.WriteEndAttribute();
        }

        public object DeserializeFromFile(Type t, string FileName)
        {
            if (FileName.Length > 0 && File.Exists(FileName))
            {
                string xml = File.ReadAllText(FileName);
                if (xml.Contains("GingerRepositoryItem"))
                {
                    // this object was saved with new RS - throw
                    throw new Exception("trying to load object with old RS while it was saved with new RS:" + t.Name + ",  FileName=" + FileName);
                }

                // first check if we need to auto upgrade the xml to latest ginger version               
                
                return DeserializeFromText(t, xml);
            }
            else
            {
                throw new Exception("File Not Found - " + FileName);
            }
        }

        public object DeserializeFromTextWithTargetObj(Type t, string xml, RepositoryItemBase targetObj = null)
        {
            if (xml.Contains("GingerRepositoryItem"))
            {
                // this object was saved with new RS - should not call this RS                    
                // object o = NewRepositorySerializer.DeserializeFromText(xml);
                // return o;
                throw new Exception("Object was create with new SR, but trying to load with old SR: " + t.Name);
            }

            string encoding = "utf-8";
            var ms = new MemoryStream(Encoding.GetEncoding(encoding).GetBytes(xml));
            var xdrs = new XmlReaderSettings()
            {
                IgnoreComments = true,
                IgnoreWhitespace = true,
                CloseInput = true
            };

            XmlReader xdr = XmlReader.Create(ms, xdrs);
            xdr.Read();
            xdr.Read();
            dynamic RootObj = xmlReadObject(null, xdr, targetObj);

            return RootObj;
        }

        public void DeserializeObservableListFromText<T>(ObservableList<T> lst, string xml)
        {
            string encoding = "utf-8";

            var ms = new MemoryStream(Encoding.GetEncoding(encoding).GetBytes(xml));
            var xdrs = new XmlReaderSettings()
            {
                IgnoreComments = true,
                IgnoreWhitespace = true,
                CloseInput = true
            };

            XmlReader xdr = XmlReader.Create(ms, xdrs);
            xdr.Read();
            xdr.Read();
            while (xdr.NodeType != XmlNodeType.EndElement)
            {
                object item = xmlReadObject(null, xdr);
                if (item != null)
                {
                    lst.Add((T)item);
                }
                else
                {
                    return;
                }
            }
            xdr.ReadEndElement();
        }

        public object DeserializeFromFile(string FileName)
        {
            if (FileName.Length > 0 && File.Exists(FileName))
            {
                string xml = File.ReadAllText(FileName);

                return DeserializeFromText(xml);
            }
            else
            {
                throw new Exception("File Not Found - " + FileName);
            }
        }

        public static object DeserializeFromText(string xml, RepositoryItemBase targetObj = null)
        {
            string encoding = "utf-8";
            //check if we need ms or maybe text reader + do using to release mem
            var ms = new MemoryStream(Encoding.GetEncoding(encoding).GetBytes(xml));
            var xdrs = new XmlReaderSettings()
            {
                IgnoreComments = true,
                IgnoreWhitespace = true,
                CloseInput = true
            };
            XmlReader xdr = XmlReader.Create(ms, xdrs);
            xdr.Read();
            xdr.Read();
            dynamic RootObj = xmlReadObject(null, xdr, targetObj);

            return RootObj;
        }

        private static void xmlReadListOfObjects(object ParentObj, XmlReader xdr, IObservableList observableList)
        {
            // read list of object into the list, add one by one, like activities, actions etc.            
            
            //Fast Load
            if (IsObseravbleListLazyLoad(xdr.Name))
            {
                // We can save line/col and reload later when needed
                string s = xdr.ReadOuterXml();
                observableList.DoLazyLoadItem(s);
                observableList.LazyLoad = true;
                return;
            }
            
            xdr.Read();
            while (xdr.NodeType != XmlNodeType.EndElement)
            {
                object item = xmlReadObject(ParentObj, xdr);
                if (item != null)
                {
                    observableList.Add(item);
                }
                else
                {
                    return;
                }

            }
            xdr.ReadEndElement();
        }

        private static bool IsObseravbleListLazyLoad(string name)
        {
            // Here we decide which Observable List we cache as string until user really ask for the data
            // for now we cache only activities which is the major issue for performance when loading solution            
            return false;
        }

        static Assembly GingerAssembly = System.Reflection.Assembly.Load("Ginger");
        static Assembly GingerCoreAssembly = System.Reflection.Assembly.GetExecutingAssembly();
        static Assembly GingerCoreCommon = typeof(RepositoryItemBase).Assembly;
        static Assembly GingerCoreNET = typeof(HTMLReportConfiguration).Assembly;


        private static object xmlReadObject(Object Parent, XmlReader xdr, RepositoryItemBase targetObj = null)
        {
            string ClassName = xdr.Name;

            // We do auto convert to classes which moved to GingerCoreCommon
            if (ClassName == "GingerCore.Actions.ActInputValue")
            {
                ClassName = typeof(ActInputValue).FullName;
            }
            if (ClassName == "GingerCore.Actions.ActReturnValue" || ClassName == "GingerCore.Common.Actions.ActInputValue")
            {
                ClassName = typeof(ActReturnValue).FullName;
            }
            if (ClassName == "GingerCore.Actions.EnhancedActInputValue")
            {
                ClassName = typeof(EnhancedActInputValue).FullName;
            }

            try
            {
                object obj = null;
                int level = xdr.Depth;

                // We first try in current assembly = GingerCore
                if (targetObj == null)
                {                   
                    obj = GingerCoreAssembly.CreateInstance(ClassName);                    
                }
                else
                { 
                    obj = targetObj; //used for DeepCopy to objects fields
                }

                if (obj == null) //Ginger assembly
                {
                    if (ClassName == "Ginger.Environments.Solution") ClassName = "Ginger.SolutionGeneral.Solution";
                    obj = GingerAssembly.CreateInstance(ClassName);
                    obj = System.Reflection.Assembly.Load("Ginger").CreateInstance(ClassName);
                }

                if (obj == null)
                {
                    obj = GingerCoreCommon.CreateInstance(ClassName);
                }

                if (obj == null) //GingerCoreCommon assembly
                {
                    if (ClassName == "GingerCore.Actions.ActInputValue" || ClassName == "GingerCore.Common.Actions.ActInputValue") ClassName = typeof(ActInputValue).FullName;
                    if (ClassName == "GingerCore.Actions.ActReturnValue") ClassName = typeof(ActReturnValue).FullName;
                    if (ClassName == "GingerCore.Environments.GeneralParam") ClassName = typeof(GeneralParam).FullName;
                    if (ClassName == "Ginger.TagsLib.RepositoryItemTag") ClassName = typeof(RepositoryItemTag).FullName;
                    if (ClassName == "GingerCore.Platforms.ApplicationPlatform") ClassName = typeof(ApplicationPlatform).FullName;
                    
                    
                    obj = GingerCoreCommon.CreateInstance(ClassName);
                }


                if (obj == null) //GingerCoreNET assembly
                {                    
                    obj = GingerCoreNET.CreateInstance(ClassName);
                }

                if (obj == null)
                {
                    throw new Exception("Error:Cannot create Class: " + ClassName);
                }

                SetObjectAttributes(xdr, obj);

                xdr.Read();
                // Set lists attrs
                // read all object sub elements like lists - obj members              
                while (xdr.Depth == level + 1)
                {
                    // Check if it one obj attr or list
                    string attrName = xdr.Name;

                    MemberInfo mi = obj.GetType().GetMember(attrName).SingleOrDefault(); 

                    // New to support prop and field - like BF.Activities
                    if (mi.MemberType == MemberTypes.Field)
                    {
                        FieldInfo FI = (FieldInfo)mi;                         
                        // We check if it is list by arg count - List<string> will have string etc...
                        // another option is check the nake to start with List, Observe...
                        //or find a better way
                        // meanwhile it is working
                        if (FI.FieldType.GenericTypeArguments.Count() > 0)
                        {
                            SetObjectListAttrs(xdr, obj);
                        }
                        else
                        {
                            // Read the attr name/move next
                            xdr.ReadStartElement();
                            // read the actual object we need to put on the attr                            
                            object item = xmlReadObject(obj, xdr);
                            // Set the attr val with the object
                            FI.SetValue(obj, item);

                            if (item is Email)//If should be removed- placing if for solving Run Set operation release issue with minimum risk
                                xdr.ReadEndElement();
                        }
                    }
                    else
                    {
                        PropertyInfo PI = (PropertyInfo)mi;
                        // obj.GetType().GetField(attrName);
                        // We check if it is list by arg count - List<string> will have string etc...
                        // another option is check the nake to start with List, Observe...
                        //or find a better way
                        // meanwhile it is working
                        if (PI.PropertyType.GenericTypeArguments.Count() > 0)
                        {
                            SetObjectListAttrs(xdr, obj);
                        }
                        else
                        {
                            // Read the attr name/move next
                            xdr.ReadStartElement();
                            // read the actual object we need to put on the attr                            
                            object item = xmlReadObject(obj, xdr);
                            // Set the attr val with the object
                            PI.SetValue(obj, item);

                            if (item is Email)//If should be removed- placing if for solving Run Set operation release issue with minimum risk
                                xdr.ReadEndElement();
                        }
                    }

                    
                    if (xdr.NodeType == XmlNodeType.EndElement)
                    {
                        xdr.ReadEndElement();
                    }
                }
                return obj;
            }
            catch (Exception ex)
            {
                throw new Exception("Error:Cannot create instance of: " + xdr.Name + " - " + ex.Message);
            }
        }

        private static void SetObjectListAttrs(XmlReader xdr, object obj)
        {
            // Handle object list etc which comes after the obj attrs - like activities, or activity actions
            string AtrrListName = xdr.Name;
            if (xdr.IsStartElement())
            {
                {
                    MemberInfo mi = obj.GetType().GetMember(AtrrListName).SingleOrDefault();
                    if (mi.MemberType == MemberTypes.Field)
                    {
                        FieldInfo fi = (FieldInfo)mi;
                        // generate same type empty list objects
                        Type t = fi.FieldType.GenericTypeArguments[0];

                        if (t == typeof(string))
                        {
                            List<string> lsts = (List<string>)fi.GetValue(obj);
                            xmlReadListOfStrings(xdr, lsts);
                        }
                        else if (t == typeof(Guid))
                        {
                            ObservableList<Guid> lstsg = (ObservableList<Guid>)fi.GetValue(obj);
                            xmlReadListOfGuids(xdr, lstsg);
                        }
                        else
                        {
                            //TODO: handle other types of list, meanwhile Assume observable list
                            IObservableList lst = (IObservableList)Activator.CreateInstance((typeof(ObservableList<>).MakeGenericType(t)));
                            //assign it to the relevant obj
                            fi.SetValue(obj, lst);
                            // Read the list from the xml
                            xmlReadListOfObjects(obj, xdr, lst);
                        }
                    }
                    else
                    {
                        PropertyInfo pi = (PropertyInfo)mi;
                        // generate same type empty list objects
                        Type t = pi.PropertyType.GenericTypeArguments[0];

                        if (t == typeof(string))
                        {
                            List<string> lsts = (List<string>)pi.GetValue(obj);
                            xmlReadListOfStrings(xdr, lsts);
                        }
                        else if (t == typeof(Guid))
                        {
                            ObservableList<Guid> lstsg = (ObservableList<Guid>)pi.GetValue(obj);
                            xmlReadListOfGuids(xdr, lstsg);
                        }
                        else
                        {

                            object result = null;
                            if (mi.MemberType == MemberTypes.Property)
                            {
                                result = ((PropertyInfo)mi).GetValue(obj);
                            }
                            else
                            {
                                result = ((FieldInfo)mi).GetValue(obj);
                            }
                            IObservableList lst = null;
                            if (result == null)
                            {
                                lst = (IObservableList)Activator.CreateInstance((typeof(ObservableList<>).MakeGenericType(t)));
                            }
                            else
                            {
                                lst = (IObservableList)result;                 
                                
                            }
                            //assign it to the relevant obj
                            pi.SetValue(obj, lst);
                            // Read the list from the xml
                            xmlReadListOfObjects(obj, xdr, lst);
                        }
                    }
                }
            }
            else
            {
                string s2 = xdr.Value;
            }
        }

        private static void xmlReadListOfGuids(XmlReader xdr, ObservableList<Guid> lstsg)
        {
            xdr.ReadStartElement();
            while (xdr.NodeType != XmlNodeType.EndElement)
            {
                string s = xdr.ReadElementContentAsString();
                Guid g = Guid.Parse(s);
                lstsg.Add(g);
            }
            xdr.ReadEndElement();
        }

        private static void xmlReadListOfStrings(XmlReader xdr, List<string> lsts)
        {
            xdr.ReadStartElement();
            while (xdr.NodeType != XmlNodeType.EndElement)
            {
                string s = xdr.ReadElementContentAsString();
                lsts.Add(s);
            }
            xdr.ReadEndElement();
        }

        private static void SetObjectAttributes(XmlReader xdr, object obj)
        {
            try
            {
                if (xdr.HasAttributes)
                {
                    xdr.MoveToFirstAttribute();
                    for (int i = 0; i < xdr.AttributeCount; i++)
                    {
                        PropertyInfo propertyInfo = obj.GetType().GetProperty(xdr.Name);
                        if (propertyInfo == null)
                        {
                            //if (obj.GetType().Assembly == typeof(RepositoryItemBase).Assembly)
                            //{
                            if (xdr.Name != "Created" && xdr.Name != "CreatedBy" && xdr.Name != "LastUpdate" && xdr.Name != "LastUpdateBy" && xdr.Name != "Version" && xdr.Name != "ExternalID")
                            {
                                Reporter.ToLog(eLogLevel.DEBUG, "Property not Found: " + xdr.Name);
                            }
                            //}
                            //else
                            //{
                            //    Reporter.ToLog(eLogLevel.DEBUG, "Property not Found: " + xdr.Name);
                            //}

                            xdr.MoveToNextAttribute();
                            continue;
                        }
                        string Value = xdr.Value;
                        if (Value != "Null")
                        {
                            SetObjAttrValue(obj, propertyInfo, Value);
                        }
                        xdr.MoveToNextAttribute();
                    }
                }
                xdr.MoveToNextAttribute();
            }
            catch (Exception ex)
            {                
                Reporter.ToLog(eLogLevel.ERROR, ex.Message);
            }
        }

        private static void SetObjAttrValue(object obj, PropertyInfo propertyInfo, string sValue)
        {
            try
            {
                System.TypeCode typeCode = Type.GetTypeCode(propertyInfo.PropertyType);
                switch (typeCode)
                {

                    case TypeCode.String:
                        propertyInfo.SetValue(obj, sValue);
                        break;

                    case TypeCode.Int32:

                        if (propertyInfo.PropertyType.IsEnum)
                        {
                            object o = Enum.Parse(propertyInfo.PropertyType, sValue);
                            if (o != null)
                            {
                                propertyInfo.SetValue(obj, o);
                            }
                            else
                            {
                                throw new Exception("Cannot convert Enum - " + sValue);
                            }
                        }
                        else
                        {
                            propertyInfo.SetValue(obj, Int32.Parse(sValue));
                        }
                        break;

                    case TypeCode.Int64:
                        propertyInfo.SetValue(obj, Int64.Parse(sValue));
                        break;
                    case TypeCode.Double:
                        propertyInfo.SetValue(obj, double.Parse(sValue));
                        break;

                    case TypeCode.Decimal:
                        propertyInfo.SetValue(obj, decimal.Parse(sValue));
                        break;

                    case TypeCode.DateTime:
                        propertyInfo.SetValue(obj, DateTime.Parse(sValue));
                        break;

                    case TypeCode.Boolean:
                        if (sValue.ToUpper() == "FALSE")
                        {
                            propertyInfo.SetValue(obj, false);
                            return;
                        }
                        if (sValue.ToUpper() == "TRUE")
                        {
                            propertyInfo.SetValue(obj, true);
                            return;
                        }

                        break;
                    case TypeCode.Object:

                        if (propertyInfo.PropertyType == typeof(System.Guid))
                        {
                            if (sValue != "00000000-0000-0000-0000-00000000")
                            {
                                propertyInfo.SetValue(obj, new Guid(sValue));
                            }
                        }
                        else
                        {
                            //check if this is nullable enum  like: Activity Status? 
                            if (Nullable.GetUnderlyingType(propertyInfo.PropertyType).IsEnum)
                            {
                                object o = Enum.Parse(Nullable.GetUnderlyingType(propertyInfo.PropertyType), sValue);
                                if (o != null)
                                {
                                    propertyInfo.SetValue(obj, o);
                                }
                                else
                                {
                                    throw new Exception("Cannot convert Enum - " + sValue);
                                }
                            }
                            else
                                if (Type.GetTypeCode(Nullable.GetUnderlyingType(propertyInfo.PropertyType)) == TypeCode.Int64)
                            {
                                if (sValue != null)
                                {
                                    propertyInfo.SetValue(obj, Int64.Parse(sValue));
                                }
                                else
                                {
                                    throw new Exception("Cannot convert Nullable Int64 - " + sValue);
                                }
                            }
                            else
                                    if (Type.GetTypeCode(Nullable.GetUnderlyingType(propertyInfo.PropertyType)) == TypeCode.Int32)
                            {
                                if (sValue != null)
                                {
                                    propertyInfo.SetValue(obj, Int32.Parse(sValue));
                                }
                                else
                                {
                                    throw new Exception("Cannot convert Nullable Int32 - " + sValue);
                                }
                            }
                            else
                                        if (Type.GetTypeCode(Nullable.GetUnderlyingType(propertyInfo.PropertyType)) == TypeCode.Double)
                            {
                                if (sValue != null)
                                {
                                    propertyInfo.SetValue(obj, Double.Parse(sValue));
                                }
                                else
                                {
                                    throw new Exception("Cannot convert Nullable Double - " + sValue);
                                }
                            }

                            else
                            {
                                throw new Exception("Serializer - Err set value, Unknown type - " + propertyInfo.PropertyType.ToString() + " Value: " + sValue);
                            }
                        }
                        break;

                    default:
                        throw new Exception("Serializer - Err set value, Unknown type - " + propertyInfo.PropertyType.ToString() + " Value: " + sValue);

                }

                //TODO: all other types
            }
            catch
            {
                string err;
                if (propertyInfo != null)
                {
                    err = "Obj=" + obj + ", Property=" + propertyInfo.Name + ", Value=" + sValue.ToString();
                }
                else
                {
                    err = "Property Not found: Obj=" + obj + " Value=" + sValue.ToString();
                }
            }
        }

        public static bool IsLegacyXmlType(string xml)
        {
            if (xml.Contains("<!--Ginger Repository Item ")) return true;
            return false;
        }

        public static void UpdateXMLGingerVersion(XDocument xmlDoc, string gingerVersionToSet)
        {
            var comments = xmlDoc.DescendantNodes().OfType<XComment>();
            string UpdatedComment = String.Format("Ginger Repository Item created with version: " + gingerVersionToSet);
            comments.ElementAt(0).ReplaceWith(new XComment(UpdatedComment));
        }

        public static string GetXMLGingerVersion(string xml, string xmlFilePath)
        {
            try
            {
                /* Expecting the 1st comment in file to contain build info and 
                * expecting  comment to look this: 
                * <!--Ginger Repository Item created with version: 0.1.2.3 -->*/
                int i1 = xml.IndexOf("<!--Ginger Repository Item created with version: ");
                int i2 = xml.IndexOf("-->");

                string BuildInfo = xml.Substring(i1, i2 - i1);
                Regex regex = new Regex(@"(\d+)\.(\d+)\.(\d+)\.(\d+)");
                Match match = regex.Match(BuildInfo);
                if (match.Success)
                {
                    //avoiding Beta + Alpha numbers because for now it is not supposed to be written to XML's, only official release numbers
                    int counter = 0;
                    string ver = string.Empty;
                    for (int indx = 0; indx < match.Value.Length; indx++)
                    {
                        if (match.Value[indx] == '.')
                            counter++;
                        if (counter == 2)
                            return ver + ".0.0";
                        else
                            ver += match.Value[indx];
                    }
                    return ver;//something wrong
                }
                else
                {
                    Reporter.ToLog(eLogLevel.WARN, string.Format("Failed to get the XML Ginger version of the XML at path = '{0}'", xmlFilePath));
                    return null;//failed to get the version
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.WARN, string.Format("Failed to get the XML Ginger version of the XML at path = '{0}'", xmlFilePath));
                Console.WriteLine(ex.StackTrace);
                return null;//failed to get the version
            }
        }

        //TODO enable to read highlights                
        void IRepositorySerializer.DeserializeObservableListFromText<T>(ObservableList<T> observableList, string s)
        {
            DeserializeObservableListFromText(observableList, s);
        }

        RepositoryItemBase IRepositorySerializer.DeserializeFromFile(Type type, string fileName)
        {
            return (RepositoryItemBase)this.DeserializeFromFile(type, fileName);
        }

        RepositoryItemBase IRepositorySerializer.DeserializeFromFile(string fileName)
        {
            return (RepositoryItemBase)DeserializeFromFile(fileName);
        }

        public object DeserializeFromFileObj(Type type, string fileName)
        {
            return DeserializeFromFile(type, fileName);
        }

        public string FileExt(Type T)
        {
            // making it old Ginger style so BF and env can load again... temp
            return "Ginger." + GetShortType(T);
        }

        public object DeserializeFromText(Type t, string s, string filePath = "")
        {
            return DeserializeFromTextWithTargetObj(t, s);
        }

        public string GetShortType(Type t)
        {
            //Not so much nice clean OO design but due to static on derived limitation, meanwhile it is working solution 
            // Put here only class which are saved as stand alone to file system
            // TODO: add interface to classes which are saved as files which will force them to im

            //TODO: more safe to use type of then Full Name - fix it!
            if (t == typeof(BusinessFlow)) { return "BusinessFlow"; }
            if (t == typeof(ActivitiesGroup)) { return "ActivitiesGroup"; }
            if (t == typeof(Activity)) { return "Activity"; }
            if (t == typeof(ErrorHandler)) { return "Activity"; }
            if (typeof(Act).IsAssignableFrom(t)) { return "Action"; }
            if (typeof(VariableBase).IsAssignableFrom(t)) { return "Variable"; }
            if (typeof(DataSourceBase).IsAssignableFrom(t)) return "DataSource";
            if (t.FullName == "GingerCore.Agent") return "Agent";
            if (t.FullName == "Ginger.Run.RunSetConfig") return "RunSetConfig";
            if (t.FullName == "Ginger.Run.BusinessFlowExecutionSummary") return "BusinessFlowExecutionSummary";
            if (t.FullName == "Ginger.Reports.ReportTemplate") return "ReportTemplate";
            if (t.FullName == "Ginger.Reports.HTMLReportTemplate") return "HTMLReportTemplate";
            if (t.FullName == "Ginger.Reports.HTMLReportConfiguration") return "HTMLReportConfiguration";
            if (t.FullName == "Amdocs.Ginger.Repository.ALMDefectProfile") return "ALMDefectProfile";
            if (t.FullName == "Ginger.Reports.HTMLReportConfiguration") return "HTMLReportConfiguration";
            if (t.FullName == "Ginger.TagsLib.RepositoryItemTag") return "RepsotirotyItemTag";
            if (t.Name == "PluginPackage") return "PluginPackage";




            // Make sure we must impl or get exception
            throw new Exception("Unknown Type for Short Type Name " + t.Name);
        }

        public static object NewRepositorySerializer_NewRepositorySerializerEvent(NewRepositorySerializerEventArgs EventArgs)
        {
            switch (EventArgs.EventType)
            {
                case NewRepositorySerializerEventArgs.eEventType.LoadWithOldSerilizerRequired:
                    Reporter.ToLog(eLogLevel.DEBUG, string.Format("New Serializer is calling Old Serializer for loading the file: '{0}'", EventArgs.FilePath));
                    return DeserializeFromText(EventArgs.XML, EventArgs.TargetObj);
            }

            return null;
        }
    }
}
