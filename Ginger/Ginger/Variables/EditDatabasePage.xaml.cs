using Amdocs.Ginger.Common;
using GingerCore;
using GingerCore.Environments;
using GingerCore.GeneralLib;
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
            xDatabaseUserName.Init(context, database, nameof(Database.User));
            xDatabasePassword.Init(context, database, nameof(Database.Pass));
            xDatabaseTNS.Init(context, database, nameof(Database.TNS));
            xDBAccEndPoint.Init(context, database, nameof(Database.User));
            xDBAccKey.Init(context, database, nameof(Database.Pass));
            xDatabaseConnectionString.Init(context, database, nameof(Database.ConnectionString));

            BindingHandler.ObjFieldBinding(xDatabaseName, TextBox.TextProperty, database, nameof(Database.Name));
            BindingHandler.ObjFieldBinding(xDatabaseDescription, TextBox.TextProperty, database, nameof(Database.Description));
            BindingHandler.ObjFieldBinding(xKeepConnectOpen, CheckBox.IsCheckedProperty, database, nameof(Database.KeepConnectionOpen));
            BindingHandler.ObjFieldBinding(xDatabaseType, TextBox.TextProperty, database, nameof(Database.DBType));

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
