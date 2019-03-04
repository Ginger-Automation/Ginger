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
using GingerCore;
using Ginger.UserControls;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.BusinessFlowWindows
{
    /// <summary>
    /// Interaction logic for OpenBusinessFlowWindow.xaml
    /// </summary>
    public partial class OpenBusinessFlowWindow : Window
    {
        // TODO: not used delete me
        public OpenBusinessFlowWindow()
        {
            InitializeComponent();

            this.Title = "Open " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow);
            grdBusinessFlows.btnRefresh.AddHandler(Button.ClickEvent, new RoutedEventHandler(RefreshGrid));
            SetBusinessFlowsGridView();
        }

        private void SetBusinessFlowsGridView()
        {
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            ObservableList<GridColView> viewCols = new ObservableList<GridColView>();
            view.GridColsView = viewCols;
            viewCols.Add(new GridColView() { Field = BusinessFlow.Fields.Name, WidthWeight = 250 });
            viewCols.Add(new GridColView() { Field = BusinessFlow.Fields.Description, WidthWeight = 250 });
            viewCols.Add(new GridColView() { Field = BusinessFlow.Fields.Status, WidthWeight = 50 });
            grdBusinessFlows.SetAllColumnsDefaultView(view);
            grdBusinessFlows.InitViewItems();
        }

        private void RefreshGrid(object sender, RoutedEventArgs e)
        {
        }

        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            //App.BusinessFlow = (BusinessFlow)grdBusinessFlows.CurrentItem;
            //App.BusinessFlow.CurrentActivity = App.BusinessFlow.Activities.FirstOrDefault();
            //this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
