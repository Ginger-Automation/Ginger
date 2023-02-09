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

using Ginger.DataSource;
using GingerWPF.UserControlsLib.UCTreeView;
using GingerCore;
using GingerCore.DataSource;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Amdocs.Ginger.Common.Enums;
using GingerWPF.TreeViewItemsLib;
using Amdocs.Ginger.Common;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Repository;
using System.Linq;

namespace Ginger.SolutionWindows.TreeViewItems
{
    class DataSourceTableTreeItem : NewTreeViewItemBase, ITreeViewItem
    {
        public DataSourceBase DSDetails { get; set; }
        public DataSourceTable DSTableDetails { get; set; }
        private DataSourceTablePage mDataSourceTablePage;        

        public string Folder { get; set; }
        public string Path { get; set; }

        Object ITreeViewItem.NodeObject()
        {
            return DSTableDetails;
        }
        override public string NodePath()
        {
            return DSTableDetails.FileName;
        }
        override public Type NodeObjectType()
        {
            return typeof(DataSourceTable);
        }

        StackPanel ITreeViewItem.Header()
        {
            return NewTVItemHeaderStyle(DSTableDetails, eImageType.DataTable, nameof(DSDetails.Name));            
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
            DSDetails.SaveBackup();//to mark the Data Source as changed
            DSDetails.StartDirtyTracking();
            if (mDataSourceTablePage == null)
            {
                mDataSourceTablePage = new DataSourceTablePage(DSTableDetails);                
            }           
            return mDataSourceTablePage;
        }

        ContextMenu ITreeViewItem.Menu()
        {
            return mContextMenu;
        }

        void ITreeViewItem.SetTools(ITreeView TV)
        {
            mTreeView = TV;
            mContextMenu = new ContextMenu();

            TreeViewUtils.AddMenuItem(mContextMenu, "Refresh", RefreshItems,null, eImageType.Refresh);
            TV.AddToolbarTool(eImageType.Refresh, "Refresh", new RoutedEventHandler(RefreshItems));

            TreeViewUtils.AddMenuItem(mContextMenu, "Commit", Commit,null,"@Commit_16x16.png");
            TV.AddToolbarTool("@Commit_16x16.png", "Commit", new RoutedEventHandler(Commit));

            TreeViewUtils.AddMenuItem(mContextMenu, "Rename", Rename, null, "@Edit_16x16.png");
            TV.AddToolbarTool("@Edit_16x16.png", "Rename", new RoutedEventHandler(Rename));

            TreeViewUtils.AddMenuItem(mContextMenu, "Duplicate", Duplicate, null, "@Duplicate_16x16.png");
            TV.AddToolbarTool("@Duplicate_16x16.png", "Duplicate", new RoutedEventHandler(Duplicate));

            TreeViewUtils.AddMenuItem(mContextMenu, "Delete", DeleteTable,null, "@Trash_16x16.png");
            TV.AddToolbarTool("@Trash_16x16.png", "Delete", new RoutedEventHandler(DeleteTable));

            TreeViewUtils.AddMenuItem(mContextMenu, "Export to Excel", ExportToExcel, null, "@Export_16x16.png");
            TV.AddToolbarTool("@Export_16x16.png", "Export to Excel", new RoutedEventHandler(ExportToExcel));

        }

        private void RefreshItems(object sender, RoutedEventArgs e)
        {   
            if (Reporter.ToUser(eUserMsgKey.LooseLocalChanges) == Amdocs.Ginger.Common.eUserMsgSelection.No)
            {
                return;
            }
            mTreeView.Tree.RefreshSelectedTreeNodeChildrens();
            if (mDataSourceTablePage != null)
            {
                mDataSourceTablePage.RefreshGrid();
            }
            DSTableDetails.DataTable.RejectChanges();
            DSTableDetails.DirtyStatus = eDirtyStatus.NoChange;                        
        }
               
        private void DeleteTable(object sender, RoutedEventArgs e)
        {
            if (Reporter.ToUser(eUserMsgKey.DeleteRepositoryItemAreYouSure, DSTableDetails.GetNameForFileName()) == Amdocs.Ginger.Common.eUserMsgSelection.Yes)
            {
                DeleteTreeItem();
            }            
        }

        public override bool DeleteTreeItem(object item, bool deleteWithoutAsking = false, bool refreshTreeAfterDelete = true)
        {
            var repoItem = item as RepositoryItemBase;
            if (repoItem != null)
            {
                if (!deleteWithoutAsking)
                {
                    if (Reporter.ToUser(eUserMsgKey.DeleteItem, repoItem.GetNameForFileName()) == Amdocs.Ginger.Common.eUserMsgSelection.No)
                    {
                        return false;
                    }
                }
            }
            DeleteTreeItem();
            return true;
        }

