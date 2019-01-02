using Amdocs.Ginger.Common;
using System;
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

            FillListBox();
        }

        private void FillListBox()
        {
            Array arr = Enum.GetValues(typeof(eUserMsgKeys));
            foreach (eUserMsgKeys o in arr)
            {
                UserMessage mess;
                bool b = Reporter.UserMessagesPool.TryGetValue(o, out mess);               
                if (!b)
                {
                    // TODO: FIXME handle missing messages
                }
            }
            xListBox.ItemsSource = arr;
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

        private void XListBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            eUserMsgKeys mess = (eUserMsgKeys)xListBox.SelectedValue;
            Amdocs.Ginger.Common.MessageBoxResult messageBoxResult = Reporter.ToUser(mess, "This is a message");
            MessageBox.Show("You selected: " + messageBoxResult);
        }
    }
}
