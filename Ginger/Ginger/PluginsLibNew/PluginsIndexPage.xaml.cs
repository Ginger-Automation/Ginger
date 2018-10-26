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
        }

        private void SetGridView()
        {
            xPluginsGrid.btnRefresh.Click += BtnRefresh_Click;
            xPluginsGrid.AddButton("Install", InstallPlugin);
            // grdActions.AddToolbarTool(eImageType.Reset, "Reset Run Details", new RoutedEventHandler(ResetAction));

            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();
                                    
            view.GridColsView.Add(new GridColView() { Field = nameof(OnlinePluginPackage.Name), WidthWeight = 30 });
            view.GridColsView.Add(new GridColView() { Field = nameof(OnlinePluginPackage.Description), WidthWeight = 30 });
            view.GridColsView.Add(new GridColView() { Field = nameof(OnlinePluginPackage.URL), WidthWeight = 30 });

            xPluginsGrid.SetAllColumnsDefaultView(view);
            xPluginsGrid.InitViewItems();
        }

        private void InstallPlugin(object sender, RoutedEventArgs e)
        {
            PluginsManager p = new PluginsManager();            
            p.InstallPluginPackage((OnlinePluginPackage)xPluginsGrid.CurrentItem);
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            GetPluginsList();
        }

        private void GetPluginsList()
        {
            SetStatus("Loading...");
            PluginsManager p = new PluginsManager();
            xPluginsGrid.DataSourceList = p.GetPluginsIndex();
            SetStatus("Found " + xPluginsGrid.DataSourceList.Count + " Plugin Packages");
        }


        private void SetStatus(string text)
        {
            xStatusTextBlock.Text = text;
            xStatusTextBlock.Refresh();
        }
        
        

    }
}
