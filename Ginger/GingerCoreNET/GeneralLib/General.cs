#region License
/*
Copyright © 2014-2024 European Support Limited

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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Repository.SolutionCategories;
using Amdocs.Ginger.CoreNET.Run.SolutionCategory;
using Amdocs.Ginger.Repository;
using Ginger.Configurations;
using GingerCore;
using GingerCore.Actions;
using GingerCore.ALM.RQM;
using GingerCore.DataSource;
using GingerCore.Environments;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace GingerCoreNET.GeneralLib
{
    public class General
    {
        #region ENUM

        public static List<string> GetEnumValues(Type EnumType)
        {
            List<string> l = [];
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


        /// <summary>  
        /// Defines the possible vertical alignment options for scrolling elements into view.  
        /// </summary>  
        public enum eScrollAlignment
        {
            /// <summary>  
            /// Aligns the top of the element with the top of the visible area of the scrollable ancestor.  
            /// </summary>  
            Start,

            /// <summary>  
            /// Centers the element vertically in the visible area of the scrollable ancestor.  
            /// </summary>  
            Center,

            /// <summary>  
            /// Aligns the bottom of the element with the bottom of the visible area of the scrollable ancestor.  
            /// </summary>  
            End,

            /// <summary>  
            /// Aligns the element with the nearest edge of the visible area of the scrollable ancestor,  
            /// either the top or bottom, depending on which is closer.  
            /// </summary>  sssss
            Nearest
        }

        #endregion ENUM

        static Regex rxvarPattern = new Regex(@"{(\bVar Name=)\w+\b[^{}]*}", RegexOptions.Compiled);
        static string GetDatetimeFormat() => DateTime.Now.ToString("ddMMyyyy_HHmmssfff");
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



        public static List<XmlNodeItem> GetXMLNodesItems(XmlDocument xmlDoc)
        {
            List<XmlNodeItem> returnDict = [];
            XmlReader rdr1 = XmlReader.Create(new System.IO.StringReader(xmlDoc.InnerXml));
            XmlReader rdr = XmlReader.Create(new System.IO.StringReader(xmlDoc.InnerXml));
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
                    {
                        ls.Add(Elm);
                    }
                    else
                    {
                        ls[rdr.Depth] = Elm;
                    }

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
            return Amdocs.Ginger.Common.GeneralLib.General.TimeConvert(s);
            //double seconds = Convert.ToDouble(s);
            //TimeSpan ts = TimeSpan.FromSeconds(seconds);
            //return ts.ToString(@"hh\:mm\:ss");
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
        //public static bool isDesignMode()
        //{
        //    //TODO: move this func to General
        //    bool designMode = (LicenseManager.UsageMode == LicenseUsageMode.Designtime);
        //    return designMode;
        //}

        public static ObservableList<T> ConvertListToObservableList<T>(List<T> List)
        {
            ObservableList<T> ObservableList = [.. List];

            return ObservableList;
        }

        public static string CheckDataSource(string DataSourceVE, ObservableList<DataSourceBase> DSList)
        {
            string DSVE = DataSourceVE;
            DataSourceBase DataSource = null;
            DataSourceTable DSTable = null;
            if (string.IsNullOrEmpty(DataSourceVE) || DSVE.IndexOf("{DS Name=") != 0)
            {
                return "Invalid Data Source Value : '" + DataSourceVE + "'";
            }
            DSVE = DSVE.Replace("{DS Name=", "");
            DSVE = DSVE.Replace("}", "");
            if (DSVE.IndexOf(" DST=") == -1)
            {
                return "Invalid Data Source Value : '" + DataSourceVE + "'";
            }
            string DSName = DSVE[..DSVE.IndexOf(" DST=")];

            foreach (DataSourceBase ds in DSList)
            {
                if (ds.Name == DSName)
                {
                    DataSource = ds;
                    break;
                }
            }

            if (DataSource == null)
            {
                return "Data Source: '" + DSName + "' used in '" + DataSourceVE + "' not found in solution.";
            }

            DSVE = DSVE[DSVE.IndexOf(" DST=")..].Trim();
            if (DSVE.IndexOf(" ") == -1)
            {
                return "Invalid Data Source Value : '" + DataSourceVE + "'";
            }
            string DSTableName = DSVE.Substring(DSVE.IndexOf("DST=") + 4, DSVE.IndexOf(" ") - 4);

            if (DataSource.DSType == DataSourceBase.eDSType.MSAccess)
            {
                DataSource.FileFullPath = WorkSpace.Instance.Solution.SolutionOperations.ConvertSolutionRelativePath(DataSource.FileFullPath);
                ObservableList<DataSourceTable> dsTables = DataSource.GetTablesList();
                if (dsTables == null)
                {
                    return "";
                }
                foreach (DataSourceTable dst in dsTables)
                {
                    if (dst.Name == DSTableName)
                    {
                        DSTable = dst;
                        break;
                    }
                }
                if (DSTable == null)
                {
                    return "Data Source Table : '" + DSTableName + "' used in '" + DataSourceVE + "' not found in solution.";
                }
            }
            return "";
        }

        public static bool CreateDefaultEnvironment()
        {
            if (!WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>().Any())
            {
                ProjEnvironment newEnv = new ProjEnvironment() { Name = "Default" };

                // Add all solution target app
                foreach (ApplicationPlatform AP in WorkSpace.Instance.Solution.ApplicationPlatforms)
                {
                    EnvApplication EA = new EnvApplication
                    {
                        Name = AP.AppName,
                        CoreProductName = AP.Core,
                        CoreVersion = AP.CoreVersion,
                        Active = true,
                        ParentGuid = AP.Guid
                    };
                    newEnv.Applications.Add(EA);
                }
                WorkSpace.Instance.SolutionRepository.AddRepositoryItem(newEnv);
                return true;
            }
            else
            {
                return false;
            }
        }

        public static void SetUniqueNameToRepoItem(ObservableList<RepositoryItemBase> itemsList, RepositoryItemBase item, string suffix = "")
        {
            string originalName = item.ItemName;
            if (itemsList.FirstOrDefault(x => x.ItemName == item.ItemName) == null)
            {
                return;//name is unique
            }

            if (!string.IsNullOrEmpty(suffix))
            {
                item.ItemName = item.ItemName + suffix;
                if (itemsList.FirstOrDefault(x => x.ItemName == item.ItemName) == null)
                {
                    return;//name with Suffix is unique
                }
            }

            int counter = 1;
            while (itemsList.FirstOrDefault(x => x.ItemName == item.ItemName) != null)
            {
                counter++;
                if (!string.IsNullOrEmpty(suffix))
                {
                    item.ItemName = originalName + suffix + counter.ToString();
                }
                else
                {
                    item.ItemName = originalName + counter.ToString();
                }
            }
        }
        /// <summary>
        /// This method is used to export the data to excel 
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="filePath"></param>
        /// <param name="sheetName"></param>
        /// <returns></returns>
        public static bool ExportToExcel(System.Data.DataTable dataTable, string filePath, string sheetName)
        {
            try
            {
                OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
                using (OfficeOpenXml.ExcelPackage xlPackage = new OfficeOpenXml.ExcelPackage())
                {
                    if (File.Exists(filePath))
                    {
                        using (var stream = File.OpenRead(filePath))
                        {
                            xlPackage.Load(stream);
                        }
                        //delete existing worksheet if worksheet with same name already exist
                        OfficeOpenXml.ExcelWorksheet excelWorksheet = xlPackage.Workbook.Worksheets.FirstOrDefault(x => x.Name == sheetName);
                        if (excelWorksheet != null)
                        {
                            xlPackage.Workbook.Worksheets.Delete(sheetName);
                        }
                        excelWorksheet = xlPackage.Workbook.Worksheets.Add(sheetName);

                        excelWorksheet.Cells["A1"].LoadFromDataTable(dataTable, true);
                        File.WriteAllBytes(filePath, xlPackage.GetAsByteArray());
                    }
                    else
                    {
                        var ws = xlPackage.Workbook.Worksheets.Add(sheetName);
                        ws.Cells["A1"].LoadFromDataTable(dataTable, true);
                        xlPackage.SaveAs(new FileInfo(filePath));
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error Occurred while exporting to excel", ex);
                return false;
            }
        }

        public static string GetSolutionCategoryValue(SolutionCategoryDefinition solutionCategoryDefinition)
        {
            SolutionCategory cat = WorkSpace.Instance.Solution.SolutionCategories.FirstOrDefault(x => x.Category == solutionCategoryDefinition.Category);
            if (cat != null)
            {
                SolutionCategoryValue catValue = cat.CategoryOptionalValues.FirstOrDefault(x => x.Guid == solutionCategoryDefinition.SelectedValueID);
                if (catValue != null)
                {
                    return catValue.Value;
                }
            }

            return null;
        }
        public static string RemoveSpecialCharactersInColumnHeader(string columnHeader)
        {
            string specialCharactersToRemove = "./[]()";
            foreach (char sc in specialCharactersToRemove)
            {
                if (columnHeader.Contains(sc.ToString()))
                {
                    columnHeader = columnHeader.Replace(sc.ToString(), string.Empty);
                }
            }
            return columnHeader;
        }

        public static string CreateTempTextFile(string content)
        {
            byte[] bytes = null;
            try
            {
                string filePath = System.IO.Path.GetTempFileName();
                bytes = System.Text.Encoding.Default.GetBytes(content);
                File.WriteAllBytes(filePath, bytes);
                return filePath;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to create temp text file", ex);
                return null;
            }
            finally
            {
                if (bytes != null)
                {
                    Array.Clear(bytes);
                }
            }
        }

        public static byte[] ImageToByteArray(Image img, System.Drawing.Imaging.ImageFormat format)
        {
            using (var ms = new MemoryStream())
            {
                img.Save(ms, format);
                return ms.ToArray();
            }
        }

        public static string DownloadImage(string ImageURL, Act act)
        {
            String currImagePath = string.Empty;
            try
            {
                currImagePath = Act.GetScreenShotRandomFileName();
                HttpResponseMessage response = SendRequest(ImageURL);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var fs = new FileStream(currImagePath, FileMode.Create, FileAccess.Write, FileShare.None);
                    response.Content.CopyToAsync(fs).ContinueWith(
                    (discard) =>
                    {
                        fs.Close();
                    });
                    act.ScreenShotsNames.Add(Path.GetFileName(currImagePath));
                    act.ScreenShots.Add(currImagePath);
                }
            }
            catch (Exception ex)
            {
                act.Error += ex.Message;
                currImagePath = string.Empty;
            }
            return currImagePath;
        }

        public static async Task<string> DownloadBaselineImage(string ImageURL, Act act)
        {
            String currImagePath = Act.GetScreenShotRandomFileName();
            try
            {
                HttpResponseMessage response = SendRequest(ImageURL);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    using (var fs = new FileStream(currImagePath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        await response.Content.CopyToAsync(fs);
                        fs.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                act.Error += ex.Message;
                Reporter.ToLog(eLogLevel.ERROR, "unable to fetch the baseline image");
            }
            return currImagePath;
        }

        public static HttpResponseMessage SendRequest(string URL)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, URL);
            HttpClient client = new HttpClient();
            HttpResponseMessage response = client.SendAsync(request).Result;
            return response;
        }

        public static string GetDataByassemblyNameandResource(string AssemblyName, string ResourceId)
        {
            string arg = string.Empty;
            Assembly asm = Assembly.Load(AssemblyName);
            Stream stream = asm.GetManifestResourceStream(ResourceId);
            using (StreamReader streamReader = new StreamReader(stream))
            {
                arg = streamReader.ReadToEnd();
            }
            return arg;
        }

        public static bool isVariableUsed(string variablestring)
        {
            MatchCollection matcheslist = rxvarPattern.Matches(variablestring);
            if (matcheslist.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static string GenerateFilePath(string folderPath, string ItemName)
        {
            string path;

            string Filename = $"{ItemName}_{GetDatetimeFormat()}.txt";
            path = $"{folderPath}{Path.DirectorySeparatorChar}{Filename}";
            return path;
        }

        public static string CreateReportLinkPerFlow(string HtmlReportUrl, string ExecutionId, string BusinessFlowInstanceGuid)
        {
            if (!string.IsNullOrEmpty(HtmlReportUrl) && !string.IsNullOrEmpty(ExecutionId) && !string.IsNullOrEmpty(BusinessFlowInstanceGuid))
            {
                if (HtmlReportUrl.Last() != '/')
                {
                    HtmlReportUrl = $"{HtmlReportUrl}/";
                }
                return $"{HtmlReportUrl}#/BusinessFlow/{ExecutionId}/{BusinessFlowInstanceGuid}";
            }
            return "";
        }

        public static bool CreateDefaultAccessiblityconfiguration()
        {
            try
            {
                if (!WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<AccessibilityConfiguration>().Any())
                {
                    AccessibilityConfiguration newAccessibilityConfiguration = new AccessibilityConfiguration() { Name = "Accessibility" };
                    WorkSpace.Instance.SolutionRepository.AddRepositoryItem(newAccessibilityConfiguration);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error creating default accessibility configuration", ex);
                return false;
            }
        }
        public static bool CreateGingerOpsConfiguration()
        {
            try
            {
                if (!WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<GingerOpsConfiguration>().Any())
                {
                    GingerOpsConfiguration newGAConfiguration = new GingerOpsConfiguration() { Name = "GingerOps" };
                    WorkSpace.Instance.SolutionRepository.AddRepositoryItem(newGAConfiguration);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error creating Ginger Analytics Configuration configuration", ex);
                return false;
            }
        }

        public static bool IsConfigPackageExists(string PackagePath, GingerCoreNET.ALMLib.ALMIntegrationEnums.eALMType eALMType)
        {
            string settingsFolder = string.Empty;
            settingsFolder = eALMType switch
            {
                GingerCoreNET.ALMLib.ALMIntegrationEnums.eALMType.Jira => "JiraSettings",
                GingerCoreNET.ALMLib.ALMIntegrationEnums.eALMType.Qtest => "QTestSettings",
                _ => "JiraSettings",
            };
            if (Directory.Exists(Path.Combine(PackagePath, settingsFolder)))
            {
                return true;
            }
            else
            {
                Reporter.ToLog(eLogLevel.WARN, "Configuration package not exist in solution, Settings not exist at: " + Path.Combine(PackagePath, settingsFolder));
            }
            return false;
        }

        /// <summary>
        /// Retrieves external fields from either online RQM or workspace solution based on configuration.
        /// </summary>
        /// <returns>List of external fields with their values.</returns>
        public static ObservableList<ExternalItemFieldBase> GetExternalFields()
        {
            ObservableList<ExternalItemFieldBase> originalExternalFields = new ObservableList<ExternalItemFieldBase>();

            var defaultALMConfig = WorkSpace.Instance.Solution.ALMConfigs.FirstOrDefault(x => x.DefaultAlm);
            var firstExternalItemField = WorkSpace.Instance.Solution.ExternalItemsFields.FirstOrDefault();

            if (defaultALMConfig != null && firstExternalItemField != null &&
                defaultALMConfig.ALMProjectGUID != firstExternalItemField.ProjectGuid)
            {
                var externalOnlineItemsFields = ImportFromRQM.GetOnlineFields(null);
                if (externalOnlineItemsFields == null)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Failed to retrieve online fields from RQM");
                    return originalExternalFields;
                }
                foreach (var externalItemField in externalOnlineItemsFields)
                {
                    originalExternalFields.Add(MapExternalField(externalItemField));
                }
            }
            else
            {
                originalExternalFields = WorkSpace.Instance.Solution.ExternalItemsFields;
            }

            return originalExternalFields;
        }

        private static ExternalItemFieldBase MapExternalField(ExternalItemFieldBase externalItemField)
        {
            var existingField = WorkSpace.Instance.Solution.ExternalItemsFields
                .FirstOrDefault(x => x.Name.Equals(externalItemField.Name, StringComparison.CurrentCultureIgnoreCase));

            return new ExternalItemFieldBase
            {
                Name = externalItemField.Name,
                ID = externalItemField.ID,
                ItemType = externalItemField.ItemType,
                Guid = externalItemField.Guid,
                IsCustomField = externalItemField.IsCustomField,
                SelectedValue = existingField != null && !string.IsNullOrEmpty(existingField.SelectedValue)
                    ? existingField.SelectedValue
                    : externalItemField.SelectedValue
            };
        }
    }

}


