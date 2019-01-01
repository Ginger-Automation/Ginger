using Amdocs.Ginger.Common;
using System.Windows;

namespace Ginger.ReporterLib
{
    /// <summary>
    /// Interaction logic for ReporterTestWindow.xaml
    /// </summary>
    public partial class ReporterTestWindow : Window
    {
        public ReporterTestWindow()
        {
            InitializeComponent();
        }

        private void XSimpleInfoMessageButton_Click(object sender, RoutedEventArgs e)
        {
            Amdocs.Ginger.Common.MessageBoxResult messageBoxResult = Reporter.ToUser(eUserMsgKeys.StaticInfoMessage, "Simple Info Message");
            MessageBox.Show("You selected: " + messageBoxResult);
        }

        private void XYesNoButton_Click(object sender, RoutedEventArgs e)
        {
            Amdocs.Ginger.Common.MessageBoxResult messageBoxResult = Reporter.ToUser(eUserMsgKeys.AskIfSureWantToClose, "param1", "param2");
            MessageBox.Show("You selected: " + messageBoxResult);
        }
    }
}
