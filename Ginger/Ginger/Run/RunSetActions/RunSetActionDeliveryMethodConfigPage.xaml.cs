using GingerCore.GeneralLib;
using System.Windows;
using System.Windows.Controls;
using GingerCore;
namespace Ginger.Run.RunSetActions
{
    /// <summary>
    /// Interaction logic for RunSetActionDeliveryMethodConfigPage.xaml
    /// </summary>
    public partial class RunSetActionDeliveryMethodConfigPage : Page
    {
        public RunSetActionDeliveryMethodConfigPage(Email runSetAction)
        {
            InitializeComponent();
            
            xSMTPMailHostTextBox.Init(null, runSetAction, nameof(Email.SMTPMailHost));
            BindingHandler.ObjFieldBinding(xSMTPPortTextBox, TextBox.TextProperty, runSetAction, nameof(Email.SMTPPort));
            xSMTPUserTextBox.Init(null, runSetAction, nameof(Email.SMTPUser));
            BindingHandler.ObjFieldBinding(xSMTPPassTextBox, TextBox.TextProperty, runSetAction, nameof(Email.SMTPPass));
            GingerCore.General.FillComboFromEnumObj(xEmailMethodComboBox, runSetAction.EmailMethod);
            BindingHandler.ObjFieldBinding(xEmailMethodComboBox, ComboBox.SelectedValueProperty, runSetAction, nameof(Email.EmailMethod));
        }
        private void xEmailMethodComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (xEmailMethodComboBox.SelectedItem.ToString() == "OUTLOOK")
            {
                xSMTPConfig.Visibility = Visibility.Collapsed;
            }
            else
            {
                xSMTPConfig.Visibility = Visibility.Visible;
            }
        }
        private void xcbConfigureCredential_Checked(object sender, RoutedEventArgs e)
        {
            xSMTPUserTextBox.Visibility = Visibility.Visible;
            xSMTPPassTextBox.Visibility = Visibility.Visible;
            xLabelPass.Visibility = Visibility.Visible;
            xLabelUser.Visibility = Visibility.Visible;
        }
        private void xcbConfigureCredential_Unchecked(object sender, RoutedEventArgs e)
        {
            xSMTPUserTextBox.Visibility = Visibility.Collapsed;
            xSMTPPassTextBox.Visibility = Visibility.Collapsed;
            xLabelPass.Visibility = Visibility.Collapsed;
            xLabelUser.Visibility = Visibility.Collapsed;
        }
        private void xSMTPPassTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            bool res = false;
            if (!EncryptionHandler.IsStringEncrypted(xSMTPPassTextBox.Text))
            {
                xSMTPPassTextBox.Text = EncryptionHandler.EncryptString(xSMTPPassTextBox.Text, ref res);
                if (res == false)
                {
                    xSMTPPassTextBox.Text = string.Empty;
                }
            }
        }
    }
}
