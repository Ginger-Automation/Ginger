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
using Amdocs.Ginger.CoreNET.External.WireMock;
using Amdocs.Ginger.Plugin.Core;
using Amdocs.Ginger.Repository;
using Ginger.PlugInsWindows;
using Ginger.UserControlsLib.TextEditor.Common;
using Ginger.UserControlsLib.TextEditor.Office;
using Ginger.UserControlsLib.TextEditor.VBS;
using GingerCore.GeneralLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Ginger.UserControlsLib.TextEditor
{
    /// <summary>
    /// Interaction logic for BasicTextEditorPage.xaml
    /// </summary>
    public partial class DocumentEditorPage : Page
    {
        static List<TextEditorBase> TextEditors = null;
        public WireMockAPI mockAPI;
        private bool isFromWireMock;
        private string wireMockmappingId;

        public DocumentEditorPage(string FileName, bool enableEdit = true, bool RemoveToolBar = false, string UCTextEditorTitle = null, bool isFromWireMock = false, string wireMockmappingId = null)
        {
            InitializeComponent();

            if (isFromWireMock)
            {
                this.isFromWireMock = true;
                this.wireMockmappingId = wireMockmappingId;
                mockAPI = new WireMockAPI();
            }

            TextEditorBase TE = GetTextEditorByExtension(FileName);

            if (TE != null)
            {
                ITextEditorPage p = TE.EditorPage;
                if (p != null)
                {
                    // text editor can return customized editor
                    EditorFrame.ClearAndSetContent(p);
                    p.Load(FileName);
                }
                else
                {
                    // Load regular UCtextEditor and init with TE
                    UCTextEditor UCTE = new UCTextEditor();
                    UCTE.Init(FileName, TE, enableEdit, RemoveToolBar);
                    if (UCTextEditorTitle != null)
                    {
                        UCTE.SetContentEditorTitleLabel(UCTextEditorTitle);
                    }
                    else if (!string.IsNullOrEmpty(TE.Title()))
                    {
                        UCTE.SetContentEditorTitleLabel(TE.Title());
                    }

                    EditorFrame.ClearAndSetContent(UCTE);
                }
            }
            else
            {
                if (IsTextFile(FileName))
                {
                    //Use the default UCTextEditor control
                    UCTextEditor UCTE = new UCTextEditor();
                    AutoDetectTextEditor AD = new AutoDetectTextEditor
                    {
                        ext = Path.GetExtension(FileName)
                    };
                    TE = AD;
                    UCTE.Init(FileName, TE, enableEdit, RemoveToolBar);
                    if (UCTextEditorTitle != null)
                    {
                        UCTE.SetContentEditorTitleLabel(UCTextEditorTitle);
                    }

                    EditorFrame.ClearAndSetContent(UCTE);
                }
                else
                {
                    // Just put a general shell doc                     
                    ITextEditorPage p = new OfficeDocumentPage();
                    p.Load(FileName);
                    EditorFrame.ClearAndSetContent(p);
                }
            }
        }



        internal void ShowAsWindow(string customeTitle = null)
        {
            string title;
            if (customeTitle != null)
            {
                title = customeTitle;
            }
            else
            {
                title = this.Title;
            }

            GingerCore.General.LoadGenericWindow(ref genWin, App.MainWindow, eWindowShowStyle.Free, title, this);
        }

        GenericWindow genWin;

        private bool IsTextFile(string fileName)
        {
            // Method #1
            if (IsTextFileByExtension(Path.GetExtension(fileName).ToLower()))
            {
                return true;
            }

            // Method #2
            //Not full proof but good enough
            // Check for consecutive nulls in the first 10K..
            byte[] content = File.ReadAllBytes(fileName);
            for (int i = 1; i < 10000 && i < content.Length; i++)
            {
                if (content[i] == 0x00 && content[i - 1] == 0x00)
                {
                    return false;
                }
            }
            return true;

        }

        static List<string> ExtensiosnList = null;

        static bool IsTextFileByExtension(string sExt)
        {
            //TODO: create match - ext to edit type - show icon per type
            // new class - ext, editortype, icon for TV, keep in static
            if (ExtensiosnList == null)
            {
                FillExtensionsList();

            }
            if (ExtensiosnList.Contains(sExt))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static void FillExtensionsList()
        {
            ExtensiosnList =
            [
                ".txt",
                ".bat",
                ".ahk",
                ".applescript",
                ".as",
                ".au3",
                ".bas",
                ".cljs",
                ".cmd",
                ".coffee",
                ".egg",
                ".egt",
                ".erb",
                ".hta",
                ".ibi",
                ".ici",
                ".ijs",
                ".ipynb",
                ".itcl",
                ".js",
                ".jsfl",
                ".lua",
                ".m",
                ".mrc",
                ".ncf",
                ".nut",
                ".php",
                ".pl",
                ".pm",
                ".ps1",
                ".ps1xml",
                ".psc1",
                ".psd1",
                ".psm1",
                ".py",
                ".pyc",
                ".pyo",
                ".r",
                ".rb",
                ".rdp",
                ".scpt",
                ".scptd",
                ".sdl",
                ".sh",
                ".syjs",
                ".sypy",
                ".tcl",
                ".vbs",
                ".xpl",
                ".ebuild",
                ".feature",
            ];
        }

        private TextEditorBase GetTextEditorByExtension(string FileName)
        {
            ScanTextEditors();

            //TODO: if there is more than one let the user pick
            string ext = Path.GetExtension(FileName).ToLower();
            foreach (TextEditorBase TE in TextEditors)
            {
                if (TE.Extensions != null)
                {
                    if (TE.Extensions.Contains(ext))
                    {
                        return TE;
                    }
                }
            }

            return null;
        }

        void ScanTextEditors()
        {
            if (TextEditors == null)
            {
                TextEditors = [];

                var list = from type in typeof(TextEditorBase).Assembly.GetTypes()
                           where type.IsSubclassOf(typeof(TextEditorBase))
                           select type;


                foreach (Type t in list)
                {
                    if (t == typeof(PlugInTextEditorWrapper) || t == typeof(ValueExpression.ValueExpressionEditor))
                    {
                        continue;
                    }

                    if (t != typeof(ITextEditor))
                    {
                        TextEditorBase TE = (TextEditorBase)Activator.CreateInstance(t);
                        TextEditors.Add(TE);
                    }
                }

                // Add all plugins TextEditors 
                ObservableList<PluginPackage> Plugins = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<PluginPackage>();

                foreach (PluginPackage pluginPackage in Plugins)
                {
                    pluginPackage.PluginPackageOperations = new PluginPackageOperations(pluginPackage);

                    if (string.IsNullOrEmpty(((PluginPackageOperations)pluginPackage.PluginPackageOperations).PluginPackageInfo.UIDLL))
                    {
                        continue;
                    }

                    foreach (ITextEditor TE in PluginTextEditorHelper.GetTextFileEditors(pluginPackage))
                    {
                        PlugInTextEditorWrapper plugInTextEditorWrapper = new PlugInTextEditorWrapper(TE);
                        TextEditors.Add(plugInTextEditorWrapper);
                    }
                }
            }
        }




        public async Task Save()
        {

            if (EditorFrame.Content is UCTextEditor)
            {
                ((UCTextEditor)EditorFrame.Content).Save();
            }
            else
            if (EditorFrame.Content is ITextEditorPage)
            {
                ((ITextEditorPage)EditorFrame.Content).Save();
            }
            else if (isFromWireMock)
            {
                await mockAPI.UpdateStubAsync(wireMockmappingId, ((UCTextEditor)EditorFrame.Content).Text);
            }
            else
            {
                //TODO: fix me to call save...
            }
        }
    }
}
