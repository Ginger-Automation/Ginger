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

using Amdocs.Ginger.Common;
using Ginger.UserControlsLib.TextEditor.ValueExpression;
using GingerCore;
using GingerCore.Environments;
using GingerCore.FlowControlLib;
using GingerCore.Variables;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Rendering;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Ginger.DataSource;
using GingerCore.DataSource;
using Ginger.Actions;
using Ginger.UserControlsLib.TextEditor;
using Ginger.Variables;
using Ginger.Environments;
using System.Reflection;
using Amdocs.Ginger.CoreNET.ValueExpression;
using Amdocs.Ginger.Repository;
using amdocs.ginger.GingerCoreNET;

namespace Ginger
{
    /// <summary>
    /// Interaction logic for ActionValueEditorWindow.xaml
    /// </summary>
    public partial class ValueExpressionEditorPage : Page
    {        
        ValueExpression mVE = new ValueExpression(App.AutomateTabEnvironment, App.BusinessFlow,App.LocalRepository.GetSolutionDataSources(),false,"",false);
        GenericWindow mWin;
        object mObj;
        string mAttrName;

        static List<HighlightingRule> mHighlightingRules = null;

        bool mHideBusinessFlowAndActivityVariables= false;

        public ValueExpressionEditorPage(object obj, string AttrName, bool hideBusinessFlowAndActivityVariables = false)
        {
            InitializeComponent();

            mObj = obj;
            mAttrName = AttrName;
            mHideBusinessFlowAndActivityVariables = hideBusinessFlowAndActivityVariables;

            ValueUCTextEditor.Bind(obj, AttrName);
            ValueUCTextEditor.HideToolBar();
            ValueUCTextEditor.lblTitle.Content = "Value";
            ValueUCTextEditor.SetDocumentEditor(new ValueExpressionEditor());

            GetHighlightingRules();
            
            BuildItemsTree();

            infoImage.ToolTip = "The value can be any simple string which include " + GingerDicser.GetTermResValue(eTermResKey.Variables) + " from different sources like: " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " " + GingerDicser.GetTermResValue(eTermResKey.Variables) + ", Environment Parameters, VBS functions and more." 
                                + Environment.NewLine +
                                "The value expression can have more than one " + GingerDicser.GetTermResValue(eTermResKey.Variable) + " in it and from different types- just add as many as you need!"
                                + Environment.NewLine +
                                "Environment Parameters enable to use the same solution on multiple environments easily.";
        }

        class RedBrush : HighlightingBrush
        {
            public override Brush GetBrush(ITextRunConstructionContext context)
            {                
                return Brushes.Blue;
            }
        }

        class LighGrayBackgroundBrush : HighlightingBrush
        {
            public override Brush GetBrush(ITextRunConstructionContext context)
            {
                return Brushes.LightGray;
            }
        }

        static RedBrush redBrush = new RedBrush();
        static LighGrayBackgroundBrush lighGrayBackgroundBrush = new LighGrayBackgroundBrush();

        // Some of the highlighing rules added in code and not xshd since we want to use the same compile regex we use to find the expressions in text
        void GetHighlightingRules()
        {
            if (mHighlightingRules == null)
            {
                mHighlightingRules = new List<HighlightingRule>();

                //Color for highlight of expression in value
                HighlightingColor color1 = new HighlightingColor();
                color1.FontStyle = FontStyles.Italic;
                color1.FontWeight = FontWeights.Bold;
                color1.Foreground = redBrush;
                color1.Background = lighGrayBackgroundBrush;
            }
            
            foreach (HighlightingRule HR in mHighlightingRules)
            {
                ValueUCTextEditor.textEditor.SyntaxHighlighting.MainRuleSet.Rules.Add(HR);
            }
        }

