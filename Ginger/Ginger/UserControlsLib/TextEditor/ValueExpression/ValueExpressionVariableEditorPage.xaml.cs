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

using Ginger.UserControlsLib.TextEditor.Common;
using GingerCore.Variables;
using System.Collections.Generic;
using System.Windows.Controls;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;

namespace Ginger.UserControlsLib.TextEditor.ValueExpression
{
    /// <summary>
    /// Interaction logic for ValueExpressionSelectedTextEditorPage.xaml
    /// </summary>
    public partial class ValueExpressionVariableEditorPage : Page
    {
        SelectedContentArgs mSelectedContentArgs;
        Context mContext;
        VariableBase var;

        public ValueExpressionVariableEditorPage(Context context, SelectedContentArgs SelectedContentArgs, VariableBase? variable = null)
        {
            InitializeComponent();

            mContext = context;
            mSelectedContentArgs = SelectedContentArgs;
            var = variable;

            List<string> lst = new List<string>();
            List<string> lstExtraParams;


            // Add the variables from solution, current BF and current activity
            foreach (VariableBase v in  WorkSpace.Instance.Solution.Variables)
            {
                lst.Add(v.Name);
            }

            if(mContext.BusinessFlow!=null)
            {
                foreach (VariableBase v in mContext.BusinessFlow.Variables)
                {
                    lst.Add(v.Name);
                }

                if(mContext.BusinessFlow.CurrentActivity!=null)
                {
                    foreach (VariableBase v in mContext.BusinessFlow.CurrentActivity.Variables)
                    {
                        lst.Add(v.Name);
                    }
                }
               
            }
           
            if (variable != null && variable.GetType() == typeof(VariableSelectionList))
            {
                VarsListExtraParams.Visibility = System.Windows.Visibility.Visible;
                lblExtraParams.Visibility = System.Windows.Visibility.Visible;
                lstExtraParams = variable.GetExtraParamsList();
                VarsListExtraParams.ItemsSource = lstExtraParams;
            }
            else
            {
                VarsListExtraParams.Visibility = System.Windows.Visibility.Collapsed;
                lblExtraParams.Visibility = System.Windows.Visibility.Collapsed;
            }

            VarsList.ItemsSource = lst;
        }
        
        public void UpdateContent()
        {
            string v = (string)VarsList.SelectedItem;
            if (!string.IsNullOrEmpty(v))
            {
                string txt = mSelectedContentArgs.TextEditor.Text.Substring(0, mSelectedContentArgs.StartPos);
                txt += "{Var Name=" + v + "}";
                txt += mSelectedContentArgs.TextEditor.Text.Substring(mSelectedContentArgs.EndPos + 1);
                mSelectedContentArgs.TextEditor.Text = txt;
            }
            else
            {
                v = (string)VarsListExtraParams.SelectedItem;
                string txt = mSelectedContentArgs.TextEditor.Text.Substring(0, mSelectedContentArgs.StartPos);
                txt += "{Var Name=" + var.Name + ", " + v + "}";
                txt += mSelectedContentArgs.TextEditor.Text.Substring(mSelectedContentArgs.EndPos + 1);
                mSelectedContentArgs.TextEditor.Text = txt;
            }


        }        
    }
}
