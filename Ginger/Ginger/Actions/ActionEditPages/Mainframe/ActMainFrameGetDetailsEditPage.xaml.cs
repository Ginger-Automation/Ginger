#region License
/*
Copyright © 2014-2019 European Support Limited

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

using GingerCore.Actions.MainFrame;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.Actions.Mainframe
{
    /// <summary>
    /// Interaction logic for ActMainFrameGetDetails.xaml
    /// </summary>
    public partial class ActMainFrameGetDetailsEditPage : Page
    {
        public ActMainframeGetDetails mAct = null;
        public ActMainFrameGetDetailsEditPage(ActMainframeGetDetails Act)
        {
            mAct = Act;
            InitializeComponent();
            GingerCore.General.FillComboFromEnumObj(GetMainFrameDetailsCombo, mAct.DetailsToFetch);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(GetMainFrameDetailsCombo, ComboBox.SelectedValueProperty, mAct, "DetailsToFetch");

            GingerCore.General.FillComboFromEnumObj(TextDetailsInstanceCombo, mAct.TextInstanceType);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(TextDetailsInstanceCombo, ComboBox.SelectedValueProperty, mAct, "TextInstanceType");
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(TextInstanceNumberTextBox,TextBox.TextProperty,mAct,"TextInstanceNumber");
        }

        private void GetMainFrameDetailsCombo_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (GetMainFrameDetailsCombo.SelectedValue == null)
                return;
            if (GetMainFrameDetailsCombo.SelectedValue.ToString () == "GetDetailsFromText")
            {
                InstanceDetailsPanel.Visibility=Visibility.Visible;
                TextDetailsInstancePanel.Visibility = Visibility.Visible;
            }
            else
            {
                InstanceDetailsPanel.Visibility = Visibility.Hidden;
                TextDetailsInstancePanel.Visibility = Visibility.Hidden;
            }
        }

        private void TextDetailsInstanceCombo_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (TextDetailsInstanceCombo.SelectedValue == null)
                return;
            if (TextDetailsInstanceCombo.SelectedValue.ToString () != "InstanceN")
            {

                InstanceDetailsPanel.Visibility = Visibility.Hidden;
            }
            else
            {

                InstanceDetailsPanel.Visibility = Visibility.Visible;
            }
        }
    }
}