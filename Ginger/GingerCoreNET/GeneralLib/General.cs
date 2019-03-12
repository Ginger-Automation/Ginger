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
using GingerCore;
using GingerCoreNET.GeneralLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security;
using System.Text.RegularExpressions;
using System.Xml;

namespace GingerCoreNET.GeneralLib
{
    public class General
    {
        public static long GetGingerVersionAsLong(string gingerVersionAsString)
        {
            try
            {
                int iMajor = 0, iMinor = 0, iBuild = 0, iRevision = 0;
                Regex regex = new Regex(@"(\d+)\.(\d+)\.(\d+)\.(\d+)");
                Match match = regex.Match(gingerVersionAsString);
                if (match.Success)
                {
                    try { iMajor = Int32.Parse(match.Groups[1].Value); }
                    catch (Exception) { }
                    try { iMinor = Int32.Parse(match.Groups[2].Value); }
                    catch (Exception) { }
                    try { iBuild = Int32.Parse(match.Groups[3].Value); }
                    catch (Exception) { }
                    try { iRevision = Int32.Parse(match.Groups[4].Value); }
                    catch (Exception) { }
                }
                else
                {
                    return 0;//failed to get the version as long
                }

                long version = iMajor * 1000000 + iMinor * 10000 + iBuild * 100 + iRevision;
                return version;
            }
            catch (Exception)
            {
                return 0;//failed to get the version as long
            }
        }
        
        #region ENUM

        public static List<string> GetEnumValues(Type EnumType)
        {
            List<string> l = new List<string>();
            foreach (object item in Enum.GetValues(EnumType))
            {          
                l.Add(GetEnumValueDescription(EnumType, item));
            }
            return l;
        }

