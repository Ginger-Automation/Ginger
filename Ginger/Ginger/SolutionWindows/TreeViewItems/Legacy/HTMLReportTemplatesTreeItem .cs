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
using Amdocs.Ginger.Core;
using Amdocs.Ginger.Common.Enums;
using Ginger.Reports;
using GingerWPF.UserControlsLib.UCTreeView;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using GingerWPF.TreeViewItemsLib;

namespace Ginger.SolutionWindows.TreeViewItems
{
    class HTMLReportTemplatesTreeItem : NewTreeViewItemBase, ITreeViewItem
    {
        private HTMLReportTemplatesPage mReportTemplatesPage;

        public string Folder { get; set; }
        public string Path { get; set; }

        ITreeView mTV;

        Object ITreeViewItem.NodeObject()
        {
            return null;
        }

        StackPanel ITreeViewItem.Header()
        {
            return TreeViewUtils.CreateItemHeader("E-mail Report Templates", "@HTMLReport_16x16.png", Ginger.SourceControl.SourceControlIntegration.GetItemSourceControlImage(Path, ref ItemSourceControlStatus));            
        }

        List<ITreeViewItem> ITreeViewItem.Childrens()
        {
            List<ITreeViewItem> Childrens = new List<ITreeViewItem>();
            ObservableList<HTMLReportTemplate> Reports = App.LocalRepository.GetSolutionHTMLReportTemplates(UseCache: false, specificFolderPath: Path);
            foreach (HTMLReportTemplate rep in Reports)
            {
                HTMLReportTemplateTreeItem RTTI = new HTMLReportTemplateTreeItem();
                RTTI.HTMLReportTemplate = rep;
                Childrens.Add(RTTI);
            }
            return Childrens;
        }

        bool ITreeViewItem.IsExpandable()
        {
            return true;
        }

        Page ITreeViewItem.EditPage()
        {
            if (mReportTemplatesPage == null)
            {
                mReportTemplatesPage = new HTMLReportTemplatesPage();
            }
            return mReportTemplatesPage;
        }

        ContextMenu ITreeViewItem.Menu()
        {
            ContextMenu CM = new ContextMenu();
            TreeViewUtils.AddMenuItem(CM, "Refresh", RefreshItems, null, eImageType.Refresh);
            TreeViewUtils.AddMenuItem(CM, "Save All", SaveAll, null, eImageType.Save);
            AddViewFolderFilesMenuItem(CM, Path);
            return CM;
        }

        void ITreeViewItem.SetTools(ITreeView TV)
        {
            mTV = TV;

            TV.AddToolbarTool(eImageType.Refresh, "Refresh", RefreshItems);
            TV.AddToolbarTool("@SaveAll_16x16.png", "Save All", SaveAll);
            TV.AddToolbarTool("@Add_16x16.png", "Add New", AddNewReport);
            TV.AddToolbarTool("@Glass_16x16.png", "Open Folder in File Explorer", ViewFolderFilesFromTool, System.Windows.Visibility.Visible, Path);
        }

        private void RefreshItems(object sender, System.Windows.RoutedEventArgs e)
        {
            RefreshChildrens();
        }
        
        private void SaveAll(object sender, System.Windows.RoutedEventArgs e)
        {
        }

        private void AddNewReport(object sender, System.Windows.RoutedEventArgs e)
        {
            HTMLReportTemplateTreeItem r = new HTMLReportTemplateTreeItem();
            r.HTMLReportTemplate = App.UserProfile.Solution.CreateNewHTMLReportTemplate();
                                        
            mTV.Tree.AddChildItemAndSelect(this, r);            
        }
        
        private void RefreshChildrens()
        {
            //App.LocalRepository.RefreshSolutionReportTemapltesCache(specificFolderPath: Path);
            //mTV.Tree.RefreshSelectedTreeNodeChildrens();
        }       
    }
}