#region License
/*
Copyright © 2014-2023 European Support Limited

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
using Amdocs.Ginger.Common;
using GingerCore.Actions;
using GingerCore.GeneralLib;

namespace Ginger.Actions
{
    /// <summary>
    /// Interaction logic for ActLogActionPage.xaml
    /// </summary>
    public partial class ActLogActionPage : Page
    {
        private ActLogAction mAct;
        public ActLogActionPage(ActLogAction act)
        {
            InitializeComponent();

            mAct = act;

            GingerCore.General.FillComboFromEnumObj(LogTypeComboBox, mAct.SelectedLogLevel);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(LogTypeComboBox, ComboBox.SelectedValueProperty, mAct, nameof(ActLogAction.SelectedLogLevel));
            
            LogValue.BindControl(Context.GetAsContext(mAct.Context), mAct, ActLogAction.Fields.LogText);            
        }
    }
}
