using GingerCore.GeneralLib;
using System.Windows;
using System.Windows.Controls;
using GingerCore;
using Amdocs.Ginger.Common;
using amdocs.ginger.GingerCoreNET;

namespace Ginger.Run.RunSetActions
{
    /// <summary>
    /// Interaction logic for RunSetActionDeliveryMethodConfigPage.xaml
    /// </summary>
    public partial class RunSetActionDeliveryMethodConfigPage : Page
    {
        public RunSetActionDeliveryMethodConfigPage(Email email)
        {
            InitializeComponent();
            Context context = new Context() { Environment = WorkSpace.Instance.RunsetExecutor.RunsetExecutionEnvironment };
            xSMTPMailHostTextBox.Init(context, email, nameof(Email.SMTPMailHost));
            BindingHandler.ObjFieldBinding(xSMTPPortTextBox, TextBox.TextProperty, email, nameof(Email.SMTPPort));
            xSMTPUserTextBox.Init(context, email, nameof(Email.SMTPUser));
            BindingHandler.ObjFieldBinding(xSMTPPassTextBox, TextBox.TextProperty, email, nameof(Email.SMTPPass));
            GingerCore.General.FillComboFromEnumObj(xEmailMethodComboBox, email.EmailMethod);
            BindingHandler.ObjFieldBinding(xEmailMethodComboBox, ComboBox.SelectedValueProperty, email, nameof(Email.EmailMethod));
            BindingHandler.ObjFieldBinding(xcbEnableSSL, CheckBox.IsCheckedProperty, email, nameof(Email.EnableSSL));
            BindingHandler.ObjFieldBinding(xcbConfigureCredential, CheckBox.IsCheckedProperty, email, nameof(Email.ConfigureCredential));
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
