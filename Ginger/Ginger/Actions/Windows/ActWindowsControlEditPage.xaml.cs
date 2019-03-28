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
using GingerCore.Actions.Windows;

namespace Ginger.Actions.Windows
{
    /// <summary>
    /// Interaction logic for ActPBControlEditPage.xaml
    /// </summary>
    public partial class ActWindowsControlEditPage : Page
    {
        private ActWindowsControl mAct;

        public ActWindowsControlEditPage(ActWindowsControl Act)
        {
            InitializeComponent();

            mAct = Act;

            GingerCore.General.FillComboFromEnumObj(ActionNameComboBox, mAct.ControlAction);            
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ActionNameComboBox, ComboBox.TextProperty, Act, ActWindowsControl.Fields.ControlAction);
        }

        private void ActionNameComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (mAct.ControlAction)
            {
                case ActWindowsControl.eControlAction.SetValue:
                    //TODO: show ValueTextBox
                case ActWindowsControl.eControlAction.GetValue:
                    //TODO: hide ValueTextBox
                    break;
                    
            }
        }
    }
}
