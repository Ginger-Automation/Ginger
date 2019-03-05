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
using System;
using System.Windows;
using System.Windows.Controls;
using Ginger.UserControls;
using GingerCore.Environments;
using GingerCore;
using Amdocs.Ginger.Common.Enums;
using amdocs.ginger.GingerCoreNET;
using GingerCore.DataSource;
using Amdocs.Ginger.Common.InterfacesLib;

namespace Ginger.Environments
{
    /// <summary>
    /// Interaction logic for AppDataBasesWindow.xaml
    /// </summary>
    public partial class AppDataBasesPage : Page
    {
        public EnvApplication AppOwner { get; set; }
        public AppDataBasesPage(EnvApplication applicationOwner)
        {
            InitializeComponent();
            AppOwner = applicationOwner;
            //Set grid look and data
            SetGridView();
            SetGridData();
            //Added for Encryption
            if (grdAppDbs.grdMain != null)
            {
                grdAppDbs.grdMain.CellEditEnding += grdMain_CellEditEnding;
            }
        }

        private void grdMain_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.Column.Header.ToString() == "User Password")
            {
                Database selectedEnvDB = (Database)grdAppDbs.CurrentItem;
                String intialValue = selectedEnvDB.Pass;
                if (!string.IsNullOrEmpty(intialValue))
                {
                    bool res = false;
                    if (!EncryptionHandler.IsStringEncrypted(intialValue))
                    {
                        selectedEnvDB.Pass = EncryptionHandler.EncryptString(intialValue, ref res);
                        if (res == false)
                        {
                            selectedEnvDB.Pass = null;
                        }
                    }                   
                }
            }
        }

        #region Events
        private void TestDBConnection(object sender, RoutedEventArgs e)
        {
            try
            {
                Database db = (Database)grdAppDbs.grdMain.CurrentItem;
                if (db == null)
                {
                    Reporter.ToUser(eUserMsgKey.AskToSelectItem);
                    return;
                }
                db.DSList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>();
                db.ProjEnvironment = App.AutomateTabEnvironment;
                db.BusinessFlow =  null;
                if (string.IsNullOrEmpty(db.ConnectionString) && !string.IsNullOrEmpty(db.TNS) && db.TNS.ToLower().Contains("data source=") && db.TNS.ToLower().Contains("password=") && db.TNS.ToLower().Contains("user id="))
                {
                    System.Data.SqlClient.SqlConnectionStringBuilder scSB = new System.Data.SqlClient.SqlConnectionStringBuilder();
                    scSB.ConnectionString = db.TNS;
                    db.TNS = scSB.DataSource;
                    db.User = scSB.UserID;
                    db.Pass = scSB.Password;
                    db.ConnectionString = scSB.ConnectionString;
                }

                db.CloseConnection();
                if (db.Connect(true))
                {
                    Reporter.ToUser(eUserMsgKey.DbConnSucceed);
                }
                else
                {
                    Reporter.ToUser(eUserMsgKey.DbConnFailed);
                }
                db.CloseConnection();
            }
            catch (Exception ex)
            {
                if (ex.Message.ToUpper().Contains("COULD NOT LOAD FILE OR ASSEMBLY 'ORACLE.MANAGEDDATAACCESS"))
                {
                    if (Reporter.ToUser(eUserMsgKey.OracleDllIsMissing, AppDomain.CurrentDomain.BaseDirectory) == Amdocs.Ginger.Common.eUserMsgSelection.Yes)
                    {
                        System.Diagnostics.Process.Start("https://docs.oracle.com/database/121/ODPNT/installODPmd.htm#ODPNT8149");
                        System.Diagnostics.Process.Start("http://www.oracle.com/technetwork/topics/dotnet/downloads/odacdeploy-4242173.html");
                        
                    }
                    return;
                }

                Reporter.ToUser(eUserMsgKey.ErrorConnectingToDataBase, ex.Message);
            }
        }
        #endregion Events

        #region Functions
        private void SetGridView()
        {
            //Set the Tool Bar look
            grdAppDbs.ShowEdit = Visibility.Collapsed;
            grdAppDbs.ShowUpDown = Visibility.Collapsed;
            grdAppDbs.ShowUndo = Visibility.Visible;
            grdAppDbs.ShowHeader = Visibility.Collapsed;
            grdAppDbs.AddToolbarTool(eImageType.DataSource, "Test Connection", new RoutedEventHandler(TestDBConnection));

            grdAppDbs.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddNewDB));

            //Set the Data Grid columns
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();

            view.GridColsView.Add(new GridColView() { Field = Database.Fields.Name, WidthWeight = 20 });
            view.GridColsView.Add(new GridColView() { Field = Database.Fields.Description, WidthWeight = 30 });
            view.GridColsView.Add(new GridColView() { Field = Database.Fields.DBVer, Header = "Version", WidthWeight = 10 });
            view.GridColsView.Add(new GridColView() { Field = Database.Fields.Type, WidthWeight = 10, StyleType = GridColView.eGridColStyleType.ComboBox, CellValuesList = Database.DbTypes, Header = "DB Type" });
            view.GridColsView.Add(new GridColView() { Field = Database.Fields.TNS, Header="TNS / File Path / Host ", WidthWeight = 30 });
            view.GridColsView.Add(new GridColView() { Field = "VE1", Header="...", WidthWeight = 5, MaxWidth = 30, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.appDataBasesWindowGrid.Resources["TNSValueExpressionButton"] });
            view.GridColsView.Add(new GridColView() { Field = Database.Fields.User, Header="User Name", WidthWeight = 10 });
            view.GridColsView.Add(new GridColView() { Field = "VE2", Header = "...", WidthWeight = 5, MaxWidth = 30, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.appDataBasesWindowGrid.Resources["UserValueExpressionButton"] });
            view.GridColsView.Add(new GridColView() { Field = Database.Fields.Pass,Header="User Password", WidthWeight = 10 });
            view.GridColsView.Add(new GridColView() { Field = "VE3", Header = "...", WidthWeight = 5, MaxWidth = 30, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.appDataBasesWindowGrid.Resources["PswdValueExpressionButton"] });
            view.GridColsView.Add(new GridColView() { Field = Database.Fields.ConnectionString, WidthWeight = 20, Header = "Connection String (Optional)" });
            view.GridColsView.Add(new GridColView() { Field = "VE4", Header = "...", WidthWeight = 5, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.appDataBasesWindowGrid.Resources["ConnStrValueExpressionButton"] });
            view.GridColsView.Add(new GridColView() { Field = Database.Fields.KeepConnectionOpen, Header = "Keep Connection Open" , StyleType= GridColView.eGridColStyleType.CheckBox, MaxWidth = 150, WidthWeight=10 });

            grdAppDbs.SetAllColumnsDefaultView(view);
            grdAppDbs.InitViewItems();
        }

        private void AddNewDB(object sender, RoutedEventArgs e)
        {
            Database db = new Database();
            db.Name = "New";
            grdAppDbs.DataSourceList.Add(db);
        }

        private void SetGridData()
        {
            grdAppDbs.DataSourceList = AppOwner.Dbs;
        }
        #endregion Functions

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            grdAppDbs.grdMain.CommitEdit();
            grdAppDbs.grdMain.CancelEdit();
        }

        private void GridTNSVEButton_Click(object sender, RoutedEventArgs e)
        {
            Database selectedEnvDB = (Database)grdAppDbs.CurrentItem;
            ValueExpressionEditorPage VEEW = new ValueExpressionEditorPage(selectedEnvDB, Database.Fields.TNS, null);
            VEEW.ShowAsWindow();
        }

        private void GridUserVEButton_Click(object sender, RoutedEventArgs e)
        {
            Database selectedEnvDB = (Database)grdAppDbs.CurrentItem;
            ValueExpressionEditorPage VEEW = new ValueExpressionEditorPage(selectedEnvDB, Database.Fields.User, null);
            VEEW.ShowAsWindow();
        }

        private void GridPswdVEButton_Click(object sender, RoutedEventArgs e)
        {
            Database selectedEnvDB = (Database)grdAppDbs.CurrentItem;
            ValueExpressionEditorPage VEEW = new ValueExpressionEditorPage(selectedEnvDB, Database.Fields.Pass, null);
            VEEW.ShowAsWindow();
        }

        private void GridConnStrVEButton_Click(object sender, RoutedEventArgs e)
        {
            Database selectedEnvDB = (Database)grdAppDbs.CurrentItem;
            ValueExpressionEditorPage VEEW = new ValueExpressionEditorPage(selectedEnvDB, Database.Fields.ConnectionString, null);
            VEEW.ShowAsWindow();
        }
    }
}
