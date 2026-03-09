#region License
/*
Copyright © 2014-2026 European Support Limited

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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using GingerCore.Actions;
using GingerCore.Variables;
using System.Linq;
using System.Windows.Controls;

namespace Ginger.Actions
{
    /// <summary>
    /// Interaction logic for ActSetVariableValuePage.xaml
    /// </summary>
    public partial class ActSetVariableValuePage : Page
    {
        private ActSetVariableValue mAct;
        private ObservableList<VariableBase> mVars;

        Context mContext;

        public ActSetVariableValuePage(ActSetVariableValue act)
        {
            InitializeComponent();
            mAct = act;
            if (mAct.Context != null)
            {
                mContext = Context.GetAsContext(mAct.Context);
            }
            SetComboListsValues();
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(VariableNameComboBox, ComboBox.TextProperty, mAct, "VariableName");
        }

        private void SetComboListsValues()
        {
            if (mContext != null && mContext.BusinessFlow != null)
            {
                mVars = mContext.BusinessFlow.GetAllVariables(mContext.Activity);
            }
            else
            {
                mVars = new(WorkSpace.Instance.Solution.Variables);
                if (mContext != null && mContext.Activity != null)
                {
                    foreach (var activityVar in mContext.Activity.Variables)
                    {
                        mVars.Add(activityVar);
                    }
                }
            }

            foreach (VariableBase v in mVars.OrderBy(nameof(VariableBase.Name)))
            {
                if ((v.GetType() != typeof(VariablePasswordString)) && (v.GetType() != typeof(VariableDynamic)))
                {
                    VariableNameComboBox.Items.Add(v.Name);
                }
            }
        }

        private void VariableNameComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            VariableBase var = mVars.FirstOrDefault(x => x.Name == VariableNameComboBox.SelectedItem.ToString());

            //Clear fields
            VariableTypeTextBox.Text = string.Empty;
            if (OperationTypeComboBox.Items != null)
            {
                OperationTypeComboBox.Items.Clear();
            }

            if (var != null)
            {
                //## Set type
                VariableTypeTextBox.Text = var.GetType().Name;

                //## Set operation options
                OperationTypeComboBox.BindControl(mAct, nameof(ActSetVariableValue.SetVariableValueOption), var.GetSupportedOperations());
            }
        }

    }
}