        private void BuildItemsTree()
        {
            AddVariables();
            AddEnvParams();
            AddGlobalParameters();
            AddVBSFunctions();
            AddRegexFunctions();
            AddVBSIfFunctions();
            AddDataSources();
            AddSecurityConfiguration();

            if (mObj != null && mObj.GetType() == typeof(FlowControl))
            {   
                //Added for Business Flow Control in RunSet
                if (App.MainWindow.MainRibbonSelectedTab == eRibbonTab.Run.ToString())
                {
                    AddBusinessFlowControlConditions();
                }
                else // For Action Flow Control
                {
                    AddFlowControlConditions();
                }                
            }
        }

        private void AddGlobalParameters()
        {
            TreeViewItem tviGlobalParams = new TreeViewItem();
            SetItemView(tviGlobalParams, "Models Global Parameters", "", "@Grid_16x16.png");
            xObjectsTreeView.Items.Add(tviGlobalParams);
            ObservableList<GlobalAppModelParameter>  mModelsGlobalParamsList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<GlobalAppModelParameter>();
            foreach (GlobalAppModelParameter v in mModelsGlobalParamsList)
                InsertNewGlobalParamTreeItem(tviGlobalParams, v);
        }

        private void InsertNewGlobalParamTreeItem(TreeViewItem parentTvi, GlobalAppModelParameter vb)
        {
            TreeViewItem tvi = new TreeViewItem();
            string GlobalParam = "{GlobalAppsModelsParam Name=" + vb.PlaceHolder + "}";
            SetItemView(tvi, vb.PlaceHolder, GlobalParam, "@Variable_16x16.png");
            parentTvi.Items.Add(tvi);
            tvi.MouseDoubleClick += tvi_MouseDoubleClick;
        }

        private void AddFlowControlConditions()
        {
            TreeViewItem tviVars = new TreeViewItem();
            SetItemView(tviVars, "Flow Control Conditions", "", "VBS16x16.png");
            xObjectsTreeView.Items.Add(tviVars); 

            AddVBSIfEval(tviVars, "Action Status = Passed", "\"{ActionStatus}\" = \"Passed\"");
            AddVBSIfEval(tviVars, "Action Status = Failed", "\"{ActionStatus}\" = \"Failed\"");

            AddVBSIfEval(tviVars, "Last Activity Status = Passed", "\"{LastActivityStatus}\" = \"Passed\"");
            AddVBSIfEval(tviVars, "Last Activity Status = Failed", "\"{LastActivityStatus}\" = \"Failed\"");
        }

        //Added for Business Flow Control in RunSet
        private void AddBusinessFlowControlConditions()
        {
            TreeViewItem tviVars = new TreeViewItem();
            SetItemView(tviVars, "Flow Control Conditions", "", "VBS16x16.png");
            xObjectsTreeView.Items.Add(tviVars);

            AddVBSIfEval(tviVars, "Business Flow Status = Passed", "\"{BusinessFlowStatus}\" = \"Passed\"");
            AddVBSIfEval(tviVars, "Business Flow Status = Failed", "\"{BusinessFlowStatus}\" = \"Failed\"");
        }

        private void AddVBSIfFunctions()
        {
            TreeViewItem tviVars = new TreeViewItem();
            SetItemView(tviVars, "VBS IF Functions", "", "VBS16x16.png");
            xObjectsTreeView.Items.Add(tviVars);

            AddVBSIfEval(tviVars, "Actual > 0", "{Actual} > 0");
            AddVBSIfEval(tviVars, "Actual Contains String 'ABC'", "InStr({Actual},\"ABC\")>0");
            AddVBSIfEval(tviVars, "Actual start with 'ABC'", "Left({Actual},3)=\"ABC\"");
            AddVBSIfEval(tviVars, "Actual String Length is 5 chars", "Len({Actual})=5");
            AddVBSIfEval(tviVars, "Actual SubString from char in position 2 length 3 is 'ABC'", "Mid({Actual},2,3)=\"ABC\"");
            AddVBSIfEval(tviVars, "Actual to Upper Case = 'ABC'", "UCase({Actual})=\"ABC\"");
        }

