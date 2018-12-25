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
using System;
using System.IO;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Amdocs.Ginger.Common.Enums;
using GingerWPF.TreeViewItemsLib;
using Amdocs.Ginger.Repository;
using amdocs.ginger.GingerCoreNET;
using GingerWPF.WizardLib;
using Ginger.DataSource.ImportExcelWizardLib;

namespace Ginger.SolutionWindows.TreeViewItems
{
    class DataSourceTreeItem : NewTreeViewItemBase, ITreeViewItem
    {        
        public DataSourceBase DSDetails { get; set; }
        private DataSourcePage mDataSourcePage;       

        public DataSourceFolderTreeItem.eDataTableView TableTreeView { get; set; }

        public string Folder { get; set; }
        public string Path { get; set; }

        public DataSourceTreeItem(DataSourceBase dsDetails, DataSourceFolderTreeItem.eDataTableView TableView)
        {
            DSDetails = dsDetails;
            TableTreeView = TableView;
            InitDSConnection();
        }

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
            
                      
            //Get Data Source Tables List
            DSDetails.DSTableList = DSDetails.DSC.GetTablesList();
            if (DSDetails.DSTableList == null)
                DSDetails.DSTableList = new ObservableList<DataSourceTable>();
            
            foreach (DataSourceTable dsTable in DSDetails.DSTableList)
            {    
                if(TableTreeView == DataSourceFolderTreeItem.eDataTableView.All || (TableTreeView == DataSourceFolderTreeItem.eDataTableView.Key && dsTable.DSTableType == DataSourceTable.eDSTableType.GingerKeyValue) || (TableTreeView == DataSourceFolderTreeItem.eDataTableView.Customized && dsTable.DSTableType == DataSourceTable.eDSTableType.Customized))
                {
                    DataSourceTableTreeItem DSTTI = new DataSourceTableTreeItem();
                    DSTTI.DSTableDetails = dsTable;
                    DSTTI.DSDetails= DSDetails;
                    Childrens.Add(DSTTI);
                }                
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

            MenuItem importMenu = TreeViewUtils.CreateSubMenu(mContextMenu, "Add New Table");

            TreeViewUtils.AddSubMenuItem(importMenu, "Add New Customized Table", AddNewCustomizedTable, null, "@Add_16x16.png");
            TreeViewUtils.AddSubMenuItem(importMenu, "Add New Key Value Table", AddNewKeyValueTable, null, "@Add_16x16.png");
            
            TreeViewUtils.AddMenuItem(mContextMenu, "Commit All", CommitAll,null, "@Commit_16x16.png");
            TV.AddToolbarTool("@Commit_16x16.png", "Commit All", new RoutedEventHandler(CommitAll));

            TreeViewUtils.AddMenuItem(mContextMenu, "Rename", Rename,null,"@Edit_16x16.png");
            TV.AddToolbarTool("@Edit_16x16.png", "Rename", new RoutedEventHandler(Rename));

            TreeViewUtils.AddMenuItem(mContextMenu, "Delete", Delete,null, "@Trash_16x16.png");
            TV.AddToolbarTool("@Trash_16x16.png", "Delete", new RoutedEventHandler(Delete));

            TreeViewUtils.AddMenuItem(mContextMenu, "Export to Excel", ExportToExcel, null, "@Export_16x16.png");
            TV.AddToolbarTool("@Export_16x16.png", "Export to Excel", new RoutedEventHandler(ExportToExcel));

            TreeViewUtils.AddMenuItem(mContextMenu, "Import from Excel", AddNewTableFromExcel, null, eImageType.ExcelFile);

            AddSourceControlOptions(mContextMenu);
        }

