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
using GingerCore.Actions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Ginger.Environments
{
    /// <summary>
    /// Interaction logic for AppDataBasesWindow.xaml
    /// </summary>
    public partial class AppDataBasesPage : Page
    {
        public EnvApplication AppOwner { get; set; }

        Context mContext;

        public AppDataBasesPage(EnvApplication applicationOwner, Context context)
        {
            InitializeComponent();
            AppOwner = applicationOwner;
            mContext = context;
            //Set grid look and data
            SetGridView();
            SetGridData();
            //Added for Encryption
            if (grdAppDbs.grdMain != null)
            {
                grdAppDbs.grdMain.CellEditEnding += grdMain_CellEditEnding;
                grdAppDbs.grdMain.PreparingCellForEdit += grdMain_PreparingCellForEdit;
            }
        }

        private void grdMain_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {
            Database selectedDB = (Database)grdAppDbs.CurrentItem;
            if (e.Column.Header.ToString() == nameof(Database.Name))
            {
                ((DatabaseOperations)selectedDB.DatabaseOperations).NameBeforeEdit = selectedDB.Name;
            }
            if (selectedDB.DBType == Database.eDBTypes.Cassandra)
            {
                DataGrid dataGrid = sender as DataGrid;
                DataGridRow row = (DataGridRow)dataGrid.ItemContainerGenerator.ContainerFromItem(dataGrid.CurrentItem);
                ToolTipService.SetToolTip(row, new ToolTip { Content = "Expected Format: host:Port/Keyspace/querytimeout=90\nKeyspace and query timeout are optional", Style = FindResource("ToolTipStyle") as Style });
                ToolTipService.SetShowDuration(row, 15000);//15sec
            }
        }

        private async void grdMain_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            switch(e.Column.DisplayIndex) //we have checked for column index for grid
            {
                case 8://for Password
                    Database selectedEnvDB = (Database)grdAppDbs.CurrentItem;
                    String intialValue = selectedEnvDB.Pass;
                    //if Pass is stored in the form of variable, encryption not required at this stage
                    if (!string.IsNullOrEmpty(intialValue) && !intialValue.Contains("{Var Name"))
                    {
                        if (!EncryptionHandler.IsStringEncrypted(intialValue))
                        {
                            selectedEnvDB.Pass = EncryptionHandler.EncryptwithKey(intialValue);
                            if (string.IsNullOrEmpty(selectedEnvDB.Pass))
                            {
                                selectedEnvDB.Pass = null;
                            }
                        }
                    }
                    break;
                case 0://for Database name
                    Database selectedDB = (Database)grdAppDbs.CurrentItem;
                    if (selectedDB.Name != ((DatabaseOperations)selectedDB.DatabaseOperations).NameBeforeEdit)
                    {
                        await UpdateDatabaseNameChange(selectedDB);
                    }
                    break;
                default:
                    break;

            }
        }

        public async Task UpdateDatabaseNameChange(Database db)
        {
            if (db == null)
            {
                return;
            }
            Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                await Task.Run(() =>
                {
                    Reporter.ToStatus(eStatusMsgKey.RenameItem, null, ((DatabaseOperations)db.DatabaseOperations).NameBeforeEdit, db.Name);
                    ObservableList<BusinessFlow> allBF = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>();
                    Parallel.ForEach(allBF, new ParallelOptions { MaxDegreeOfParallelism = 5 }, businessFlow =>
                    {
                        Parallel.ForEach(businessFlow.Activities, new ParallelOptions { MaxDegreeOfParallelism = 5 }, activity =>
                        {
                            Parallel.ForEach(activity.Acts, new ParallelOptions { MaxDegreeOfParallelism = 5 }, act =>
                            {
                                if (act.GetType() == typeof(ActDBValidation))
                                {
                                    ActDBValidation actDB = (ActDBValidation)act;
                                    if (actDB.DBName == ((DatabaseOperations)db.DatabaseOperations).NameBeforeEdit)
                                    {
                                        businessFlow.DirtyStatus = eDirtyStatus.Modified;
                                        actDB.DBName = db.Name;
                                    }
                                }

                            });
                        });
                    });
                });

                ((DatabaseOperations)db.DatabaseOperations).NameBeforeEdit = db.Name;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Error occurred while renaming DBName", ex);
            }
            finally
            {
                Reporter.HideStatusMessage();
                Mouse.OverrideCursor = null;
            }
        }

        #region Events
        private void TestDBConnection(object sender, RoutedEventArgs e)
        {
            try
            {
                Database db = (Database)grdAppDbs.grdMain.SelectedItem;
                if (db == null)
                {
                    Reporter.ToUser(eUserMsgKey.AskToSelectItem);
                    return;
                }
                db.DSList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>();
                db.ProjEnvironment = mContext.Environment;
                db.BusinessFlow = null;

                db.DatabaseOperations.CloseConnection();
                if (db.DatabaseOperations.Connect(true))
                {
                    Reporter.ToUser(eUserMsgKey.DbConnSucceed);
                }
                else
                {
                    Reporter.ToUser(eUserMsgKey.DbConnFailed);
                }
                db.DatabaseOperations.CloseConnection();
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Oracle.ManagedDataAccess.dll is missing"))
                {
                    if (Reporter.ToUser(eUserMsgKey.OracleDllIsMissing, AppDomain.CurrentDomain.BaseDirectory) == Amdocs.Ginger.Common.eUserMsgSelection.Yes)
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo() { FileName = "https://docs.oracle.com/database/121/ODPNT/installODPmd.htm#ODPNT8149", UseShellExecute = true });
                        System.Threading.Thread.Sleep(2000);
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo() { FileName = "http://www.oracle.com/technetwork/topics/dotnet/downloads/odacdeploy-4242173.html", UseShellExecute = true });

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

            view.GridColsView.Add(new GridColView() { Field = nameof(Database.Name), WidthWeight = 20 });
            view.GridColsView.Add(new GridColView() { Field = nameof(Database.Description), WidthWeight = 30 });
            view.GridColsView.Add(new GridColView() { Field = nameof(Database.DBVer), Header = "Version", WidthWeight = 10 });
            view.GridColsView.Add(new GridColView()
            {
                Field = nameof(Database.DBType),
                WidthWeight = 10,
                StyleType = GridColView.eGridColStyleType.Template,
                CellTemplate = ucGrid.GetGridComboBoxTemplate(GingerCore.General.GetEnumValuesForCombo(typeof(Database.eDBTypes)), nameof(Database.DBType), false, true),
                Header = "DB Type"
            });
            view.GridColsView.Add(new GridColView() { Field = nameof(Database.TNS), Header = "TNS / File Path / Host ", WidthWeight = 30 });
            view.GridColsView.Add(new GridColView() { Field = "VE1", Header = "...", WidthWeight = 5, MaxWidth = 30, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.appDataBasesWindowGrid.Resources["TNSValueExpressionButton"] });
            view.GridColsView.Add(new GridColView() { Field = nameof(Database.User), Header = "User Name/Endpoint URI", WidthWeight = 10 });
            view.GridColsView.Add(new GridColView() { Field = "VE2", Header = "...", WidthWeight = 5, MaxWidth = 30, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.appDataBasesWindowGrid.Resources["UserValueExpressionButton"] });
            view.GridColsView.Add(new GridColView() { Field = nameof(Database.Pass), Header = "User Password/Primary Key", WidthWeight = 10 });
            view.GridColsView.Add(new GridColView() { Field = "VE3", Header = "...", WidthWeight = 5, MaxWidth = 30, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.appDataBasesWindowGrid.Resources["PswdValueExpressionButton"] });
            view.GridColsView.Add(new GridColView() { Field = nameof(Database.ConnectionString), WidthWeight = 20, Header = "Connection String (Optional)" });
            view.GridColsView.Add(new GridColView() { Field = "VE4", Header = "...", WidthWeight = 5, MaxWidth = 30, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.appDataBasesWindowGrid.Resources["ConnStrValueExpressionButton"] });
            view.GridColsView.Add(new GridColView() { Field = nameof(Database.KeepConnectionOpen), Header = "Keep Connection Open", StyleType = GridColView.eGridColStyleType.CheckBox, MaxWidth = 150, WidthWeight = 10 });
            grdAppDbs.SetAllColumnsDefaultView(view);
            grdAppDbs.InitViewItems();
        }
        private void AddNewDB(object sender, RoutedEventArgs e)
        {
            Database db = new Database();
            db.Name = "New";
            db.PropertyChanged += db_PropertyChanged;
            grdAppDbs.DataSourceList.Add(db);

            DatabaseOperations databaseOperations = new DatabaseOperations(db);
            db.DatabaseOperations = databaseOperations;
        }

        private void SetGridData()
        {
            foreach (Database db in AppOwner.Dbs)
            {
                db.PropertyChanged -= db_PropertyChanged;
                db.PropertyChanged += db_PropertyChanged;
                DatabaseOperations databaseOperations = new DatabaseOperations(db);
                db.DatabaseOperations = databaseOperations;
            }
            grdAppDbs.DataSourceList = AppOwner.Dbs;
        }
        #endregion Functions

        private void db_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Database db = (Database)sender;
            if (db.DBType != Database.eDBTypes.Cassandra && db.DBType != Database.eDBTypes.Couchbase && db.DBType != Database.eDBTypes.MongoDb)
            {
                if (e.PropertyName == nameof(Database.TNS))
                {
                    if (db.DatabaseOperations.CheckUserCredentialsInTNS())
                    {
                        db.DatabaseOperations.SplitUserIdPassFromTNS();
                    }
                }
                if (e.PropertyName == nameof(Database.TNS) || e.PropertyName == nameof(Database.User) || e.PropertyName == nameof(Database.Pass))
                {
                    db.DatabaseOperations.CreateConnectionString();
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            grdAppDbs.grdMain.CommitEdit();
            grdAppDbs.grdMain.CancelEdit();
        }

        private void GridTNSVEButton_Click(object sender, RoutedEventArgs e)
        {
            Database selectedEnvDB = (Database)grdAppDbs.CurrentItem;
            ValueExpressionEditorPage VEEW = new ValueExpressionEditorPage(selectedEnvDB, nameof(Database.TNS), null);
            VEEW.ShowAsWindow();
        }

        private void GridUserVEButton_Click(object sender, RoutedEventArgs e)
        {
            Database selectedEnvDB = (Database)grdAppDbs.CurrentItem;
            ValueExpressionEditorPage VEEW = new ValueExpressionEditorPage(selectedEnvDB, nameof(Database.User), null);
            VEEW.ShowAsWindow();
        }

        private void GridPswdVEButton_Click(object sender, RoutedEventArgs e)
        {
            Database selectedEnvDB = (Database)grdAppDbs.CurrentItem;
            ValueExpressionEditorPage VEEW = new ValueExpressionEditorPage(selectedEnvDB, nameof(Database.Pass), null);
            VEEW.ShowAsWindow();
        }

        private void GridConnStrVEButton_Click(object sender, RoutedEventArgs e)
        {
            Database selectedEnvDB = (Database)grdAppDbs.CurrentItem;
            ValueExpressionEditorPage VEEW = new ValueExpressionEditorPage(selectedEnvDB, nameof(Database.ConnectionString), null);
            VEEW.ShowAsWindow();
        }
    }
}
