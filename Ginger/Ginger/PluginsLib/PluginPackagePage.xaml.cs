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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Repository.PlugInsLib;
using Amdocs.Ginger.Repository;
using Ginger.UserControls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Ginger.PlugInsWindows
{
    /// <summary>
    /// Interaction logic for PluginPackagePage.xaml
    /// </summary>
    public partial class PluginPackagePage : Page
    {
        PluginPackage mPluginPackage;

        public PluginPackagePage(PluginPackage p)
        {
            InitializeComponent();
            mPluginPackage = p;
         
            Init();
         
        }

        private void Init()
        {            
            // xSummaryTextBlock.Text = string.Empty;
            // TextBlockHelper TBH = new TextBlockHelper(txtBlkDescritpion);
            // xSummaryTextBlock.Text = mPluginPackage.des

            PlugInNamelbl.BindControl(mPluginPackage, nameof(PluginPackage.PluginId));
            xPlugInPackageVersionLabel.BindControl(mPluginPackage, nameof(PluginPackage.PluginPackageVersion));
            xPlugInFolderLabel.BindControl(mPluginPackage, nameof(PluginPackage.Folder));

            mPluginPackage.LoadServicesFromJSON();
            SetServicesGrid();
            SetActionsGrid();
            // SetTextEditorGrid();
        }


        private void SetServicesGrid()
        {            
            xServicesGrid.ShowEdit = Visibility.Collapsed;
            xServicesGrid.ShowUpDown = Visibility.Collapsed;
            xServicesGrid.ShowTitle = Visibility.Collapsed;

            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();
            view.GridColsView.Add(new GridColView() { Field = nameof(PluginServiceInfo.ServiceId), Header = "Service Id", AllowSorting = true, WidthWeight = 200, BindingMode = BindingMode.OneWay });
            view.GridColsView.Add(new GridColView() { Field = nameof(PluginServiceInfo.Description), Header = "Description", AllowSorting = true, WidthWeight = 400, BindingMode = BindingMode.OneWay });

            xServicesGrid.SetAllColumnsDefaultView(view);
            xServicesGrid.InitViewItems();
            xServicesGrid.DataSourceList = mPluginPackage.Services;


        }

        private void SetActionsGrid()
        {
            xServiceActionGrid.ShowEdit = Visibility.Collapsed;
            xServiceActionGrid.ShowUpDown = Visibility.Collapsed;
            // xServiceActionGrid.ShowTitle = Visibility.Collapsed;

            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();
            
            view.GridColsView.Add(new GridColView() { Field = nameof(PluginServiceActionInfo.ActionId), Header = "Action Id", AllowSorting = true, WidthWeight = 200, BindingMode = BindingMode.OneWay });
            view.GridColsView.Add(new GridColView() { Field = nameof(PluginServiceActionInfo.Description), Header = "Description", AllowSorting = true, WidthWeight = 400, BindingMode = BindingMode.OneWay });

            xServiceActionGrid.SetAllColumnsDefaultView(view);
            xServiceActionGrid.InitViewItems();            
        }

        private void SetTextEditorGrid()
        {
            PlugInsEditorActionsGrid.ShowEdit = Visibility.Collapsed;
            PlugInsEditorActionsGrid.ShowUpDown = Visibility.Collapsed;
            PlugInsEditorActionsGrid.ShowTitle = Visibility.Collapsed;

            GridViewDef view1 = new GridViewDef(GridViewDef.DefaultViewName);
            view1.GridColsView = new ObservableList<GridColView>();
            view1.GridColsView.Add(new GridColView() { Field = "EditorName", Header = "Editor Name", WidthWeight = 300, BindingMode = BindingMode.OneWay });
            view1.GridColsView.Add(new GridColView() { Field = "ExtensionsAsString", Header = "Supported File Extensions", WidthWeight = 300, BindingMode = BindingMode.OneWay });

            PlugInsEditorActionsGrid.SetAllColumnsDefaultView(view1);
            PlugInsEditorActionsGrid.InitViewItems();

            PlugInsEditorActionsGrid.Grid.ItemsSource = PluginTextEditorHelper.GetTextFileEditors(mPluginPackage); 
        }

        

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //try
            //{
            //    if (PlugInTab.SelectedItem != null)
            //    {
            //        foreach (TabItem tab in PlugInTab.Items)
            //        {
            //            foreach (object ctrl in ((StackPanel)(tab.Header)).Children)

            //                if (ctrl.GetType() == typeof(TextBlock))
            //                {
            //                    if (PlugInTab.SelectedItem == tab)
            //                        ((TextBlock)ctrl).Foreground = (SolidColorBrush)FindResource("$SelectionColor_Pink");
            //                    else
            //                        ((TextBlock)ctrl).Foreground = (SolidColorBrush)FindResource("$Color_DarkBlue");

            //                    ((TextBlock)ctrl).FontWeight = FontWeights.Bold;
            //                }
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Reporter.ToLog(eAppReporterLogLevel.ERROR, "Error in PlugIn tabs style", ex);
            //}
        }

        private void XServicesGrid_RowChangedEvent(object sender, System.EventArgs e)
        {
            PluginServiceInfo pluginServiceInfo = (PluginServiceInfo)xServicesGrid.CurrentItem;
            xServiceActionGrid.DataSourceList = new ObservableList<PluginServiceActionInfo>(pluginServiceInfo.Actions);
        }
    }
}