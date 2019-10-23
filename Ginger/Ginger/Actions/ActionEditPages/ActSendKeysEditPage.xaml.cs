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
            Value.Init(mAct.GetOrCreateInputParam(ActSendKeys.Fields.Value), comboBoxItemsList, true, context:Context.GetAsContext(mAct.Context));
        }

        public List<ComboItem> GeneratecomboBoxItemsListSendkeys()
        {
            List<ComboItem> comboBoxItemsList = new List<ComboItem>();

            ComboItem CBI1 = new ComboItem();
            CBI1.text = "{BACKSPACE}";
            CBI1.text = "{BACKSPACE}";

            ComboItem CBI2 = new ComboItem();
            CBI2.text = "{BREAK}";
            CBI2.Value = "{BREAK}";

            ComboItem CBI3 = new ComboItem();
            CBI3.text = "{DELETE}";
            CBI3.Value = "{DELETE}";

            ComboItem CBI4 = new ComboItem();
            CBI4.text = "{CAPSLOCK}";
            CBI4.Value = "{CAPSLOCK}";

            ComboItem CBI5 = new ComboItem();
            CBI5.text = "DOWN ARROW";
            CBI5.Value = "{DOWN}";

            ComboItem CBI6 = new ComboItem();
            CBI6.text = "{END}";
            CBI6.Value = "{END}";

            ComboItem CBI7 = new ComboItem();
            CBI7.text = "{ENTER}";
            CBI7.Value = "{ENTER}";

            ComboItem CBI8 = new ComboItem();
            CBI8.text = "{ESC}";
            CBI8.Value = "{ESC}";

            ComboItem CBI9 = new ComboItem();
            CBI9.text = "{HELP}";
            CBI9.Value = "{HELP}";

            ComboItem CBI10 = new ComboItem();
            CBI10.text = "{HOME}";
            CBI10.Value = "{HOME}";

            ComboItem CBI11 = new ComboItem();
            CBI11.text = "{INSERT}";
            CBI11.Value = "{INSERT}";

            ComboItem CBI12 = new ComboItem();
            CBI12.text = "{LEFT}";
            CBI12.Value = "{LEFT}";

            ComboItem CBI13 = new ComboItem();
            CBI13.text = "{HOME}";
            CBI13.Value = "{HOME}";

            ComboItem CBI14 = new ComboItem();
            CBI14.text = "{NUMLOCK}";
            CBI14.Value = "{NUMLOCK}";

            ComboItem CBI15 = new ComboItem();
            CBI15.text = "{PGDN}";
            CBI15.Value = "{PGDN}";

            ComboItem CBI16 = new ComboItem();
            CBI16.text = "PAGE UP";
            CBI16.Value = "{PGUP}";

            ComboItem CBI17 = new ComboItem();
            CBI17.text = "{RIGHT}";
            CBI17.Value = "{RIGHT}";

            ComboItem CBI18 = new ComboItem();
            CBI18.text = "{SCROLLLOCK}";
            CBI18.Value = "{SCROLLLOCK}";

            ComboItem CBI19 = new ComboItem();
            CBI19.text = "{TAB}";
            CBI19.Value = "{TAB}";

            ComboItem CBI20 = new ComboItem();
            CBI20.text = "{UP}";
            CBI20.Value = "{UP}";

            ComboItem CBI21 = new ComboItem();
            CBI21.text = "+";
            CBI21.Value = "+";

            ComboItem CBI22 = new ComboItem();
            CBI22.text = "^";
            CBI22.Value = "^";

            ComboItem CBI23 = new ComboItem();
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
