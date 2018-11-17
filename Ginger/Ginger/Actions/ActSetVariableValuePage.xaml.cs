#region License
/*
Copyright © 2014-2018 European Support Limited

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

using System.Linq;
using System.Windows.Controls;
using GingerCore.Actions;
using GingerCore.Variables;
using Amdocs.Ginger.Common;

namespace Ginger.Actions
{
    /// <summary>
    /// Interaction logic for ActSetVariableValuePage.xaml
    /// </summary>
    public partial class ActSetVariableValuePage : Page
    {
        private ActSetVariableValue mAct;
        private ObservableList<VariableBase> mVars;

        public ActSetVariableValuePage(ActSetVariableValue act)
        {
            InitializeComponent();
            mAct = act;
            SetComboListsValues(); 
            App.ObjFieldBinding(VariableNameComboBox, ComboBox.TextProperty, mAct, "VariableName");                  
        }

        private void SetComboListsValues()
        {            
            mVars = App.BusinessFlow.GetAllHierarchyVariables();
         
            foreach (VariableBase v in mVars.OrderBy(VariableBase.Fields.Name))
            {
                if ((v.GetType() != typeof(VariablePasswordString))&& (v.GetType() != typeof(VariableDynamic)))
                {
                    VariableNameComboBox.Items.Add(v.Name);
                }
            }            
        }

        private void VariableNameComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            VariableBase var = mVars.Where(x => x.Name == VariableNameComboBox.SelectedItem.ToString()).FirstOrDefault();
            
            //Clear fields
            VariableTypeTextBox.Text = string.Empty;
            if (OperationTypeComboBox.Items != null)
            {
                OperationTypeComboBox.Items.Clear();
            }
           
            if (var!=null)
            {
                //## Set type
                VariableTypeTextBox.Text = var.GetType().Name;

                //## Set operation options
                OperationTypeComboBox.BindControl(mAct, nameof(ActSetVariableValue.SetVariableValueOption), var.GetSupportedOperations());
            }
        }
 
    }
}
