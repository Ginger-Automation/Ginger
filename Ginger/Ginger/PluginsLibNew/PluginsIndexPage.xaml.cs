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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.PlugInsLib;
using Amdocs.Ginger.Repository;
using Ginger.UserControls;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.PlugInsWindows
{
    /// <summary>
    /// Interaction logic for PluginsIndexPage.xaml
    /// </summary>
    public partial class PluginsIndexPage : Page
    {
        public PluginsIndexPage()
        {
            InitializeComponent();
            SetGridView();
            GetPluginsList();
            xVersionComboBox.SelectionChanged += XVersionComboBox_SelectionChanged;
        }

        private void XVersionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            xPublishedTextBlock.Text = null;
            xReleaseNameTextBlock.Text = null;
            xSizeTextBlock.Text = null;
            xDownloads.Text = null;
            xInstallButonn.Visibility = Visibility.Collapsed;

            OnlinePluginPackageRelease release = (OnlinePluginPackageRelease)xVersionComboBox.SelectedItem;
            if (release != null)
            {
                xPublishedTextBlock.Text = release.published_at;
                xReleaseNameTextBlock.Text = release.name;
                if (release.assets.Count > 0)
                {
                    xSizeTextBlock.Text = release.assets[0].size / 1000 + " KB";
                    xDownloads.Text = release.assets[0].download_count;
                    xInstallButonn.Visibility = Visibility.Visible;
                }            
            }            
        }

        private void SetGridView()
        {
            xPluginsGrid.btnRefresh.Click += BtnRefresh_Click;        

            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();
                                    
            view.GridColsView.Add(new GridColView() { Field = nameof(OnlinePluginPackage.Id), WidthWeight = 30 });
            view.GridColsView.Add(new GridColView() { Field = nameof(OnlinePluginPackage.Description), WidthWeight = 30 });
            view.GridColsView.Add(new GridColView() { Field = nameof(OnlinePluginPackage.URL), WidthWeight = 30 });
            view.GridColsView.Add(new GridColView() { Field = nameof(OnlinePluginPackage.Status), WidthWeight = 30 });

            xPluginsGrid.SetAllColumnsDefaultView(view);
            xPluginsGrid.InitViewItems();

            xPluginsGrid.SelectedItemChanged += XPluginsGrid_SelectedItemChanged;
        }

        private void XPluginsGrid_SelectedItemChanged(object selectedItem)
        {
            ShowPluginInfo();
        }


        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            GetPluginsList();
        }

        private void GetPluginsList()
        {            
            PluginsManager p = new PluginsManager(WorkSpace.Instance.SolutionRepository);
            xProcessingImage.Visibility = Visibility.Visible;
            xPluginsGrid.DataSourceList = p.GetOnlinePluginsIndex();
            xProcessingImage.Visibility = Visibility.Collapsed;         
        }


        private void ShowPluginInfo()
        {            
            xProcessingImage.Visibility = Visibility.Visible;                        
            OnlinePluginPackage pluginPackageInfo = (OnlinePluginPackage)xPluginsGrid.CurrentItem;            
            xNameTextBlock.Text = pluginPackageInfo.Id;
            ObservableList<OnlinePluginPackageRelease> list = null;
            Task.Factory.StartNew(() =>
            {
                list = pluginPackageInfo.Releases;
                
            }).ContinueWith((a) => {
                    Dispatcher.Invoke(() =>
                    {
                        xVersionComboBox.ItemsSource = list;
                        xVersionComboBox.DisplayMemberPath = nameof(OnlinePluginPackageRelease.Version);
                        // select the first item/latest release
                        xVersionComboBox.SelectedIndex = 0;
                        xProcessingImage.Visibility = Visibility.Collapsed;
                    });
            });
        }

        private void xInstallButonn_Click(object sender, RoutedEventArgs e)
        {
            xProcessingImage.Visibility = Visibility.Visible;
            OnlinePluginPackageRelease release = (OnlinePluginPackageRelease)xVersionComboBox.SelectedItem;
            Task.Factory.StartNew(() =>
            {                                
                PluginsManager p = new PluginsManager(WorkSpace.Instance.SolutionRepository);
                OnlinePluginPackage onlinePluginPackage = (OnlinePluginPackage)xPluginsGrid.CurrentItem;
                p.InstallPluginPackage(onlinePluginPackage, release);
                onlinePluginPackage.Status = "Installed";
            }).ContinueWith((a) =>
            {                
                Dispatcher.Invoke(() =>
                {
                    xProcessingImage.Visibility = Visibility.Collapsed;
                });
            });
        }
    }
}
