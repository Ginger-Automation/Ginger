﻿#region License
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Xml;

namespace Amdocs.Ginger.Common.GeneralLib
{
    public static class General
    {

        static string mAppDataFolder = null;
        public static string LocalUserApplicationDataFolderPath
        {
            get
            {
                if (mAppDataFolder == null)
                {                    
                    string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    appDataFolder = Path.Combine(appDataFolder, "amdocs", "Ginger");

                    if (!Directory.Exists(appDataFolder))
                    {
                        Directory.CreateDirectory(appDataFolder);
                    }
                    mAppDataFolder = appDataFolder;
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

        public static string GingerLogFile
        {
            get
            {
                return Path.Combine(LocalUserApplicationDataFolderPath, "WorkingFolder", "Logs", "Ginger_Log.txt");
            }
        }




        /// <summary>
        /// Should use the function temporary till solution will be implemented for VE fields search
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static bool IsFieldToAvoidInVeFieldSearch(string fieldName)
        {
            if (fieldName == "BackupDic" || fieldName == "GetNameForFileName" || fieldName == "FilePath" || fieldName == "FileName" ||
                fieldName == "ObjFileExt" || fieldName == "ItemNameField" || fieldName == "ItemImageType" || fieldName == "ItemName" ||
                fieldName == "RelativeFilePath" ||
                fieldName == "ObjFolderName" || fieldName == "ContainingFolder" || fieldName == "ContainingFolderFullPath" ||
                fieldName == "ActInputValues" || fieldName == "ActReturnValues" || fieldName == "ActFlowControls" ||
                fieldName == "ScreenShots" ||
                fieldName == "ListStringValue" || fieldName == "ListDynamicValue" || fieldName == "ValueExpression")
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
            double dbl = (double)a.Item1 / (double)a.Item2;
            if ((int)((double)boxHight * dbl) <= boxWidth)
            {
                return new Tuple<int, int>((int)((double)boxHight * dbl), boxHight);
            }
            else
            {
                return new Tuple<int, int>(boxWidth, (int)((double)boxWidth / dbl));
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
            double dbl = (double)Img.Width / (double)Img.Height;
            if ((int)((double)boxHight * dbl) <= boxWidth)
            {
                return new Tuple<int, int>((int)((double)boxHight * dbl), boxHight);
            }
            else
            {
                return new Tuple<int, int>(boxWidth, (int)((double)boxWidth / dbl));
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
            double seconds = Convert.ToDouble(s);
            TimeSpan ts = TimeSpan.FromSeconds(seconds);
            return ts.ToString(@"hh\:mm\:ss");
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
            foreach (char invalidChar in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(invalidChar.ToString(), "");
            }
            fileName = fileName.Replace(@".", "");
            fileName = fileName.Replace(@"?", "");  // on Linux it is valid but we do not want it
            // !!!!!!!!!!!!!!!!!
            //TODO: add more chars remove - see https://blog.josephscott.org/2007/02/12/things-that-shouldnt-be-in-file-names-for-1000-alex/

            return fileName;
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
            List<XmlNodeItem> returnDict = new List<XmlNodeItem>();
            XmlReader rdr1 = XmlReader.Create(new System.IO.StringReader(xmlDoc.InnerXml));
            XmlReader rdr = null;
            if (DisableProhibitDtd)
            {
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.DtdProcessing = DtdProcessing.Parse;

                rdr = XmlReader.Create(new System.IO.StringReader(xmlDoc.InnerXml), settings);
            }
            else
            {
                rdr = XmlReader.Create(new System.IO.StringReader(xmlDoc.InnerXml));
            }

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

        public static string CorrectJSON(string WrongJson)
        {
            string CleanJson = WrongJson.Replace("\\", "");
            string CleanJson1 = CleanJson.Substring(CleanJson.IndexOf("{"));
            string CleanJson2 = CleanJson1.Substring(0, CleanJson1.LastIndexOf("}") + 1);
            return CleanJson2;
        }
    }
}