        private void AddVBSFunctions()
        {
            TreeViewItem tviVars = new TreeViewItem();
            SetItemView(tviVars, "VBS Functions", "", "VBS16x16.png");
            xObjectsTreeView.Items.Add(tviVars);
            
            AddVBSEval(tviVars, "Trim whitespace", "Trim(\"  Hello  \")");
            AddVBSEval(tviVars, "Trim whitespace & line breaks", "Trim(Replace(\"{Actual}\",\"vbCrLf\",\"\"))");
            AddVBSEval(tviVars, "Current Windows username", "WScript.CreateObject(\"WScript.Network\").UserName");
            AddVBSEval(tviVars, "Now", "now()");
            AddVBSEval(tviVars, "Tomorrow", "now()+1");
            AddVBSEval(tviVars, "Current Time Hour", "Int(Timer/3600)");
            AddVBSEval(tviVars, "Current Time Minutes", "((Int(Timer-Int(Timer/3600)*3600))/60)");
            AddVBSEval(tviVars, "Current Time Seconds", "Int(Timer-(Int(Timer/60)*60))");
            AddVBSEval(tviVars, "Current Time Hour (0# format)", "Right(\"0\" & Int(Timer/3600),2)");
            AddVBSEval(tviVars, "Current Time Minute (0# format)", "Right(\"0\" &  Int((Timer-(Int(Timer/3600)*3600))/60),2)");
            AddVBSEval(tviVars, "Current Time Seconds (0# format)", "Right(\"0\" & Int(Timer-(Int(Timer/60)*60)),2)");
            AddVBSEval(tviVars, "Current Month (0# format)", "Right(\"0\" & Month(Now), 2)");
            AddVBSEval(tviVars, "Current Day (0# format)", "Right(\"0\" & Day(Now), 2)");
            AddVBSEval(tviVars, "Current Year (#### format)", "DatePart(\"yyyy\", Now)");
            AddVBSEval(tviVars, "Current Year (## format)", "Right(DatePart(\"yyyy\", Now),2)");
            AddVBSEval(tviVars, "Current Date +7 days", "DateSerial(Year(Now), Month(Now),Day(DateAdd(\"d\",7,Now)))");
            AddVBSEval(tviVars, "Current Day of month +7 days (0# format) ", "Right(\"0\" & Day(DateAdd(\"d\",7,Now)), 2)");
            AddVBSEval(tviVars, "Current Date -1 month", "DateSerial(Year(Now), Month(DateAdd(\"m\",-1,Now)),Day(Now))");
            AddVBSEval(tviVars, "Current Month -1 (0# format)", "Right(\"0\" & Month(DateAdd(\"m\",-1,Now)), 2)");
            AddVBSEval(tviVars, "Current Day of Week (Name)","WeekdayName(DatePart(\"w\",Now))");
            AddVBSEval(tviVars, "Get # of days between 2 dates", "DateDiff(\"d\",\"5-16-2016\",\"6-16-2016\")");
            AddVBSEval(tviVars, "Check if date is valid", "CStr(IsDate(\"5/18/2016\"))");
            AddVBSEval(tviVars, "Sample Math 2+5*2", "2+5*2");
            AddVBSEval(tviVars, "Mid of String", "mid(\"hello\",1,2)");
            AddVBSEval(tviVars, "Choose 1st val from list", "Split(\"A,B,C,D\",\",\")(1)");
            AddVBSEval(tviVars, "Concatenate list w delimiter", "Join(Array(\"A\",\"B\",\"C\",\"D\"),\"|\")");
            AddVBSIfEval(tviVars, "Get Inner String Index", "{VBS Eval=InStr(\"Hello World!\",\"World\")}");
        }

