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

        public ValueExpressionVariableEditorPage(Context context, SelectedContentArgs SelectedContentArgs)
        {
            InitializeComponent();

            mContext = context;
            mSelectedContentArgs = SelectedContentArgs;
            
            List<string> lst = new List<string>();

            // Add the variables from solution, current BF and current activity
            foreach (VariableBase v in  WorkSpace.UserProfile.Solution.Variables)
            {
                lst.Add(v.Name);
            }

            foreach (VariableBase v in mContext.BusinessFlow.Variables)
            {
                lst.Add(v.Name);
            }

            foreach (VariableBase v in mContext.BusinessFlow.CurrentActivity.Variables)
            {
                lst.Add(v.Name);
            }


            VarsList.ItemsSource = lst;
        }
        
        public void UpdateContent()
        {
            string v = (string)VarsList.SelectedItem;
            string txt = mSelectedContentArgs.TextEditor.Text.Substring(0, mSelectedContentArgs.StartPos);
            txt += "{Var Name=" + v + "}";
            txt += mSelectedContentArgs.TextEditor.Text.Substring(mSelectedContentArgs.EndPos + 1);
            mSelectedContentArgs.TextEditor.Text = txt;
        }        
    }
}
