using Amdocs.Ginger.Common;
using GingerCore.Environments;
using GingerCore.GeneralLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static GingerCore.Environments.Database;

namespace Ginger.Environments
{
    /// <summary>
    /// Interaction logic for AddNewDatabasePage.xaml
    /// </summary>
    public partial class AddNewDatabasePage : Page
    {
        GenericWindow _pageGenericWin = null;
        private readonly IObservableList dataSourceList;
        private readonly AppDataBasesPage appDataBasesPage;

        public AddNewDatabasePage(IObservableList dataSourceList, AppDataBasesPage appDataBasesPage)
        {
            this.dataSourceList = dataSourceList;
            this.appDataBasesPage = appDataBasesPage;
            InitializeComponent();

            xDatabaseComboBox.ItemsSource = GingerCore.General.GetEnumValuesForCombo(typeof(Database.eDBTypes));
        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog, bool ShowCancelButton = true)
        {

            Button okBtn = new Button();
            okBtn.Content = "Add Database";


            Button testConnectionButton = new Button();
            testConnectionButton.Content = "Test DB Connection";

            ObservableList<Button> winButtons = new ObservableList<Button>();
            winButtons.Add(okBtn);
            winButtons.Add(testConnectionButton);
            WeakEventManager<ButtonBase, RoutedEventArgs>.AddHandler(source: okBtn, eventName: nameof(ButtonBase.Click), handler: OKButton_Click);
            WeakEventManager<ButtonBase, RoutedEventArgs>.AddHandler(source: testConnectionButton, eventName: nameof(ButtonBase.Click), handler: TestConnection_Click);


            GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, windowStyle, this.Title, this, winButtons, ShowCancelButton, "Cancel");
        }

        private void TestConnection_Click(object? sender, RoutedEventArgs e)
        {

            if (!DatabaseValueValidation())
            {
                return;
            }


            Database db = GetDatabase();

            // Should I wait?? The result of this task is not required
            Task.Run(() =>
            {
                this.appDataBasesPage.TestDatabase(db);
            });

        }

        private bool DatabaseValueValidation()
        {
            var comboBoxItem = xDatabaseComboBox.SelectedValue as ComboEnumItem;

            if(comboBoxItem == null || xDatabaseComboBox.SelectedIndex == -1)
            {
                xDatabaseTypeError.Visibility = Visibility.Visible;
                xDatabaseTypeError.Text = "Database Type is mandatory";
                return false;
            }

            xDatabaseTypeError.Visibility = Visibility.Collapsed;



            if (string.IsNullOrEmpty(xDatabaseName.Text))
            {
                xDatabaseNameError.Text = "Database Name is mandatory";
                xDatabaseNameError.Visibility = Visibility.Visible;
                return false;
            }

            xDatabaseNameError.Visibility = Visibility.Collapsed;


            if (string.IsNullOrEmpty(xDatabasePassword.Text))
            {
                xDatabaseUserPassError.Text = "Password is mandatory";
                xDatabaseUserPassError.Visibility = Visibility.Visible;
                return false;
            }

            xDatabaseUserPassError.Visibility = Visibility.Collapsed;


            if (string.IsNullOrEmpty(xDatabaseUserName.Text))
            {
                xDatabaseUserNameError.Text = "User Name is mandatory";
                xDatabaseUserNameError.Visibility = Visibility.Visible;
                return false;
            }

            xDatabaseUserNameError.Visibility = Visibility.Collapsed;


            if (string.IsNullOrEmpty(xDatabaseTNS.Text))
            {
                xDatabaseTNSError.Text = "TNS/File Path/Host is mandatory";
                xDatabaseUserNameError.Visibility = Visibility.Visible;
                return false;
            }

            xDatabaseUserNameError.Visibility = Visibility.Collapsed;


            return true;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            // validation

            if (!DatabaseValueValidation())
            {
                return;
            }


            Database db = GetDatabase();
            dataSourceList.Add(db);
            db.PropertyChanged += appDataBasesPage.db_PropertyChanged;

            _pageGenericWin?.Close();
        }

        private Database GetDatabase()
        {
            Database db = new Database();
            db.Name = xDatabaseName.Text;
            db.TNS = xDatabaseTNS.Text;
            db.Pass = xDatabasePassword.Text;
            db.User = xDatabaseUserName.Text;
            db.ConnectionString = xDatabaseConnectionString.Text;
            db.DBType = (eDBTypes)(xDatabaseComboBox.SelectedValue as ComboEnumItem).Value;
            DatabaseOperations databaseOperations = new DatabaseOperations(db);
            db.DatabaseOperations = databaseOperations;

            return db;
        }

        private void Db_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if(sender == null)
            {
                return;
            }
            
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

        private void xDatabaseComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            var comboEnumItem = xDatabaseComboBox.SelectedValue as ComboEnumItem;

            if(comboEnumItem == null)
            {
                return;
            }


            eDBTypes databaseType = (eDBTypes)comboEnumItem.Value;

            if(!databaseType.Equals(eDBTypes.Couchbase) && !databaseType.Equals(eDBTypes.Cassandra) && !databaseType.Equals(eDBTypes.Hbase))
            {
                xDBConnectionStringPanel.Visibility = Visibility.Visible;
            }
            else
            {
                xDBConnectionStringPanel.Visibility = Visibility.Collapsed;
            }
        }
    }
}
