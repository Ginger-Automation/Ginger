#region License
/*
Copyright © 2014-2025 European Support Limited

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

using Amdocs.Ginger.Common.Repository.SolutionCategories;
using Amdocs.Ginger.CoreNET.Run.SolutionCategory;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Xml;
using GingerCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Amdocs.Ginger.Common.GeneralLib
{
    public static class General
    {
        static string mCommonApplicationDataFolderPath = null;
        public static string CommonApplicationDataFolderPath
        {
            get
            {
                if (mCommonApplicationDataFolderPath == null)
                {
                    try
                    {
                        // DoNotVerify so on Linux it will not return empty
                        string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData, Environment.SpecialFolderOption.Create);

                        folderPath = Path.Combine(folderPath, "amdocs", "Ginger");

                        if (!Directory.Exists(folderPath))
                        {
                            Directory.CreateDirectory(folderPath);
                        }
                        mCommonApplicationDataFolderPath = folderPath;
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.WARN, "Failed to get/create the Common users Ginger workspace folder path", ex);
                        mCommonApplicationDataFolderPath = string.Empty;
                    }
                }
                return mCommonApplicationDataFolderPath;
            }
        }

        static string mAppDataFolder = null;
        public static string LocalUserApplicationDataFolderPath
        {
            get
            {
                if (mAppDataFolder == null)
                {
                    try
                    {
                        // DoNotVerify so on Linux it will not return empty
                        string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.DoNotVerify);

                        if (!Directory.Exists(appDataFolder))  // on Linux it sometimes not exist like on Azure build
                        {
                            Directory.CreateDirectory(appDataFolder);
                        }

                        appDataFolder = Path.Combine(appDataFolder, "amdocs", "Ginger");

                        if (!Directory.Exists(appDataFolder))
                        {
                            Directory.CreateDirectory(appDataFolder);
                        }
                        mAppDataFolder = appDataFolder;
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.WARN, "Failed to get/create the user Ginger workspace folder path", ex);
                        mAppDataFolder = string.Empty;
                    }
                }
                return mAppDataFolder;
            }
        }

        public static string DefualtUserLocalWorkingFolder
        {
            get
            {
                string workingFolder = Path.Combine(LocalUserApplicationDataFolderPath, "WorkingFolder");

                if (!Directory.Exists(workingFolder))
                {
                    Directory.CreateDirectory(workingFolder);
                }

                return workingFolder;
            }
        }






        /// <summary>
        /// Should use the function temporary till solution will be implemented for VE fields search
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static bool IsFieldToAvoidInVeFieldSearch(string fieldName)
        {
            if (fieldName is "BackupDic" or "GetNameForFileName" or "FilePath" or "FileName" or
                "ObjFileExt" or "ItemNameField" or "ItemImageType" or "ItemName" or
                "RelativeFilePath" or
                "ObjFolderName" or "ContainingFolder" or "ContainingFolderFullPath" or
                "ActInputValues" or "ActReturnValues" or "ActFlowControls" or
                "ScreenShots" or
                "ListStringValue" or "ListDynamicValue" or "ValueExpression")
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        public static Tuple<int, int> RecalculatingSizeWithKeptRatio(Tuple<int, int> a, int boxWidth, int boxHight)
        {
            //calculate the ratio
            double dbl = a.Item1 / (double)a.Item2;
            if ((int)(boxHight * dbl) <= boxWidth)
            {
                return new Tuple<int, int>((int)(boxHight * dbl), boxHight);
            }
            else
            {
                return new Tuple<int, int>(boxWidth, (int)(boxWidth / dbl));
            }
        }

        public static Tuple<int, int> GetImageHeightWidth(string path)
        {
            Tuple<int, int> a;
            using (Stream stream = File.OpenRead(path))
            {
                using (System.Drawing.Image sourceImage = System.Drawing.Image.FromStream(stream, false, false))
                {
                    a = new Tuple<int, int>(sourceImage.Width, sourceImage.Height);
                }
            }
            return a;
        }


        public static Tuple<int, int> RecalculatingSizeWithKeptRatio(Image Img, int boxWidth, int boxHight)
        {


            //calculate the ratio
            double dbl = Img.Width / (double)Img.Height;
            if ((int)(boxHight * dbl) <= boxWidth)
            {
                return new Tuple<int, int>((int)(boxHight * dbl), boxHight);
            }
            else
            {
                return new Tuple<int, int>(boxWidth, (int)(boxWidth / dbl));
            }
        }


        public static string ImagetoBase64String(Image Img)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                // Convert Image to byte[]
                Img.Save(ms, ImageFormat.Bmp);
                byte[] imageBytes = ms.ToArray();

                // Convert byte[] to base 64 string
                string base64String = Convert.ToBase64String(imageBytes);
                return base64String;
            }
        }

        public static string BitmapToBase64(Bitmap bitmap)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                byte[] byteImage = ms.ToArray();
                return Convert.ToBase64String(byteImage); //Get Base64
            }
        }



        public static string TimeConvert(string s)
        {
            double mseconds = Convert.ToDouble(s) * 1000;
            TimeSpan ts = TimeSpan.FromMilliseconds(mseconds);
            return string.Concat(ts.Hours.ToString("00"), ":", ts.Minutes.ToString("00"), ":", ts.Seconds.ToString("00"), ".", ts.Milliseconds.ToString("00"));
        }

        public static Image Base64StringToImage(string v)
        {
            byte[] imageBytes = Convert.FromBase64String(v);
            MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length);

            // Convert byte[] to Image
            ms.Write(imageBytes, 0, imageBytes.Length);
            Image image = Image.FromStream(ms, true);

            return image;
        }
        public static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            // If the source directory does not exist, throw an exception.
            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            // If the destination directory does not exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the file contents of the directory to copy.
            FileInfo[] files = dir.GetFiles();

            foreach (FileInfo file in files)
            {
                // Create the path to the new copy of the file.
                string temppath = System.IO.Path.Combine(destDirName, file.Name);

                // Copy the file.
                file.CopyTo(temppath, true);
            }

            // If copySubDirs is true, copy the subdirectories.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    // Create the subdirectory.
                    string temppath = System.IO.Path.Combine(destDirName, subdir.Name);

                    // Copy the subdirectories.
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }

        public static void ClearDirectoryContent(string DirPath)
        {
            //clear directory
            System.IO.DirectoryInfo di = new DirectoryInfo(DirPath);
            foreach (FileInfo file in di.GetFiles())
                file.Delete();
            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                dir.Delete(true);
            }
        }

        public static string RemoveInvalidFileNameChars(string fileName)
        {
            if (!string.IsNullOrEmpty(fileName))
            {
                foreach (char invalidChar in Path.GetInvalidFileNameChars())
                {
                    fileName = fileName.Replace(invalidChar.ToString(), "");
                }
                fileName = fileName.Replace(@".", "");
                fileName = fileName.Replace(@"?", "");  // on Linux it is valid but we do not want it
                                                        // !!!!!!!!!!!!!!!!!
                                                        //TODO: add more chars remove - see https://blog.josephscott.org/2007/02/12/things-that-shouldnt-be-in-file-names-for-1000-alex/
            }
            return fileName;

        }

        public static string RemoveInvalidCharsCombinePath(string filePath, string fileName)
        {
            return Path.Combine(filePath, RemoveInvalidFileNameChars(fileName));
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

        public static List<XmlNodeItem> GetXMLNodesItems(XmlDocument xmlDoc, bool DisableProhibitDtd = false)
        {
            List<XmlNodeItem> returnDict = [];
            XmlReader rdr1 = XmlReader.Create(new System.IO.StringReader(xmlDoc.InnerXml));
            XmlReader rdr = null;
            if (DisableProhibitDtd)
            {
                XmlReaderSettings settings = new XmlReaderSettings
                {
                    DtdProcessing = DtdProcessing.Parse
                };

                rdr = XmlReader.Create(new System.IO.StringReader(xmlDoc.InnerXml), settings);
            }
            else
            {
                rdr = XmlReader.Create(new System.IO.StringReader(xmlDoc.InnerXml));
            }

            XmlReader subrdr = null;
            string Elm = "";

            ArrayList ls = [];
            Dictionary<string, int> lspath = [];
            List<string> DeParams = [];
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

        public static string GetEnumValueDescription(Type EnumType, object EnumValue)
        {
            try
            {
                EnumValueDescriptionAttribute[] attributes = EnumType.GetField(EnumValue.ToString()) != null ?
                    (EnumValueDescriptionAttribute[])EnumType.GetField(EnumValue.ToString()).GetCustomAttributes(typeof(EnumValueDescriptionAttribute), false) : null;
                string s;
                if (attributes != null && attributes.Length > 0)
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

        public static string CorrectJSON(string WrongJson)
        {
            string CleanJson = WrongJson.Replace("\\", "");
            string CleanJson1 = CleanJson[CleanJson.IndexOf("{")..];
            string CleanJson2 = CleanJson1[..(CleanJson1.LastIndexOf("}") + 1)];
            return CleanJson2;
        }

        public static List<T> ConvertObservableListToList<T>(ObservableList<T> List)
        {
            List<T> ObservableList = [.. List];
            return ObservableList;
        }
        public static bool isDesignMode()
        {
            //TODO: move this func to General
            bool designMode = (LicenseManager.UsageMode == LicenseUsageMode.Designtime);
            return designMode;
        }

        public static bool IsNumeric(string sValue)
        {
            // simple method to check is strign is number
            // there are many other alternatives, just keep it simple and make sure it run fast as it is going to be used a lot, for every return value calc   
            // regec and other are more expensive

            foreach (char c in sValue)
            {
                if (!char.IsDigit(c) && c != '.')
                {
                    return false;
                }
            }
            return true;
        }
        public static Dictionary<string, object> DeserializeJson(string json)
        {
            if (json.StartsWith("["))
            {
                Dictionary<string, object> dictionary = [];

                JArray a = JArray.Parse(json);

                int ArrayCount = 1;
                foreach (JObject o in a.Children<JObject>())
                {
                    dictionary.Add(ArrayCount.ToString(), o);
                    ArrayCount++;

                }
                return dictionary;
            }
            else
            {
                Dictionary<string, object> dictionary =
                    JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                return dictionary;
            }
        }

        
        public static string FileContentProvider(string filename)
        {
            Uri url = new Uri(filename);
            string originalJson;
            if (url.IsFile)
            {
                try
                {
                    if (!File.Exists(filename))
                    {
                        Reporter.ToLog(eLogLevel.ERROR, "File not found: " + filename);
                        throw new FileNotFoundException("Cannot find file", filename);
                    }
                    originalJson = File.ReadAllText(filename);
                    if (string.IsNullOrEmpty(originalJson))
                    {
                        Reporter.ToLog(eLogLevel.ERROR, "File is empty: " + filename);
                        throw new Exception("File is empty: " + filename);
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, $"Error occurred while reading {filename}", ex);
                    throw;
                }
            }
            else
            {
                try
                {
                    Reporter.ToLog(eLogLevel.DEBUG, $"Downloading {filename}");
                    originalJson = Common.GeneralLib.HttpUtilities.Download(url);
                    if (string.IsNullOrEmpty(originalJson))
                    {
                        Reporter.ToLog(eLogLevel.ERROR, "Downloaded content is empty: " + filename);
                        throw new Exception("Downloaded content is empty: " + filename);
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, $"Error occurred while downloading {filename}", ex);
                    throw;
                }
            }
            return originalJson;
        }

        /// <summary>
        /// Represents a minimal record of a variable, including its name, initial value, and current value.
        /// </summary>
        /// <param name="Name">The name of the variable.</param>
        /// <param name="InitialValue">The initial value of the variable.</param>
        /// <param name="CurrentValue">The current value of the variable.</param>
        public record VariableMinimalRecord(string Name, string InitialValue, string CurrentValue);

        public static void EnsureAllCategories(ObservableList<SolutionCategoryDefinition> categoriesDefinitions)
        {
            var existingCategories = categoriesDefinitions.Select(x => x.Category).ToHashSet();
            var allCategories = Enum.GetValues<eSolutionCategories>();
            foreach (var category in allCategories)
            {
                if (!existingCategories.Contains(category))
                {
                    categoriesDefinitions.Add(new SolutionCategoryDefinition(category));
                }
            }
        }
    }
}
