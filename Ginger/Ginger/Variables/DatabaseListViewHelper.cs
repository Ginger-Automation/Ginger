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
using System.Windows;

namespace Ginger.BusinessFlowPages.ListHelpers
{
    public class DatabaseListViewHelper : IListViewHelper
    {
        private readonly ObservableList<IDatabase> Databases = new ObservableList<IDatabase>();
        private readonly Context mContext;
        public DatabaseListViewHelper(ObservableList<IDatabase> Databases, Context context) 
        {
            this.Databases = Databases;
            this.mContext = context;
        }
        private UcListView mListView = null;
        public UcListView ListView {
            get
            {
                return mListView;
            }
            set
            {
                mListView = value;
            }
        }

        public General.eRIPageViewMode PageViewMode { get; set ; }
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
            return null;
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
            List<ListItemOperation> operationsList = new List<ListItemOperation>();

            ListItemOperation testDatabase = new ListItemOperation();
            testDatabase.SupportedViews = new List<General.eRIPageViewMode>() { General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone };
            testDatabase.AutomationID = "test";
            testDatabase.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Database;
            testDatabase.ToolTip = "Test Database";
            testDatabase.OperationHandler = TestDatabase;
            testDatabase.Margin = new Thickness(-5, 0, -5, 0);

            operationsList.Add(testDatabase);

            ListItemOperation editDB = new ListItemOperation();
            editDB.SupportedViews = new List<General.eRIPageViewMode>() { General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone };
            editDB.AutomationID = "edit";
            editDB.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Edit;
            editDB.ToolTip = "Edit Database";
            editDB.OperationHandler = EditDatabase;
            editDB.Margin = new Thickness(0,0,20,0);
            operationsList.Add(editDB);


            return operationsList;
        }

        private void EditDatabase(object sender, RoutedEventArgs e)
        {


        }

        private void TestDatabase(object sender, RoutedEventArgs e)
        {
            try
            {
                Database? db = ((ucButton)sender).Tag as Database;
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
            List<ListItemOperation> operationsList = new List<ListItemOperation>();

            ListItemOperation addNew = new ListItemOperation();
            addNew.SupportedViews = new List<General.eRIPageViewMode>() { General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone };
            addNew.AutomationID = "addNew";
            addNew.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Add;
            addNew.ToolTip = "Add New Database";
            addNew.OperationHandler = AddNewHandler;
            operationsList.Add(addNew);

            ListItemOperation deleteSelected = new ListItemOperation();
            deleteSelected.SupportedViews = new List<General.eRIPageViewMode>() { General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone };
            deleteSelected.AutomationID = "deleteSelected";
            deleteSelected.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Delete;
            deleteSelected.ToolTip = "Delete Selected Database (Del)";
            deleteSelected.OperationHandler = DeleteSelectedHandler;
            operationsList.Add(deleteSelected);

            return operationsList;
        }

        private void DeleteSelectedHandler(object sender, RoutedEventArgs e)
        {
            if (ListView.List.SelectedItems.Count == 0)
            {
                Reporter.ToUser(eUserMsgKey.SelectItemToDelete);
                return;
            }

            if (Reporter.ToUser(eUserMsgKey.SureWantToDeleteSelectedItems, "Database", (ListView.List.SelectedItems[0] as Database)?.Name ?? string.Empty) == eUserMsgSelection.Yes)
            {
                var dbList = ListView.List.SelectedItems.Cast<Database>().ToList();
                for(int indx = 0; indx< dbList.Count(); indx++)
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

        public void TestDatabase(Database db)
        {
            try
            {
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


        public void Paste()
        {
            throw new NotImplementedException();
        }

        public void SetItem(object item)
        {
            throw new NotImplementedException();
        }
    }
}
