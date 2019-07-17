using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.CoreNET.TelemetryLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

namespace Ginger.GeneralWindows
{
    /// <summary>
    /// Interaction logic for VersionAndNewsPage.xaml
    /// </summary>
    public partial class VersionAndNewsPage : Page
    {
        GenericWindow _pageGenericWin = null;

        public VersionAndNewsPage()
        {
            InitializeComponent();

            xMessage.Content = Telemetry.VersionAndNewsInfo;
        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {
            this.Width = 550;            
            GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, windowStyle, this.Title, this, null, true, "Cancel");            
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
