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
using System.Reflection;
using Amdocs.Ginger.Repository;
using Ginger.Run;
using System.Linq;
using System;
using GingerCore.Environments;
using GingerCore;
using System.Collections;
using GingerCore.Activities;

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

            List<string> lst = new List<string>();
            PropertyInfo[] properties = null;
            FieldInfo[] fields = null;
            Type objType;
            switch (mObj)
            {
                case GingerCore.ValueExpression.eFlowDetailsObjects.Solution:
                    objType = typeof(Ginger.SolutionGeneral.Solution);
                    break;
                case GingerCore.ValueExpression.eFlowDetailsObjects.Environment:
                    objType = typeof(ProjEnvironment);
                    break;
                case GingerCore.ValueExpression.eFlowDetailsObjects.Runset:
                    objType = typeof(RunSetConfig);
                    break;
                case GingerCore.ValueExpression.eFlowDetailsObjects.Runner:
                    objType = typeof(GingerExecutionEngine);
                    break;
                case GingerCore.ValueExpression.eFlowDetailsObjects.BusinessFlow:
                case GingerCore.ValueExpression.eFlowDetailsObjects.PreviousBusinessFlow:
                case GingerCore.ValueExpression.eFlowDetailsObjects.LastFailedBusinessFlow:
                    objType = typeof(BusinessFlow);
                    break;
                case GingerCore.ValueExpression.eFlowDetailsObjects.ActivitiesGroup:
                case GingerCore.ValueExpression.eFlowDetailsObjects.ErrorHandlerOriginActivitiesGroup:
                    objType = typeof(ActivitiesGroup);
                    break;
                case GingerCore.ValueExpression.eFlowDetailsObjects.Activity:
                case GingerCore.ValueExpression.eFlowDetailsObjects.PreviousActivity:
                case GingerCore.ValueExpression.eFlowDetailsObjects.ErrorHandlerOriginActivity:
                case GingerCore.ValueExpression.eFlowDetailsObjects.LastFailedActivity:
                    objType = typeof(Activity);
                    break;
                case GingerCore.ValueExpression.eFlowDetailsObjects.Action:
                case GingerCore.ValueExpression.eFlowDetailsObjects.PreviousAction:
                case GingerCore.ValueExpression.eFlowDetailsObjects.LastFailedAction:
                case GingerCore.ValueExpression.eFlowDetailsObjects.ErrorHandlerOriginAction:
                    objType = typeof(GingerCore.Actions.Act);
                    break;
                default:
                    throw new KeyNotFoundException();
            }

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
            string txt = mSelectedContentArgs.TextEditor.Text.Substring(0, mSelectedContentArgs.StartPos);
            txt += "{FD Object=" + mObj + " Field=" + v + "}";
            txt += mSelectedContentArgs.TextEditor.Text.Substring(mSelectedContentArgs.EndPos + 1);
            mSelectedContentArgs.TextEditor.Text = txt;
        }        
    }
}
