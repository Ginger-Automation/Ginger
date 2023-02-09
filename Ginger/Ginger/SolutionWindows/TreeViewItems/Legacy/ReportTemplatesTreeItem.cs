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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Repository;
using Ginger.Reports;
using GingerWPF.TreeViewItemsLib;
using GingerWPF.UserControlsLib.UCTreeView;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace Ginger.SolutionWindows.TreeViewItems
{
    class ReportTemplatesTreeItem : NewTreeViewItemBase, ITreeViewItem
    {
        private ReportTemplatesPage mReportTemplatesPage;

        RepositoryFolder<ReportTemplate> mReportFolder;
        
        ITreeView mTV;

        public ReportTemplatesTreeItem(RepositoryFolder<ReportTemplate> reportFolder)
        {
            mReportFolder = reportFolder;
        }

        Object ITreeViewItem.NodeObject()
        {
            return null;
        }

        StackPanel ITreeViewItem.Header()
        {            
            return TreeViewUtils.NewRepositoryItemTreeHeader("Document Report Templates", nameof(RepositoryFolder<ReportTemplate>.DisplayName), eImageType.Report, GetSourceControlImage(mReportFolder), false);
        }

        List<ITreeViewItem> ITreeViewItem.Childrens()
        {
            return GetChildrentGeneric<ReportTemplate>(mReportFolder);         
        }

        bool ITreeViewItem.IsExpandable()
        {
            return true;
        }

        Page ITreeViewItem.EditPage(Amdocs.Ginger.Common.Context mContext)
        {
            if (mReportTemplatesPage == null)
            {
                mReportTemplatesPage = new ReportTemplatesPage();
            }
            return mReportTemplatesPage;
        }

        ContextMenu ITreeViewItem.Menu()
        {
            ContextMenu CM = new ContextMenu();
            TreeViewUtils.AddMenuItem(CM, "Refresh", RefreshItems, null, eImageType.Refresh);
            TreeViewUtils.AddMenuItem(CM, "Save All", SaveAll, null, eImageType.Save);
            
            TreeViewUtils.AddMenuItem(CM, "Set Default Report Template", SetDefaultTemplate, null, eImageType.Check);
            AddViewFolderFilesMenuItem(CM, mReportFolder.FolderFullPath);
            return CM;
        }

        void ITreeViewItem.SetTools(ITreeView TV)
        {
            mTV = TV;
            TV.AddToolbarTool(eImageType.Refresh, "Refresh", RefreshItems);
            TV.AddToolbarTool("@SaveAll_16x16.png", "Save All", SaveAll);
            TV.AddToolbarTool("@Add_16x16.png", "Add New", AddNewReport);
            TV.AddToolbarTool("@Glass_16x16.png", "Open Folder in File Explorer", ViewFolderFilesFromTool, System.Windows.Visibility.Visible, mReportFolder.FolderFullPath);
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
            ReportTemplateTreeItem r = new ReportTemplateTreeItem();
            r.ReportTemplate = (Ginger.Reports.ReportTemplate) WorkSpace.Instance.Solution.CreateNewReportTemplate();
            if (r.ReportTemplate!= null)                       
                mTV.Tree.AddChildItemAndSelect(this, r);            
        }

        private void SetDefaultTemplate(object sender, System.Windows.RoutedEventArgs e)
        {
            ReportTemplateSelector RTS = new ReportTemplateSelector();
            RTS.ShowAsWindow();
            if (RTS.SelectedReportTemplate != null)
            {
                 WorkSpace.Instance.UserProfile.ReportTemplateName = RTS.SelectedReportTemplate.Name;
            }
        }

        private void RefreshChildrens()
        {
            //App.LocalRepository.RefreshSolutionReportTemapltesCache(specificFolderPath: Path);
            //mTV.Tree.RefreshSelectedTreeNodeChildrens();
        }
    }
}