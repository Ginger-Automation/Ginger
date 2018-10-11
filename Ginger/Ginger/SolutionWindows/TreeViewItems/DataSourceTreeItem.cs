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
using Ginger.DataSource;
using GingerWPF.UserControlsLib.UCTreeView;
using GingerCore;
using GingerCore.DataSource;
using GingerCore.SourceControl;
using System;
using System.IO;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Amdocs.Ginger.Common.Enums;
using GingerWPF.TreeViewItemsLib;
using Amdocs.Ginger.Repository;
using amdocs.ginger.GingerCoreNET;

namespace Ginger.SolutionWindows.TreeViewItems
{
    class DataSourceTreeItem : NewTreeViewItemBase, ITreeViewItem
    {
        public DataSourceBase DSDetails { get; set; }
        private DataSourcePage mDataSourcePage;

        public string Folder { get; set; }
        public string Path { get; set; }

        Object ITreeViewItem.NodeObject()
        {
            return DSDetails;
        }
        override public string NodePath()
        {
            return DSDetails.FileName;
        }
        override public Type NodeObjectType()
        {
            return typeof(DataSourceBase);
        }

        StackPanel ITreeViewItem.Header()
        {
            return NewTVItemHeaderStyle(DSDetails);
        }

        List<ITreeViewItem> ITreeViewItem.Childrens()
        {
             List<ITreeViewItem> Childrens = new List<ITreeViewItem>();
            
            if (mDataSourcePage == null)
            {
                mDataSourcePage = new DataSourcePage(DSDetails);
            }            
            //Add Data Sources
            DSDetails.DSTableList = mDataSourcePage.GetTableList();
            if (DSDetails.DSTableList == null)
                DSDetails.DSTableList = new ObservableList<DataSourceTable>();
            
            foreach (DataSourceTable dsTable in DSDetails.DSTableList)
            {
                DataSourceTableTreeItem DSTTI = new DataSourceTableTreeItem();                
                DSTTI.DSTableDetails = dsTable;
                DSTTI.DSDetails= DSDetails;
                Childrens.Add(DSTTI);
            }

            return Childrens;    
        }

        bool ITreeViewItem.IsExpandable()
        {
            return true;
        }

        Page ITreeViewItem.EditPage()
        {
            if (mDataSourcePage == null)
            {
                mDataSourcePage = new DataSourcePage(DSDetails);
            }
            return mDataSourcePage;
        }

        ContextMenu ITreeViewItem.Menu()
        {
            return mContextMenu;
        }

        void ITreeViewItem.SetTools(ITreeView TV)
        {
            mTreeView = TV;
            mContextMenu = new ContextMenu();
                        
            TreeViewUtils.AddMenuItem(mContextMenu, "Refresh", RefreshItems, null, eImageType.Refresh);
            TV.AddToolbarTool(eImageType.Refresh, "Refresh", RefreshItems);

            TreeViewUtils.AddMenuItem(mContextMenu, "Duplicate", Duplicate, null, "@Duplicate_16x16.png");
            TV.AddToolbarTool("@Duplicate_16x16.png", "Duplicate", new RoutedEventHandler(Duplicate));

            TreeViewUtils.AddMenuItem(mContextMenu, "Add New Table", AddNewTable, null, "@Add_16x16.png");
            TV.AddToolbarTool("@Add_16x16.png", "Add New Table", AddNewTable);

            TreeViewUtils.AddMenuItem(mContextMenu, "Commit All", CommitAll,null, "@Commit_16x16.png");
            TV.AddToolbarTool("@Commit_16x16.png", "Commit All", new RoutedEventHandler(CommitAll));

            TreeViewUtils.AddMenuItem(mContextMenu, "Rename", Rename,null,"@Edit_16x16.png");
            TV.AddToolbarTool("@Edit_16x16.png", "Rename", new RoutedEventHandler(Rename));

            TreeViewUtils.AddMenuItem(mContextMenu, "Delete", Delete,null, "@Trash_16x16.png");
            TV.AddToolbarTool("@Trash_16x16.png", "Delete", new RoutedEventHandler(Delete));

            TreeViewUtils.AddMenuItem(mContextMenu, "Export to Excel", ExportToExcel, null, "@Export_16x16.png");
            TV.AddToolbarTool("@Export_16x16.png", "Export to Excel", new RoutedEventHandler(ExportToExcel));

            AddSourceControlOptions(mContextMenu);
        }  
        
