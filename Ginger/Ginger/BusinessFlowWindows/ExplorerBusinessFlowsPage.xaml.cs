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
using Amdocs.Ginger.Repository;
using amdocs.ginger.GingerCoreNET;

namespace Ginger.BusinessFlowWindows
{
    /// <summary>
    /// Interaction logic for ExplorerBusinessFlowsPage.xaml
    /// </summary>
    public partial class ExplorerBusinessFlowsPage : Page
    {        
        public ExplorerBusinessFlowsPage(RepositoryFolder<BusinessFlow> repositoryFolder)
        {
            InitializeComponent();
                        
            grdBusinessFlows.btnRefresh.Visibility = Visibility.Collapsed; 
            SetBusinessFlowsGridView();

            if (repositoryFolder.IsRootFolder)
            {
                grdBusinessFlows.DataSourceList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>();
            }
            else
            {
                grdBusinessFlows.DataSourceList = repositoryFolder.GetFolderItems();
            }
        }

        private void SetBusinessFlowsGridView()
        {
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();

            view.GridColsView.Add(new GridColView() { Field = nameof(BusinessFlow.Name), WidthWeight = 250 });
            view.GridColsView.Add(new GridColView() { Field = nameof(BusinessFlow.Description), WidthWeight = 250 });
            view.GridColsView.Add(new GridColView() { Field = nameof(BusinessFlow.FilePath) ,Header = "Local Path", WidthWeight = 250 });
            view.GridColsView.Add(new GridColView() { Field = nameof(BusinessFlow.Status), WidthWeight = 50 });

            grdBusinessFlows.SetAllColumnsDefaultView(view);
            grdBusinessFlows.InitViewItems();
            grdBusinessFlows.ShowTagsFilter = Visibility.Visible;
        }

        


    }
}