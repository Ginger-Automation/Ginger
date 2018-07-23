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

using Amdocs.Ginger.Common;
using Ginger.Repository;
using GingerCore;
using GingerCore.Actions.PlugIns;
using GingerPlugIns;
using GingerPlugIns.TextEditorLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using static GingerCore.Actions.PlugIns.PlugInWrapper;

namespace Ginger.PlugInsLib
{
    class PlugInsIntegration
    {
        public static PlugInWrapper AddNewPlugIn( bool IsEmbeddedPlugin, string SolutionFolder)
        {
            string PlugInSourcePath = General.OpenSelectFolderDialog("Select Plugin Root Folder");
            if (PlugInSourcePath == null)
            {
                return null;
            }
            string[] JsonFile = Directory.GetFiles(PlugInSourcePath, "GingerPluginInfo.json", SearchOption.AllDirectories);
            string PlugInConfigFile = JsonFile[0];
            string PlugInConfigFileFolder = Directory.GetParent(PlugInConfigFile).ToString();

            //Read PluginInfo json file
            if (!File.Exists(PlugInConfigFile))
            {
                Reporter.ToUser(eUserMsgKeys.PlugInFileNotFound, PlugInConfigFile);
                return null;
            }
            string StringPluginInfo = System.IO.File.ReadAllText(PlugInConfigFile);
            Dictionary<string, object> PluginInfoDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(StringPluginInfo);
            if (!PluginInfoDict.ContainsKey("PlugInName") || !PluginInfoDict.ContainsKey("DllFile") || !PluginInfoDict.ContainsKey("DllRelativePath") || !PluginInfoDict.ContainsKey("ClassName"))
            {
                Reporter.ToUser(eUserMsgKeys.StaticWarnMessage, "Cannot add new Plugin, GingerPluginInfo.json file missing value/s.");
                Reporter.ToLog(eLogLevel.ERROR, "Cannot add new Plugin, GingerPluginInfo.json file missing value/s.");
                return null;
            }
            // plugInName for the directory before loading the DLL file
            string plugInName = PluginInfoDict["PlugInName"].ToString();
            string PlugInDllFile = PluginInfoDict["DllFile"].ToString();
            string PlugInDllRelativePath = PluginInfoDict["DllRelativePath"].ToString();
            string PlugInClassName = PluginInfoDict["ClassName"].ToString();

            string solutionPlugInsPath = Path.Combine(SolutionFolder, "Plugins", plugInName);
            if (Directory.Exists(solutionPlugInsPath))
                DeletePlugInDirectory(solutionPlugInsPath);//delete the existing one and copy

            string PlugInRootPath = string.Empty;
            ePluginType PlugInType;

            if (IsEmbeddedPlugin)
            {
                GingerCore.General.DirectoryCopy(PlugInSourcePath, solutionPlugInsPath, true);
                PlugInRootPath = solutionPlugInsPath;
                PlugInType = ePluginType.Embedded;
            }
            else
            {
                Directory.CreateDirectory(solutionPlugInsPath);
                PlugInRootPath = PlugInSourcePath;
                PlugInType = ePluginType.System;
            }
            PlugInWrapper PW = new PlugInWrapper( PlugInClassName, PlugInRootPath, Path.Combine(PlugInDllRelativePath, PlugInDllFile), PlugInType);
            try
            {
                PW.Init();
            }
            catch (Exception ex)
            {
                Reporter.ToUser(eUserMsgKeys.PlugInFileNotFound , ex.Message + " " + ex.InnerException);
                return null;
            }
            PW.SaveToFile(LocalRepository.GetRepoItemFileName(PW, solutionPlugInsPath));
            return PW;           
        }
        
        public static PlugInWrapper GetPlugInWrapperByID(string PlugInID)
        {
            ObservableList<PlugInWrapper> PlugInsList = App.LocalRepository.GetSolutionPlugIns();

            foreach (PlugInWrapper PIW in PlugInsList)
            {
                //Need to check if there is a better way to the PlugIn
                if (PIW.ID == PlugInID)
                    return PIW;
            }
            return null;
        }

        private static void DeletePlugInDirectory(string OldPluginPath)
        {
            System.IO.DirectoryInfo di = new DirectoryInfo(OldPluginPath);

            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                dir.Delete(true);
            }
        }

        public static string GetTamplateContentByPlugInExtension(string plugInExtension)
        {
            ObservableList<PlugInWrapper> PlugInsList = App.LocalRepository.GetSolutionPlugIns();

            foreach (PlugInWrapper PIW in PlugInsList)
            {
                foreach (PlugInTextFileEditorBase PITFEB in PIW.TextEditors())
                {
                    if (PITFEB.Extensions.Contains(plugInExtension))
                    {
                        return PITFEB.GetTemplatesByExtensions(plugInExtension);
                    }
                }

            }
            return null;
        }
    }
}
