using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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

namespace Ginger.Help
{
    /// <summary>
    /// Interaction logic for GingerWebHelpPage.xaml
    /// </summary>
    public partial class GingerWebHelpPage : Page
    {
        string mHelpFolder = string.Empty;
        string mHelpIndexFile = string.Empty;
        WebBrowser mWebBrowser = null;
        GenericWindow _pageGenericWin = null;
        public GingerWebHelpPage()
        {
            InitializeComponent();

            mHelpFolder = GetGingerHelpLibraryFolder();
            mHelpIndexFile = System.IO.Path.Combine(mHelpFolder, "index.html");

            mWebBrowser = new WebBrowser();
            HelpBrowserFrm.Content = mWebBrowser;

            if (File.Exists(mHelpIndexFile))
            {
                mWebBrowser.Navigate(mHelpIndexFile);
            }
            else
            {
            //show msg
           
                mWebBrowser.Navigate(new Uri(@"https://ginger-automation.github.io/Ginger-Web-Help/"));
            }

            //Add support for search on load
            //https://ginger-automation.github.io/Ginger-Web-Help/?rhsearch=aaa
        }

        private void xBackButton_Click(object sender, RoutedEventArgs e)
        {
            mWebBrowser.GoBack();
        }

        private void xRefreshButton_Click(object sender, RoutedEventArgs e)
        {
            mWebBrowser.Refresh();
        }

        private void xOpenExternallyBtn_Click(object sender, RoutedEventArgs e)
        {
            //if (MainTreeView.Tree.CurrentSelectedTreeViewItem != null)
            //{
            //    TreeViewItemBase item = (TreeViewItemBase)MainTreeView.Tree.CurrentSelectedTreeViewItem;
            //    if (System.IO.File.Exists(item.NodePath()))
            //        Process.Start(item.NodePath());
            //}
            if (File.Exists(mHelpIndexFile))
            {
                Process.Start(mHelpIndexFile);
            }
        }

        private string GetGingerHelpLibraryFolder()
        {
            string folder = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            folder = System.IO.Path.Combine(folder, "Help", "Library");
            if (!System.IO.Directory.Exists(folder))
            {
                System.IO.Directory.CreateDirectory(folder);
            }
            return folder;
        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Free)
        {
            GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, windowStyle, "Ginger by Amdocs Help", this);
        }
    }
}
