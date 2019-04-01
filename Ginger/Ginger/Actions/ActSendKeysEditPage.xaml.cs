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

using System.Collections.Generic;
using System.Windows.Controls;

namespace Ginger.Actions
{
    /// <summary>
    /// Interaction logic for ActSendKeysEditPage.xaml
    /// </summary>
    public partial class ActSendKeysEditPage : Page
    {
        private ActSendKeys mAct;

        public ActSendKeysEditPage(ActSendKeys Act)
        {
            InitializeComponent();
            this.mAct = Act;
             GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(SendKeysSlowly, CheckBox.IsCheckedProperty, mAct, ActSendKeys.Fields.IsSendKeysSlowly);
             GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(WindowFocusRequiredCheckBox, CheckBox.IsCheckedProperty, mAct, ActSendKeys.Fields.ISWindowFocusRequired);
            List<GingerCore.General.ComboItem> comboBoxItemsList = GeneratecomboBoxItemsListSendkeys();
            Value.Init(mAct.GetOrCreateInputParam(ActSendKeys.Fields.Value), comboBoxItemsList, true);
        }

        public List<GingerCore.General.ComboItem> GeneratecomboBoxItemsListSendkeys()
        {
            List<GingerCore.General.ComboItem> comboBoxItemsList = new List<GingerCore.General.ComboItem>();

            GingerCore.General.ComboItem CBI1 = new GingerCore.General.ComboItem();
            CBI1.text = "{BACKSPACE}";
            CBI1.text = "{BACKSPACE}";

            GingerCore.General.ComboItem CBI2 = new GingerCore.General.ComboItem();
            CBI2.text = "{BREAK}";
            CBI2.Value = "{BREAK}";

            GingerCore.General.ComboItem CBI3 = new GingerCore.General.ComboItem();
            CBI3.text = "{DELETE}";
            CBI3.Value = "{DELETE}";

            GingerCore.General.ComboItem CBI4 = new GingerCore.General.ComboItem();
            CBI4.text = "{CAPSLOCK}";
            CBI4.Value = "{CAPSLOCK}";

            GingerCore.General.ComboItem CBI5 = new GingerCore.General.ComboItem();
            CBI5.text = "DOWN ARROW";
            CBI5.Value = "{DOWN}";

            GingerCore.General.ComboItem CBI6 = new GingerCore.General.ComboItem();
            CBI6.text = "{END}";
            CBI6.Value = "{END}";

            GingerCore.General.ComboItem CBI7 = new GingerCore.General.ComboItem();
            CBI7.text = "{ENTER}";
            CBI7.Value = "{ENTER}";

            GingerCore.General.ComboItem CBI8 = new GingerCore.General.ComboItem();
            CBI8.text = "{ESC}";
            CBI8.Value = "{ESC}";

            GingerCore.General.ComboItem CBI9 = new GingerCore.General.ComboItem();
            CBI9.text = "{HELP}";
            CBI9.Value = "{HELP}";

            GingerCore.General.ComboItem CBI10 = new GingerCore.General.ComboItem();
            CBI10.text = "{HOME}";
            CBI10.Value = "{HOME}";

            GingerCore.General.ComboItem CBI11 = new GingerCore.General.ComboItem();
            CBI11.text = "{INSERT}";
            CBI11.Value = "{INSERT}";

            GingerCore.General.ComboItem CBI12 = new GingerCore.General.ComboItem();
            CBI12.text = "{LEFT}";
            CBI12.Value = "{LEFT}";

            GingerCore.General.ComboItem CBI13 = new GingerCore.General.ComboItem();
            CBI13.text = "{HOME}";
            CBI13.Value = "{HOME}";

            GingerCore.General.ComboItem CBI14 = new GingerCore.General.ComboItem();
            CBI14.text = "{NUMLOCK}";
            CBI14.Value = "{NUMLOCK}";

            GingerCore.General.ComboItem CBI15 = new GingerCore.General.ComboItem();
            CBI15.text = "{PGDN}";
            CBI15.Value = "{PGDN}";

            GingerCore.General.ComboItem CBI16 = new GingerCore.General.ComboItem();
            CBI16.text = "PAGE UP";
            CBI16.Value = "{PGUP}";

            GingerCore.General.ComboItem CBI17 = new GingerCore.General.ComboItem();
            CBI17.text = "{RIGHT}";
            CBI17.Value = "{RIGHT}";

            GingerCore.General.ComboItem CBI18 = new GingerCore.General.ComboItem();
            CBI18.text = "{SCROLLLOCK}";
            CBI18.Value = "{SCROLLLOCK}";

            GingerCore.General.ComboItem CBI19 = new GingerCore.General.ComboItem();
            CBI19.text = "{TAB}";
            CBI19.Value = "{TAB}";

            GingerCore.General.ComboItem CBI20 = new GingerCore.General.ComboItem();
            CBI20.text = "{UP}";
            CBI20.Value = "{UP}";

            GingerCore.General.ComboItem CBI21 = new GingerCore.General.ComboItem();
            CBI21.text = "+";
            CBI21.Value = "+";

            GingerCore.General.ComboItem CBI22 = new GingerCore.General.ComboItem();
            CBI22.text = "^";
            CBI22.Value = "^";

            GingerCore.General.ComboItem CBI23 = new GingerCore.General.ComboItem();
            CBI23.text = "%";
            CBI23.Value = "%";
            
            comboBoxItemsList.Add(CBI1);
            comboBoxItemsList.Add(CBI2);
            comboBoxItemsList.Add(CBI3);
            comboBoxItemsList.Add(CBI4);
            comboBoxItemsList.Add(CBI5);
            comboBoxItemsList.Add(CBI6);
            comboBoxItemsList.Add(CBI7);
            comboBoxItemsList.Add(CBI8);
            comboBoxItemsList.Add(CBI9);
            comboBoxItemsList.Add(CBI10);
            comboBoxItemsList.Add(CBI11);
            comboBoxItemsList.Add(CBI12);
            comboBoxItemsList.Add(CBI13);
            comboBoxItemsList.Add(CBI14);
            comboBoxItemsList.Add(CBI15);
            comboBoxItemsList.Add(CBI16);
            comboBoxItemsList.Add(CBI17);
            comboBoxItemsList.Add(CBI18);
            comboBoxItemsList.Add(CBI19);
            comboBoxItemsList.Add(CBI20);
            comboBoxItemsList.Add(CBI21);
            comboBoxItemsList.Add(CBI22);
            comboBoxItemsList.Add(CBI23);

            return comboBoxItemsList;
        }
    }
}
