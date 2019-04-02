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

using System.Windows.Controls;
using GingerCore.Actions.Java;

namespace Ginger.Actions.Java
{
    /// <summary>
    /// Interaction logic for ActJavaElementEditPage.xaml
    /// </summary>
    public partial class ActJavaElementEditPage : Page
    {
        private ActJavaElement mAct;

        public ActJavaElementEditPage(ActJavaElement act)
        {
            InitializeComponent();

            mAct = act;

            GingerCore.General.FillComboFromEnumObj(ControlActionComboBox, mAct.ControlAction);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ControlActionComboBox, ComboBox.SelectedValueProperty, mAct, ActJavaElement.Fields.ControlAction);

            GingerCore.General.FillComboFromEnumObj(WaitforIdleComboBox, mAct.WaitforIdle);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(WaitforIdleComboBox, ComboBox.SelectedValueProperty, mAct, ActJavaElement.Fields.WaitforIdle);
        }

        private void ControlActionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ControlActionComboBox.SelectedItem.ToString() == ActJavaElement.eControlAction.Click.ToString())
            {
                WaitforIdleComboBox.SelectedValue = ActJavaElement.eWaitForIdle.Medium.ToString();
            }
        }
    }
}
