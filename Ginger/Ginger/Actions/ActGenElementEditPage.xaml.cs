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

using System.Windows;
using System.Windows.Controls;
using Amdocs.Ginger.Common;
using GingerCore.Actions;

namespace Ginger.Actions
{
    /// <summary>
    /// Interaction logic for ActTextBoxEditPage.xaml
    /// </summary>
    public partial class ActGenElementEditPage : Page
    {
        private GingerCore.Actions.ActGenElement mAct;

        public ActGenElementEditPage(GingerCore.Actions.ActGenElement Act)
        {
            InitializeComponent();

            this.mAct = Act;

            GingerCore.General.FillComboFromEnumObj(ActionNameComboBox, Act.GenElementAction);                     
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ActionNameComboBox, ComboBox.SelectedValueProperty, Act, "GenElementAction"); 

            Xoffset.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(ActGenElement.Fields.Xoffset), true);
            Yoffset.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(ActGenElement.Fields.Yoffset), true);
        }

        private void ActionNameSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ActionNameComboBox.SelectedValue.Equals(ActGenElement.eGenElementAction.XYClick) || ActionNameComboBox.SelectedValue.Equals(ActGenElement.eGenElementAction.XYDoubleClick) || ActionNameComboBox.SelectedValue.Equals(ActGenElement.eGenElementAction.XYSendKeys))
                CanvasStackPanel.Visibility = Visibility.Visible;
            else
                CanvasStackPanel.Visibility = Visibility.Collapsed;
        }
    }
}
