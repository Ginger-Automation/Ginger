using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Ginger.ALM
{
    /// <summary>
    /// Interaction logic for SSOPage.xaml
    /// </summary>
    public partial class SSOPage : Page
    {
        private string ssoURL = string.Empty;
        GenericWindow _pageGenericWin;
        public SSOPage(string loginURL)
        {
            InitializeComponent();
            this.ssoURL = loginURL;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            xBrowser.Navigate(new Uri(this.ssoURL));
        }


        public void ShowAsWindow()
        {
            GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, eWindowShowStyle.Dialog, this.Title, this, closeEventHandler: CloseWindow);
        }

        private void CloseWindow(object sender, EventArgs e)
        {
            xBrowser.Dispose();
            _pageGenericWin.Close();
        }

    }
}
