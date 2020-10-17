#region License
/*
Copyright © 2014-2020 European Support Limited

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

namespace Ginger.UserControlsLib.TextEditor.ValueExpression
{
    /// <summary>
    /// Interaction logic for ValueExpressionFlowDetailsEditorPage.xaml
    /// </summary>
    public partial class ValueExpressionFlowDetailsEditorPage : Page
    {
        SelectedContentArgs mSelectedContentArgs;
        Context mContext;
        string mObj;

        public ValueExpressionFlowDetailsEditorPage(Context context, SelectedContentArgs SelectedContentArgs, string obj,string field)
        {
            InitializeComponent();

            mContext = context;
            mSelectedContentArgs = SelectedContentArgs;
            mObj = obj;
            
            List<string> lst = new List<string>();
            PropertyInfo[] properties = null;
            FieldInfo[] fields = null;
           
            RunSetConfig runset = null;
            GingerRunner runner = null;
            if (WorkSpace.Instance.RunsetExecutor != null)
            {
                runset = WorkSpace.Instance.RunsetExecutor.RunSetConfig;
            }
            if (runset != null)
            {
                runner = WorkSpace.Instance.RunsetExecutor.Runners.Where(x => x.BusinessFlows.Where(bf => mContext.BusinessFlow != null && bf.Name == mContext.BusinessFlow.Name).FirstOrDefault() != null).FirstOrDefault();
            }
            RepositoryItemBase objtoEval = null;
            switch (mObj)
            {
                case "Runset":
                    objtoEval = runset;
                    break;
                case "Runner":
                    objtoEval = runner;
                    break;
                case "BusinessFlow":
                    objtoEval = mContext.BusinessFlow;
                    break;
                case "Environment":
                    objtoEval = mContext.Environment;
                    break;
                case "Activity":
                    objtoEval = mContext.BusinessFlow.CurrentActivity;
                    break;
                case "Action":
                    objtoEval = (RepositoryItemBase)mContext.BusinessFlow.CurrentActivity.Acts.CurrentItem;
                    break;
                case "PreviousBusinessFlow":
                    if(runner!=null)
                    {
                       objtoEval = runner.PreviousBusinessFlow;
                    }                    
                    break;
                case "PreviousActivity":
                    objtoEval = mContext.BusinessFlow.PreviousActivity;
                    break;
                case "PreviousAction":
                    objtoEval = mContext.BusinessFlow.PreviousAction;
                    break;
                case "LastFailedAction":
                    objtoEval = mContext.BusinessFlow.LastFailedAction;
                    break;
            }
            if (objtoEval != null)
            {
                properties = objtoEval.GetType().GetProperties();
                fields = objtoEval.GetType().GetFields();
            }
            if(properties!=null)
            {
                foreach (PropertyInfo prop in properties)
                {
                    lst.Add(prop.Name);
                }
            }
            if (fields != null)
            {
                foreach (FieldInfo f in fields)
                {
                    lst.Add(f.Name);
                }
            }
            fieldList.ItemsSource = lst;
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
