using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Environments;
using GingerCore.GeneralLib;
using Microsoft.VisualStudio.Services.Common;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
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

            xConnectionStringInfo.ToolTip = """
                1. DB2: "Server={Server URL};Database={Database Name};UID={User Name};PWD={User Password};"
                2. PostgreSQL: "Server={Server URL};User Id={User Name}; Password={User Password};Database={Database Name};"
                3. MySQL: "Server={Server URL};Database={Database Name};UID={User Name};PWD={User Password};"
                4. CosmosDB: "AccountEndpoint={End Point URL};AccountKey={Account Key};"
                5. HBase: "Server={Server URL};Port={Port No};User Id={User Name}; Password={Password};Database={Database Name};"
                6. Other Databases: "Data Source={Data Source};User Id={User Name};Password={User Password};"
                """;


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
            BindingHandler.ObjFieldBinding(xDatabaseType, TextBox.TextProperty, database, nameof(Database.DBType));

            xDatabaseName.TextChanged += XDatabaseName_TextChanged;

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
            }

        }

        private void XDatabaseName_TextChanged(object sender, TextChangedEventArgs e)
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
                if (!EncryptionHandler.IsStringEncrypted(database.Pass))
                {
                    database.Pass = EncryptionHandler.EncryptwithKey(database.Pass);
                }
        }
    }
}