        private void AddSecurityConfiguration()
        {
            TreeViewItem tviSecuritySettings = new TreeViewItem();
            SetItemView(tviSecuritySettings, "General Functions", "", "@Config_16x16.png");
            xObjectsTreeView.Items.Add(tviSecuritySettings);
            try
            {
                Type t = typeof(Amdocs.Ginger.CoreNET.ValueExpression.ValueExpessionGeneralFunctions);
                MemberInfo[] members = t.GetMembers();
                ValueExpressionFunctionAttribute token = null;

                foreach (MemberInfo mi in members)
                {
                    token = Attribute.GetCustomAttribute(mi, typeof(ValueExpressionFunctionAttribute), false) as ValueExpressionFunctionAttribute;
                    ValueExpressionFunctionDescription desc = Attribute.GetCustomAttribute(mi, typeof(ValueExpressionFunctionDescription), false) as ValueExpressionFunctionDescription;
                    ValueExpressionFunctionExpression expr = Attribute.GetCustomAttribute(mi, typeof(ValueExpressionFunctionExpression), false) as ValueExpressionFunctionExpression;
                    if (token == null)
                        continue;

                    AddWSSecurityConfig(tviSecuritySettings, desc.DefaultValue, expr.DefaultValue); //GetUTCDateTimeStamp());
                }
            }
            catch (Exception ex)
            {

                Reporter.ToLog(eLogLevel.ERROR, "Add Security Configuration Failed: ", ex);
            }
        }

        private void AddRegexFunctions()
        {
            TreeViewItem tviVars = new TreeViewItem();
            SetItemView(tviVars, "RegEx Functions", "", "@Regex16x16.png");
            xObjectsTreeView.Items.Add(tviVars);
            AddRegexEval(tviVars, "Extract Initial Digits", "1 Pat=([\\d\\D]{2}).*$ P1=12345");
            AddRegexEval(tviVars, "Extract Last Digits", "1 Pat=.+([\\d\\D]{2})$ P1=12345");
            AddRegexEval(tviVars, "Extract Number From Text", "matchValue Pat=\\d+ P1= aaa 123 bbb");
            AddRegexEval(tviVars, "Remove Characters From Numbers", "Replace Pat=\\D+ P1=(404) 200-0352");
        }

        private void AddRegexEval(TreeViewItem tviVars, string Desc, string Eval)
        {
            TreeViewItem tvi = new TreeViewItem();
            string VarExpression = "{RegEx Fun=" + Eval + "}";
            SetItemView(tvi, Desc, VarExpression, "@Regex16x16.png");
            tviVars.Items.Add(tvi);
            tvi.MouseDoubleClick += tvi_MouseDoubleClick;
        }

        private void AddVBSEval(TreeViewItem tviVars, string Desc, string Eval)
        {
            TreeViewItem tvi = new TreeViewItem();
            string VarExpression = "{VBS Eval=" + Eval + "}";
            SetItemView(tvi, Desc, VarExpression, "VBS16x16.png");
            tviVars.Items.Add(tvi);
            tvi.MouseDoubleClick += tvi_MouseDoubleClick;                        
        }

        private void AddWSSecurityConfig(TreeViewItem tviSecSets, string Desc, string Eval)
        {
            TreeViewItem tviSecuritySettings = new TreeViewItem();
            string VarExpression = Eval ;
            SetItemView(tviSecuritySettings, Desc, VarExpression, "@Config_16x16.png");
            tviSecSets.Items.Add(tviSecuritySettings);
            tviSecuritySettings.MouseDoubleClick += tvi_MouseDoubleClick;
        }

        private void AddVBSIfEval(TreeViewItem tviVars, string Desc, string Eval)
        {           
            TreeViewItem tvi = new TreeViewItem();
            string VarExpression = Eval;
            SetItemView(tvi, Desc, VarExpression, "VBS16x16.png");
            tviVars.Items.Add(tvi);
            tvi.MouseDoubleClick += tvi_MouseDoubleClick;
        }

