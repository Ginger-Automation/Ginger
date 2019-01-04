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
    /// Interaction logic for AppUnixServersWindow.xaml
    /// </summary>
    public partial class AppUnixServersPage : Page
    {
        public EnvApplication AppOwner { get; set; }

        public AppUnixServersPage(EnvApplication applicationOwner)
        {
            InitializeComponent();

            AppOwner = applicationOwner;
            //Set grid look and data
            SetGridView();
            SetGridData();            
        }

        #region Events
        private void TestUnixConnection(object sender, RoutedEventArgs e)
        {
            //TODO:             
            Reporter.ToUser(eUserMsgKeys.MissingImplementation);
        }

        private void AddUnixSvr(object sender, RoutedEventArgs e)
        {
            AppOwner.UnixServers.Add(new UnixServer() { Name = "Unix " + AppOwner.UnixServers.Count, Description = "unix server", Host = "", Username = "", Password = "", RootPath = @"~/" });
        }        
        #endregion Events

        #region Functions
        private void SetGridView()
        {
            //Set the grid name
            grdAppUnixs.Title = "'" + AppOwner.Name + "' Application Unix Servers";

            //Set the Tool Bar look
            grdAppUnixs.ShowEdit = Visibility.Collapsed;
            grdAppUnixs.ShowUpDown = Visibility.Collapsed;
            grdAppUnixs.ShowUndo = Visibility.Visible;
            grdAppUnixs.AddButton("Test Connection", new RoutedEventHandler(TestUnixConnection));
            grdAppUnixs.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddUnixSvr));
            //Set the Data Grid columns
            
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();

            view.GridColsView.Add(new GridColView() { Field = nameof(UnixServer.Name), WidthWeight = 300 });
            view.GridColsView.Add(new GridColView() { Field = nameof(UnixServer.Description), WidthWeight = 150 });
            view.GridColsView.Add(new GridColView() { Field = nameof(UnixServer.Host), WidthWeight = 150 });
            view.GridColsView.Add(new GridColView() { Field = nameof(UnixServer.Username), WidthWeight = 150 });
            view.GridColsView.Add(new GridColView() { Field = nameof(UnixServer.Password), WidthWeight = 150 });
            view.GridColsView.Add(new GridColView() { Field = nameof(UnixServer.PrivateKey), WidthWeight = 150 });
            view.GridColsView.Add(new GridColView() { Field = nameof(UnixServer.PrivateKeyPassPhrase), WidthWeight = 150 });
            view.GridColsView.Add(new GridColView() { Field = nameof(UnixServer.RootPath), WidthWeight = 200, Header = "Root Path" });
            
            grdAppUnixs.SetAllColumnsDefaultView(view);
            grdAppUnixs.InitViewItems();
        }

        private void SetGridData()
        {
            //###TODO: Add dynamic data load
             grdAppUnixs.DataSourceList = AppOwner.UnixServers;
            
        }
        #endregion Functions
    }
}
