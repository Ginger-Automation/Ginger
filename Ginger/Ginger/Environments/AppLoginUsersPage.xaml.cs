#region License
/*
Copyright © 2014-2018 European Support Limited

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
using System.Windows;
using System.Windows.Controls;
using Ginger.UserControls;
using GingerCore.Environments;
using GingerCore;

namespace Ginger.Environments
{
    /// <summary>
    /// Interaction logic for AppLoginUsersWindow.xaml
    /// </summary>
    public partial class AppLoginUsersPage : Page
    {
        public EnvApplication AppOwner { get; set; }

        public AppLoginUsersPage(EnvApplication applicationOwner)
        {
            InitializeComponent();
            AppOwner = applicationOwner;
            //Set grid look and data
            SetGridView();
            SetGridData();
        }

        #region Events

        private void AddLogin(object sender, RoutedEventArgs e)
        {
            AppOwner.LoginUsers.Add(new LoginUser() { UserProfileName = "Profile " + AppOwner.LoginUsers.Count, Type = "", Username = "", Password = "" });
        }    

        #endregion Events

        #region Functions

        private void SetGridView()
        {
            //Set the Tool Bar look
            grdAppLogins.ShowEdit = Visibility.Collapsed;
            grdAppLogins.ShowUpDown = Visibility.Collapsed;
            grdAppLogins.ShowUndo = Visibility.Visible;
            grdAppLogins.ShowHeader = Visibility.Collapsed;
            grdAppLogins.ShowRefresh = Visibility.Collapsed;
            grdAppLogins.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddLogin));
            //Set the Data Grid columns
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();
            view.GridColsView.Add(new GridColView() { Field = nameof(LoginUser.Type), Header = "Type", WidthWeight = 150 }); //TODO: to show types in combo box?
            view.GridColsView.Add(new GridColView() { Field = nameof(LoginUser.UserProfileName), Header = "User Profile Name", WidthWeight = 200 });
            view.GridColsView.Add(new GridColView() { Field = nameof(LoginUser.Username), Header = "User Name", WidthWeight = 150 });
            view.GridColsView.Add(new GridColView() { Field = nameof(LoginUser.Password), Header = "Password", WidthWeight = 150 });
            grdAppLogins.SetAllColumnsDefaultView(view);
            grdAppLogins.InitViewItems();
        }

        private void SetGridData()
        {
            //###TODO: Add dynamic data load

            grdAppLogins.DataSourceList = AppOwner.LoginUsers;
        }

        #endregion Functions
    }
}
