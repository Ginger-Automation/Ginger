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
    class HTMLReportTemplatesTreeItem : NewTreeViewItemBase, ITreeViewItem
    {
        private HTMLReportTemplatesPage mReportTemplatesPage;
        RepositoryFolder<HTMLReportTemplate> mReportFolder;        
        ITreeView mTV;

        public HTMLReportTemplatesTreeItem(RepositoryFolder<HTMLReportTemplate> reportFolder)
        {
            mReportFolder = reportFolder;
        }

        Object ITreeViewItem.NodeObject()
        {
            return null;
        }

        StackPanel ITreeViewItem.Header()
        {            
             return TreeViewUtils.NewRepositoryItemTreeHeader(mReportFolder, nameof(RepositoryFolder<HTMLReportTemplate>.DisplayName), eImageType.HtmlReport, GetSourceControlImage(mReportFolder), false);
        }

        List<ITreeViewItem> ITreeViewItem.Childrens()
        {
            return GetChildrentGeneric<HTMLReportTemplate>(mReportFolder);           
        }

        bool ITreeViewItem.IsExpandable()
        {
            return true;
        }

        Page ITreeViewItem.EditPage(Amdocs.Ginger.Common.Context mContext)
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
            TreeViewUtils.AddMenuItem(CM, "Save All", SaveAll, null, eImageType.Save);
            AddViewFolderFilesMenuItem(CM, mReportFolder.FolderFullPath);
            return CM;
        }

        void ITreeViewItem.SetTools(ITreeView TV)
        {
            mTV = TV;            
            TV.AddToolbarTool("@SaveAll_16x16.png", "Save All", SaveAll);
            TV.AddToolbarTool("@Add_16x16.png", "Add New", AddNewReport);
            TV.AddToolbarTool("@Glass_16x16.png", "Open Folder in File Explorer", ViewFolderFilesFromTool, System.Windows.Visibility.Visible, mReportFolder.FolderFullPath);
        }

        
        private void SaveAll(object sender, System.Windows.RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void AddNewReport(object sender, System.Windows.RoutedEventArgs e)
        {
            HTMLReportTemplateTreeItem r = new HTMLReportTemplateTreeItem();
            r.HTMLReportTemplate = CreateNewHTMLReportTemplate();
                                        
            mTV.Tree.AddChildItemAndSelect(this, r);            
        }
        
      

        internal HTMLReportTemplate CreateNewHTMLReportTemplate(string path = "")
        {
            //TODO: change to wizard
            HTMLReportTemplate NewReportTemplate = new HTMLReportTemplate() { Name = "New Report Template", Status = HTMLReportTemplate.eReportStatus.Development };

            HTMLReportTemplateSelector RTS = new HTMLReportTemplateSelector();
            RTS.ShowAsWindow();

            if (RTS.SelectedReportTemplate != null)
            {
                NewReportTemplate.HTML = RTS.SelectedReportTemplate.HTML;

                //Make it Generic or Const string for names used for File
                string NewReportName = string.Empty;
                if (GingerCore.General.GetInputWithValidation("Add New Report", "Report Name:", ref NewReportName, null, false, NewReportTemplate))
                {
                    NewReportTemplate.Name = NewReportName;
                    WorkSpace.Instance.SolutionRepository.AddRepositoryItem(NewReportTemplate);                                        
                }
                return NewReportTemplate;
            }
            return null;
        }
    }
}