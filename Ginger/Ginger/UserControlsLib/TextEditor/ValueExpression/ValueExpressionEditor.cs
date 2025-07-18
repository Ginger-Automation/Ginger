#region License
/*
Copyright © 2014-2025 European Support Limited

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
using Amdocs.Ginger.Plugin.Core;
using Ginger.Actions;
using Ginger.UserControlsLib.TextEditor.Common;
using GingerCore;
using GingerCore.Variables;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Highlighting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Ginger.UserControlsLib.TextEditor.ValueExpression
{
    public class ValueExpressionEditor : TextEditorBase
    {
        public override string Descritpion { get { return ""; } }
        public override Image Icon { get { throw new NotImplementedException(); } }

        private Page p = null;

        public static readonly Regex rxVarFormulaParams = new Regex(@"[A-Za-z]*=[ A-Za-z0-9!@#$%^&*_+?/><-]*", RegexOptions.Compiled);

        public override List<string> Extensions
        {
            get
            {
                return null;
            }
        }

        public override IHighlightingDefinition HighlightingDefinition
        {
            get
            {
                return GetHighlightingDefinitionFromResource(Properties.Resources.ValueExpressionHighlighting);
            }
        }

        public override IFoldingStrategy FoldingStrategy
        {
            get
            {
                return null;

            }
        }

        Context mContext;
        public ValueExpressionEditor(Context context)
        {
            mContext = context;
        }
        public override Page GetSelectedContentPageEditor(SelectedContentArgs SelectedContentArgs)
        {
            // SelectedContentArgs.TextEditor.Document.
            //TODO: use SelectedContentArgs.TextEditor.Document  - documents function is better

            //start by searching the closest { to the caret offset
            int i1 = SelectedContentArgs.TextEditor.CaretOffset;
            int closestpos = -1;
            while (i1 >= 0 && i1 < SelectedContentArgs.TextEditor.Text.Length)
            {
                string c = SelectedContentArgs.TextEditor.Text.Substring(i1, 1);
                if (c == "{")
                {
                    closestpos = i1;
                    break;
                }
                if (c == "}" && i1 != SelectedContentArgs.TextEditor.CaretOffset)  // we stop if we found } - except for the first char - in case the user click on the closing } of exp
                {
                    break;
                }
                i1--;
            }

            if (closestpos == -1)
            {
                return null;
            }

            // find the = after the closest { pos

            int i0 = SelectedContentArgs.TextEditor.Text.IndexOf("}", closestpos);

            // Verify caret are in between { }
            if (closestpos < SelectedContentArgs.TextEditor.CaretOffset && i0 >= SelectedContentArgs.TextEditor.CaretOffset)
            {
                int i2 = SelectedContentArgs.TextEditor.Text.IndexOf("=", closestpos);
                if (i2 >= 0)
                {
                    SelectedContentArgs.StartPos = closestpos;
                    SelectedContentArgs.EndPos = i0;
                    Page p = GetPageFor(SelectedContentArgs);
                    return p;
                }
            }
            return null;
        }

        public override List<ICompletionData> GetCompletionData(string txt, SelectedContentArgs SelectedContentArgs)
        {
            List<ICompletionData> list = [];
            if (txt == "{")
            {
                list.Add(new TextCompletionData("{Var Name=}", "Variable"));
                list.Add(new TextCompletionData("{VBS Eval=}", "VB Script Evaluate"));

                AddVars(list);
            }
            return list;
        }

        void AddVars(List<ICompletionData> list)
        {
            foreach (VariableBase v in WorkSpace.Instance.Solution.Variables)
            {
                list.Add(GETVariableTCD(v));
            }
            if (mContext.BusinessFlow != null)
            {
                foreach (VariableBase v in mContext.BusinessFlow.Variables)
                {
                    list.Add(GETVariableTCD(v));
                }
            }
            if (mContext.BusinessFlow != null && mContext.BusinessFlow.CurrentActivity != null)
            {
                foreach (VariableBase v in mContext.BusinessFlow.CurrentActivity.Variables)
                {
                    list.Add(GETVariableTCD(v));
                }
            }
        }

        private TextCompletionData GETVariableTCD(VariableBase v)
        {
            TextCompletionData TCD = new TextCompletionData("{Var Name=" + v.Name + "}")
            {
                Description = "Variable " + v.Name + " " + v.VariableType
            };


            //TODO: replace with General.GetImage("@Variable_32x32.png")
            BitmapImage b = new BitmapImage();
            b.BeginInit();
            b.UriSource = new Uri(@"/Images/@Variable_32x32.png", UriKind.RelativeOrAbsolute);
            b.EndInit();

            TCD.Image = b;
            return TCD;
        }

        private Page GetPageFor(SelectedContentArgs SelectedContentArgs)
        {
            string txt = SelectedContentArgs.TextEditor.Text.Substring(SelectedContentArgs.StartPos, SelectedContentArgs.Length);
            p = null;
            if (txt.StartsWith("{Var Name="))
            {
                p = new ValueExpressionVariableEditorPage(mContext, SelectedContentArgs, GetVariableFromText(txt));
            }

            if (txt.StartsWith("{DS Name="))
            {
                p = new ActDataSourcePage(SelectedContentArgs);
            }
            if (txt.StartsWith("{EnvApp="))
            {
                p = new ValidationDBPage(SelectedContentArgs, new GingerCore.Actions.ActDBValidation() { Context = mContext });
            }
            if (txt.StartsWith("{FD Object="))
            {
                Tuple<GingerCore.ValueExpression.eFlowDetailsObjects, string> expParams = GingerCore.ValueExpression.GetFlowDetailsParams(txt);
                if (expParams != null)
                {
                    p = new ValueExpressionFlowDetailsEditorPage(mContext, SelectedContentArgs, expParams.Item1, expParams.Item2);
                }
            }

            if (txt.StartsWith("{MockDataExp Fun="))
            {
                Mockdata expParams = GingerCore.ValueExpression.GetMockDataDatasetsFunction(txt);
                if (expParams != null)
                {
                    p = new ValueExpressionMockDataEditorPage(mContext, SelectedContentArgs, expParams.MockDataDatasets, expParams.Function, expParams.Locale, expParams.MockExpression);
                }
            }

            if (txt.StartsWith("{EnvURL App="))
            {
                //TODO: impl get page for Env

            }
            // TODO: if we are on {Actual}  - show help to explain...

            return p;
        }

        private VariableBase? GetVariableFromText(string VarName)
        {
            string[] var = rxVarFormulaParams.Match(VarName).Value.Split('=');
            VarName = var[1].Trim();
            VariableBase vb = WorkSpace.Instance.Solution.Variables.FirstOrDefault(var => var.Name == VarName);
            if (vb == null && mContext.BusinessFlow != null && mContext.BusinessFlow.Variables.Any(var => var.Name == VarName))
            {
                vb = mContext.BusinessFlow.Variables.FirstOrDefault(var => var.Name == VarName);
            }
            else if (vb == null && mContext.BusinessFlow != null && mContext.BusinessFlow.CurrentActivity != null)
            {
                vb = mContext.BusinessFlow.CurrentActivity.Variables.FirstOrDefault(var => var.Name == VarName);
            }
            return vb;
        }

        public override void UpdateSelectedContent()
        {
            try
            {
                if (p.GetType() == typeof(ValueExpressionVariableEditorPage))
                {
                    ((ValueExpressionVariableEditorPage)p).UpdateContent();
                }
                else if (p.GetType() == typeof(ActDataSourcePage))
                {
                    ((ActDataSourcePage)p).UpdateContent();
                }
                else if (p.GetType() == typeof(ValueExpressionFlowDetailsEditorPage))
                {
                    ((ValueExpressionFlowDetailsEditorPage)p).UpdateContent();
                }
                else if (p.GetType() == typeof(ValueExpressionMockDataEditorPage))
                {
                    ((ValueExpressionMockDataEditorPage)p).UpdateContent();
                }
                else if (p.GetType() == typeof(ValidationDBPage))
                {
                    ((ValidationDBPage)p).UpdateContent();
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to update the selected content", ex);
            }
        }

        // if we want to add tool bar item and handler this is the place
        public override List<ITextEditorToolBarItem> Tools
        {
            get { return null; }
        }
    }
}
