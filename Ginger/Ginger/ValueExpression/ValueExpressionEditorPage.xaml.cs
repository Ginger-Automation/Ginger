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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.ValueExpression;
using Amdocs.Ginger.Repository;
using Ginger.Actions;
using Ginger.SolutionGeneral;
using Ginger.UserControlsLib.TextEditor.ValueExpression;
using Ginger.Variables;
using GingerCore;
using GingerCore.DataSource;
using GingerCore.Environments;
using GingerCore.FlowControlLib;
using GingerCore.Variables;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Rendering;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;
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
using Ginger.SolutionGeneral;
using System.IO;
using System.Dynamic;
using Newtonsoft.Json.Linq;
using Amdocs.Ginger.Common.InterfacesLib;
using System.Linq;
using Amdocs.Ginger.CoreNET.RosLynLib.Refrences;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
namespace Ginger
{
    /// <summary>
    /// Interaction logic for ActionValueEditorWindow.xaml
    /// </summary>

  

    public partial class ValueExpressionEditorPage : Page
    {
        private static Regex VBSReg = new Regex(@"{VBS Eval=([^}])*}", RegexOptions.Compiled);      
        VEReferenceList Tvel = new VEReferenceList();
        GenericWindow mWin;
        object mObj;
        string mAttrName;
        Context mContext;
        ValueExpression mVE = null;
        static List<HighlightingRule> mHighlightingRules = null;
        private Dictionary<string, TreeViewItem> Categories = new Dictionary<string, TreeViewItem>();

