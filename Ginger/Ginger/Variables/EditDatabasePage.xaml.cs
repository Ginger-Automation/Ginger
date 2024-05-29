using Amdocs.Ginger.Common;
using GingerCore.Environments;
using GingerCore.GeneralLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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

            var DBTypes = GingerCore.General
                                .GetEnumValuesForCombo(typeof(Database.eDBTypes))
                                .Select((db) => (eDBTypes)db.Value);
            int selectedIndex = 0;
            foreach (var dbType in DBTypes)
            {
                if (dbType.Equals(database.DBType))
                {
                    break;
                }

                selectedIndex++;
            }
            
            InitializeComponent();

            xDatabaseComboBox.ItemsSource = DBTypes;
            xDatabaseComboBox.SelectedIndex = selectedIndex;

            xDatabaseUserName.Init(context, database, nameof(Database.User));
            xDatabasePassword.Init(context, database, nameof(Database.Pass));
            xDatabaseTNS.Init(context, database, nameof(Database.TNS));
            xDBAccEndPoint.Init(context, database, nameof(Database.User));
            xDBAccKey.Init(context, database, nameof(Database.Pass));

            BindingHandler.ObjFieldBinding(xDatabaseConnectionString, TextBox.TextProperty, database, nameof(Database.ConnectionString));
            BindingHandler.ObjFieldBinding(xDatabaseName, TextBox.TextProperty, database, nameof(Database.Name));
            BindingHandler.ObjFieldBinding(xDatabaseDescription, TextBox.TextProperty, database, nameof(Database.Description));
            BindingHandler.ObjFieldBinding(xDatabaseComboBox, ComboBox.SelectedValueProperty, database, nameof(Database.DBType));
            BindingHandler.ObjFieldBinding(xKeepConnectOpen, CheckBox.IsCheckedProperty, database, nameof(Database.KeepConnectionOpen));
        }



    }
}
