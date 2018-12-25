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
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using GingerCore;
using GingerCore.Drivers;
using GingerCore.Drivers.Appium;
using GingerCore.Drivers.ConsoleDriverLib;
using GingerCore.Drivers.InternalBrowserLib;
using Ginger.UserControls;
using GingerCore.Drivers.ASCF;
using GingerCore.DataSource;
using Ginger.Actions;
using GingerCore.Drivers.PBDriver;
using Ginger.WindowExplorer;
using GingerCore.Drivers.WebServicesDriverLib;
using GingerCore.Drivers.WindowsLib;
using GingerCore.Drivers.JavaDriverLib;
using Ginger.Drivers;
using GingerCore.Drivers.MainFrame;
using GingerCore.Drivers.AndroidADB;
using GingerCore.GeneralLib;

namespace Ginger.DataSource
{
    /// <summary>
    /// Interaction logic for AgentEditPage.xaml
    /// </summary>
    public partial class DataSourcePage : Page
    {
        DataSourceBase mDSDetails;
        ObservableList<DataSourceTable> mDSTableList;

        public DataSourcePage(DataSourceBase dsDetails)
        {
            InitializeComponent();
           
            if (dsDetails != null)    
            {
                mDSDetails = dsDetails;
                App.ObjFieldBinding(DataSourceNameTextBox, TextBox.TextProperty, mDSDetails, DataSourceBase.Fields.Name);
                App.ObjFieldBinding(txtDataSourcePath, TextBox.TextProperty, mDSDetails, DataSourceBase.Fields.FilePath, BindingMode.OneWay);
                App.ObjFieldBinding(DataSourceTypeTextBox, TextBox.TextProperty, mDSDetails, DataSourceBase.Fields.DSType, BindingMode.OneWay);
                                
                SetGridView();
                SetGridData();                
            }
            grdTableList.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddTable));
            grdTableList.btnEdit.AddHandler(Button.ClickEvent, new RoutedEventHandler(RenameTable));
            grdTableList.AddToolbarTool("@Delete_16x16.png", "Delete Table", new RoutedEventHandler(DeleteSelectedTables));
            grdTableList.AddToolbarTool("@Trash_16x16.png", "Delete All Tables", new RoutedEventHandler(DeleteAllTables));
            grdTableList.btnRefresh.AddHandler(Button.ClickEvent, new RoutedEventHandler(RefreshTableList));
            grdTableList.AddToolbarTool("@Commit_16x16.png", "Commit All", new RoutedEventHandler(SaveAllTables));
        }
        #region Functions
        private void SetGridView()
        {
            //Set the grid name
            grdTableList.Title = "'" + mDSDetails.Name + "' Tables List";
            //Set the Tool Bar look
            
            grdTableList.ShowUpDown = Visibility.Collapsed;            
            grdTableList.ShowUndo = Visibility.Collapsed;

            //Set the Data Grid columns            
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();

            view.GridColsView.Add(new GridColView() { Field = DataSourceTable.Fields.Name, Header = "Table Name", WidthWeight = 150 });
            view.GridColsView.Add(new GridColView() { Field = DataSourceTable.Fields.DSTableType,Header = "Table Type", WidthWeight = 150 });

            grdTableList.SetAllColumnsDefaultView(view);
            grdTableList.InitViewItems();
        }
        
        private void SetGridData()
        {
            mDSTableList = mDSDetails.DSC.GetTablesList();           
            grdTableList.DataSourceList = mDSTableList;           
        }
        
        private void DeleteSelectedTables(object sender, RoutedEventArgs e)
        {
            if (grdTableList.Grid.SelectedItems.Count == 0)
            {
                Reporter.ToUser(eUserMsgKeys.SelectItemToDelete);
                return;
            }
            List<object> SelectedItemsList = grdTableList.Grid.SelectedItems.Cast<object>().ToList();

            foreach (object o in SelectedItemsList)
            {
                if (Reporter.ToUser(eUserMsgKeys.DeleteRepositoryItemAreYouSure, ((DataSourceTable)o).GetNameForFileName()) == MessageBoxResult.Yes)
                {
                    ((DataSourceTable)o).DSC.DeleteTable(((DataSourceTable)o).Name);
                }
            }
            SetGridData();
        }
        private void DeleteAllTables(object sender, RoutedEventArgs e)
        {
            if (grdTableList.Grid.Items.Count == 0)
            {
                Reporter.ToUser(eUserMsgKeys.NoItemToDelete);
                return;
            }
            if ((Reporter.ToUser(eUserMsgKeys.SureWantToDeleteAll)) == MessageBoxResult.Yes)
            {
                mDSTableList = mDSDetails.DSC.GetTablesList();
                foreach (DataSourceTable dsTable in mDSTableList)
                    mDSDetails.DSC.DeleteTable(dsTable.Name);

                SetGridData();
            }           
        }
        private void SaveAllTables(object sender, RoutedEventArgs e)
        {
            mDSTableList = mDSDetails.DSC.GetTablesList();
            foreach (DataSourceTable dsTable in mDSTableList)
            {
                if (dsTable.DataTable != null)
                    dsTable.DSC.SaveTable(dsTable.DataTable);

            }
        }

        private void RefreshTableList(object sender, RoutedEventArgs e)
        {
            SetGridData();
        }

        private void RenameTable(object sender, RoutedEventArgs e)
        {
            if (grdTableList.Grid.SelectedItems.Count == 0)
            {
                Reporter.ToUser(eUserMsgKeys.AskToSelectItem);
                return;
            }
            List<object> SelectedItemsList = grdTableList.Grid.SelectedItems.Cast<object>().ToList();

            foreach (object o in SelectedItemsList)
            {
                if (Reporter.ToUser(eUserMsgKeys.RenameRepositoryItemAreYouSure, ((DataSourceTable)o).GetNameForFileName()) == MessageBoxResult.Yes)
                {
                    string oldName = ((DataSourceTable)o).Name;
                    InputBoxWindow.OpenDialog("Rename", "Table Name:", ((DataSourceTable)o), DataSourceBase.Fields.Name);
                    ((DataSourceTable)o).DSC.RenameTable(oldName, ((DataSourceTable)o).Name);                   
                }
            }
            SetGridData();
        }

            private void AddTable(object sender, RoutedEventArgs e)
        {
            AddNewTablePage ANTP = new AddNewTablePage();
            ANTP.ShowAsWindow();

            DataSourceTable dsTableDetails = ANTP.DSTableDetails;
            

            if (dsTableDetails != null)
            {
                dsTableDetails.DSC = mDSDetails.DSC;
                                
                if (dsTableDetails.DSTableType == DataSourceTable.eDSTableType.GingerKeyValue)
                    dsTableDetails.DSC.AddTable(dsTableDetails.Name, "[GINGER_ID] AUTOINCREMENT,[GINGER_KEY_NAME] Text,[GINGER_KEY_VALUE] Text,[GINGER_LAST_UPDATED_BY] Text,[GINGER_LAST_UPDATE_DATETIME] Text");
                else if (dsTableDetails.DSTableType == DataSourceTable.eDSTableType.Customized)
                    dsTableDetails.DSC.AddTable(dsTableDetails.Name, "[GINGER_ID] AUTOINCREMENT,[GINGER_USED] Text,[GINGER_LAST_UPDATED_BY] Text,[GINGER_LAST_UPDATE_DATETIME] Text");
                
                mDSTableList.Add(dsTableDetails);
            }           
        }
        
        #endregion Functions
    }
}
