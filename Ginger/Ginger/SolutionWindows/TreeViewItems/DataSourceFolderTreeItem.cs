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
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Repository;
using Ginger.DataSource;
using Ginger.Repository;
using GingerCore.DataSource;
using GingerWPF.TreeViewItemsLib;
using GingerWPF.UserControlsLib.UCTreeView;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.SolutionWindows.TreeViewItems
{
    class DataSourceFolderTreeItem : NewTreeViewItemBase, ITreeViewItem
    {
        private DataSourcesPage mDataSourcesPage;
        private RepositoryFolder<DataSourceBase> mDataSourcesRepositoryFolder;

        public DataSourceFolderTreeItem(RepositoryFolder<DataSourceBase> repositoryFolder)
        {
            mDataSourcesRepositoryFolder = repositoryFolder;
        }

        
        Object ITreeViewItem.NodeObject()
        {
            return mDataSourcesRepositoryFolder;
        }
        override public string NodePath()
        {
            return mDataSourcesRepositoryFolder.FolderFullPath;
        }
        override public Type NodeObjectType()
        {
            return typeof(DataSourceBase);
        }

        StackPanel ITreeViewItem.Header()
        {
            return TreeViewUtils.NewRepositoryItemTreeHeader(mDataSourcesRepositoryFolder, nameof(RepositoryFolder<DataSourceBase>.DisplayName), eImageType.DataSource, GetSourceControlImage(mDataSourcesRepositoryFolder), false);
        }

        List<ITreeViewItem> ITreeViewItem.Childrens()
        {
            return GetChildrentGeneric<DataSourceBase>(mDataSourcesRepositoryFolder);
        }

        List<ITreeViewItem> GetChildrentGeneric<T>(RepositoryFolder<T> RF)
        {
            return GetChildrentGeneric<DataSourceBase>(mDataSourcesRepositoryFolder, nameof(DataSourceBase.Name));
           
        }

        public override ITreeViewItem GetTreeItem(object item)
        {
            if (item is DataSourceBase)
            {
                return new DataSourceTreeItem() { DSDetails = (DataSourceBase)item };
            }

            if (item is RepositoryFolderBase)
            {
                return new DataSourceFolderTreeItem((RepositoryFolder<DataSourceBase>)item);
            }

            throw new Exception("Error unknown item added to Agents folder");
        }


        //private void AddsubFolders(string sDir, List<ITreeViewItem> Childrens)
        //{
        //    //try
        //    //{
        //    //    foreach (string d in Directory.GetDirectories(Path))
        //    //    {
        //    //        DataSourceFolderTreeItem FolderItem = new DataSourceFolderTreeItem();
        //    //        string FolderName = System.IO.Path.GetFileName(d);

        //    //        FolderItem.Folder = FolderName;
        //    //        FolderItem.Path = d;

        //    //        Childrens.Add(FolderItem);
        //    //    }

        //    //}
        //    //catch (System.Exception excpt)
        //    //{
        //    //    Console.WriteLine(excpt.Message);
        //    //}
        //}

        internal void AddDataSource(object sender, RoutedEventArgs e)
        {
            AddNewDataSourcePage ADSP = new AddNewDataSourcePage(mDataSourcesRepositoryFolder);
            ADSP.ShowAsWindow();
        }

        bool ITreeViewItem.IsExpandable()
        {
            return true;
        }

        Page ITreeViewItem.EditPage()
        {
            if (mDataSourcesPage == null)
            {
                mDataSourcesPage = new DataSourcesPage();
            }
            return mDataSourcesPage;            
        }

        ContextMenu ITreeViewItem.Menu()
        {
            return mContextMenu;
        }

        void ITreeViewItem.SetTools(ITreeView TV)
        {
            mTreeView = TV;
            mContextMenu = new ContextMenu();

            if (IsGingerDefualtFolder)
                AddFolderNodeBasicManipulationsOptions(mContextMenu, "Data Source", allowCopyItems: false, allowSaveAll: false, allowCutItems: false, allowPaste: false, allowRenameFolder: false, allowDeleteFolder: false);
            else
                AddFolderNodeBasicManipulationsOptions(mContextMenu, "Data Source", allowCopyItems: false, allowSaveAll: false, allowCutItems: false, allowPaste: false);

            AddSourceControlOptions(mContextMenu, false, false);
        }

        public override void PreDeleteTreeItemHandler()
        {
            List<ITreeViewItem> childNodes = mTreeView.Tree.GetTreeNodeChildsIncludingSubChilds((ITreeViewItem)this);

            foreach (ITreeViewItem node in childNodes)
            {
                if (node != null && node is DataSourceTreeItem)
                {
                    DataSourceBase DSDetails = ((DataSourceTreeItem)node).DSDetails;
                    if (File.Exists(DSDetails.FileFullPath))
                    {
                        DSDetails.DSC.Close();                        
                    }
                }
            }
        }
        
        public override void PostSaveTreeItemHandler()
        {
            List<ITreeViewItem> childNodes = mTreeView.Tree.GetTreeNodeChildsIncludingSubChilds((ITreeViewItem)this);

            foreach (ITreeViewItem node in childNodes)
            {               
                if (node != null && node is DataSourceTableTreeItem)
                {                    
                    ((DataSourceTableTreeItem)node).SaveTreeItem();
                }
            }
            
        }
        public override void AddTreeItem()
        {            
            AddNewDataSourcePage ADSP = new AddNewDataSourcePage(mDataSourcesRepositoryFolder);
            ADSP.ShowAsWindow();
           
            mTreeView.Tree.RefresTreeNodeChildrens(this);
        }
    }
}