        private void AddEnvParams()
        {
            TreeViewItem tviEnvs = new TreeViewItem();
            SetItemView(tviEnvs, "Environments", "", "@Environment_16x16.png");
            xObjectsTreeView.Items.Add(tviEnvs);

            ObservableList<ProjEnvironment> Envs = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>();

            foreach (ProjEnvironment env in Envs)
            {                
                TreeViewItem tviEnv = new TreeViewItem();
                SetItemView(tviEnv, env.Name, "", "@Environment_16x16.png");
                tviEnvs.Items.Add(tviEnv);

                TreeViewItem tviEnvApps = new TreeViewItem();
                SetItemView(tviEnvApps, "Applications", "", "@Window_16x16.png");
                tviEnv.Items.Add(tviEnvApps);

                foreach (EnvApplication a in env.Applications)
                {
                    TreeViewItem tviEnvApp = new TreeViewItem();
                    SetItemView(tviEnvApp, a.Name, "", "@Window_16x16.png");
                    tviEnvApps.Items.Add(tviEnvApp);

                    //Add Env URL
                    TreeViewItem tviEnvAppURL = new TreeViewItem();
                    string URLval = "{EnvURL App=" + a.Name + "}";
                    SetItemView(tviEnvAppURL, a.Name + " URL =" + a.Url, URLval, "URL16x16.png");
                    tviEnvApp.Items.Add(tviEnvAppURL);
                    tviEnvAppURL.MouseDoubleClick += tvi_MouseDoubleClick;

                    //Add App Global Params
                    TreeViewItem tviEnvAppGlobalParam = new TreeViewItem();
                    SetItemView(tviEnvAppGlobalParam, "Global Params", "", "GlobalParam16x16.png");
                    tviEnvApp.Items.Add(tviEnvAppGlobalParam);
                    tviEnvAppGlobalParam.MouseDoubleClick += tvi_MouseDoubleClick;

                    // Add all App General Params
                    foreach (GeneralParam gp in a.GeneralParams)
                    {
                        TreeViewItem tviEnvAppParam = new TreeViewItem();
                        string Paramval = "{EnvParam App=" + a.Name + " Param=" + gp.Name + "}";
                        SetItemView(tviEnvAppParam, gp.Name + " =" + gp.Value, Paramval, "GlobalParam16x16.png");
                        tviEnvAppGlobalParam.Items.Add(tviEnvAppParam);
                        tviEnvAppParam.MouseDoubleClick += tvi_MouseDoubleClick;
                    }
                }
            }
        }

        private void AddVariables()
        {
            if (App.UserProfile.Solution != null)
            {
                TreeViewItem solutionVars = new TreeViewItem();
                solutionVars.Items.IsLiveSorting = true;
                SetItemView(solutionVars, "Global " + GingerDicser.GetTermResValue(eTermResKey.Variables), "", "@Variable_16x16.png");
                xObjectsTreeView.Items.Add(solutionVars);
                 
                foreach (VariableBase v in App.UserProfile.Solution.Variables.OrderBy("Name"))
                    InsertNewVarTreeItem(solutionVars, v);
                InsertAddNewVarTreeItem(solutionVars, eVariablesLevel.Solution);
            }

            if (mHideBusinessFlowAndActivityVariables == false)
            {
                if (App.BusinessFlow != null)
                {
                    TreeViewItem tviVars = new TreeViewItem();
                    tviVars.Items.IsLiveSorting = true;
                    SetItemView(tviVars, GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " " + GingerDicser.GetTermResValue(eTermResKey.Variables), "", "@Variable_16x16.png");
                    xObjectsTreeView.Items.Add(tviVars);
                    
                    foreach (VariableBase v in App.BusinessFlow.Variables.OrderBy("Name"))
                        InsertNewVarTreeItem(tviVars, v);
                    InsertAddNewVarTreeItem(tviVars, eVariablesLevel.BusinessFlow);
                    tviVars.IsExpanded = true;
                }

                if (App.BusinessFlow.CurrentActivity != null)
                {
                    TreeViewItem activityVars = new TreeViewItem();
                    activityVars.Items.IsLiveSorting = true;
                    SetItemView(activityVars, GingerDicser.GetTermResValue(eTermResKey.Activity) + " " + GingerDicser.GetTermResValue(eTermResKey.Variables), "", "@Variable_16x16.png");
                    xObjectsTreeView.Items.Add(activityVars);

                    foreach (VariableBase v in App.BusinessFlow.CurrentActivity.Variables.OrderBy("Name"))
                        InsertNewVarTreeItem(activityVars, v);
                    InsertAddNewVarTreeItem(activityVars, eVariablesLevel.Activity);
                }
            }
        }

