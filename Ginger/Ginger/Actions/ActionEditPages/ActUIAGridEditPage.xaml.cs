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
using Amdocs.Ginger.Common.UIElement;
using GingerCore.Actions;
using GingerCore.Actions.Common;

namespace Ginger.Actions
{
    /// <summary>
    /// Interaction logic for ActTextBoxEditPage.xaml
    /// </summary>
    public partial class ActUIAGridEditPage : Page
    {
        public static Act currentAct;

        public ActUIAGridEditPage(GingerCore.Actions.ActUIAGrid Act)
        {
            InitializeComponent();

            currentAct = Act;
            GingerCore.General.FillComboFromEnumObj(ActionNameComboBox, Act.GridAction);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ActionNameComboBox, ComboBox.TextProperty, Act, "GridAction"); 
        }

        private void ActionNameComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //Get Data For data grid
            //Open power builder
            //search data grid
            //create your data
            //Load above Data in Data Grid

            ActUIAGrid act = new ActUIAGrid();
            act.LocateBy = eLocateBy.ByAutomationID;
            act.GridAction = ActUIAGrid.eGridAction.GetFullGridData;
            act.LocateValueCalculated = "1021";
            act.Active = true;
        }
    }
}
