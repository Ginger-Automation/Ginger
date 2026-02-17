#region License
/*
Copyright © 2014-2026 European Support Limited
 
Licensed under the Apache License, Version 2.0 (the "License")
you may not use this file except in compliance with the License.
You may obtain a copy of the License at
 
http://www.apache.org/licenses/LICENSE-2.0
 
Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS, 
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
See the License for the specific language governing permissions and 
limitations under the License. 
*/
#endregion
using GingerCore;
using GingerCore.Variables;
using System.Windows.Controls;
using System.Windows.Input;

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
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(txtPasswordValue, TextBox.TextProperty, var, nameof(VariablePasswordString.Password));
        }



        private void txtPasswordValue_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (!EncryptionHandler.IsStringEncrypted(txtPasswordValue.Text))
            {
                txtPasswordValue.Text = EncryptionHandler.EncryptwithKey(txtPasswordValue.Text);
            }
        }

    }
}
