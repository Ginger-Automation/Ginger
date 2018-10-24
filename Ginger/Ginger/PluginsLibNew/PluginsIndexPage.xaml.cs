using Amdocs.Ginger.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
            GetPluginsList();                        
        }

        private void GetPluginsList()
        {
            xJSON.Text = "Loading...";
            xJSON.Refresh();
            PluginsManager p = new PluginsManager();
            string s = p.GetPluginsIndex();
            xJSON.Text = s;
        }

        private void xRefreshButton_Click(object sender, RoutedEventArgs e)
        {
            GetPluginsList();
        }

        private void xInstallButton_Click(object sender, RoutedEventArgs e)
        {
            PluginsManager p = new PluginsManager();
            //TODO: get the selected Plugin Package and install
            p.InstallPluginPackage();
        }



        

    }
}
