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
using Amdocs.Ginger.Common;
using GingerCore.Actions;

namespace Ginger.Actions
{
    public partial class ActPWLEditPage : Page
    {
        private GingerCore.Actions.ActPWL f;

        public ActPWLEditPage(GingerCore.Actions.ActPWL Act)
        {
            InitializeComponent();

            this.f = Act;
            List<object> l = new List<object>();
            foreach (var v in f.AvailableLocateBy())
            {
                l.Add(v);
            }
            // List<object> l = a.AvailableLocateBy(). .ToList<object>();
            GingerCore.General.FillComboFromEnumObj(cbooLocateBy, f.AvailableLocateBy(), l);

            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(cbooLocateBy, ComboBox.TextProperty, f, ActPWL.Fields.OLocateBy);

            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(txtoLocateValue, TextBox.TextProperty, f, ActPWL.Fields.OLocateValue);
            GingerCore.General.FillComboFromEnumObj(ActionNameComboBox, Act.PWLAction);
            //TODO: fix hard coded ButtonAction use Fields
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ActionNameComboBox, ComboBox.TextProperty, Act, "PWLAction");
            txtoLocateValue.Init(Context.GetAsContext(f.Context), f, ActPWL.Fields.OLocateValue);

            txtoLocateValue.ValueTextBox.Text = f.OLocateValue;
        }
    }
}