        private void AddNewTable(object sender, System.Windows.RoutedEventArgs e)
        {
            AddNewTablePage ANTP = new AddNewTablePage();
            ANTP.ShowAsWindow();

            DataSourceTable dsTableDetails = ANTP.DSTableDetails;            

            if (dsTableDetails != null)
            {
                dsTableDetails.DSC = DSDetails.DSC;
                DataSourceTableTreeItem DSTTI = new DataSourceTableTreeItem();
                DSTTI.DSTableDetails = dsTableDetails;
                DSTTI.DSDetails = DSDetails;

              if (dsTableDetails.DSTableType == DataSourceTable.eDSTableType.GingerKeyValue)
                    dsTableDetails.DSC.AddTable(dsTableDetails.Name, "[GINGER_ID] AUTOINCREMENT,[GINGER_KEY_NAME] Text,[GINGER_KEY_VALUE] Text,[GINGER_LAST_UPDATED_BY] Text,[GINGER_LAST_UPDATE_DATETIME] Text");
                else if(dsTableDetails.DSTableType == DataSourceTable.eDSTableType.Customized)
                    dsTableDetails.DSC.AddTable(dsTableDetails.Name, "[GINGER_ID] AUTOINCREMENT,[GINGER_USED] Text,[GINGER_LAST_UPDATED_BY] Text,[GINGER_LAST_UPDATE_DATETIME] Text");

                mTreeView.Tree.AddChildItemAndSelect(this, DSTTI);
                DSDetails.DSTableList.Add(dsTableDetails);
            }
        }

        private void ExportToExcel(object sender, System.Windows.RoutedEventArgs e)
        {                      
            Ginger.DataSource.DataSourceExportToExcel DSEE = new Ginger.DataSource.DataSourceExportToExcel();
            DSEE.ShowAsWindow();

            string SolutionFolder = App.UserProfile.Solution.Folder.ToUpper();
            string sExcelPath = DSEE.ExcelPath;
            string sSheetName = DSEE.SheetName;

            if (sExcelPath != "")
            {
                if (sExcelPath.Contains(SolutionFolder))
                {
                    sExcelPath = sExcelPath.Replace(SolutionFolder, @"~\");
                }
            }
            if (sExcelPath == "")
            {
                return;
            }

            if (DSDetails.DSTableList == null)
            {
                if (mDataSourcePage == null)
                {
                    mDataSourcePage = new DataSourcePage(DSDetails);
                }
                //Add Data Sources
                DSDetails.DSTableList = mDataSourcePage.GetTableList();
            }

            foreach (DataSourceTable dsTable in DSDetails.DSTableList)
            {
                Reporter.ToGingerHelper(eGingerHelperMsgKey.ExportItem, null, dsTable.Name, "Data Source Table");
                dsTable.DSC.ExporttoExcel(dsTable.Name, sExcelPath, dsTable.Name);                    
                Reporter.CloseGingerHelper();
            }
        }

        private void RefreshItems(object sender, RoutedEventArgs e)
        {
            RefreshTreeItems();            
        }
        private void RefreshTreeItems()
        {
            mTreeView.Tree.RefreshSelectedTreeNodeChildrens();
            foreach(DataSourceTable dsTable in DSDetails.DSTableList)
            {
                if(dsTable.DataTable != null)
                    dsTable.DataTable.RejectChanges();
            }
        }

        private void Delete(object sender, RoutedEventArgs e)
        {            
            base.DeleteTreeItem(DSDetails);            
            if (File.Exists(DSDetails.FileFullPath))
            {
                DSDetails.DSC.Close();
                try
                {
                    File.Delete(DSDetails.FileFullPath);
                }
                catch(Exception ex)
                {
                    Reporter.ToLog(eAppReporterLogLevel.WARN, "Error while deleteing Data Soucre File", ex);
                    Reporter.ToUser(eUserMsgKeys.DeleteDSFileError, DSDetails.FileFullPath);                    
                }
            }
                
        }

        private void CommitAll(object sender, RoutedEventArgs e)
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
        
        private void Duplicate(object sender, RoutedEventArgs e)
        {   
            AccessDataSource dsDetailsCopy = (AccessDataSource)CopyTreeItemWithNewName((RepositoryItemBase)DSDetails);
            dsDetailsCopy.FilePath = DSDetails.ContainingFolder + "\\" + dsDetailsCopy.Name + ".mdb";
            dsDetailsCopy.FileFullPath = DSDetails.ContainingFolderFullPath + "\\"+ dsDetailsCopy.Name + ".mdb";

            if (File.Exists(dsDetailsCopy.FileFullPath))
            { Reporter.ToUser(eUserMsgKeys.DuplicateDSDetails, dsDetailsCopy.FileFullPath); return; }

            File.Copy(DSDetails.FileFullPath, dsDetailsCopy.FileFullPath);            

            (WorkSpace.Instance.SolutionRepository.GetItemRepositoryFolder(((RepositoryItemBase)DSDetails))).AddRepositoryItem(dsDetailsCopy);

        }
        private void Rename(object sender, RoutedEventArgs e)
        {
            RenameItem("DataSource Name:", DSDetails, DataSourceBase.Fields.Name);
        }
    }
}
