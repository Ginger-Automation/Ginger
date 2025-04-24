#region License
/*
Copyright © 2014-2025 European Support Limited

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
using Ginger.UserControlsLib.UCListView;
using Ginger.Variables;
using GingerCore.Environments;
using GingerCore.GeneralLib;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Ginger.Environments
{
    public partial class AppDataBasesPage : Page
    {
        public EnvApplication AppOwner { get; set; }

        private readonly DatabaseListViewHelper dbListViewHelper;
        private UcListView? DatabaseListView = null;
        private readonly Context mContext;

        public AppDataBasesPage(EnvApplication applicationOwner, Context context)
        {
            InitializeComponent();
            AppOwner = applicationOwner;
            mContext = context;
            dbListViewHelper = new DatabaseListViewHelper(applicationOwner.Dbs, context);
            SetDBListView();
            AddPropertyChangedEventToDbList();
            ShowOrHideEditPage(null);
        }

        private void SetDBListView()
        {

            DatabaseListView = new()
            {
                ListTitleVisibility = Visibility.Collapsed,
                ListImageVisibility = Visibility.Collapsed
            };

            dbListViewHelper.DatabaseListItemEvent += ShowEditPageEvent;
            DatabaseListView.SetDefaultListDataTemplate(dbListViewHelper);
            DatabaseListView.ListSelectionMode = SelectionMode.Extended;


            DatabaseListView.List.MouseDoubleClick += DBListView_MouseDoubleClick;
            DatabaseListView.List.SetValue(ScrollViewer.CanContentScrollProperty, true);
            DatabaseListView.DataSourceList = AppOwner.Dbs;
        }

        private void ShowEditPageEvent(ListItemEventArgs EventArgs)
        {
            if (EventArgs.EventType.Equals(ListItemEventArgs.eEventType.ShowEditPage))
            {
                ShowOrHideEditPage((Database)EventArgs.EventObject);
            }
        }

        private void DBListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

            if (DatabaseListView?.CurrentItem != null)
            {
                ShowOrHideEditPage(DatabaseListView.CurrentItem as Database);
            }
        }

        private void ShowOrHideEditPage(Database? database)
        {
            if (database != null)
            {
                xBackToListGrid.Visibility = Visibility.Visible;
                BindingHandler.ObjFieldBinding(xSelectedItemTitleText, TextBlock.TextProperty, database, nameof(Database.Name));
                BindingHandler.ObjFieldBinding(xSelectedItemTitleText, TextBlock.ToolTipProperty, database, nameof(Database.Name));
                EditDatabasePage editDatabasePage = new(database, mContext);
                xMainFrame.SetContent(editDatabasePage);
            }
            else
            {
                xBackToListGrid.Visibility = Visibility.Collapsed;
                xMainFrame.SetContent(DatabaseListView);
            }
        }
        private void AddPropertyChangedEventToDbList()
        {
            foreach (Database db in AppOwner.Dbs)
            {
                db.PropertyChanged -= this.dbListViewHelper.DbPropertyChanged;
                db.PropertyChanged += this.dbListViewHelper.DbPropertyChanged;
                DatabaseOperations databaseOperations = new(db);
                db.DatabaseOperations = databaseOperations;
            }
        }

        private void xGoToList_Click(object sender, RoutedEventArgs e)
        {
            ShowOrHideEditPage(null);
        }

        private async void TestDatabase(object sender, RoutedEventArgs e)
        {
            Database? database = this.DatabaseListView?.CurrentItem as Database;
            bool IsConnectionSuccessful = await this.dbListViewHelper.TestSingleDatabase(database);

            if (IsConnectionSuccessful)
            {
                Reporter.ToUser(eUserMsgKey.DbConnSucceed, database?.Name);
            }

            else
            {
                Reporter.ToUser(eUserMsgKey.DbConnFailed, database?.Name);
            }
        }
    }
}
