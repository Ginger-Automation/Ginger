using Ginger.SolutionGeneral;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace Ginger.UserControlsLib
{
    /// <summary>
    /// Interaction logic for UCEncryptionKey.xaml
    /// </summary>
    public partial class UCEncryptionKey : UserControl
    {
        public Solution mSolution;
        public UCEncryptionKey()
        {
            InitializeComponent();
        }
        private void ShowPassword_PreviewMouseDown(object sender, MouseButtonEventArgs e) => ShowPasswordFunction();
        private void ShowPassword_PreviewMouseUp(object sender, MouseButtonEventArgs e) => HidePasswordFunction();
        private void ShowPassword_MouseLeave(object sender, MouseEventArgs e) => HidePasswordFunction();

        private void ValidateKeyMouseDown(object sender, MouseButtonEventArgs e) => ValidateKey();

        private void ShowPasswordFunction()
        {
            //ShowPassword.Text = "HIDE";
            ShowPassword.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Invisible;
            EncryptionKeyTextBox.Visibility = Visibility.Visible;
            EncryptionKeyPasswordBox.Visibility = Visibility.Hidden;
            EncryptionKeyTextBox.Text = EncryptionKeyPasswordBox.Password;
        }

        private void HidePasswordFunction()
        {
            //ShowPassword.Text = "SHOW";
            ShowPassword.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Visible;
            EncryptionKeyTextBox.Visibility = Visibility.Hidden;
            EncryptionKeyPasswordBox.Visibility = Visibility.Visible;
        }

        public bool ValidateKey()
        {
            if (mSolution.ValidateKey(EncryptionKeyPasswordBox.Password))
            {
                ValidFlag.Visibility = Visibility.Visible;
                InvalidFlag.Visibility = Visibility.Collapsed;
                return true;
            }
            else
            {
                ValidFlag.Visibility = Visibility.Collapsed;
                InvalidFlag.Visibility = Visibility.Visible;
                return false;
            }
        }

        public bool CheckKeyCombination()
        {
            Regex regex = new Regex(@"^.*(?=.{8,16})(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[!*@#$%^&+=]).*$");
            if (!regex.IsMatch(EncryptionKeyPasswordBox.Password))
            {
                InvalidFlag.Visibility = Visibility.Visible;
                ValidFlag.Visibility = Visibility.Collapsed;
                return false;
            }
            InvalidFlag.Visibility = Visibility.Collapsed;
            ValidFlag.Visibility = Visibility.Visible;
            return true;
        }
    }
}
