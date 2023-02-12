#region License
/*
Copyright © 2014-2023 European Support Limited

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

using Ginger.UserControlsLib.TextEditor;
using GingerWPF.UserControlsLib.UCTreeView;
using GingerCore;
using GingerCore.SourceControl;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using GingerWPF.TreeViewItemsLib;
using Amdocs.Ginger.Common.Enums;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;

namespace Ginger.SolutionWindows.TreeViewItems
{
    class DocumentTreeItem : NewTreeViewItemBase, ITreeViewItem
    {
        private Page mDocumentViewerPage;
        public string FileName { get; set; }
        public string Path { get; set; }

        Object ITreeViewItem.NodeObject()
        {
            return FileName;
        }
        override public string NodePath()
        {
            return Path;
        }
        override public Type NodeObjectType()
        {
            return null;
        }

        StackPanel ITreeViewItem.Header()
        {


            //string icon = string.Empty;

            //if (FileName != null && System.IO.Path.GetExtension(FileName).ToUpper() == ".FEATURE")
            //    icon = "@FeatureFile_16X16.png";
            //else
            //    icon = "@Documents_16x16.png";
            //StackPanel SP =  TreeViewUtils.CreateItemHeader(FileName, icon, Ginger.SourceControl.SourceControlUI.GetItemSourceControlImage(Path, ref ItemSourceControlStatus));
            //return SP;

            return TreeViewUtils.NewRepositoryItemTreeHeader(null, FileName, eImageType.File, GetSourceControlImageByPath(Path), false);
        }

        List<ITreeViewItem> ITreeViewItem.Childrens()
        {
            return null;
        }

        bool ITreeViewItem.IsExpandable()
        {
            return false;
        }

        Page ITreeViewItem.EditPage(Amdocs.Ginger.Common.Context mContext)
        {
            if (mDocumentViewerPage == null)
            {
                //TODO: init and load the correct file editor in BasicTextEditor
                mDocumentViewerPage = new DocumentEditorPage(Path, true);
            }

            return mDocumentViewerPage;
        }

        ContextMenu ITreeViewItem.Menu()
        {
            return mContextMenu;
        }

        void ITreeViewItem.SetTools(ITreeView TV)
        {
            mTreeView = TV;
            mContextMenu = new ContextMenu();

            AddItemNodeBasicManipulationsOptions(mContextMenu, allowCopy: false, allowCut: false, allowDuplicate: false, allowViewXML: false);

            TreeViewUtils.AddMenuItem(mContextMenu, "Open External", OpenDocument, null, eImageType.ShareExternal);

            AddSourceControlOptions(mContextMenu);
            AddGherkinOptions(mContextMenu);
        }

        public void AddGherkinOptions(ContextMenu CM)
        {
            if (System.IO.Path.GetExtension(FileName) == ".feature" &&  WorkSpace.Instance.UserProfile.UserTypeHelper.IsSupportAutomate)
            {
                MenuItem GherkinMenu = TreeViewUtils.CreateSubMenu(CM, "Gherkin");
                //TOD Change Icon
                TreeViewUtils.AddSubMenuItem(GherkinMenu, "Automate mapped " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow), GoToGherkinBusinessFlow, null, eImageType.File);
            }
        }

        private void GoToGherkinBusinessFlow(object sender, RoutedEventArgs e)
        {
            ObservableList<BusinessFlow> businessFlows = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>();
            BusinessFlow BF = businessFlows.Where(x => x.ExternalID != null ? System.IO.Path.GetFullPath(x.ExternalID.Replace("~",  WorkSpace.Instance.Solution.Folder)) == System.IO.Path.GetFullPath(Path) : false).FirstOrDefault();
            if (BF == null)
                Reporter.ToUser(eUserMsgKey.GherkinNotifyBFIsNotExistForThisFeatureFile, FileName);
            else
            {
                App.OnAutomateBusinessFlowEvent(BusinessFlowWindows.AutomateEventArgs.eEventType.Automate, BF);
            }
        }

        private void OpenDocument(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                if (System.IO.File.Exists(Path))
                {
                    Process.Start(new ProcessStartInfo() { FileName = Path, UseShellExecute = true });
                }
            }
            catch (Exception ex)
            {
                Reporter.ToUser(eUserMsgKey.StaticErrorMessage, "Failed to open document using external application, error: " + ex.Message);                
            }
}

        public override bool SaveTreeItem(object item, bool saveOnlyIfDirty = false)
        {
            if (mDocumentViewerPage is DocumentEditorPage)
            {
                ((DocumentEditorPage)mDocumentViewerPage).Save();
                return true;
            }
            return false;
        }

        public override bool DeleteTreeItem(object item, bool deleteWithoutAsking = false, bool refreshTreeAfterDelete = true)
        {
            if (Reporter.ToUser(eUserMsgKey.DeleteRepositoryItemAreYouSure, FileName) == Amdocs.Ginger.Common.eUserMsgSelection.No)
                return false;

            try
            {
                if (System.IO.File.Exists(Path))
                {
                    System.IO.File.Delete(Path);
                    mTreeView.Tree.DeleteItemAndSelectParent((ITreeViewItem)this);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToUser(eUserMsgKey.StaticErrorMessage, "Delete document failed, error: " + ex.Message);
                return false;
            }

            return false;
        }
    }
}
