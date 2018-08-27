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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common.Enums;
using Ginger.ALM;
using Ginger.BusinessFlowFolder;
using Ginger.Exports.ExportToJava;
using Ginger.UserControlsLib.TextEditor;
using Ginger.VisualAutomate;
using GingerCore;
using GingerWPF.TreeViewItemsLib;
using GingerWPF.UserControlsLib.UCTreeView;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.SolutionWindows.TreeViewItems
{
    public class BusinessFlowTreeItem : NewTreeViewItemBase, ITreeViewItem
    {
        private BusinessFlowPage mBusinessFlowPage;
        public BusinessFlow BusinessFlow { get; set; }
        private StackPanel mHeader;
        public eBusinessFlowsTreeViewMode mViewMode;

        Object ITreeViewItem.NodeObject()
        {
            return BusinessFlow;
        }
        override public string NodePath()
        {
            return BusinessFlow.FileName;
        }
        override public Type NodeObjectType()
        {
            return typeof(BusinessFlow);
        }

        public BusinessFlowTreeItem(eBusinessFlowsTreeViewMode viewMode = eBusinessFlowsTreeViewMode.ReadWrite)
        {
            mViewMode = viewMode;
        }

        StackPanel ITreeViewItem.Header()
        {
            return NewTVItemStyle(BusinessFlow, eImageType.BusinessFlow, nameof(BusinessFlow.Name));
            //mHeader = TreeViewUtils.CreateItemHeader(BusinessFlow.Name, "@WorkFlow_16x16.png", Ginger.SourceControl.SourceControlIntegration.GetItemSourceControlImage(BusinessFlow.FileName , ref ItemSourceControlStatus), BusinessFlow, BusinessFlow.Fields.Name, BusinessFlow.IsDirty);
            //return mHeader;
        }

        List<ITreeViewItem> ITreeViewItem.Childrens()
        {
            return null;
        }

        bool ITreeViewItem.IsExpandable()
        {
            return false;
        }

        Page ITreeViewItem.EditPage()
        {
            if (mBusinessFlowPage == null)
            {
                mBusinessFlowPage = new BusinessFlowPage(BusinessFlow);
            }
            return mBusinessFlowPage;
        }

        ContextMenu ITreeViewItem.Menu()
        {
            return mContextMenu;
        }

        private void VisualAutomate(object sender, RoutedEventArgs e)
        {
            VisualAutomatePage p = new VisualAutomatePage(BusinessFlow);
            p.ShowAsWindow();
        }

        private void ExportToJava(object sender, RoutedEventArgs e)
        {
            BFToJava j = new BFToJava();
            j.BusinessFlowToJava(BusinessFlow);
        }

        void ITreeViewItem.SetTools(ITreeView TV)
        {
            mTreeView = TV;
            mContextMenu = new ContextMenu();
            if (mViewMode == eBusinessFlowsTreeViewMode.ReadWrite)
            {
                AddItemNodeBasicManipulationsOptions(mContextMenu);
                AddSourceControlOptions(mContextMenu);

                if (App.UserProfile.UserTypeHelper.IsSupportAutomate)
                {
                    MenuItem automateMenu = TreeViewUtils.CreateSubMenu(mContextMenu, "Automate");
                    TreeViewUtils.AddSubMenuItem(automateMenu, "Automate", Automate, null, "@Automate_16x16.png");
                    TreeViewUtils.AddSubMenuItem(automateMenu, "Visual Automate", VisualAutomate, null, "@Flow_16x16.png");
                }

                MenuItem ExportMenu = TreeViewUtils.CreateSubMenu(mContextMenu, "Export");
                TreeViewUtils.AddSubMenuItem(ExportMenu, "Export to ALM", ExportToALM, null, "@ALM_16x16.png");
                TreeViewUtils.AddSubMenuItem(ExportMenu, "Export to CSV", ExportToCSV, null, "@CSV_16x16.png");
                if (WorkSpace.Instance.BetaFeatures.BFExportToJava)
                    TreeViewUtils.AddSubMenuItem(ExportMenu, "Export to Java", ExportToJava, null, "");
            }

            AddGherkinOptions(mContextMenu);
        }

        public void AddGherkinOptions(ContextMenu CM)
        {
            if (BusinessFlow.Source == BusinessFlow.eSource.Gherkin)
            {
                MenuItem GherkinMenu = TreeViewUtils.CreateSubMenu(CM, "Gherkin");
                //TOD Change Icon
                TreeViewUtils.AddSubMenuItem(GherkinMenu, "Open Feature file", GoToGherkinFeatureFile, null, "@FeatureFile_16X16.png");
            }
        }
        private void GoToGherkinFeatureFile(object sender, RoutedEventArgs e)
        {
            DocumentEditorPage documentEditorPage = new DocumentEditorPage(BusinessFlow.ExternalID.Replace("~", App.UserProfile.Solution.Folder), true);
            documentEditorPage.Title = "Gherkin Page";
            documentEditorPage.Height = 700;
            documentEditorPage.Width = 1000;
            documentEditorPage.ShowAsWindow();

        }
        
        public override void PostDeleteTreeItemHandler()
        {
            if (App.BusinessFlow == BusinessFlow)
            {
                if (App.LocalRepository.GetSolutionBusinessFlows().Count != 0)
                    App.BusinessFlow = App.LocalRepository.GetSolutionBusinessFlows()[0];
                else
                    App.BusinessFlow = null;
            }
        }

        private void Automate(object sender, System.Windows.RoutedEventArgs e)
        {
            App.MainWindow.AutomateBusinessFlow(BusinessFlow);
        }

        private void ExportToALM(object sender, System.Windows.RoutedEventArgs e)
        {
            ALMIntegration.Instance.ExportBusinessFlowToALM(BusinessFlow, true);
        }
        private void ExportToCSV(object sender, System.Windows.RoutedEventArgs e)
        {
            Export.GingerToCSV.BrowseForFilename();
            Export.GingerToCSV.BusinessFlowToCSV(BusinessFlow);
        }

        //private void RefreshParentFolderChildrens()
        //{
        //    App.LocalRepository.RefreshSolutionBusinessFlowsCache(specificFolderPath: BusinessFlow.ContainingFolderFullPath);
        //    mTreeView.Tree.RefreshSelectedTreeNodeParent();
        //}
    }
}
