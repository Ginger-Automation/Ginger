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
using GingerCore.Actions.ASCF;

namespace Ginger.Actions
{
    /// <summary>
    /// Interaction logic for ActASCFControlEditPage.xaml
    /// </summary>
    public partial class ActASCFBrowserControlEditPage : Page
    {
        private ActASCFBrowserElement mAct;

        public ActASCFBrowserControlEditPage(ActASCFBrowserElement act)
        {
            InitializeComponent();
            mAct = act;
            //TODO: use .Fields
            GingerCore.General.FillComboFromEnumObj(ControlActionComboBox, mAct.ControlAction);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ControlActionComboBox, ComboBox.SelectedValueProperty, mAct, ActASCFBrowserElement.Fields.ControlAction);            
        }
    }
}