        private void InsertNewVarTreeItem(TreeViewItem parentTvi, VariableBase vb)
        {
            TreeViewItem tvi = new TreeViewItem();
            string VarExpression = "{Var Name=" + vb.Name + "}";
            SetItemView(tvi, vb.Name, VarExpression, "@Variable_16x16.png");
            parentTvi.Items.Add(tvi);
            tvi.MouseDoubleClick += tvi_MouseDoubleClick;
        }

        private void InsertAddNewVarTreeItem(TreeViewItem parentTvi, eVariablesLevel varLevel)
        {
            TreeViewItem newVarTvi = new TreeViewItem();
            SetItemView(newVarTvi, "Add New String Variable", varLevel, "@Add_16x16.png");
            parentTvi.Items.Add(newVarTvi);
            newVarTvi.MouseDoubleClick += tviAddNewVarTreeItem_MouseDoubleClick;
        }

        private void AddDataSources()
        {
            TreeViewItem tviDataSources = new TreeViewItem();
            SetItemView(tviDataSources, "Data Sources", "", "@DataSource_16x16.png");
            xObjectsTreeView.Items.Add(tviDataSources);
            
            ObservableList<DataSourceBase> DataSources = App.LocalRepository.GetSolutionDataSources();

            foreach (DataSourceBase ds in DataSources)
            {
                if (ds.FilePath.StartsWith("~"))
                {
                    ds.FileFullPath = ds.FilePath.Replace("~", "");
                    ds.FileFullPath = App.UserProfile.Solution.Folder + ds.FileFullPath;
                }
                ds.Init(ds.FileFullPath);
                TreeViewItem tviDataSource = new TreeViewItem();
                if (ds.DSType == DataSourceBase.eDSType.MSAccess)
                    SetItemView(tviDataSource, ds.Name, ds.Name, "@AccessDataSource_16x16.png");                
                else
                    SetItemView(tviDataSource, ds.Name, ds.Name, "@DataSource_16x16.png");
                tviDataSources.Items.Add(tviDataSource);
               
                ObservableList<DataSourceTable> dsList = ds.GetTablesList();
                if (dsList!=null)
                {
                    foreach (DataSourceTable dsTable in dsList)
                    {
                        TreeViewItem tviDSTable = new TreeViewItem();
                        SetItemView(tviDSTable, dsTable.Name, dsTable.DSTableType.ToString(), "@DataTable_16x16.png");
                        tviDataSource.Items.Add(tviDSTable);

                        tviDSTable.DataContext = dsTable;
                        tviDSTable.MouseDoubleClick += tviDSTable_MouseDoubleClick;
                    }

                }
                    
            }
        }

        private void tviDSTable_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem tvi = (TreeViewItem)e.Source;
            DataSourceTable dsTable = (DataSourceTable)tvi.DataContext;
            ActDataSourcePage dsVEPage;
            string VE = "";
                dsVEPage = new ActDataSourcePage(((TreeViewItem)tvi.Parent).Tag.ToString(),dsTable);
                dsVEPage.ShowAsWindow();
                VE = dsVEPage.VE;
            if (VE != "")    
                AddExpToValue(VE);
        }

