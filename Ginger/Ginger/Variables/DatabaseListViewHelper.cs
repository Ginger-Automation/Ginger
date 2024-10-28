#region License
/*
Copyright © 2014-2024 European Support Limited

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
using Amdocs.Ginger.UserControls;
using Ginger.Environments;
using Ginger.UserControlsLib.UCListView;
using GingerCore.DataSource;
using GingerCore.Environments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Ginger.BusinessFlowPages.ListHelpers
{
    public class DatabaseListViewHelper : IListViewHelper
    {
        private readonly ObservableList<IDatabase> Databases = [];
        private readonly Context mContext;

        public delegate void DatabaseListItemEventHandler(ListItemEventArgs EventArgs);
        public event DatabaseListItemEventHandler DatabaseListItemEvent;
        private void OnActionListItemEvent(ListItemEventArgs.eEventType eventType, Object eventObject = null)
        {
            DatabaseListItemEventHandler handler = DatabaseListItemEvent;
            if (handler != null)
            {
                handler(new ListItemEventArgs(eventType, eventObject));
            }
        }

        public DatabaseListViewHelper(ObservableList<IDatabase> Databases, Context context)
        {
            this.Databases = Databases;
            this.mContext = context;
        }
        private UcListView mListView = null;
        public UcListView ListView
        {
            get
            {
                return mListView;
            }
            set
            {
                mListView = value;
            }
        }

        public General.eRIPageViewMode PageViewMode { get; set; }
        public bool AllowExpandItems { get; set; } = true;
        public bool ExpandItemOnLoad { get; set; } = true;

        public bool ShowIndex { get { return false; } }

        public void CopySelected()
        {

        }

        public void CutSelected()
        {

        }

        public void DeleteSelected()
        {

        }

        public string GetItemActiveField()
        {
            return null;
        }

        public string GetItemDescriptionField()
        {
            return nameof(Database.ConnectionString);
        }

        public string GetItemErrorField()
        {
            return null;
        }

        public List<ListItemOperation> GetItemExecutionOperationsList(object item)
        {
            return [];
        }

        public string GetItemExecutionStatusField()
        {
            return nameof(Database.TestConnectionStatus);
        }

        public List<ListItemOperation> GetItemExtraOperationsList(object item)
        {
            return [];
        }

        public List<ListItemNotification> GetItemGroupNotificationsList(string GroupName)
        {
            return [];
        }

        public List<ListItemGroupOperation> GetItemGroupOperationsList()
        {
            return [];
        }

        public string GetItemIconField()
        {
            return nameof(Database.Image);
        }

        public string GetItemIconTooltipField()
        {
            return nameof(Database.DBType);
        }

        public string GetItemMandatoryField()
        {
            return string.Empty;
        }

        public string GetItemNameExtentionField()
        {
            return nameof(Database.DBType);
        }

        public string GetItemNameField()
        {
            return nameof(Database.Name);
        }

        public List<ListItemNotification> GetItemNotificationsList(object item)
        {
            return [];
        }

        public List<ListItemOperation> GetItemOperationsList(object item)
        {
            List<ListItemOperation> operationsList = [];

            ListItemOperation testDatabase = new ListItemOperation
            {
                SupportedViews = [General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone],
                AutomationID = "test",
                ImageType = Amdocs.Ginger.Common.Enums.eImageType.Run,
                ToolTip = "Test Database",
                OperationHandler = TestDatabase
            };
            operationsList.Add(testDatabase);

            ListItemOperation editDB = new ListItemOperation
            {
                SupportedViews = [General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone],
                AutomationID = "edit",
                ImageType = Amdocs.Ginger.Common.Enums.eImageType.Edit,
                ToolTip = "Edit Database",
                OperationHandler = EditDatabase
            };
            operationsList.Add(editDB);

            ListItemOperation deleteDB = new ListItemOperation
            {
                SupportedViews = [General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone],
                AutomationID = "edit",
                ImageType = Amdocs.Ginger.Common.Enums.eImageType.Delete,
                ToolTip = "Delete Database",
                OperationHandler = DeleteDatabase
            };
            operationsList.Add(deleteDB);
            return operationsList;
        }

        private void DeleteDatabase(object sender, RoutedEventArgs e)
        {
            if (sender == null)
            {
                return;
            }

            Database database = (Database)((ucButton)sender).Tag;

            if (database == null)
            {
                return;
            }

            if (Reporter.ToUser(eUserMsgKey.SureWantToDeleteSelectedItems, "Database", database.Name) == eUserMsgSelection.Yes)
            {
                Databases.Remove(database);
            }


        }

        private void EditDatabase(object sender, RoutedEventArgs e)
        {
            if (sender == null)
            {
                return;
            }


            Database? database = (Database)((ucButton)sender).Tag;

            if (database == null)
            {
                return;
            }

            OnActionListItemEvent(ListItemEventArgs.eEventType.ShowEditPage, database);
        }
        //Edit, Add , Test , Database List, Test All
        public async Task<bool> TestSingleDatabase(Database? db)
        {
            if (db == null)
            {
                Reporter.ToUser(eUserMsgKey.AskToSelectItem);
                return false;
            }

            try
            {

                db.DSList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>();
                db.ProjEnvironment = mContext.Environment;
                db.BusinessFlow = null;
                db.TestConnectionStatus = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Running;

                db.DatabaseOperations.CloseConnection();
                var connectionTask = await Task.Run(() => db.DatabaseOperations.Connect(true));
                if (connectionTask)
                {
                    db.DatabaseOperations.CloseConnection();
                    db.TestConnectionStatus = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed;
                    return true;
                }
                else
                {
                    db.DatabaseOperations.CloseConnection();
                }
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
                }
                Reporter.ToLog(eLogLevel.ERROR, ex.Message, ex);
            }

            db.TestConnectionStatus = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;

            return false;
        }

        private async void TestDatabase(object sender, RoutedEventArgs e)
        {
            Database? db = ((ucButton)sender).Tag as Database;
            bool IsConnectionSuccessful = await TestSingleDatabase(db);
            if (IsConnectionSuccessful)
            {
                Reporter.ToUser(eUserMsgKey.DbConnSucceed, db?.Name);
            }

            else
            {
                Reporter.ToUser(eUserMsgKey.DbConnFailed, db?.Name);
            }

        }

        public string GetItemTagsField()
        {
            return null;
        }

        public ListItemUniqueIdentifier GetItemUniqueIdentifier(object item)
        {
            return null;
        }

        public List<ListItemOperation> GetListExtraOperations()
        {
            return [];
        }

        public List<ListItemOperation> GetListOperations()
        {
            List<ListItemOperation> operationsList = [];


            if (mContext.Environment.GOpsFlag)
            {
                return operationsList;
            }

            ListItemOperation addNew = new ListItemOperation
            {
                SupportedViews = [General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone],
                AutomationID = "addNew",
                ImageType = Amdocs.Ginger.Common.Enums.eImageType.Add,
                ToolTip = "Add New Database",
                OperationHandler = AddNewHandler
            };
            operationsList.Add(addNew);

            ListItemOperation deleteSelected = new ListItemOperation
            {
                SupportedViews = [General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone],
                AutomationID = "deleteSelected",
                ImageType = Amdocs.Ginger.Common.Enums.eImageType.Delete,
                ToolTip = "Delete Selected Database (Del)",
                OperationHandler = DeleteDBs
            };
            operationsList.Add(deleteSelected);

            ListItemOperation testAllDatabases = new ListItemOperation
            {
                SupportedViews = [General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone],
                AutomationID = "testAllDatabases",
                ImageType = Amdocs.Ginger.Common.Enums.eImageType.Run,
                ToolTip = "Test All Databases",
                OperationHandler = TestAllDatabases
            };

            operationsList.Add(testAllDatabases);

            return operationsList;
        }

        private async void TestAllDatabases(object sender, RoutedEventArgs e)
        {

            foreach (var database in Databases)
            {
                await TestSingleDatabase(database as Database);
            }
        }

        private void DeleteDBs(object sender, RoutedEventArgs e)
        {
            if (ListView.List.SelectedItems.Count == 0)
            {
                Reporter.ToUser(eUserMsgKey.SelectItemToDelete);
                return;
            }

            if (Reporter.ToUser(eUserMsgKey.DeleteSelectedDB) == eUserMsgSelection.Yes)
            {
                var dbList = ListView.List.SelectedItems.Cast<Database>().ToList();
                for (int indx = 0; indx < dbList.Count; indx++)
                {
                    Databases.Remove(dbList[indx]);
                }
            }
        }

        private void AddNewHandler(object sender, RoutedEventArgs e)
        {
            AddNewDatabasePage addNewDatabasePage = new(this.Databases, mContext, this);

            addNewDatabasePage.ShowAsWindow();
        }

        public void DbPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Database db = (Database)sender;
            if (db.DBType is not Database.eDBTypes.Cassandra and not Database.eDBTypes.Couchbase and not Database.eDBTypes.MongoDb)
            {
                if (e.PropertyName == nameof(Database.TNS))
                {
                    if (db.DatabaseOperations.CheckUserCredentialsInTNS())
                    {
                        db.DatabaseOperations.SplitUserIdPassFromTNS();
                    }
                }
                if (e.PropertyName is (nameof(Database.TNS))
                    or (nameof(Database.User))
                    or (nameof(Database.Pass))
                    or (nameof(Database.Name)))
                {
                    db.DatabaseOperations.CreateConnectionString();
                }
            }
        }

        public void Paste()
        {

        }

        public void SetItem(object item)
        {

        }
    }
}
