#region License
/*
Copyright © 2014-2018 European Support Limited

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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using Amdocs.Ginger.Common;

namespace Amdocs.Ginger.Repository
{

    // This class is for storing RepositoryItem on disk, it needs to be serialized to XML
    // reason for not using some of the exisiting options:
    // Binary - makes it diffcult to compare version/history in CC + some say it is slower!?
    // XML formatter - the default have some chllenges and with some NG objects    
    // Pros - With our own seriliaztion we can solve the problem of copy vs link of Action, during load/save we can take the items from repo
    // We can have several style of serialization - 1 store to repo - not all attrs are save, 2 store local save most attrs
    // It should work faster - to be tested and optimized
    // + We can keep backword compatibiity much easier
    // + It solve the copy/link to other repo item during serailzation/de-serailzation
    // It will also solve problems with older agents - no need to update all agents, since sending xml and parsing with defaults
    // we can also decide on ad hoc serialzation based on the target: if we send it to agent, save to disk or other
    // We can also decide on ignore error and get partial object - to be fixed- but maybe better than nothing
    // We cam also read partial files - i.e: if we just need the Business flow name for list, no need to read all file
    // We can add custom attr at the top

    public class NewRepositorySerializer : IRepositorySerializer
    {
       

        private const string cGingerRepositoryItem = "GingerRepositoryItem";
        private const string cGingerRepositoryItemHeader = "Header";
        private const string cHeaderGingerVersion= "GingerVersion";

        // We keep year/month/day hour/minutes - removed seconds and millis
        private const string cDateTimeXMLFormat = "yyyyMMddHHmm";

        public delegate object NewRepositorySerializerEventHandler(NewRepositorySerilizerEventArgs EventArgs);
        public static event NewRepositorySerializerEventHandler NewRepositorySerializerEvent;        
        public static object OnNewRepositorySerializerEvent(NewRepositorySerilizerEventArgs.eEventType EvType, string FilePath, string XML, RepositoryItemBase TargetObj)
        {
            NewRepositorySerializerEventHandler handler = NewRepositorySerializerEvent;
            if (handler != null)
            {
               return handler(new NewRepositorySerilizerEventArgs(EvType, FilePath, XML, TargetObj));
            }

            return null;
        }
        public  void SaveToFile(RepositoryItemBase ri, string FileName)
        {
            string txt = SerializeToString(ri);
            File.WriteAllText(FileName, txt);
        }

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

                        // We serialize only the top item and add header to it
                        if (ri.RepositoryItemHeader == null)
                        {
                            ri.InitHeader();
                        }
                        else
                        {
                            ri.UpdateHeader();
                        }

                        // Header
                        xml.WriteStartElement(cGingerRepositoryItem);                        

                        // Write the object data
                        xmlwriteHeader(xml, ri);
                        xml.WriteWhitespace("\n");

                        xmlwriteObject(xml, ri);
                        xml.WriteEndElement();
                        xml.WriteEndDocument();
                    }
                    string result = Encoding.UTF8.GetString(output.ToArray());
                    return result;
                }
            }
            else
                return string.Empty;
        }

      



        private void xmlwriteHeader(XmlTextWriter xml, RepositoryItemBase repositoryItem)
        {
            // Since Header is simple object and unique, we write the attrs in the order we want            
            xml.WriteStartElement(cGingerRepositoryItemHeader);

            if (repositoryItem.RepositoryItemHeader.ItemType == null)
            {
                repositoryItem.RepositoryItemHeader.ItemType = repositoryItem.GetType().Name;
            }

            repositoryItem.RepositoryItemHeader.ItemGuid = repositoryItem.Guid;

            xml.WriteAttributeString(nameof(RepositoryItemHeader.ItemGuid), repositoryItem.RepositoryItemHeader.ItemGuid.ToString());
            xml.WriteAttributeString(nameof(RepositoryItemHeader.ItemType), repositoryItem.RepositoryItemHeader.ItemType);
            xml.WriteAttributeString(nameof(RepositoryItemHeader.CreatedBy), repositoryItem.RepositoryItemHeader.CreatedBy);
            xml.WriteAttributeString(nameof(RepositoryItemHeader.Created), repositoryItem.RepositoryItemHeader.Created.ToString(cDateTimeXMLFormat));

            xml.WriteAttributeString(nameof(RepositoryItemHeader.GingerVersion), repositoryItem.RepositoryItemHeader.GingerVersion.ToString());
            string ver = repositoryItem.RepositoryItemHeader.Version.ToString();
            xml.WriteAttributeString(nameof(RepositoryItemHeader.Version), ver);


            //Why not always?
            if (repositoryItem.RepositoryItemHeader.LastUpdateBy == null)
            {
                repositoryItem.RepositoryItemHeader.LastUpdateBy = Environment.UserName;
            }
            xml.WriteAttributeString("LastUpdateBy", repositoryItem.RepositoryItemHeader.LastUpdateBy.ToString());

            xml.WriteAttributeString(nameof(RepositoryItemHeader.LastUpdate), repositoryItem.RepositoryItemHeader.LastUpdate.ToString(cDateTimeXMLFormat));

            xml.WriteEndElement();
        }

        



        //TODO: later on get back this function it is more organize, but causing saving problems  -to be fixed later
        private void xmlwriteObject(XmlTextWriter xml, RepositoryItemBase ri)
        {           
            string ClassName = ri.GetType().Name;
            xml.WriteStartElement(ClassName);

            WriteRepoItemAttrs(xml, ri);            
            xml.WriteEndElement();
        }

        class RIAttr
        {
            public string Name;
            public Type ttt;
            public object value;
            public IsSerializedForLocalRepositoryAttribute attrIS;
        }

        private void WriteRepoItemAttrs(XmlTextWriter xml, RepositoryItemBase ri)
        {
            //TODO: cache class how to serialzie so will work faster and use reflection sort etc... only for first time

            // Get all serialized attrs (properties and fields)            
            var attrs = ri.GetType().GetMembers().OrderBy(x => x.Name);         // Order by name so XML compare will be easier  

            List<RIAttr> SimpleAttrs = new List<RIAttr>();
            List<RIAttr> ListAttrs = new List<RIAttr>();
            // order by attrs with simple prop first then lists latest 

            foreach (MemberInfo mi in attrs)  
            {
                IsSerializedForLocalRepositoryAttribute isSerialziedAttr = (IsSerializedForLocalRepositoryAttribute)mi.GetCustomAttribute(typeof(IsSerializedForLocalRepositoryAttribute));
                if (isSerialziedAttr != null)
                {
                    Type tt;
                    object value;
                    if (mi.MemberType == MemberTypes.Property)
                    {
                        tt = ((PropertyInfo)mi).PropertyType;
                        value = ri.GetType().GetProperty(mi.Name).GetValue(ri);
                    }
                    else
                    {
                        tt = ((FieldInfo)mi).FieldType;
                        value = ri.GetType().GetField(mi.Name).GetValue(ri);
                    }


                    RIAttr rIAttr = new RIAttr() { Name = mi.Name, ttt = tt, value = value, attrIS = isSerialziedAttr };
                    if (value is IObservableList || value is List<string> || value is RepositoryItemBase) 
                    {
                        
                        ListAttrs.Add(rIAttr);
                    }
                    else
                    {
                        SimpleAttrs.Add(rIAttr);
                        
                    }
                }
            }

            // Write simple attr: string, int etc.
            foreach (RIAttr mi in SimpleAttrs)  
            {                                                
                WriteRepoItemAttr(xml, mi);                
            }

            // Write list
            foreach (RIAttr mi in ListAttrs)
            {
                WriteRepoItemAttr(xml, mi);
            }
        }

        private void WriteRepoItemAttr(XmlTextWriter xml, RIAttr rIAttr)
        {

            //if (LazyLoadAttr.Contains(rIAttr.Name))
            //{
            //    bool b;
            //    if (mi.MemberType == MemberTypes.Field)
            //    {
            //        b = ((IObservableList)(ri.GetType().GetField(mi.Name).GetValue(ri))).LazyLoad;
            //    }
            //    else
            //    {
            //        b = ((IObservableList)(ri.GetType().GetProperty(mi.Name).GetValue(ri))).LazyLoad;
            //    }

            //    if (b)
            //    {
            //        // Hurray! we use the cache ready made string
            //        string s = ((IObservableList)(ri.GetType().GetField(fi.Name).GetValue(ri))).StringData;
            //        xml.WriteStartElement(mi.Name);
            //        xml.WriteString(s);
            //        xml.WriteEndElement();
                //}
            //}

            // We write the property value only if it is not null and different than default when serialized
            if (rIAttr.value == null)
            {
                return;
            }
            if (IsValueDefault(rIAttr.value, rIAttr.attrIS))
            {
                return;
            }


            if (rIAttr.value is IObservableList)
            {
                IObservableList vv = (IObservableList)rIAttr.value;
                if (vv.Count != 0)  // Write only if we have items - save xml space
                {
                    xmlwriteObservableList(xml, rIAttr.Name, (IObservableList)rIAttr.value);
                }
            }
            else if(rIAttr.value is List<string>)
            {
                    xmlwriteStringList(xml, rIAttr.Name, (List<string>)rIAttr.value);
            }
            else if (rIAttr.value is RepositoryItemBase)
            {
                xmlwriteSingleObjectField(xml, rIAttr.Name, rIAttr.value);
            }
            else
            {
                if (rIAttr.value != null)
                {                     
                    xmlwriteatrr(xml, rIAttr.Name, rIAttr.value.ToString());                     
                }                    
            }
            
        }

     
        private bool IsValueDefault(object attrValue, IsSerializedForLocalRepositoryAttribute IsSerializedForLocalRepository)
        {
            object attrDefaultValue = IsSerializedForLocalRepository.GetDefualtValue();
            if (attrDefaultValue == null)
            {
                // DefaultValue annotation not exist on attr
                if (attrValue is bool)   // in case of bool do not write False as it is the default
                {
                    if ((bool)attrValue == false)
                    {
                        return true;
                    }
                }

                if (attrValue is string)
                {
                    if ((string)attrValue == string.Empty)
                    {
                        return true;
                    }
                }

                if (attrValue is int)
                {
                    if ((int)attrValue == 0)
                    {
                        return true;
                    }
                }

                return false;  
            }
                
            if (attrValue.Equals(attrDefaultValue))
            {
                return true;
            }
            else
            {
                return false;
            }
            //object def = GetDefault(v.GetType());
            //if (def != null)
            //{
            //    // if (((object)v) == def) return true;  // NOT WORKING
            //    string s1 = ((object)v).ToString();
            //    if (string.IsNullOrEmpty(s1)) return true;
            //    string s2 = def.ToString();
            //    if (s1 == s2) return true;
            //}
            //return false;
        }

        //TODO: Make it class.attr
        static List<string> LazyLoadAttr = new List<string>();

        public static void AddLazyLoadAttr(string name)
        {
            LazyLoadAttr.Add(name);
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
                else if (v is RepositoryItemKey)
                {
                    xml.WriteElementString(v.GetType().Name, v.ToString());
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
            xml.WriteStartAttribute(Name); //Attribute 'Name'
            xml.WriteString(Value); //Attribute 'Value'
            xml.WriteEndAttribute();
        }

        public object DeserializeFromFileObj(Type t, string FileName)
        {
            object o = DeserializeFromFile(t, FileName);
            return o;
        }
        

        public RepositoryItemBase DeserializeFromFile(Type t, string FileName)
        {
            AppReporter.ToConsole("DeserializeFromFile the file: " + FileName);
            
            if (FileName.Length > 0 && File.Exists(FileName))
            {
                string xml = File.ReadAllText(FileName);

                // first check if we need to auto upgrade the xml to latest ginger version
                //long XMLVersion = RepositorySerializer.GetXMLVersionAsLong(xml);
                //long GingerVersion = GetCurrentVersionAsLong();
                //GingerCore.XMLConverters.XMLConverterBase.eGingerFileType FileType =  XMLConverterBase.GetGingerFileTypeFromFilename(FileName);
                //if (GingerVersion > XMLVersion)
                //{
                //    xml = XMLUpgrade.Convert(FileType, xml, XMLVersion);
                //    File.WriteAllText(FileName, xml);
                //}    

                // CP ERROR Commented
                // XMLUpgrade.UpgradeXMLIfNeeded(xml, FileName);

                return DeserializeFromText(xml, filePath: FileName);
            }
            else
            {
                throw new Exception("File Not Found - " + FileName);
            }
        }


        public static void DeserializeObservableListFromText<T>(ObservableList<T> lst, string xml)
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


        public RepositoryItemBase DeserializeFromFile(string FileName)
        {

            if (FileName.Length > 0 && File.Exists(FileName))
            {
                string xml = File.ReadAllText(FileName);

                // first check if we need to auto upgrade the xml to latest ginger version
                //long XMLVersion = RepositorySerializer.GetXMLVersionAsLong(xml);
                //long GingerVersion = GetCurrentVersionAsLong();
                //GingerCore.XMLConverters.XMLConverterBase.eGingerFileType FileType = XMLConverterBase.GetGingerFileTypeFromFilename(FileName);
                //if (GingerVersion > XMLVersion)
                //{  
                //    xml = XMLUpgrade.Convert(FileType, xml, XMLVersion);
                //    File.WriteAllText(FileName, xml);
                //}


                // CP ERROR Commented
                // XMLUpgrade.UpgradeXMLIfNeeded(xml, FileName);

                return DeserializeFromText(xml, filePath: FileName);
            }
            else
            {
                throw new Exception("File Not Found - " + FileName);
            }
        }



        public static RepositoryItemBase DeserializeFromText(string xml, RepositoryItemBase targetObj = null, string filePath = "")
        {
            string encoding = "utf-8"; // make it static or remove string creation
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
            object RootObj;
            if (xdr.Name == cGingerRepositoryItem)
            {
                // New style with header
                xdr.Read();  // Now we are in the header

                RepositoryItemHeader RIH = new RepositoryItemHeader();
                xdr.MoveToFirstAttribute();
                for (int i = 0; i < xdr.AttributeCount; i++)
                {
                    SetRepositoryItemHeaderAttr(RIH, xdr.Name, xdr.Value);
                    xdr.MoveToNextAttribute();
                }

                // After we are done reading the RI header attrs we moved to the main object
                xdr.Read();

                RootObj = xmlReadObject(null, xdr, targetObj);
                ((RepositoryItemBase)RootObj).RepositoryItemHeader = RIH;
            }
            else
            {
                //Item saved by old Serialzier so calling it to load the XML 
                return (RepositoryItemBase)OnNewRepositorySerializerEvent(NewRepositorySerilizerEventArgs.eEventType.LoadWithOldSerilizerRequired, filePath, xml, targetObj);
            }

            return (RepositoryItemBase)RootObj;
        }

     

        private static void SetRepositoryItemHeaderAttr(RepositoryItemHeader RIH, string name, string value)
        {
            if (name == nameof(RepositoryItemHeader.ItemType)) { RIH.ItemType = value; return; }
            if (name == nameof(RepositoryItemHeader.GingerVersion)) { RIH.GingerVersion = value; return; }
            if (name == nameof(RepositoryItemHeader.CreatedBy)) { RIH.CreatedBy = value; return; }
            if (name == nameof(RepositoryItemHeader.Created)) { RIH.Created = DateTime.ParseExact(value, cDateTimeXMLFormat, CultureInfo.InvariantCulture); return; }
            if (name == nameof(RepositoryItemHeader.Version)) { RIH.Version = int.Parse(value); return; }
            if (name == nameof(RepositoryItemHeader.LastUpdateBy)) { RIH.LastUpdateBy = value; return; }
            if (name == nameof(RepositoryItemHeader.LastUpdate)) { RIH.LastUpdate = DateTime.ParseExact(value, cDateTimeXMLFormat, CultureInfo.InvariantCulture); return; }
            if (name == nameof(RepositoryItemHeader.ItemGuid)) { RIH.ItemGuid = Guid.Parse(value); return; }

            throw new Exception("Unknown attribue in repository header: " + name);
        }

        private static void xmlReadListOfObjects(object ParentObj, XmlReader xdr, IObservableList observableList)
        {
            // read list of object into the list, add one by one, like activities, actions etc.

            //TODO: Think/check if we want to make all observ as lazy load
            if (LazyLoadAttr.Contains(xdr.Name))
            // if (FastLoad) // && xdr.Name == nameof(BusinessFlow.Activities) || xdr.Name != nameof(Activity.Acts))
            {
                // We can save line/col and reload later when needed

                string s = xdr.ReadOuterXml(); // .ReadInnerXml(); // .Read();
                observableList.DoLazyLoadItem(s);

                return;
            }

            if (observableList.GetType() == typeof(ObservableList<RepositoryItemKey>))
            {
                xdr.Read();
                while (xdr.NodeType != XmlNodeType.EndElement)
                {
                    string RIKey = xdr.ReadElementContentAsString();
                    
                    if (RIKey != null)
                    {
                        RepositoryItemKey repositoryItemKey = new RepositoryItemKey();
                        repositoryItemKey.Key = RIKey;                        
                        observableList.Add(repositoryItemKey);
                    }
                    else
                    {
                        return;
                    }

                }
                xdr.ReadEndElement();
            }
            else
            {

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
        }


        //TODO: remove from here get in init

        //TODO: move to common global for others to use if needed 
        // public static Assembly GingerCoreNETAssembly = typeof(GingerCoreNET.SolutionRepositoryLib.RepositoryItem).Assembly;
        public static Assembly GingerCoreCommonAssembly = typeof(RepositoryItemBase).Assembly;        

        private static object xmlReadObject(Object Parent, XmlReader xdr, RepositoryItemBase targetObj = null)
        {
            string className = xdr.Name;            

            try
            {
                
                int level = xdr.Depth;

                object obj;

                if (targetObj == null)
                {
                    obj = CreateObject(className);
                }
                else
                {
                    obj = targetObj;
                }

                SetObjectSerialziedAttrDefaultValue(obj);
                SetObjectAttributes(xdr, obj);

                xdr.Read();
                // Set lists attrs
                // read all object sub elements like lists - obj members              
                while (xdr.Depth == level + 1)
                {
                    // Check if it one obj attr or list
                    string attrName = xdr.Name;                    

                    MemberInfo mi = obj.GetType().GetMember(attrName).SingleOrDefault();

                    if (mi==null)
                    {                        
                        throw new MissingFieldException("Error: Cannot find attrbiute. Class: " + className + ", Attribute: " + xdr.Name);
                    }

                    

                    // We check if it is list by arg count - List<string> will have string etc...
                    // another option is check the name to start with List, Observ...
                    //or find a better way
                    // meanwhile it is working


                    
                    if (mi.MemberType == MemberTypes.Property)
                    {
                        // check if this is kind of a list
                        if (((PropertyInfo)mi).PropertyType.GenericTypeArguments.Count() > 0)
                        // if (((PropertyInfo)mi).PropertyType is IObservableList)
                        {
                            SetObjectListAttrs(xdr, obj);
                        }
                        else
                        {
                            xdr.ReadStartElement();
                            object item = xmlReadObject(obj, xdr);
                            xdr.ReadEndElement();
                            ((PropertyInfo)mi).SetValue(obj, item);
                            
                        }
                    }
                    else
                    {
                        if (((FieldInfo)mi).FieldType.GenericTypeArguments.Count() > 0)
                        {
                            SetObjectListAttrs(xdr, obj);
                        }
                        else
                        {
                            xdr.ReadStartElement();
                            object item = xmlReadObject(obj, xdr);
                            xdr.ReadEndElement();
                            ((FieldInfo)mi).SetValue(obj, item);                            
                        }
                    }

                    //Keep it here
                    if (xdr.NodeType == XmlNodeType.EndElement)
                    {
                        xdr.ReadEndElement();
                    }

                }

                //if (conversion)
                //{
                //    //for converting old actions keep the ID
                //    if (obj is DriverAction)
                //    {
                //        DriverAction DA = ((DriverAction)obj);
                //        DA.OldClassName = OldClassName;


                //        //temp moved from here to conversion class or function
                //        if (DA.OldClassName == "GingerCore.Actions.ActGotoURL")
                //        {
                //            DA.ID = "GotoURL";
                //            DA.InputValues[0].Param = "URL"; //convert param name 'Value' to 'URL'
                //        }

                //        if (DA.OldClassName == "GingerCore.Actions.ActTextBox")
                //        {
                //            DA.ID = "UIElementAction";
                //            string LocateBy = "ByID";
                //            string LocateValue = "UserName";  // temp !!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                //            string Action = "SetValue";
                //            string Value = (from x in DA.InputValues where x.Param == "Value" select x.Value).FirstOrDefault();
                //            DA.InputValues.Clear();
                //            DA.InputValues.Add(new ActInputValue() { Param = "LocateBy", Value = LocateBy });
                //            DA.InputValues.Add(new ActInputValue() { Param = "LocateValue", Value = LocateValue });
                //            DA.InputValues.Add(new ActInputValue() { Param = "Action", Value = Action });
                //            DA.InputValues.Add(new ActInputValue() { Param = "Value", Value = Value });

                //        }

                //    }
                //}


                return obj;
            }
            catch (Exception ex)
            {               
                throw new Exception("Error: Cannot create instance of: " + className + ", for attribute: " + xdr.Name + " - " + ex.Message);
            }
        }

        
        private static object CreateObject(string name)
        {
            if (mClassDictionary.Count == 0)
            {
                throw new Exception("NewRepositorySerializer: Unable to create class object - " + name + " + bacause mClassDictionary was not initilized" );
            }

            object obj;
            Type t;
            //             bool b = mClassDictionary.TryGetValue(name, out t);
            bool b = mClassDictionary.TryGetValue(name, out t);
            if (b)
            {
                obj = t.Assembly.CreateInstance(t.FullName);                
                return obj;
            }
            
            throw new Exception("NewRepositorySerializer: Unable to create class object - " + name);

        }

        private static void SetObjectSerialziedAttrDefaultValue(object obj)
        {
            // TODO: check if we can combine the below into one faster func
            try
            {
                var properties = obj.GetType().GetMembers().Where(x => x.MemberType == MemberTypes.Property);
                foreach (MemberInfo mi in properties)
                {
                    IsSerializedForLocalRepositoryAttribute token = Attribute.GetCustomAttribute(mi, typeof(IsSerializedForLocalRepositoryAttribute), false) as IsSerializedForLocalRepositoryAttribute;
                    if (token != null)
                    {
                        if (token.GetDefualtValue() != null)
                        {
                            obj.GetType().GetProperty(mi.Name).SetValue(obj, token.GetDefualtValue());
                        }
                    }
                }

                var fields = obj.GetType().GetMembers().Where(x => x.MemberType == MemberTypes.Field);
                foreach (MemberInfo mi in fields)
                {
                    IsSerializedForLocalRepositoryAttribute token = Attribute.GetCustomAttribute(mi, typeof(IsSerializedForLocalRepositoryAttribute), false) as IsSerializedForLocalRepositoryAttribute;
                    if (token != null)
                    {
                        if (token.GetDefualtValue() != null)
                        {
                            obj.GetType().GetField(mi.Name).SetValue(obj, token.GetDefualtValue());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to set default value of serialized Repository Item - " + ex.Message);
            }
        }

        static Dictionary<string, Type> mClassDictionary = new Dictionary<string, Type>();


        public static void AddClassesFromAssembly(Assembly a)
        {            
            var RepositoryItemTypes =              
              from type in a.GetTypes()
                  //where type.IsSubclassOf(typeof(RepositoryItemBase))              
              where typeof(RepositoryItemBase).IsAssignableFrom(type) // Will load all sub classes including level 2,3 etc.
              select type;

            foreach (Type t in RepositoryItemTypes)
            {
                mClassDictionary.Add(t.Name, t);
                mClassDictionary.Add(t.FullName, t);
            }
        }

        public static void AddClasses(Dictionary<string, Type> list)
        {
            mClassDictionary = mClassDictionary.Concat(list).ToDictionary(pair => pair.Key, pair => pair.Value);
        }


        

        /// <summary>
        /// Convert short name to long full name - GingerCoreNET
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private static string GetFullClassName(string className)
        {
            // TODO: use dictinary or something smarter - check perf 
            int i = className.LastIndexOf(".");
            if (i > 0)
            {
                className = className.Substring(i + 1);
            }

            Type t;
            bool b = mClassDictionary.TryGetValue(className, out t);
            if (b)
            { 
                return t.FullName;
            }
            else
            {
                return null;
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

                    Type elementType;
                    object value;
                    if (mi.MemberType == MemberTypes.Property)
                    {
                        // generate same type empty list objects
                        elementType = ((PropertyInfo)mi).PropertyType.GenericTypeArguments[0];
                        value = ((PropertyInfo)mi).GetValue(obj);
                    }
                    else
                    {
                        elementType = ((FieldInfo)mi).FieldType.GenericTypeArguments[0];
                        value = ((FieldInfo)mi).GetValue(obj);
                    }


                    if (elementType == typeof(string))
                    {
                        List<string> lsts = (List<string>)value;
                        xmlReadListOfStrings(xdr, lsts);
                        //fi.SetValue(obj, lsts);
                    }
                    else if (elementType == typeof(Guid))
                    {
                        ObservableList<Guid> lstsg = (ObservableList<Guid>)value;
                        xmlReadListOfGuids(xdr, lstsg);
                    }
                    else
                    {
                        //TODO: handle other types of list, meanwhile Assume observb list
                        IObservableList lst = (IObservableList)Activator.CreateInstance((typeof(ObservableList<>).MakeGenericType(elementType)));
                        //assign it to the relevant obj

                        if (mi.MemberType == MemberTypes.Property)
                        {
                            ((PropertyInfo)mi).SetValue(obj, lst);                            
                        }
                        else
                        {
                            ((FieldInfo)mi).SetValue(obj, lst);                            
                        }
                        
                        // Read the list from the xml
                        xmlReadListOfObjects(obj, xdr, lst);
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
                            if (xdr.Name == "Created" || xdr.Name == "CreatedBy" || xdr.Name == "LastUpdate" || xdr.Name == "LastUpdateBy" || xdr.Name == "Version" || xdr.Name == "ExternalID")
                            {
                                // Ignore common attr on RI which were removed in GingerCoreNET
                            }
                            else
                            {
                                AppReporter.ToLog(eAppReporterLogLevel.WARN, "Property not Found: " + xdr.Name);
                            }
                            xdr.MoveToNextAttribute();
                            continue;
                        }
                        string Value = xdr.Value;                                          
                        if (Value != "Null")
                        {
                            if (propertyInfo.CanWrite)
                            {

                                SetObjAttrValue(obj, propertyInfo, Value);
                            }
                            else
                            {
                                // this is for case like Activity.PercentAutomation - we had it serialzed but set was removed, we can ignore
                                // Ignore 
                            }
                        }
                        xdr.MoveToNextAttribute();
                    }

                }
                xdr.MoveToNextAttribute();
            }
            catch (Exception ex)
            {
                AppReporter.ToLog(eAppReporterLogLevel.ERROR, "NewRepositorySerilizer- Error when setting Property: " + xdr.Name, ex);
                throw ex;
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
                        else if (propertyInfo.PropertyType == typeof(RepositoryItemKey))
                        {
                            RepositoryItemKey repositoryItemKey = new RepositoryItemKey();
                            repositoryItemKey.Key = sValue;
                            propertyInfo.SetValue(obj, repositoryItemKey);
                            
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
                                // handle long?   = int64 nullable  - used in elapsed 
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
                        throw new Exception("Serializer - Err set value, Unknow type - " + propertyInfo.PropertyType.ToString() + " Value: " + sValue);

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
                throw new Exception(err);
            }
        }

        public static void UpdateXMLGingerVersion(XDocument xmlDoc, string gingerVersionToSet)
        {
            var element = xmlDoc.Descendants(cGingerRepositoryItemHeader).SingleOrDefault();                
            element.Attribute(cHeaderGingerVersion).SetValue(gingerVersionToSet);
        }

        public static string GetXMLGingerVersion(string xml, string xmlFilePath)
        {
            try
            {
                /* expecting  XML to look like this: 
                * <Header ... GingerVersion="2.6.0.0" Version="0" .../>*/
                //int indx = xml.IndexOf("GingerVersion=");
                int indx = xml.IndexOf(cHeaderGingerVersion);
                string version = xml.Trim().Substring(indx + 14, 9);

                Regex regex = new Regex(@"(\d+)\.(\d+)\.(\d+)\.(\d+)");
                Match match = regex.Match(version);
                if (match.Success)
                {
                    //return match.Value;
                    //avoiding Beta + Alpha numbers because for now it is not supposed to be writen to XML's, only oficial release numbers
                    int counter = 0;
                    string ver = string.Empty;
                    for (int index = 0; index < match.Value.Length; index++)
                    {
                        if (match.Value[index] == '.')
                            counter++;
                        if (counter == 2)
                            return ver + ".0.0";
                        else
                            ver += match.Value[index];
                    }
                    return ver;//something wrong
                }
                else
                {
                    return null;//failed to get te XML version
                    //TODO: write to log
                }
            }
            catch (Exception)
            {
                return null;//failed to get te XML version
                //TODO: write to log
            }
        }



        //Prep if we want to switch enable JSON
        //public class JSonHelper
        //{
        //    static JsonSerializer mJsonSerializer = new JsonSerializer();
        //    public static void SaveObjToJSonFile(object obj, string FileName)
        //    {
        //        //TODO: for speed we can do it async on another thread...

        //        using (StreamWriter SW = new StreamWriter(FileName))
        //        using (JsonWriter writer = new JsonTextWriter(SW))
        //        {
        //            mJsonSerializer.Serialize(writer, obj);
        //        }
        //    }

        //    public static object LoadObjFromJSonFile(string FileName, Type t)
        //    {
        //        using (StreamReader SR = new StreamReader(FileName))
        //        using (JsonReader reader = new JsonTextReader(SR))
        //        {
        //            return mJsonSerializer.Deserialize(reader, t);
        //        }
        //    }
        //}


        //public void SaveToJSON(RepositoryItem ri, string FileName)
        //{
        //    JSonHelper.SaveObjToJSonFile(ri, FileName);
        //}

        //TODO enable to read highlights                
        void IRepositorySerializer.DeserializeObservableListFromText<T>(ObservableList<T> observableList, string s)
        {
            DeserializeObservableListFromText(observableList, s);
        }

        RepositoryItemBase IRepositorySerializer.DeserializeFromFile(Type type, string fileName)
        {
            return this.DeserializeFromFile(type, fileName);
        }

        RepositoryItemBase IRepositorySerializer.DeserializeFromFile(string fileName)
        {
            return this.DeserializeFromFile(fileName);
        }

        public object DeserializeFromFileObj<T>(Type type, string fileName)
        {
            throw new NotImplementedException();
        }

        public string FileExt(Type T)
        {

            return "Ginger." + T.Name;
        }

        public object DeserializeFromText(Type t, string s, string filePath= "")
        {
            return DeserializeFromText(s, filePath: filePath);
        }

        public string GetShortType(Type t)
        {
            return t.Name;
        }


        // We have Repository item but we want to reload from disk
        // Can happen if we modified the file on the file system 
        internal static void ReloadObjectFromFile(RepositoryItemBase repositoryItem)
        {
            string txt = File.ReadAllText(repositoryItem.FilePath);
            DeserializeFromText(txt, repositoryItem, filePath: repositoryItem.FilePath);                        
        }


        public object DeserializeFromTextWithTargetObj(Type t, string xml, RepositoryItemBase targetObj = null)
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
            // Skip the header
            xdr.Read();
            xdr.Read();            
            xdr.Read();
            xdr.Read();

            object RootObj = xmlReadObject(null, xdr, targetObj);

            return RootObj;
        }


    }


    //TODO: move to seperate file
    public class NewRepositorySerilizerEventArgs
    {
        public enum eEventType
        {
            LoadWithOldSerilizerRequired,            
        }

        public eEventType EventType;
        public string FilePath;
        public string XML;
        public RepositoryItemBase TargetObj;

        public NewRepositorySerilizerEventArgs(eEventType EventType, string FilePath, string XML, RepositoryItemBase TargetObj)
        {
            this.EventType = EventType;
            this.FilePath = FilePath;
            this.XML = XML;
            this.TargetObj = TargetObj;
        }
    }

    


}
