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
using GingerCore.Actions;

namespace Ginger.Actions
{
    /// <summary>
    /// Interaction logic for ActTextBoxEditPage.xaml
    /// </summary>
    public partial class ActUIATextBoxEditPage : Page
    {
        public static Act currentAct;

        public ActUIATextBoxEditPage(GingerCore.Actions.ActUIATextBox Act)
        {
            InitializeComponent();

            currentAct = Act;

            GingerCore.General.FillComboFromEnumObj(ActionNameComboBox, Act.UIATextBoxAction);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ActionNameComboBox, ComboBox.TextProperty, Act, "UIATextBoxAction");

            //TODO add dynamic parent windows population
           // lstParentWindows = new ObservableCollection<string>();
           // lstParentWindows.Add("Root");
           // lstParentWindows.Add("Parent Window");
           // ParentComboBox.ItemsSource = lstParentWindows;
        }

        void OnWindowClosing(object sender, RoutedEventArgs e)
        {
        //    //added closing event for UIAutomation parent event
        //    //TODO make parent window element locator object oriented
        //    if (currentAct.GetType() == typeof(GingerCore.Actions.ActUIATextBox) || currentAct.GetType() == typeof(GingerCore.Actions.ActUIAButton))
        //    {
        //        try
        //        {
        //            UIAutomation UIA = new UIAutomation();
        //            if (RootWindow)
        //            {
        //                if (currentAct.LocateValue.IndexOf('#') < 0) { currentAct.LocateValue = currentAct.LocateValue + "#RootWindow"; return; }

        //                if (currentAct.LocateValue.IndexOf('#') >= 0)
        //                {
        //                    string[] lstLocateValue = currentAct.LocateValue.Split('#');
        //                    currentAct.LocateValue = lstLocateValue[0] + "#RootWindow";
        //                }
        //            }
        //            else
        //            {
        //                if (currentAct.LocateValue.IndexOf('#') < 0)
        //                {
        //                    currentAct.LocateValue = currentAct.LocateValue + "#" + UIA.getSelectedLocators(olParentLocators); return;
        //                }
        //                if (currentAct.LocateValue.IndexOf('#') >= 0)
        //                {
        //                    string[] lstLocateValue = currentAct.LocateValue.Split('#');
        //                    currentAct.LocateValue = lstLocateValue[0] + "#" + UIA.getSelectedLocators(olParentLocators); return;
        //                }
        //            }
        //        }
        //        catch (Exception ex) { }
                
        //    }
            
        }

        public void SetGridView()
        {
        }
        
        private void ActionNameComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
