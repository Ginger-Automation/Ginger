using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Environments;
using GingerCore.GeneralLib;
using Microsoft.VisualStudio.Services.Common;
using System;
using System.Globalization;
using System.Windows.Controls;
using static GingerCore.Environments.Database;

namespace Ginger.Variables
{
    /// <summary>
    /// Interaction logic for EditDatabasePage.xaml
    /// </summary>
    public partial class EditDatabasePage : Page
    {
        private readonly Database database;
        public EditDatabasePage(Database database, Context context)
        {
            this.database = database;
            InitializeComponent();

            xConnectionStringInfo.ToolTip = database.GetConnectionStringToolTip();


            xDatabaseUserName.Init(context, database, nameof(Database.User));
            xDatabasePassword.Init(context, database, nameof(Database.Pass));
            xDatabaseTNS.Init(context, database, nameof(Database.TNS));
            xDBAccEndPoint.Init(context, database, nameof(Database.User));
            xDBAccKey.Init(context, database, nameof(Database.Pass));
            xDatabaseConnectionString.Init(context, database, nameof(Database.ConnectionString));

            ((DatabaseOperations)this.database.DatabaseOperations).NameBeforeEdit = this.database.Name;

            BindingHandler.ObjFieldBinding(xDatabaseName, TextBox.TextProperty, database, nameof(Database.Name));
            BindingHandler.ObjFieldBinding(xDatabaseDescription, TextBox.TextProperty, database, nameof(Database.Description));
            BindingHandler.ObjFieldBinding(xKeepConnectOpen, CheckBox.IsCheckedProperty, database, nameof(Database.KeepConnectionOpen));
            BindingHandler.ObjFieldBinding(xOracleVersion, CheckBox.IsCheckedProperty, database, nameof(Database.IsOracleVersionLow));

            BindingHandler.ObjFieldBinding(xDatabaseType, TextBox.TextProperty, database, nameof(Database.DBType));

            xDatabaseName.AddValidationRule(new DBNameValidationRule());

            if (database.DBType.Equals(eDBTypes.CosmosDb))
            {
                xCosmosDetailsPanel.Visibility = System.Windows.Visibility.Visible;
                xDatabaseDetailsPanel.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                xCosmosDetailsPanel.Visibility = System.Windows.Visibility.Collapsed;
                xDatabaseDetailsPanel.Visibility = System.Windows.Visibility.Visible;
                ChangeTheTNSName();
                ShowBrowseBtn();
                if (database.DBType.Equals(eDBTypes.Oracle))
                {
                    xVersionStackPanel.Visibility = System.Windows.Visibility.Visible;
                }
            }

        }
        private void XDatabaseName_TextChanged(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {

            string NameBeforeEdit = ((DatabaseOperations)database.DatabaseOperations).NameBeforeEdit;

            if (string.Equals(NameBeforeEdit, this.database.Name))
            {
                return;
            }
            try
            {
                Reporter.ToStatus(eStatusMsgKey.RenameItem, null, NameBeforeEdit, database.Name);
                ObservableList<BusinessFlow> allBF = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>();
                allBF
                    .ForEach(
                    (businessFlow) =>
                    {

                        businessFlow.Activities
                        .ForEach(activity =>
                        {
                            activity.Acts
                            .ForEach((act) =>
                            {
                                if (act.GetType() == typeof(ActDBValidation))
                                {
                                    ActDBValidation actDB = (ActDBValidation)act;
                                    if (string.Equals(actDB.DBName, NameBeforeEdit))
                                    {
                                        businessFlow.StartDirtyTracking();
                                        actDB.DBName = database.Name;
                                    }
                                }
                            });
                        });
                    });

                ((DatabaseOperations)database.DatabaseOperations).NameBeforeEdit = database.Name;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to Update Database Name Change", ex);
            }
            finally
            {
                Reporter.HideStatusMessage();
            }
        }

        private void ShowBrowseBtn()
        {
            if (this.database.DBType.Equals(eDBTypes.MSAccess))
            {
                xDatabaseTNS.SetBrowserBtn();
                xDatabaseTNS.Width = 335;
            }
        }

        private void ChangeTheTNSName()
        {

            if (this.database.DBType.Equals(eDBTypes.MySQL)
                || this.database.DBType.Equals(eDBTypes.Hbase)
                || this.database.DBType.Equals(eDBTypes.PostgreSQL)
                || this.database.DBType.Equals(eDBTypes.DB2)
                )
            {
                xDatabaseTNSName.Content = "Server";
            }
        }

        private void ChangeDatabasePass(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
                if (!string.IsNullOrEmpty(database.Pass) &&
                    !database.Pass.Contains("{Var Name") && 
                    !EncryptionHandler.IsStringEncrypted(database.Pass))
                {
                    database.Pass = EncryptionHandler.EncryptwithKey(database.Pass);
                }
        }
    }

    class DBNameValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string dbName = (string)value;

            if (string.IsNullOrWhiteSpace(dbName))
            {
                return new ValidationResult(false, "Database Name cannot be empty");
            }


            return new ValidationResult(true, null);
        }
    }
}
