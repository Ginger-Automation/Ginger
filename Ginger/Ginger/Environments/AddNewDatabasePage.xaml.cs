#region License
/*
Copyright © 2014-2026 European Support Limited
 
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
using Ginger.BusinessFlowPages.ListHelpers;
using GingerCore.Environments;
using GingerCore.GeneralLib;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using static GingerCore.Environments.Database;

namespace Ginger.Environments
{
    /// <summary>
    /// Interaction logic for AddNewDatabasePage.xaml
    /// </summary>
    public partial class AddNewDatabasePage : Page
    {
        GenericWindow _pageGenericWin = null;
        private readonly ObservableList<IDatabase> dataSourceList;
        private readonly Database database;
        private readonly Button testConnectionButton = new();
        private readonly Button okBtn = new();
        private readonly DatabaseListViewHelper dbListViewHelper;
        public AddNewDatabasePage(ObservableList<IDatabase> dataSourceList, Context context, DatabaseListViewHelper dbListViewHelper)
        {
            this.dbListViewHelper = dbListViewHelper;
            this.dataSourceList = dataSourceList;
            InitializeComponent();

            // Initializes the Database Combobox
            this.database = new()
            {
                ProjEnvironment = context.Environment
            };

            SQL_Selected(null, null);


            this.database.DatabaseOperations = new DatabaseOperations(database);
            xDatabaseUserName.Init(context, database, nameof(Database.User));
            xDatabasePassword.Init(context, database, nameof(Database.Pass));
            xDatabaseTNS.Init(context, database, nameof(Database.TNS));
            xDBAccEndPoint.Init(context, database, nameof(Database.User));
            xDBAccKey.Init(context, database, nameof(Database.Pass));

            xDatabaseConnectionString.Init(context, database, nameof(Database.ConnectionString));
            xDatabaseConnectionString.Row.Height = new System.Windows.GridLength(100);
            xDatabaseConnectionString.ValueTextBox.Height = 40;
            BindingHandler.ObjFieldBinding(xDatabaseName, TextBox.TextProperty, database, nameof(Database.Name));
            BindingHandler.ObjFieldBinding(xDatabaseDescription, TextBox.TextProperty, database, nameof(Database.Description));
            BindingHandler.ObjFieldBinding(xDatabaseComboBox, ComboBox.SelectedValueProperty, database, nameof(Database.DBType));
            BindingHandler.ObjFieldBinding(xKeepConnectOpen, CheckBox.IsCheckedProperty, database, nameof(Database.KeepConnectionOpen));
            BindingHandler.ObjFieldBinding(xOracleVersion, CheckBox.IsCheckedProperty, database, nameof(Database.IsOracleVersionLow));

            database.PropertyChanged += this.dbListViewHelper.DbPropertyChanged;
        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog, bool ShowCancelButton = true)
        {

            okBtn.Content = "Add Database";


            testConnectionButton.Content = "Test DB Connection";
            ObservableList<Button> winButtons = [okBtn, testConnectionButton];
            WeakEventManager<ButtonBase, RoutedEventArgs>.AddHandler(source: okBtn, eventName: nameof(ButtonBase.Click), handler: OKButton_Click);
            WeakEventManager<ButtonBase, RoutedEventArgs>.AddHandler(source: testConnectionButton, eventName: nameof(ButtonBase.Click), handler: TestConnection_Click);


            GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, windowStyle, this.Title, this, winButtons, ShowCancelButton, "Cancel");
        }

        private async void TestConnection_Click(object? sender, RoutedEventArgs e)
        {
            try
            {
                if (!DatabaseValueValidation())
                {
                    return;
                }

                this.testConnectionButton.IsEnabled = false;

                bool IsConnectionSuccessful = await this.dbListViewHelper.TestSingleDatabase(database);

                if (IsConnectionSuccessful)
                {
                    Reporter.ToUser(eUserMsgKey.DbConnSucceed, database?.Name);
                }
                else
                {
                    Reporter.ToUser(eUserMsgKey.DbConnFailed, database?.Name);
                }

                this.testConnectionButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, ex.Message, ex);
            }
        }

        private bool DatabaseValueValidation()
        {
            if (string.IsNullOrEmpty(xDatabaseName.Text))
            {
                xDatabaseNameError.Text = "Database Name is mandatory";
                xDatabaseNameError.Visibility = Visibility.Visible;
                return false;
            }

            return true;
        }

        private async void OKButton_Click(object sender, RoutedEventArgs e)
        {
            // validation


            try
            {
                if (!DatabaseValueValidation())
                {
                    return;
                }

                dataSourceList.Add(database);

                okBtn.IsEnabled = false;

                await this.dbListViewHelper.TestSingleDatabase(database);

                okBtn.IsEnabled = true;
                _pageGenericWin?.Close();
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, ex.Message, ex);
            }
        }


        private eDBTypes? GetDBType()
        {
            var comboEnumItem = xDatabaseComboBox.SelectedValue;
            if (comboEnumItem == null)
            {
                return null;
            }
            return (eDBTypes)comboEnumItem;
        }
        private void xDatabaseComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            eDBTypes? databaseType = GetDBType();

            if (databaseType == null)
            {
                return;
            }
            xConnectionStrCheckBox.IsChecked = false;
            ClearDatabaseDetails();

            xConnectionStringInfo.ToolTip = database.GetConnectionStringToolTip();


            if (!databaseType.Equals(eDBTypes.Couchbase) && !databaseType.Equals(eDBTypes.Cassandra) && !databaseType.Equals(eDBTypes.Hbase))
            {
                xConnectionStrStackPanel.Visibility = Visibility.Visible;
            }
            else
            {
                xConnectionStrStackPanel.Visibility = Visibility.Collapsed;
            }


            if (databaseType.Equals(eDBTypes.Oracle))
            {
                xVersionStackPanel.Visibility = Visibility.Visible;
            }
            else
            {
                xVersionStackPanel.Visibility = Visibility.Collapsed;
            }

            if (this.database.DBType.Equals(eDBTypes.MySQL)
                || this.database.DBType.Equals(eDBTypes.Hbase)
                || this.database.DBType.Equals(eDBTypes.PostgreSQL)
                || this.database.DBType.Equals(eDBTypes.DB2))
            {
                xDatabaseTNSName.Content = "Server:";
            }
            else
            {
                xDatabaseTNSName.Content = "Data Source:";
            }


            if (databaseType.Equals(eDBTypes.MSAccess))
            {
                xDatabaseTNS.SetBrowserBtn();
                xDatabaseTNS.Width = 335;
            }
            else
            {
                xDatabaseTNS.HideBrowserBTN();
            }



            if (databaseType.Equals(eDBTypes.CosmosDb))
            {
                xCosmosDetailsPanel.Visibility = Visibility.Visible;
                xDatabaseDetailsPanel.Visibility = Visibility.Collapsed;
            }

            else
            {
                xCosmosDetailsPanel.Visibility = Visibility.Collapsed;
                xDatabaseDetailsPanel.Visibility = Visibility.Visible;
            }

        }

        private void ClearDatabaseDetails()
        {
            if (database == null)
            {
                return;
            }

            this.database.User = string.Empty;
            this.database.Pass = string.Empty;
            this.database.TNS = string.Empty;
            this.database.ConnectionString = string.Empty;
            this.database.Name = string.Empty;
            this.database.Description = string.Empty;
            this.database.IsOracleVersionLow = false;
            this.database.KeepConnectionOpen = false;
        }


        private void ConnectionString_Checked(object sender, RoutedEventArgs e)
        {
            xDBConnectionStringPanel.Visibility = Visibility.Visible;

            eDBTypes? dBType = GetDBType();
            if (dBType == null)
            {
                return;
            }

            if (dBType.Equals(eDBTypes.Oracle))
            {
                xVersionStackPanel.Visibility = Visibility.Collapsed;
            }


            if (dBType.Equals(eDBTypes.CosmosDb))
            {
                xCosmosDetailsPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                xDatabaseDetailsPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void ConnectionString_UnChnecked(object sender, RoutedEventArgs e)
        {
            xDBConnectionStringPanel.Visibility = Visibility.Collapsed;

            eDBTypes? dBType = GetDBType();
            if (dBType == null)
            {
                return;
            }

            if (dBType.Equals(eDBTypes.Oracle))
            {
                xVersionStackPanel.Visibility = Visibility.Visible;
            }

            if (dBType.Equals(eDBTypes.CosmosDb))
            {
                xCosmosDetailsPanel.Visibility = Visibility.Visible;
            }
            else
            {
                xDatabaseDetailsPanel.Visibility = Visibility.Visible;
            }

        }

        private void SQL_Selected(object sender, RoutedEventArgs e)
        {
            ClearDatabaseDetails();
            if (xDatabaseComboBox != null)
            {
                xDatabaseComboBox.ItemsSource = GingerCore.General
                                .GetEnumValuesForCombo(typeof(Database.eDBTypes))
                                .Select((db) => (eDBTypes)db.Value)
                                .Where((dbType) => dbType.Equals(eDBTypes.MSSQL)
                                || dbType.Equals(eDBTypes.MSAccess) || dbType.Equals(eDBTypes.Oracle)
                                || dbType.Equals(eDBTypes.MySQL) || dbType.Equals(eDBTypes.PostgreSQL)
                                || dbType.Equals(eDBTypes.DB2));
                xDatabaseComboBox.SelectedIndex = 0;
            }
        }
        private void NoSQL_Selected(object sender, RoutedEventArgs e)
        {
            ClearDatabaseDetails();

            if (xDatabaseComboBox != null)
            {
                xDatabaseComboBox.ItemsSource = GingerCore.General
                                .GetEnumValuesForCombo(typeof(Database.eDBTypes))
                                .Select((db) => (eDBTypes)db.Value)
                                .Where((dbType) =>
                                dbType.Equals(eDBTypes.Couchbase) || dbType.Equals(eDBTypes.Cassandra)
                                || dbType.Equals(eDBTypes.CosmosDb) || dbType.Equals(eDBTypes.MongoDb)
                                || dbType.Equals(eDBTypes.Hbase));

                xDatabaseComboBox.SelectedIndex = 0;
                xDatabaseDetailsPanel.Visibility = Visibility.Visible;
            }



        }
        private void ChangeDatabasePass(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            database.EncryptDatabasePass();
        }
    }
}
