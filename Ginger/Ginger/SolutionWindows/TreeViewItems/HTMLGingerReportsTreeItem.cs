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
using Ginger.Environments;
using Ginger.Reports;
using Ginger.Repository;
using GingerWPF.TreeViewItemsLib;
using GingerWPF.UserControlsLib.UCTreeView;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.SolutionWindows.TreeViewItems
{
    class HTMLGingerReportsTreeItem : NewTreeViewItemBase, ITreeViewItem
    {
        public RepositoryFolder<HTMLReportConfiguration> mHtmlReportsFolder;
        private HTMLReportTemplatesListPage mHTMLReportTemplatesListPage;
       

        public HTMLGingerReportsTreeItem(RepositoryFolder<HTMLReportConfiguration> htmlReportsFolder)
        {
            mHtmlReportsFolder = htmlReportsFolder;
        }

        Object ITreeViewItem.NodeObject()
        {
            return mHtmlReportsFolder;//in case of folder we return it RepositoryFolder to allow manipulating it from tree
        }

        override public string NodePath()
        {
            return mHtmlReportsFolder.FolderFullPath;
        }

        override public Type NodeObjectType()
        {
            return typeof(HTMLReportConfiguration);
        }

        StackPanel ITreeViewItem.Header()
        {           
            return NewTVItemFolderHeaderStyle(mHtmlReportsFolder);
        }

        public override ITreeViewItem GetFolderTreeItem(RepositoryFolderBase folder)
        {
            return new HTMLGingerReportsTreeItem((RepositoryFolder<HTMLReportConfiguration>)folder);
        }

        public string Folder { get; set; }
        public string Path { get; set; }


        List<ITreeViewItem> ITreeViewItem.Childrens()
        {
            //Add direct children's 
            List<ITreeViewItem> Childrens = new List<ITreeViewItem>();
            ObservableList<HTMLReportConfiguration> templates = Ginger.Reports.GingerExecutionReport.ExtensionMethods.GetSolutionHTMLReportConfigurations();                   
            templates.CollectionChanged -= TreeFolderItems_CollectionChanged;
            templates.CollectionChanged += TreeFolderItems_CollectionChanged;//adding event handler to add/remove tree items automatically based on folder items collection changes
            foreach (HTMLReportConfiguration template in templates.OrderBy(nameof(HTMLReportConfiguration.Name)))
            {
                HTMLGingerReportTreeItem RTTI = new HTMLGingerReportTreeItem(template);               
                Childrens.Add(RTTI);
            }            

            return Childrens;
        }

        public override ITreeViewItem GetTreeItem(object item)
        {
            if (item is HTMLReportConfiguration)
            {
                return new HTMLGingerReportTreeItem((HTMLReportConfiguration)item);
            }

            throw new Exception("Error unknown item added to Agents folder");
        }

        internal void AddItemHandler(object sender, RoutedEventArgs e)
        {
            AddNewTemplateItem();
        }

        bool ITreeViewItem.IsExpandable()
        {
            return true;
        }

        Page ITreeViewItem.EditPage(Amdocs.Ginger.Common.Context mContext)
        {
            if (mHTMLReportTemplatesListPage == null)
            {
                mHTMLReportTemplatesListPage = new HTMLReportTemplatesListPage();
            }
            return mHTMLReportTemplatesListPage;
        }

        ContextMenu ITreeViewItem.Menu()
        {
            return mContextMenu;
        }

        void ITreeViewItem.SetTools(ITreeView TV)
        {
            mTreeView = TV;
            mContextMenu = new ContextMenu();

            if (mHtmlReportsFolder.IsRootFolder)
                AddFolderNodeBasicManipulationsOptions(mContextMenu, nodeItemTypeName: "HTML Report Template", allowDeleteFolder: false, allowRenameFolder: false, allowRefresh: false, allowPaste: false, allowCutItems: false, allowCopyItems: false, allowAddSubFolder: false, allowDeleteAllItems: true);
            else//Not supposed to have sub folders
                AddFolderNodeBasicManipulationsOptions(mContextMenu, nodeItemTypeName: "HTML Report Template", allowRefresh: false, allowPaste: false, allowCutItems: false, allowCopyItems: false, allowRenameFolder: false, allowAddSubFolder: false, allowDeleteFolder: false);          

            TreeViewUtils.AddMenuItem(mContextMenu, "Open HTML Reports Default Folder", OpenHTMLReportsFolder, null, eImageType.OpenFolder);

            AddSourceControlOptions(mContextMenu, false, false);
        }
        

        public override void AddTreeItem()
        {
            AddNewTemplateItem();
        }

        private void AddNewTemplateItem()
        {
            HTMLReportTemplatePage mHTMLReportTemplatePage = new HTMLReportTemplatePage();
            mHTMLReportTemplatePage.ShowAsWindow();

            HTMLReportConfiguration mHTMLReportConfiguration = mHTMLReportTemplatePage.newHTMLReportConfiguration;

            if (mHTMLReportConfiguration != null)
            {
                //HTMLGingerReportTreeItem r = new HTMLGingerReportTreeItem(mHTMLReportConfiguration);
                WorkSpace.Instance.SolutionRepository.AddRepositoryItem(mHTMLReportConfiguration);
            }
            else
            {
                WorkSpace.Instance.Solution.HTMLReportsConfigurationSetList.Where(x => (x.IsSelected == true)).FirstOrDefault().HTMLReportTemplatesSeq = WorkSpace.Instance.Solution.HTMLReportsConfigurationSetList.Where(x => (x.IsSelected == true)).FirstOrDefault().HTMLReportTemplatesSeq - 1;
            }
        }

        private void OpenHTMLReportsFolder(object sender, RoutedEventArgs e)
        {
            HTMLReportsConfiguration _selectedHTMLReportConfiguration =  WorkSpace.Instance.Solution.HTMLReportsConfigurationSetList.Where(x => (x.IsSelected == true)).FirstOrDefault();
            if (_selectedHTMLReportConfiguration != null)
            {
                string path = Ginger.Reports.GingerExecutionReport.ExtensionMethods.GetReportDirectory(_selectedHTMLReportConfiguration.HTMLReportsFolder);
                if (System.IO.Directory.Exists(path))
                {
                    Process.Start(new ProcessStartInfo() { FileName = Path, UseShellExecute = true }); 
                }
            }
        }
    }
}