#region License
/*
Copyright © 2014-2023 European Support Limited

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
using GingerCore;
using GingerCore.DataSource;
using Ginger.UserControls;
using amdocs.ginger.GingerCoreNET;
using System;

namespace Ginger.DataSource
{
    /// <summary>
    /// Interaction logic for ExplorerBusinessFlowsPage.xaml
    /// </summary>
    public partial class DataSourcesPage : Page
    {
        public string Path { get; set; }

        public DataSourcesPage()
        {
            InitializeComponent();
            SetDataSourcesGridView();
            grdDataSources.DataSourceList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>();            
            grdDataSources.btnDelete.AddHandler(Button.ClickEvent, new RoutedEventHandler(DeleteDataSource));
            grdDataSources.btnRefresh.Visibility = Visibility.Collapsed;
        }

        private void DeleteDataSource(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void SetDataSourcesGridView()
        {
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();
            view.GridColsView.Add(new GridColView() { Field = DataSourceBase.Fields.Name, Header = "Data Source Name", WidthWeight = 250 });
            view.GridColsView.Add(new GridColView() { Field = DataSourceBase.Fields.DSType, Header = "Data Source Type", WidthWeight = 250 });
            view.GridColsView.Add(new GridColView() { Field = DataSourceBase.Fields.FilePath, Header = "File Path", WidthWeight = 250 });
            grdDataSources.SetAllColumnsDefaultView(view);
            grdDataSources.InitViewItems();
        }

        

    }
}
