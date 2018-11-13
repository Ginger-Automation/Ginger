using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using Ginger.UserControls;
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

            dynamic release = (dynamic)xVersionComboBox.SelectedItem;
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
                                    
            view.GridColsView.Add(new GridColView() { Field = nameof(OnlinePluginPackage.Name), WidthWeight = 30 });
            view.GridColsView.Add(new GridColView() { Field = nameof(OnlinePluginPackage.Description), WidthWeight = 30 });
            view.GridColsView.Add(new GridColView() { Field = nameof(OnlinePluginPackage.URL), WidthWeight = 30 });

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
            SetStatus("Loading...");            
            xPluginsGrid.DataSourceList = WorkSpace.Instance.PlugInsManager.GetPluginsIndex();
            SetStatus("Found " + xPluginsGrid.DataSourceList.Count + " Plugin Packages");
        }


        private void SetStatus(string text)
        {
            xStatusTextBlock.Text = text;
            xStatusTextBlock.Refresh();
        }


        private void ShowPluginInfo()
        {                        
            OnlinePluginPackage pluginPackageInfo = (OnlinePluginPackage)xPluginsGrid.CurrentItem;            
            xNameTextBlock.Text = pluginPackageInfo.Name;            
            xVersionComboBox.ItemsSource = WorkSpace.Instance.PlugInsManager.GetPluginReleases(pluginPackageInfo.URL);
            xVersionComboBox.DisplayMemberPath = "tag_name";
            // select the first item/latest release
            xVersionComboBox.SelectedIndex = 0;
        }

        private void xInstallButonn_Click(object sender, RoutedEventArgs e)
        {
            dynamic release = (dynamic)xVersionComboBox.SelectedItem;
            string zipFileURL = release.assets[0].browser_download_url;
            string version = release.tag_name;
            WorkSpace.Instance.PlugInsManager.InstallPluginPackage((OnlinePluginPackage)xPluginsGrid.CurrentItem, version , zipFileURL);
        }
    }
}