        private void tvi_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem tvi = (TreeViewItem)e.Source;
            AddExpToValue(tvi.Tag + "");            
        }

        private void tviAddNewVarTreeItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            VariableString newStringVar = new VariableString();
            eVariablesLevel varLevel = (eVariablesLevel)((sender as TreeViewItem).Tag);
            switch (varLevel)
            {
                case eVariablesLevel.Solution:
                    ((Solution)App.UserProfile.Solution).AddVariable(newStringVar);
                    break;
                case eVariablesLevel.BusinessFlow:
                    ((BusinessFlow)App.BusinessFlow).AddVariable(newStringVar);
                    break;
                case eVariablesLevel.Activity:
                    ((Activity)App.BusinessFlow.CurrentActivity).AddVariable(newStringVar);
                    break;
            }

            VariableEditPage varEditPage = new VariableEditPage(newStringVar);
            varEditPage.ShowAsWindow(eWindowShowStyle.Dialog);

            //make sure name is unique
            switch (varLevel)
            {
                case eVariablesLevel.Solution:
                    ((Solution)App.UserProfile.Solution).SetUniqueVariableName(newStringVar);
                    break;
                case eVariablesLevel.BusinessFlow:
                    ((BusinessFlow)App.BusinessFlow).SetUniqueVariableName(newStringVar);
                    break;
                case eVariablesLevel.Activity:
                    ((Activity)App.BusinessFlow.CurrentActivity).SetUniqueVariableName(newStringVar);
                    break;
            }

            if (newStringVar != null)
            {
                TreeViewItem newTvi = new TreeViewItem();
                string VarExpression = "{Var Name=" + newStringVar.Name + "}";
                SetItemView(newTvi, newStringVar.Name, VarExpression, "@Variable_16x16.png");
                TreeViewItem parentTvi = (TreeViewItem)((TreeViewItem)xObjectsTreeView.SelectedItem).Parent;
                parentTvi.Items.Insert(parentTvi.Items.Count - 1, newTvi);

                //TODO: make added variable as selected item
                //newTvi.IsSelected = true;//Not working
                newTvi.MouseDoubleClick += tvi_MouseDoubleClick;
                AddExpToValue(newTvi.Tag + "");
            }
        }

        private void AddExpToValue(string exp)
        {            
            ValueUCTextEditor.textEditor.TextArea.Selection.ReplaceSelectionWithText(exp);            
        }

        private void SetItemView(TreeViewItem item, string HeaderText, object value,  string ImageFile)
        {
            item.Tag = value;

            StackPanel stack = new StackPanel();
            stack.Orientation = Orientation.Horizontal;

            // create Image
            Image image = new Image();
            image.Source = new BitmapImage(new Uri("pack://application:,,,/Ginger;component/Images/" + ImageFile));

            // Label
            Label lbl = new Label();
            lbl.Content = HeaderText;

            // Add into stack
            stack.Children.Add(image);            
            stack.Children.Add(lbl);

            // assign stack to header
            item.Header = stack;         
        }

        private void AddToValueButton_Click(object sender, RoutedEventArgs e)
        {
            TreeViewItem tvi = (TreeViewItem)xObjectsTreeView.SelectedItem;
            if (tvi != null)
            {
                //Using double click event to trigger any operation configured on the tree double click
                var args = new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left){ RoutedEvent = Control.MouseDoubleClickEvent};
                tvi.RaiseEvent(args);
            }
            else
            {
                Reporter.ToUser(eUserMsgKeys.AskToSelectItem);
            }
        }

        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            mVE.Value = this.ValueUCTextEditor.textEditor.Text;
            ValueCalculatedTextBox.Text = mVE.ValueCalculated;            
        }
                
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            //Update the obj attr with new Value
            mObj.GetType().GetProperty(mAttrName).SetValue(mObj, ValueUCTextEditor.textEditor.Text);
            mWin.Close();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {            
            this.ValueUCTextEditor.textEditor.Text = "";
            ValueCalculatedTextBox.Text = "";
        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {
            Button OKButton = new Button();
            OKButton.Content = "OK";
            OKButton.Click += new RoutedEventHandler(OKButton_Click);
                        
            GingerCore.General.LoadGenericWindow(ref mWin, App.MainWindow, windowStyle, this.Title, this, new ObservableList<Button> { OKButton }, true,"Cancel");
        }

        private void ClearCalculatedButton_Click(object sender, RoutedEventArgs e)
        {
            ValueCalculatedTextBox.Text = "";
        }
        
        private void ValueUCTextEditor_Loaded(object sender, RoutedEventArgs e)
        {     
        }
    }
}