        /// <summary>
        /// This method is used to add the new table from excel file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddNewTableFromExcel(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                WizardWindow.ShowWizard(new ImportDataSourceFromExcelWizard(DSDetails));
                mTreeView.Tree.RefresTreeNodeChildrens(this);                
                //   RefreshTreeItems();
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, ex.StackTrace);
            }
        }

        /// <summary>
        /// This method is used to add the new Customized table
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddNewCustomizedTable(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                string name = string.Empty;
                char[] pathChars = System.IO.Path.GetInvalidPathChars();
                char[] inValidTableNameChars = new char[pathChars.Length + 1];
                pathChars.CopyTo(inValidTableNameChars, 0);
                inValidTableNameChars[pathChars.Length] = ' ';
                if (GingerCore.GeneralLib.InputBoxWindow.GetInputWithValidation("Add New Customized Table", "Table Name", ref name, inValidTableNameChars))
                {
                    CreateTable(name, "[GINGER_ID] AUTOINCREMENT,[GINGER_USED] Text,[GINGER_LAST_UPDATED_BY] Text,[GINGER_LAST_UPDATE_DATETIME] Text", DataSourceTable.eDSTableType.Customized);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, ex.StackTrace);
            }
        }

        /// <summary>
        /// This method is used to add new KeyValue table
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddNewKeyValueTable(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                string name = string.Empty;
                char[] pathChars = System.IO.Path.GetInvalidPathChars();
                char[] inValidTableNameChars = new char[pathChars.Length+1];
                pathChars.CopyTo(inValidTableNameChars,0);
                inValidTableNameChars[pathChars.Length] = ' ';
                if (GingerCore.GeneralLib.InputBoxWindow.GetInputWithValidation("Add New Key Value Table", "Table Name", ref name, inValidTableNameChars))
                {
                    CreateTable(name, "[GINGER_ID] AUTOINCREMENT,[GINGER_KEY_NAME] Text,[GINGER_KEY_VALUE] Text,[GINGER_LAST_UPDATED_BY] Text,[GINGER_LAST_UPDATE_DATETIME] Text",DataSourceTable.eDSTableType.GingerKeyValue);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, ex.StackTrace);
            }
        }

        /// <summary>
        /// This method is used to create the table
        /// </summary>
        /// <param name="query"></param>
        private string CreateTable(string name, string query, DataSourceTable.eDSTableType DSTableType=DataSourceTable.eDSTableType.GingerKeyValue)
        {           
            string fileName = string.Empty;
            try
            {
                DataSourceTable dsTableDetails = new DataSourceTable();                
                dsTableDetails.Name = name;
                dsTableDetails.DSC = DSDetails.DSC;
                dsTableDetails.DSTableType = DSTableType;
                DataSourceTableTreeItem DSTTI = new DataSourceTableTreeItem();
                DSTTI.DSTableDetails = dsTableDetails;
                DSTTI.DSDetails = DSDetails;
                dsTableDetails.DSC.AddTable(dsTableDetails.Name, query);
                mTreeView.Tree.AddChildItemAndSelect(this, DSTTI);
                DSDetails.DSTableList.Add(dsTableDetails);

                fileName = this.DSDetails.FileFullPath;                
            }
            catch (Exception ex)
            {
                Reporter.ToUser(eUserMsgKeys.CreateTableError, ex.Message);
                Reporter.ToLog(eAppReporterLogLevel.ERROR, ex.StackTrace);
            }
            return fileName;
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
            
                
            //Add Data Source Tables List
            DSDetails.DSTableList = DSDetails.DSC.GetTablesList();

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
                    Reporter.ToLog(eAppReporterLogLevel.WARN, "Error while deleting Data Source File", ex);
                    Reporter.ToUser(eUserMsgKeys.DeleteDSFileError, DSDetails.FileFullPath);                    
                }
            }
                
        }

        private void CommitAll(object sender, RoutedEventArgs e)
        {
            SaveTreeItem(DSDetails);            
            List<ITreeViewItem> childNodes = mTreeView.Tree.GetTreeNodeChildsIncludingSubChilds((ITreeViewItem)this);
                
            foreach (ITreeViewItem node in childNodes)
            {
                if (node != null && node is DataSourceTableTreeItem)
                {
                    if(((DataSourceTable)node.NodeObject()).DirtyStatus == eDirtyStatus.Modified)
                    { 
                        ((DataSourceTableTreeItem)node).SaveTreeItem();
                    }
                }                
            }
        }        
        
        private void Duplicate(object sender, RoutedEventArgs e)
        {   
            AccessDataSource dsDetailsCopy = (AccessDataSource)CopyTreeItemWithNewName((RepositoryItemBase)DSDetails);
            if (dsDetailsCopy == null)
            { 
                return;
            }
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
        private void InitDSConnection()
        {
            if (DSDetails.DSType == DataSourceBase.eDSType.MSAccess)
            {
                DataSourceBase ADC;
                ADC = new AccessDataSource();
                if (DSDetails.FilePath.StartsWith("~"))
                {
                    DSDetails.FileFullPath = DSDetails.FilePath.Replace(@"~\", "").Replace("~", "");
                    DSDetails.FileFullPath = System.IO.Path.Combine(App.UserProfile.Solution.Folder, DSDetails.FileFullPath);
                }
                ADC.Init(DSDetails.FileFullPath);
                DSDetails.DSC = ADC;
            }
        }
    }
}
