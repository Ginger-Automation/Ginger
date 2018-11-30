using System;
using System.Collections.Generic;
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
using GingerCore.Variables;
using GingerCore.GeneralLib;
using GingerCore;



namespace Ginger.Variables
{
    /// <summary>
    /// Interaction logic for VariableStringPage.xaml
    /// </summary>
    public partial class VariablePasswordStringPage : Page
    {
        VariablePasswordString mVar;
        public VariablePasswordStringPage(VariablePasswordString var)
        {
            InitializeComponent();
            mVar = var;
            App.ObjFieldBinding(txtPasswordValue, TextBox.TextProperty, var, nameof(VariablePasswordString.Password));
        }



        private void txtPasswordValue_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            bool res, checkValueDecrypt;
            res = false;
            checkValueDecrypt = true;
            EncryptionHandler.DecryptString(txtPasswordValue.Text, ref checkValueDecrypt);

            if (!checkValueDecrypt) txtPasswordValue.Text = EncryptionHandler.EncryptString(txtPasswordValue.Text, ref res);

        }

    }
}