        public static string GetEnumValueDescription(Type EnumType, object EnumValue)
        {
            try
            {
                EnumValueDescriptionAttribute[] attributes = (EnumValueDescriptionAttribute[])EnumType.GetField(EnumValue.ToString()).GetCustomAttributes(typeof(EnumValueDescriptionAttribute), false);
                string s;
                if (attributes.Length > 0)
                {
                    s = attributes[0].ValueDescription;
                    //for supporting multi Terminology types
                    s = s.Replace("Business Flow", GingerDicser.GetTermResValue(eTermResKey.BusinessFlow));
                    s = s.Replace("Business Flows", GingerDicser.GetTermResValue(eTermResKey.BusinessFlows));
                    s = s.Replace("Activities Group", GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup));
                    s = s.Replace("Activities Groups", GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroups));
                    s = s.Replace("Activity", GingerDicser.GetTermResValue(eTermResKey.Activity));
                    s = s.Replace("Conversion Mechanism", GingerDicser.GetTermResValue(eTermResKey.ConversionMechanism));
                    s = s.Replace("Activities", GingerDicser.GetTermResValue(eTermResKey.Activities));
                    s = s.Replace("Variable", GingerDicser.GetTermResValue(eTermResKey.Variable));
                    s = s.Replace("Variables", GingerDicser.GetTermResValue(eTermResKey.Variables));
                    s = s.Replace("Run Set", GingerDicser.GetTermResValue(eTermResKey.RunSet));
                    s = s.Replace("Run Sets", GingerDicser.GetTermResValue(eTermResKey.RunSets));
                }
                else
                {
                    s = EnumValue.ToString();
                }
                return s;
            }
            catch
            {
                //TODO: fixme ugly catch - check why we come here and fix it, todo later
                return EnumValue.ToString();
            }
        }

        // TODO: move to sperate class
        public class ComboEnumItem
        {

            public static class Fields
            {
                public static string text = "text";
                public static string Value = "Value";
            }

            public override String ToString()
            {
                return text;
            }


            public string text { get; set; }
            public object Value { get; set; }
        }

        public class XmlNodeItem
        {
            public XmlNodeItem(string p, string v, string xp)
            {
                param = p;
                value = v;
                path = xp;
            }

            public static class Fields
            {
                public static string param = "param";
                public static string value = "value";
                public static string path = "path";
            }

            public override String ToString()
            {
                return "Param:" + param + Environment.NewLine + "value:" + value + Environment.NewLine + "path:" + path;
            }

            public string param { get; set; }
            public string value { get; set; }
            public string path { get; set; }
        }

        public class ComboItem
        {

            public static class Fields
            {
                public static string text = "text";
                public static string Value = "Value";
            }

            public override String ToString()
            {
                return text;
            }


            public string text { get; set; }
            public object Value { get; set; }
        }

        #endregion ENUM


        #region Binding
        #endregion Binding
        
        public static T ParseEnum<T>(string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }
        
        public static bool IsNumeric(string sValue)
        {
            // simple method to check is strign is number
            // there are many other alternatives, just keep it simple and make sure it run fast as it is going to be used a lot, for every return value calc     
            
            foreach (char c in sValue)
            {
                if (!char.IsDigit(c) && c != '.')
                {
                    return false;
                }
            }
            return true;
        }

        public static List<General.ComboEnumItem> GetEnumValuesForCombo(Type Etype)
        {
            List<General.ComboEnumItem> list = new List<General.ComboEnumItem>();
            foreach (object item in Enum.GetValues(Etype))
            {
                General.ComboEnumItem CEI = new General.ComboEnumItem();
                CEI.text = General.GetEnumValueDescription(Etype, item);
                CEI.Value = item;

                list.Add(CEI);
            }

            return list;

        }

        public static List<XmlNodeItem> GetXMLNodesItems(XmlDocument xmlDoc)
        {
            List<XmlNodeItem> returnDict = new List<XmlNodeItem>();
            XmlReader rdr1 = XmlReader.Create(new System.IO.StringReader(xmlDoc.InnerXml));
            XmlReader rdr = XmlReader.Create(new System.IO.StringReader(xmlDoc.InnerXml));
            XmlReader subrdr = null;
            string Elm = "";
            
            ArrayList ls = new ArrayList();
            Dictionary<string, int> lspath = new Dictionary<string, int>();
            List<string> DeParams = new List<string>();
            while (rdr.Read())
            {
                if (rdr.NodeType == XmlNodeType.Element)
                {
                    Elm = rdr.Name;
                    if (ls.Count <= rdr.Depth)
                        ls.Add(Elm);
                    else
                        ls[rdr.Depth] = Elm;
                    foreach (var p in DeParams)
                    {
                        if (p == rdr.Name)
                        {
                            subrdr = rdr.ReadSubtree();
                            subrdr.ReadToFollowing(p);
                            returnDict.Add(new XmlNodeItem("AllDescOf" + p, subrdr.ReadInnerXml(), "/" + string.Join("/", ls.ToArray().Take(rdr.Depth))));
                            subrdr = null;
                        }
                    }
                }

                if (rdr.NodeType == XmlNodeType.Text)
                {
                    // soup req contains sub xml, so parse them 
                    if (rdr.Value.StartsWith("<?xml"))
                    {
                        XmlDocument xmlnDoc = new XmlDocument();
                        xmlnDoc.LoadXml(rdr.Value);
                        GetXMLNodesItems(xmlnDoc);
                    }
                    else
                    {
                        if (!lspath.Keys.Contains("/" + string.Join("/", ls.ToArray().Take(rdr.Depth)) + "/" + Elm))
                        {
                            returnDict.Add(new XmlNodeItem(Elm, rdr.Value, "/" + string.Join("/", ls.ToArray().Take(rdr.Depth))));
                            lspath.Add("/" + string.Join("/", ls.ToArray().Take(rdr.Depth)) + "/" + Elm, 0);
                        }
                        else if (lspath.Keys.Contains("/" + string.Join("/", ls.ToArray().Take(rdr.Depth)) + "/" + Elm))
                        {
                            returnDict.Add(new XmlNodeItem(Elm + "_" + lspath["/" + string.Join("/", ls.ToArray().Take(rdr.Depth)) + "/" + Elm], rdr.Value, "/" + string.Join("/", ls.ToArray().Take(rdr.Depth))));
                            lspath["/" + string.Join("/", ls.ToArray().Take(rdr.Depth)) + "/" + Elm]++;
                        }
                    }
                }
            }
            return returnDict;
        }
        
        //HTML Report related methods added here 
        public static string TimeConvert(string s)
        {
            double seconds = Convert.ToDouble(s);
            TimeSpan ts = TimeSpan.FromSeconds(seconds);
            return ts.ToString(@"hh\:mm\:ss");
        }

        public static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = System.IO.Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = System.IO.Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }

        /// <summary>
        /// Replaces invalid XML characters in a string with their valid XML equivalent.
        /// </summary>
        /// <param name="str">The string within which to escape invalid characters.</param>
        /// <returns>The input string with invalid characters replaced.</returns>
        public static string ConvertInvalidXMLCharacters(string str)
        {
            return SecurityElement.Escape(str);
        }
        public static bool isDesignMode()
        {
            //TODO: move this func to General
            bool designMode = (LicenseManager.UsageMode == LicenseUsageMode.Designtime);
            return designMode;
        }

        public static ObservableList<T> ConvertListToObservableList<T>(List<T> List)
        {
            ObservableList<T> ObservableList = new ObservableList<T>();
            foreach (T o in List)
                ObservableList.Add(o);
            return ObservableList;
        }
    }
}


