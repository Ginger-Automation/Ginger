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
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.CoreNET.PlugInsLib;
using Amdocs.Ginger.Repository;
using Ginger.UserControls;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

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
            xInstallButton.Visibility = Visibility.Collapsed;

            OnlinePluginPackageRelease release = (OnlinePluginPackageRelease)xVersionComboBox.SelectedItem;            
            if (release != null)
            {
                xPublishedTextBlock.Text = release.published_at;
                xReleaseNameTextBlock.Text = release.name;
                if (release.assets.Count > 0)
                {
                    xSizeTextBlock.Text = release.assets[0].size / 1000 + " KB";
                    xDownloads.Text = release.assets[0].download_count;
                    xInstallButton.Visibility = Visibility.Visible;
                }
               
            }
            SetInstallButton();
        }

        private void SetInstallButton()
        {
            OnlinePluginPackageRelease release = (OnlinePluginPackageRelease)xVersionComboBox.SelectedItem;
            OnlinePluginPackage pluginPackageInfo = (OnlinePluginPackage)xPluginsGrid.CurrentItem;
            if (release!=null && pluginPackageInfo.CurrentPackage == release.Version)
            {
                xInstallButton.IsEnabled = false;
            }
            else
            {
                xInstallButton.IsEnabled = true;
            }
        }

        private void SetGridView()
        {
                 

            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();
                                    
            view.GridColsView.Add(new GridColView() { Field = nameof(OnlinePluginPackage.Id), Header="Plugin ID", WidthWeight = 30, ReadOnly=true });
            view.GridColsView.Add(new GridColView() { Field = nameof(OnlinePluginPackage.Description), WidthWeight = 30, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(OnlinePluginPackage.URL), WidthWeight = 30, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(OnlinePluginPackage.Status), WidthWeight = 30, ReadOnly = true });

            xPluginsGrid.SetAllColumnsDefaultView(view);
            xPluginsGrid.InitViewItems();

            xPluginsGrid.AddToolbarTool(eImageType.OpenFolder, "View Downloaded Plugins Packages", new RoutedEventHandler(OpenDownloadedPluginsFolder));
            xPluginsGrid.btnRefresh.Click += BtnRefresh_Click;
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
            xProcessingImage.Visibility = Visibility.Visible;
            xPluginsGrid.DataSourceList = WorkSpace.Instance.PlugInsManager.GetOnlinePluginsIndex();
            xProcessingImage.Visibility = Visibility.Collapsed;         
        }


        private void ShowPluginInfo()
        {            
            xProcessingImage.Visibility = Visibility.Visible;                        
            OnlinePluginPackage pluginPackageInfo = (OnlinePluginPackage)xPluginsGrid.CurrentItem;
            if(!string.IsNullOrEmpty(pluginPackageInfo.CurrentPackage))
            {
                xInstalledSection.Visibility = Visibility.Visible;
                xInstalledVersion.Text = pluginPackageInfo.CurrentPackage;
            }
            else
            {
                xInstalledSection.Visibility = Visibility.Collapsed;
            }
            xNameTextBlock.Text = pluginPackageInfo.Id;
            xHyperLinkBtn.Content = pluginPackageInfo.URL;           
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
      
        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(((Button)sender).Content.ToString());
        }

        private void xInstallButonn_Click(object sender, RoutedEventArgs e)
        {
            xInstallButton.ButtonText = "Downloading & Installing...";
            xProcessingImage.Visibility = Visibility.Visible;
            OnlinePluginPackageRelease release = (OnlinePluginPackageRelease)xVersionComboBox.SelectedItem;
            OnlinePluginPackage onlinePluginPackage = (OnlinePluginPackage)xPluginsGrid.CurrentItem;
            if (!string.IsNullOrEmpty(onlinePluginPackage.CurrentPackage) && onlinePluginPackage.CurrentPackage != release.Version)
            {
                UninstallPlugin();
            }           
            Task.Factory.StartNew(() =>
            {
                WorkSpace.Instance.PlugInsManager.InstallPluginPackage(onlinePluginPackage, release);
                onlinePluginPackage.Status = "Installed";                
            }).ContinueWith((a) =>
            {
                Dispatcher.Invoke(() =>
                {
                    xProcessingImage.Visibility = Visibility.Collapsed;
                    xInstallButton.ButtonText = "Install";
                    xInstalledVersion.Text = onlinePluginPackage.CurrentPackage;                    
                });
            });
            xInstalledSection.Visibility = Visibility.Visible;                                                  
                        
            ((OnlinePluginPackage)xPluginsGrid.CurrentItem).CurrentPackage = release.Version;

            SetInstallButton();
        }

        private void OpenDownloadedPluginsFolder(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(PluginPackage.LocalPluginsFolder))
            {
                Process.Start(PluginPackage.LocalPluginsFolder);
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.StaticErrorMessage, string.Format("Failed to find the folder '{0}'", PluginPackage.LocalPluginsFolder));
            }
        }

        private void xUnInstallButonn_Click(object sender, RoutedEventArgs e)
        {
            UninstallPlugin();
            xInstalledSection.Visibility = Visibility.Collapsed;
            SetInstallButton();
        }        

        private void UninstallPlugin()
        {
            OnlinePluginPackage pluginPackageInfo = (OnlinePluginPackage)xPluginsGrid.CurrentItem;
            WorkSpace.Instance.PlugInsManager.UninstallPluginPackage(pluginPackageInfo);            
            pluginPackageInfo.Status = string.Empty;
            pluginPackageInfo.CurrentPackage = string.Empty;            
        }
    }
}
