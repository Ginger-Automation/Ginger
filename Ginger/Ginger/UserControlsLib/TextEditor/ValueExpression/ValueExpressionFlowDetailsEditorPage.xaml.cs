#region License
/*
Copyright © 2014-2024 European Support Limited

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
using Ginger.Run;
using Ginger.UserControlsLib.TextEditor.Common;
using GingerCore;
using GingerCore.Activities;
using GingerCore.Environments;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;

namespace Ginger.UserControlsLib.TextEditor.ValueExpression
{
    /// <summary>
    /// Interaction logic for ValueExpressionFlowDetailsEditorPage.xaml
    /// </summary>
    public partial class ValueExpressionFlowDetailsEditorPage : Page
    {
        SelectedContentArgs mSelectedContentArgs;
        Context mContext;
        GingerCore.ValueExpression.eFlowDetailsObjects mObj;

        public ValueExpressionFlowDetailsEditorPage(Context context, SelectedContentArgs SelectedContentArgs, GingerCore.ValueExpression.eFlowDetailsObjects obj, string field)
        {
            InitializeComponent();

            mContext = context;
            mSelectedContentArgs = SelectedContentArgs;
            mObj = obj;

            List<string> lst = [];
            PropertyInfo[] properties = null;
            FieldInfo[] fields = null;
            var objType = mObj switch
            {
                GingerCore.ValueExpression.eFlowDetailsObjects.Solution => typeof(Ginger.SolutionGeneral.Solution),
                GingerCore.ValueExpression.eFlowDetailsObjects.Environment => typeof(ProjEnvironment),
                GingerCore.ValueExpression.eFlowDetailsObjects.Runset => typeof(RunSetConfig),
                GingerCore.ValueExpression.eFlowDetailsObjects.Runner => typeof(GingerExecutionEngine),
                GingerCore.ValueExpression.eFlowDetailsObjects.BusinessFlow or GingerCore.ValueExpression.eFlowDetailsObjects.PreviousBusinessFlow or GingerCore.ValueExpression.eFlowDetailsObjects.LastFailedBusinessFlow => typeof(BusinessFlow),
                GingerCore.ValueExpression.eFlowDetailsObjects.ActivitiesGroup or GingerCore.ValueExpression.eFlowDetailsObjects.ErrorHandlerOriginActivitiesGroup => typeof(ActivitiesGroup),
                GingerCore.ValueExpression.eFlowDetailsObjects.Activity or GingerCore.ValueExpression.eFlowDetailsObjects.PreviousActivity or GingerCore.ValueExpression.eFlowDetailsObjects.ErrorHandlerOriginActivity or GingerCore.ValueExpression.eFlowDetailsObjects.LastFailedActivity => typeof(Activity),
                GingerCore.ValueExpression.eFlowDetailsObjects.Action or GingerCore.ValueExpression.eFlowDetailsObjects.PreviousAction or GingerCore.ValueExpression.eFlowDetailsObjects.LastFailedAction or GingerCore.ValueExpression.eFlowDetailsObjects.ErrorHandlerOriginAction => typeof(GingerCore.Actions.Act),
                _ => throw new KeyNotFoundException(),
            };
            properties = objType.GetProperties();
            fields = objType.GetFields();

            if (properties != null)
            {
                foreach (PropertyInfo prop in properties)
                {
                    if (!typeof(IObservableList).IsAssignableFrom(prop.PropertyType))
                    {
                        lst.Add(prop.Name);
                    }
                }
            }
            if (fields != null)
            {
                foreach (FieldInfo f in fields)
                {
                    if (!typeof(IObservableList).IsAssignableFrom(f.FieldType))
                    {
                        lst.Add(f.Name);
                    }
                }
            }
            fieldList.ItemsSource = lst.OrderBy(x => x).ToList();
        }

        public void UpdateContent()
        {
            string v = (string)fieldList.SelectedItem;
            string txt = mSelectedContentArgs.TextEditor.Text[..mSelectedContentArgs.StartPos];
            txt += "{FD Object=" + mObj + " Field=" + v + "}";
            txt += mSelectedContentArgs.TextEditor.Text[(mSelectedContentArgs.EndPos + 1)..];
            mSelectedContentArgs.TextEditor.Text = txt;
        }
    }
}
