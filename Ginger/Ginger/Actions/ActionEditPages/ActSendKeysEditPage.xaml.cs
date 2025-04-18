#region License
/*
Copyright © 2014-2025 European Support Limited

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

using Amdocs.Ginger.Common;
using GingerCore.GeneralLib;
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
            List<ComboItem> comboBoxItemsList = GeneratecomboBoxItemsListSendkeys();
            Value.Init(mAct.GetOrCreateInputParam(ActSendKeys.Fields.Value), comboBoxItemsList, true, context: Context.GetAsContext(mAct.Context));
        }

        public List<ComboItem> GeneratecomboBoxItemsListSendkeys()
        {
            List<ComboItem> comboBoxItemsList = [];

            ComboItem CBI1 = new ComboItem
            {
                text = "{BACKSPACE}"
            };
            CBI1.text = "{BACKSPACE}";

            ComboItem CBI2 = new ComboItem
            {
                text = "{BREAK}",
                Value = "{BREAK}"
            };

            ComboItem CBI3 = new ComboItem
            {
                text = "{DELETE}",
                Value = "{DELETE}"
            };

            ComboItem CBI4 = new ComboItem
            {
                text = "{CAPSLOCK}",
                Value = "{CAPSLOCK}"
            };

            ComboItem CBI5 = new ComboItem
            {
                text = "DOWN ARROW",
                Value = "{DOWN}"
            };

            ComboItem CBI6 = new ComboItem
            {
                text = "{END}",
                Value = "{END}"
            };

            ComboItem CBI7 = new ComboItem
            {
                text = "{ENTER}",
                Value = "{ENTER}"
            };

            ComboItem CBI8 = new ComboItem
            {
                text = "{ESC}",
                Value = "{ESC}"
            };

            ComboItem CBI9 = new ComboItem
            {
                text = "{HELP}",
                Value = "{HELP}"
            };

            ComboItem CBI10 = new ComboItem
            {
                text = "{HOME}",
                Value = "{HOME}"
            };

            ComboItem CBI11 = new ComboItem
            {
                text = "{INSERT}",
                Value = "{INSERT}"
            };

            ComboItem CBI12 = new ComboItem
            {
                text = "{LEFT}",
                Value = "{LEFT}"
            };

            ComboItem CBI13 = new ComboItem
            {
                text = "{HOME}",
                Value = "{HOME}"
            };

            ComboItem CBI14 = new ComboItem
            {
                text = "{NUMLOCK}",
                Value = "{NUMLOCK}"
            };

            ComboItem CBI15 = new ComboItem
            {
                text = "{PGDN}",
                Value = "{PGDN}"
            };

            ComboItem CBI16 = new ComboItem
            {
                text = "PAGE UP",
                Value = "{PGUP}"
            };

            ComboItem CBI17 = new ComboItem
            {
                text = "{RIGHT}",
                Value = "{RIGHT}"
            };

            ComboItem CBI18 = new ComboItem
            {
                text = "{SCROLLLOCK}",
                Value = "{SCROLLLOCK}"
            };

            ComboItem CBI19 = new ComboItem
            {
                text = "{TAB}",
                Value = "{TAB}"
            };

            ComboItem CBI20 = new ComboItem
            {
                text = "{UP}",
                Value = "{UP}"
            };

            ComboItem CBI21 = new ComboItem
            {
                text = "+",
                Value = "+"
            };

            ComboItem CBI22 = new ComboItem
            {
                text = "^",
                Value = "^"
            };

            ComboItem CBI23 = new ComboItem
            {
                text = "%",
                Value = "%"
            };

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