        public void DeleteTreeItem()
        {
            mTreeView.Tree.DeleteItemAndSelectParent(this);
            DSDetails.DSTableList.Remove(DSTableDetails);
            DSTableDetails.DSC.DeleteTable(DSTableDetails.Name);
        }


        private void ExportToExcel(object sender, RoutedEventArgs e)
        {                      
            Ginger.DataSource.DataSourceExportToExcel DSEE = new Ginger.DataSource.DataSourceExportToExcel(DSTableDetails);
            DSEE.ShowAsWindow();

            string SolutionFolder =  WorkSpace.Instance.Solution.Folder.ToUpper();
            string sExcelPath = DSEE.mExcelConfig.ExcelPath;
            string sSheetName = DSEE.mExcelConfig.ExcelSheetName;
            string sTableQueryValue =  DSEE.mExcelConfig.ExportQueryValue;

            if(sExcelPath !="")
            {
                if (sExcelPath.Contains(SolutionFolder))
                {
                    sExcelPath = sExcelPath.Replace(SolutionFolder, @"~\");
                }
            }
            if(sExcelPath == "")
            {
                return;
            }
            if (sSheetName == "")
            {
                sSheetName= DSTableDetails.Name;
            }
            Reporter.ToStatus(eStatusMsgKey.ExportItem, null, DSTableDetails.Name, "Data Source Table");
            DSTableDetails.DSC.ExporttoExcel(DSTableDetails.Name, sExcelPath, sSheetName,sTableQueryValue);            
            Reporter.HideStatusMessage();
        }

        private void Commit(object sender, RoutedEventArgs e)
        {
            SaveTreeItem();            
        }

        public void SaveTreeItem()
        {
            if(mDataSourceTablePage != null && ItemIsDirty(this.DSTableDetails))
            {
                mDataSourceTablePage.SaveTable();
                this.DSTableDetails.ClearBackup();
                ResetDirtyStatusForDataSourceTable();
            }            
        }    

        private void ResetDirtyStatusForDataSourceTable()
        {
            this.DSTableDetails.DirtyStatus = eDirtyStatus.NoChange;
        }

        private void Rename(object sender, RoutedEventArgs e)
        {         
            if (Reporter.ToUser(eUserMsgKey.LooseLocalChanges) == Amdocs.Ginger.Common.eUserMsgSelection.No)
            {
                return;
            }
            string oldName = DSTableDetails.Name;
            string newName = DSTableDetails.Name;

            bool wasRenamed = GingerCore.GeneralLib.InputBoxWindow.OpenDialog("Rename", "Table Name:", ref newName);
            if (wasRenamed)
            {
                try
                {
                    if (!newName.Equals(oldName, StringComparison.OrdinalIgnoreCase))
                    {
                        bool tableExist = DSDetails.DSTableList.Any(t => t.Name == newName);

                        if (tableExist)
                        {
                            Reporter.ToUser(eUserMsgKey.DbTableNameError, newName);
                        }
                        else
                        {
                            DSTableDetails.DSC.RenameTable(oldName, newName);
                            DSTableDetails.Name = newName;
                            if (mDataSourceTablePage != null)
                            {
                                mDataSourceTablePage.RefreshGrid();
                            }
                        }
                    }
                    DSTableDetails.DirtyStatus = eDirtyStatus.NoChange;
                }
                catch (Exception ex)
                {
                    Reporter.ToUser(eUserMsgKey.RenameItemError, ex.Message);
                    DSTableDetails.Name = oldName;
                }
            }
        }

         private void Duplicate(object sender, RoutedEventArgs e)
         {
            if (Reporter.ToUser(eUserMsgKey.LooseLocalChanges) == Amdocs.Ginger.Common.eUserMsgSelection.No)
            {
                return;
            }
            DataSourceTable copy = (DataSourceTable)DSTableDetails.CreateCopy();
            copy.Name = DSDetails.CopyTable(DSTableDetails.Name);
            copy.DataTable = DSTableDetails.DataTable;
            copy.DSC = DSTableDetails.DSC;
            copy.DSTableType = DSTableDetails.DSTableType;

            DSDetails.DSTableList.Add(copy);
           
            mTreeView.Tree.RefreshSelectedTreeNodeParent();
        }
    }
}