        public ValueExpressionEditorPage(object obj, string AttrName, Context context)
        {
            InitializeComponent();

            mObj = obj;
            mAttrName = AttrName;
            mContext = context;

            ValueUCTextEditor.Bind(obj, AttrName);
            ValueUCTextEditor.HideToolBar();
            ValueUCTextEditor.lblTitle.Content = "Value";
            ValueUCTextEditor.SetDocumentEditor(new ValueExpressionEditor(mContext));

            GetHighlightingRules();
            
            BuildItemsTree();

            infoImage.ToolTip = "The value can be any simple string which include " + GingerDicser.GetTermResValue(eTermResKey.Variables) + " from different sources like: " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " " + GingerDicser.GetTermResValue(eTermResKey.Variables) + ", Environment Parameters, VBS functions and more." 
                                + Environment.NewLine +
                                "The value expression can have more than one " + GingerDicser.GetTermResValue(eTermResKey.Variable) + " in it and from different types- just add as many as you need!"
                                + Environment.NewLine +
                                "Environment Parameters enable to use the same solution on multiple environments easily.";
            ValueUCTextEditor_LostFocus(ValueUCTextEditor, null);
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

        // Some of the highlighting rules added in code and not xshd since we want to use the same compile regex we use to find the expressions in text
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
            AddCSFunctions();
           //AddVBSFunctions();
            //AddRegexFunctions();
            //AddVBSIfFunctions();
            AddDataSources();
            AddSecurityConfiguration();

            if (mObj != null && mObj.GetType() == typeof(FlowControl))
            {   
                //Added for Business Flow Control in RunSet
                if (App.MainWindow.SelectedSolutionTab == MainWindow.eSolutionTabType.Run)
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




            TreeViewItem Parent = new TreeViewItem();
            SetItemView(Parent, "Flow Control Conditions", "", "@Config3_16x16.png");
            xObjectsTreeView.Items.Add(Parent);



            ValueExpressionReference VERActionpassd = new ValueExpressionReference
            {
                Name = "Action is Passed",
                Expression = "{CS Exp=\"{ActionStatus}\".Equals(\"Passed\")}",
                UseCase = "Flowcontrol Condition will execute if the action is Passed",
            };
            TreeViewItem tvi = new TreeViewItem();
            SetItemView(tvi, VERActionpassd.Name, VERActionpassd.Expression, VERActionpassd.IconImageName == null ? "@Config3_16x16.png" : VERActionpassd.IconImageName);
            Parent.Items.Add(tvi);
            tvi.MouseDoubleClick += tvi_MouseDoubleClick;
            tvi.Selected += UpdateHelpForCSFunction;
            tvi.Tag = VERActionpassd;




            ValueExpressionReference VERACtionFailed = new ValueExpressionReference
            {
                Name = "Action is Failed",
                Expression = "{CS Exp=\"{ActionStatus}\".Equals(\"Failed\")}",
                UseCase = "Flowcontrol Condition will execute if the action is Failed",
            };
            TreeViewItem tvi2 = new TreeViewItem();
            SetItemView(tvi2, VERACtionFailed.Name, VERACtionFailed.Expression, VERACtionFailed.IconImageName == null ? "@Config3_16x16.png" : VERACtionFailed.IconImageName);
            Parent.Items.Add(tvi2);
            tvi2.MouseDoubleClick += tvi_MouseDoubleClick;
            tvi2.Selected += UpdateHelpForCSFunction;
            tvi2.Tag = VERACtionFailed;




            ValueExpressionReference VERLastActivityPassed = new ValueExpressionReference
            {
                Name = "Last Activity Passed",
                Expression = "{CS Exp=\"{LastActivityStatus}\".Equals(\"Passed\")}",
                UseCase = "Flowcontrol Condition will execute if the Last Activity is Passed",
            };
            TreeViewItem tvi3 = new TreeViewItem();
            SetItemView(tvi3, VERLastActivityPassed.Name, VERLastActivityPassed.Expression, VERLastActivityPassed.IconImageName == null ? "@Config3_16x16.png" : VERLastActivityPassed.IconImageName);
            Parent.Items.Add(tvi3);
            tvi3.MouseDoubleClick += tvi_MouseDoubleClick;
            tvi3.Selected += UpdateHelpForCSFunction;
            tvi3.Tag = VERLastActivityPassed;


            ValueExpressionReference VERLastActivityFailed = new ValueExpressionReference
            {
                Name = "Last Activity Failed",
                Expression = "{CS Exp=\"{LastActivityStatus}\".Equals(\"Failed\")}",
                UseCase = "Flowcontrol Condition will execute if the Last Activity is Failed",
            };
            TreeViewItem tvi4 = new TreeViewItem();
            SetItemView(tvi4, VERLastActivityFailed.Name, VERLastActivityFailed.Expression, VERLastActivityFailed.IconImageName == null ? "@Config3_16x16.png" : VERLastActivityFailed.IconImageName);
            Parent.Items.Add(tvi4);
            tvi4.MouseDoubleClick += tvi_MouseDoubleClick;
            tvi4.Selected += UpdateHelpForCSFunction;
            tvi4.Tag = VERLastActivityFailed;



        }

        //Added for Business Flow Control in RunSet
        private void AddBusinessFlowControlConditions()
        {


            TreeViewItem Parent = new TreeViewItem();
            SetItemView(Parent, "Flow Control Conditions", "", "@Config3_16x16.png");
            xObjectsTreeView.Items.Add(Parent);

            ValueExpressionReference VERActionpassd = new ValueExpressionReference
            {
                Name = GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " is Passed",
                Expression = "{CS Exp=\"{BusinessFlowStatus}\".Equals(\"Passed\")}",
                UseCase = "BusinessFlowStatus Condition will execute if the action is Passed",
            };
            TreeViewItem tvi = new TreeViewItem();
            SetItemView(tvi, VERActionpassd.Name, VERActionpassd.Expression, VERActionpassd.IconImageName == null ? "@Config3_16x16.png" : VERActionpassd.IconImageName);
            Parent.Items.Add(tvi);
            tvi.MouseDoubleClick += tvi_MouseDoubleClick;
            tvi.Selected += UpdateHelpForCSFunction;
            tvi.Tag = VERActionpassd;

            ValueExpressionReference VERACtionFailed = new ValueExpressionReference
            {
                Name = GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " is Failed",
                Expression = "{CS Exp=\"{BusinessFlowStatus}\".Equals(\"Failed\")}",
                UseCase = "Flowcontrol Condition will execute if the action is Failed",
            };
            TreeViewItem tvi2 = new TreeViewItem();
            SetItemView(tvi2, VERACtionFailed.Name, VERACtionFailed.Expression, VERACtionFailed.IconImageName == null ? "@Config3_16x16.png" : VERACtionFailed.IconImageName);
            Parent.Items.Add(tvi2);
            tvi2.MouseDoubleClick += tvi_MouseDoubleClick;
            tvi2.Selected += UpdateHelpForCSFunction;
            tvi2.Tag = VERACtionFailed;

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

        private void AddCSFunctions()
        {
            WorkSpace.VERefrences= VEReferenceList.LoadFromJson(Path.Combine(new string[] { Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "RosLynLib", "ValueExpressionRefrences.json" }));


            foreach (ValueExpressionReference VER in WorkSpace.VERefrences.Refrences)
            {
                TreeViewItem Parent;
                if (!Categories.TryGetValue(VER.Category, out Parent))
                {
                    Parent = new TreeViewItem();
                    SetItemView(Parent, VER.Category, "",VER.IconImageName==null? "@Config3_16x16.png":VER.IconImageName);
                    xObjectsTreeView.Items.Add(Parent);
                    Categories.Add(VER.Category, Parent);
                }

                TreeViewItem tvi = new TreeViewItem();

                SetItemView(tvi, VER.Name, VER.Expression, VER.IconImageName == null ? "@Config3_16x16.png" : VER.IconImageName);
                Parent.Items.Add(tvi);
                tvi.MouseDoubleClick += tvi_MouseDoubleClick;
                tvi.Selected += UpdateHelpForCSFunction;
                tvi.Tag = VER;
            }


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
            AddVBSEval(tviVars, "Current Month Date +3 Days", "DateSerial(Year(Now), Month(Now),Day(DateAdd(\"d\",3,Now)))");
            AddVBSEval(tviVars, "Current Date +5 Days", "FormatDateTime(DateAdd(\"d\",5,Now),2)");
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
            Tvel.Refrences.Add(new ValueExpressionReference() { Category = "Regular Expressions", Name = Desc, Expression = Eval,IconImageName= "@Regex16x16.png"});
            }

        private void AddVBSEval(TreeViewItem tviVars, string Desc, string Eval)
        {
            TreeViewItem tvi = new TreeViewItem();
            string VarExpression = "{VBS Eval=" + Eval + "}";
            SetItemView(tvi, Desc, VarExpression, "VBS16x16.png");
            tviVars.Items.Add(tvi);
            tvi.MouseDoubleClick += tvi_MouseDoubleClick;
            Tvel.Refrences.Add(new ValueExpressionReference() { Category = "Date Time Functions", Name = Desc, Expression = Eval });
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

            Tvel.Refrences.Add(new ValueExpressionReference() {Category="Date Time Functions",Name=Desc,Expression=Eval });
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
            if (WorkSpace.UserProfile.Solution != null)
            {
                TreeViewItem solutionVars = new TreeViewItem();
                solutionVars.Items.IsLiveSorting = true;
                SetItemView(solutionVars, "Global " + GingerDicser.GetTermResValue(eTermResKey.Variables), "", "@Variable_16x16.png");
                xObjectsTreeView.Items.Add(solutionVars);

                foreach (VariableBase v in WorkSpace.UserProfile.Solution.Variables.OrderBy("Name"))
                    InsertNewVarTreeItem(solutionVars, v);
                InsertAddNewVarTreeItem(solutionVars, eVariablesLevel.Solution);
            }

            if (mContext!= null && mContext.BusinessFlow != null)
            {
                TreeViewItem tviVars = new TreeViewItem();
                tviVars.Items.IsLiveSorting = true;
                SetItemView(tviVars, GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " " + GingerDicser.GetTermResValue(eTermResKey.Variables), "", "@Variable_16x16.png");
                xObjectsTreeView.Items.Add(tviVars);

                foreach (VariableBase v in mContext.BusinessFlow.Variables.OrderBy("Name"))
                    InsertNewVarTreeItem(tviVars, v);
                InsertAddNewVarTreeItem(tviVars, eVariablesLevel.BusinessFlow);
                tviVars.IsExpanded = true;

                if (mContext.BusinessFlow.CurrentActivity != null)
                {
                    TreeViewItem activityVars = new TreeViewItem();
                    activityVars.Items.IsLiveSorting = true;
                    SetItemView(activityVars, GingerDicser.GetTermResValue(eTermResKey.Activity) + " " + GingerDicser.GetTermResValue(eTermResKey.Variables), "", "@Variable_16x16.png");
                    xObjectsTreeView.Items.Add(activityVars);

                    foreach (VariableBase v in mContext.BusinessFlow.CurrentActivity.Variables.OrderBy("Name"))
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
            tvi.Selected += UpdateHelpForVariables;
            tvi.Tag = vb;
      

        }

        private void InsertAddNewVarTreeItem(TreeViewItem parentTvi, eVariablesLevel varLevel)
        {
            TreeViewItem newVarTvi = new TreeViewItem();
            SetItemView(newVarTvi, "Add New String " + GingerDicser.GetTermResValue(eTermResKey.Variable) , varLevel, "@Add_16x16.png");
            parentTvi.Items.Add(newVarTvi);
            newVarTvi.MouseDoubleClick += tviAddNewVarTreeItem_MouseDoubleClick;
        }

        private void AddDataSources()
        {
            TreeViewItem tviDataSources = new TreeViewItem();
            SetItemView(tviDataSources, "Data Sources", "", "@DataSource_16x16.png");
            xObjectsTreeView.Items.Add(tviDataSources);
            
            ObservableList<DataSourceBase> DataSources = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>();

            foreach (DataSourceBase ds in DataSources)
            {
                //if (ds.FilePath.StartsWith("~"))
                //{
                //    ds.FileFullPath = ds.FilePath.Replace(@"~\", "").Replace("~", "");
                //    ds.FileFullPath = Path.Combine( WorkSpace.UserProfile.Solution.Folder , ds.FileFullPath);
                //}
                ds.FileFullPath = amdocs.ginger.GingerCoreNET.WorkSpace.Instance.SolutionRepository.ConvertSolutionRelativePath(ds.FilePath);

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
            if (tvi.Tag.GetType() == typeof(ValueExpressionReference))
            {
                ValueExpressionReference ver = tvi.Tag as ValueExpressionReference;

                AddExpToValue(ver.Expression + "");
            }
            else if (typeof(VariableBase).IsInstanceOfType(tvi.Tag))
            {

                AddExpToValue("{Var Name=" + tvi.Tag + "} ");
            }
            else
            {


                AddExpToValue(tvi.Tag + "");
            }
        }

        private void tviAddNewVarTreeItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            VariableString newStringVar = new VariableString();
            eVariablesLevel varLevel = (eVariablesLevel)((sender as TreeViewItem).Tag);
            switch (varLevel)
            {
                case eVariablesLevel.Solution:
                    ((Solution) WorkSpace.UserProfile.Solution).AddVariable(newStringVar);
                    break;
                case eVariablesLevel.BusinessFlow:
                    ((BusinessFlow)mContext.BusinessFlow).AddVariable(newStringVar);
                    break;
                case eVariablesLevel.Activity:
                    ((Activity)mContext.BusinessFlow.CurrentActivity).AddVariable(newStringVar);
                    break;
            }

            VariableEditPage varEditPage = new VariableEditPage(newStringVar, mContext);
            varEditPage.ShowAsWindow(eWindowShowStyle.Dialog);

            //make sure name is unique
            switch (varLevel)
            {
                case eVariablesLevel.Solution:
                    ((Solution) WorkSpace.UserProfile.Solution).SetUniqueVariableName(newStringVar);
                    break;
                case eVariablesLevel.BusinessFlow:
                    ((BusinessFlow)mContext.BusinessFlow).SetUniqueVariableName(newStringVar);
                    break;
                case eVariablesLevel.Activity:
                    ((Activity)mContext.BusinessFlow.CurrentActivity).SetUniqueVariableName(newStringVar);
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
                Reporter.ToUser(eUserMsgKey.AskToSelectItem);
            }
        }

        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            if (mVE == null)
            {
                mVE = new ValueExpression(App.AutomateTabEnvironment, mContext.BusinessFlow, WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>(), false, "", false);
            }
            mVE.Value = this.ValueUCTextEditor.textEditor.Text;
            ValueCalculatedTextBox.Text = mVE.ValueCalculated;
        }
                
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            string value = ValueUCTextEditor.textEditor.Text;

            //Update the obj attr with new Value
            if (mObj is ExpandoObject)
            {
                ((IDictionary<string, object>)mObj)[mAttrName] = value;
            }
            else if (mObj is JObject)
            {
                ((JObject)mObj).Property(mAttrName).Value = value;
            }
            else
            {
                mObj.GetType().GetProperty(mAttrName).SetValue(mObj, value);
            }
            
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

        private void UpdateHelpForVariables(object sender, RoutedEventArgs e)
        {

            TreeViewItem TVI = sender as TreeViewItem;
            VariableBase Var = TVI.Tag as VariableBase;

            UpdateHelp(true,"Variable: " +Var.Name, "Variable " + Var.VariableType(), "Current Value", Var.Value);
        }

        private void UpdateHelpForCSFunction(object sender, RoutedEventArgs e)
        {

            TreeViewItem TVI = sender as TreeViewItem;
            ValueExpressionReference VER = TVI.Tag as ValueExpressionReference;
            string samples = string.Empty;
            foreach (string sample in VER.Samples)
            {
                if (string.IsNullOrEmpty(samples))
                {
                    samples = sample;
                }
                else
                {
                    samples += System.Environment.NewLine + sample;
                }

            }
            UpdateHelp(false, VER.Name, VER.Category, "Samples", samples, "Expression:" + System.Environment.NewLine + VER.Expression);
        }

        private void UpdateHelp(bool ShowHelpCategory, string Title, string Category, string HelpContentName, string HelpContent, string HelpExtraInfo = null)
        {

            xWarningPanel.Visibility = Visibility.Collapsed;
            xHelpPanel.Visibility = Visibility.Visible;

            if (ShowHelpCategory)
            {
                xHelpCategoryPanel.Visibility = Visibility.Visible;
            }
            else
            {

                xHelpCategoryPanel.Visibility = Visibility.Collapsed;
            }

            xHelpTitle.Content = Title;
            XHelpCategory.Content = Category;
            XHelpContentName.Text = HelpContentName + ": ";
            XHelpContent.Text = HelpContent;
            XHelpExtra.Text = HelpExtraInfo == null ? string.Empty : HelpExtraInfo;
        }

        private async void ValueUCTextEditor_LostFocus(object sender, RoutedEventArgs e)
        {
            string warningExpression = string.Empty;
            string VEText = ValueUCTextEditor.Text;
            await Task.Run(() =>
            {
                foreach (Match m in VBSReg.Matches(VEText))
                {
                    if (string.IsNullOrEmpty(warningExpression))
                    {
                        warningExpression = m.Value;
                    }
                    else
                    {
                        warningExpression += System.Environment.NewLine + m.Value;
                    }
                }
            });

            if (!string.IsNullOrEmpty(warningExpression))
            {
                xWarningPanel.Visibility = Visibility.Visible;
                xHelpPanel.Visibility = Visibility.Collapsed;
                XWarningValueExpression.Text = warningExpression;
            }
            else
            {
                xWarningPanel.Visibility = Visibility.Collapsed;
                xHelpPanel.Visibility = Visibility.Collapsed;
                XWarningValueExpression.Text = string.Empty;
            }
        }

        private void XObjectsTreeView_LostFocus(object sender, RoutedEventArgs e)
        {
            if(!string.IsNullOrEmpty(XWarningValueExpression.Text))
            {
                xWarningPanel.Visibility = Visibility.Visible;
                xHelpPanel.Visibility = Visibility.Collapsed;
            
            }
        }
    }
}
