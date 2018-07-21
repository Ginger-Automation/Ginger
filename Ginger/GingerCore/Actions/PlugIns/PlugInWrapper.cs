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

using Amdocs.Ginger.Repository;
using GingerPlugIns;
using GingerPlugIns.ActionsLib;
using GingerPlugIns.TextEditorLib;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System;
using System.Linq;

namespace GingerCore.Actions.PlugIns
{
    public class PlugInWrapper : RepositoryItem
    {
        private IGingerPlugIn mPlugIn;
        public IGingerPlugIn PlugIn { get { if (!IsLoaded) LoadPlugIn(); return mPlugIn; } set { mPlugIn = value; } }   

        public new static class Fields
        {
            public static string Name = "Name";
            public static string Description = "Description";
            public static string PlugInFolder = "PlugInFolder";
            public static string PlugInType = "PlugInType";
            public static string DllPath = "DllPath";

        }

        public enum ePluginType
        {
            Embedded,
            System
        }

        //Serialized fields
        [IsSerializedForLocalRepository]
        public string Name { get; set; }
        [IsSerializedForLocalRepository]
        public string Description { get; set; }
        [IsSerializedForLocalRepository]
        public string ID { get; set; }
        [IsSerializedForLocalRepository]
        public string PlugInVersion { get; set; }

        [IsSerializedForLocalRepository]
        public string DllRelativePath { get; set; }//need to be relative path

        public string FullDllPath
        {
            get
            {
                return Path.Combine(FullPlugInRootPath, DllRelativePath.TrimStart(new char[]{'\\','/'}));
            }
        }

        [IsSerializedForLocalRepository]
        public string ClassName { get; set; }
        [IsSerializedForLocalRepository]
        public string PlugInRootPath { get; set; }

        public string FullPlugInRootPath
        {
            get
            {
                if (PlugInType == ePluginType.System)
                    return PlugInRootPath;
                else
                {
                    //return the contaning folder of the wrapper in the Solution
                    try
                    {
                        if (Directory.Exists(ContainingFolderFullPath))
                            return ContainingFolderFullPath;//to return current local folder
                        else
                            return PlugInRootPath;
                    }
                    catch(Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}");
                        return PlugInRootPath;
                    }                    
                }               
            }
        }

        private Assembly mAssembly;
        public Assembly Assembly { get { if (!IsLoaded) LoadPlugIn(); return mAssembly; } set { mAssembly = value; } }

        public bool IsLoaded { get; set; }

        

        public PlugInWrapper(string className, string plugInRootPath, string dllRelativePath, ePluginType plugInType)
        {
            ClassName = className;
            PlugInRootPath = plugInRootPath;
            DllRelativePath = dllRelativePath;
            PlugInType = plugInType;
        }
        public PlugInWrapper(){}

        public void Init()
        {         
            if (!IsLoaded) LoadPlugIn();

            Name = PlugIn.Name();
            Description = PlugIn.Description();
            ID = PlugIn.ID();
            PlugInVersion = PlugIn.PlugInVersion();
        }
        
        public void LoadPlugIn()
        {
            try
            {
                Assembly DLL = Assembly.LoadFrom(FullDllPath);
                var o = DLL.CreateInstance(ClassName);
                if (o != null)
                {
                    PlugIn = (IGingerPlugIn)o;
                    Assembly = DLL;
                    IsLoaded = true;
                }
                else { }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to load the plugin '" + Name +"'", ex);
                throw ex;
            }
        }
        

        public override string GetNameForFileName()
        {
            return Name;
        }
        public override string ItemName { get { return Name; } set { } }


        public string GetEditPage(string PlugInActionID)
        {
            string EditPageName = (from x in Actions where x.ID == PlugInActionID select x.EditPage).FirstOrDefault();
            if (EditPageName != null && EditPageName.EndsWith(".xaml", StringComparison.OrdinalIgnoreCase))
            {
                EditPageName = Path.Combine(GetXamlFolder(), EditPageName);
            }
            return EditPageName;
        }


        private ePluginType mPluginType;

        [IsSerializedForLocalRepository]
        public ePluginType PlugInType {get { return mPluginType; }set { mPluginType = value; }}

        public void RunAction(GingerAction act)
        {
            PIActionsCapability.RunAction(act);
        }

        internal string GetXamlFolder()
        {
            string workingfolder = Path.GetDirectoryName(FullDllPath);
            return Path.Combine(workingfolder, "Xaml");
        }

        PlugInActionsBase mPIActionsCapability = null;
        PlugInActionsBase PIActionsCapability
        {
            get
            {
                // We cache the ref to the PluginActions handler
                if (mPIActionsCapability == null)
                {
                    foreach (PlugInCapability c in PlugIn.Capabilities())
                    {
                        if (c is PlugInActionsBase)
                        {
                            mPIActionsCapability = (PlugInActionsBase)c;
                        }
                    }
                }
                return mPIActionsCapability;
            }
        }
        
        // We search the Plugin if it has capability of Actions, if yes we return the actions it can perform
        public List<PlugInAction> Actions
        {
            get
            {
                if (PIActionsCapability != null)
                {
                    return PIActionsCapability.Actions();
                }
                else
                {
                    return null;
                }
            }

        }

        // We search the plug in for all files it can edit if this plugin implemented capability of text editor
        public List<PlugInTextFileEditorBase> TextEditors()
        {
            List<PlugInTextFileEditorBase> list = new List<PlugInTextFileEditorBase>();
            foreach (PlugInCapability c in PlugIn.Capabilities())
            {
                if (c is PlugInTextFileEditorBase)
                {
                    list.Add((PlugInTextFileEditorBase)c);
                }
            }
            return list;
        }

        public List<string> GetExtentionsByEditorID(string plugInEditorID)
        {
            foreach (PlugInTextFileEditorBase PITFEB in TextEditors())
            {
                if (PITFEB.EditorID == plugInEditorID)
                {
                    return PITFEB.Extensions;
                }
            }
            return null;
        }

        public PlugInTextFileEditorBase GetPlugInTextFileEditorBaseByEditorID(string plugInEditorID)
        {
            foreach (PlugInTextFileEditorBase PITFEB in TextEditors())
            {
                if (PITFEB.EditorID == plugInEditorID)
                {
                    return PITFEB;
                }
            }
            return null;
        }

        public string GetTemplateContentByEditorID(string plugInEditorID, string plugInExtension)
        {
            foreach (PlugInTextFileEditorBase PITFEB in TextEditors())
            {
                if (PITFEB.EditorID == plugInEditorID)
                {
                    return PITFEB.GetTemplatesByExtensions("." + plugInExtension);
                }
            }
            return null;
        }
    }
}
