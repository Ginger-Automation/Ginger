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
using GingerCore;
using Ginger.UserControls;

namespace Ginger.BusinessFlowWindows
{
    /// <summary>
    /// Interaction logic for ExplorerBusinessFlowsPage.xaml
    /// </summary>
    public partial class ExplorerBusinessFlowsPage : Page
    {
        string mFolder;
        public ExplorerBusinessFlowsPage(string Folder)
        {
            InitializeComponent();
            
            mFolder = Folder;
            grdBusinessFlows.btnRefresh.AddHandler(Button.ClickEvent, new RoutedEventHandler(RefreshGrid));
            SetBusinessFlowsGridView();
            SetGridData();
        }

        private void SetBusinessFlowsGridView()
        {
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();

            view.GridColsView.Add(new GridColView() { Field = BusinessFlow.Fields.Name, WidthWeight = 250 });
            view.GridColsView.Add(new GridColView() { Field = BusinessFlow.Fields.Description, WidthWeight = 250 });
            view.GridColsView.Add(new GridColView() { Field = RepositoryItem.Fields.FileName,Header = "Local Path", WidthWeight = 250 });
            view.GridColsView.Add(new GridColView() { Field = BusinessFlow.Fields.Status, WidthWeight = 50 });

            grdBusinessFlows.SetAllColumnsDefaultView(view);
            grdBusinessFlows.InitViewItems();
            grdBusinessFlows.ShowTagsFilter = Visibility.Visible;
        }

        private void RefreshGrid(object sender, RoutedEventArgs e)
        {
            SetGridData();
        }

        private void SetGridData()
        {
            grdBusinessFlows.DataSourceList = App.LocalRepository.GetSolutionBusinessFlows(specificFolderPath: mFolder, includeSubFolders: true);
        }
    }
}