#region License
/*
Copyright © 2014-2019 European Support Limited

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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using Ginger.UserControls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Ginger.PlugInsWindows
{
    /// <summary>
    /// Interaction logic for PlugInsPage.xaml
    /// </summary>
    public partial class PlugInsPage : Page
    {
        public PlugInsPage()
        {
            InitializeComponent();
            SetGrids();
        }

        public void SetGrids()
        {
            SetPlugInsGridView();            
        }

        private void SetPlugInsGridView()
        {
            //Set the Tool Bar look
            PlugInsGrid.ShowEdit = Visibility.Collapsed;
            PlugInsGrid.ShowUpDown = Visibility.Collapsed;
            PlugInsGrid.SetTitleLightStyle = true;
            
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();

            view.GridColsView.Add(new GridColView() { Field = nameof(PluginPackage.PluginId) ,Header = "Plugin ID", WidthWeight = 300, BindingMode = BindingMode.OneWay });
            view.GridColsView.Add(new GridColView() { Field = nameof(PluginPackage.Folder ), Header = "Folder", WidthWeight = 300, BindingMode = BindingMode.OneWay });
            view.GridColsView.Add(new GridColView() { Field = nameof(PluginPackage.PluginPackageVersion ), Header = "Version", WidthWeight = 300, BindingMode = BindingMode.OneWay });
            //view.GridColsView.Add(new GridColView() { Field = "Description", WidthWeight = 300, BindingMode = BindingMode.OneWay });
            //view.GridColsView.Add(new GridColView() { Field = "PlugInType", Header = "Type", WidthWeight = 300, BindingMode = BindingMode.OneWay }); 
            

            PlugInsGrid.SetAllColumnsDefaultView(view);
            PlugInsGrid.InitViewItems();            
            PlugInsGrid.DataSourceList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<PluginPackage>();
        }

        

        
    }
}