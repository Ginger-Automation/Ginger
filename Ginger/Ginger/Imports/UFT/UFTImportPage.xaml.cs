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
using Amdocs.Ginger.Common.Repository;
using Amdocs.Ginger.Common.UIElement;
using Ginger.Actions;
using Ginger.UserControls;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Platforms;
using GingerCore.Variables;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Xml;

namespace Ginger.Imports.UFT
{
    /// <summary>
    /// Interaction logic for UFTImportPage.xaml
    /// </summary>
    public partial class UFTImportPage : Page
    {
        //Enum for Script conversion
        private enum eFilter
        {
            AllLines,
            ConvertedtoScript,
            Ignored,
            NotConverted
        }

        //Normal Variables
        public GenericWindow _pageGenericWin = null;
        public string Locator = "";

        //List and Dictionaries
        public Dictionary<string, string> Dictionary_Variables = new Dictionary<string, string>();
        public List<ObjectRepositoryItem> Objectlist_ORI = new List<ObjectRepositoryItem>();
        public List<BusFunction> BusList = new List<BusFunction>();
        public List<string> ListOfSelectedGuis = new List<string>();
        public ObservableList<TargetBase> TargetApplicationsList = new ObservableList<TargetBase>();
        public CommonFunctionConvertor Convertor = new CommonFunctionConvertor();

        // Data Table
        public DataTable dt_BizFlow = new DataTable();

        //Objects
        public ObservableList<ConvertedCodeLine> mCCL = new ObservableList<ConvertedCodeLine>();
        public BusinessFlow mBusinessFlow = new BusinessFlow(); 
        public CommonFunctionConvertor mCommonFunctionConvertor = new CommonFunctionConvertor();

        public UFTImportPage()
        {
            InitializeComponent();

            //handles the basic display of the UFT Import dialog
            ResultsDataGrid.DataSourceList = new ObservableList<ConvertedCodeLine>();
            SetActivitiesGridView();

            eFilter mFilter = eFilter.AllLines;
            App.FillComboFromEnumVal(FilterComboBox, mFilter);
            
            //Pre Load all the Target Applications
            TargetApplication.Items.Add("Google");
            TargetApplication.Items.Add("CRM");
            TargetApplication.Items.Add("CSM");
            TargetApplication.Items.Add("SOM");
            TargetApplication.Items.Add("Mediation");
            TargetApplication.Items.Add("PriceLine");

            InitCommonFunctionMappingUCGrid();

            TargetApplication sTarget = new TargetApplication();
            sTarget.AppName = App.UserProfile.Solution.MainApplication.ToString();
            sTarget.Selected = true;
            TargetApplicationsList.Add(sTarget);
            mBusinessFlow.TargetApplications = TargetApplicationsList;
        }

        public void InitCommonFunctionMappingUCGrid()
        {
            //Add Save and Load button
            CommonFunctionMappingUCGrid.AddButton("Save", new RoutedEventHandler(SaveCommonFunctionMapping));
            CommonFunctionMappingUCGrid.AddButton("Load", new RoutedEventHandler(LoadCommonFunctionMapping));

            //Add Button Handlers for Save and Load
            CommonFunctionMappingUCGrid.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddAction));
            CommonFunctionMappingUCGrid.btnEdit.AddHandler(Button.ClickEvent, new RoutedEventHandler(EditAction));
            
            CommonFunctionMappingUCGrid.DataSourceList = mCommonFunctionConvertor.CommonFunctionMappingList;
            
            GridViewDef defView = new GridViewDef(GridViewDef.DefaultViewName);
            defView.GridColsView = new ObservableList<GridColView>();
            defView.GridColsView.Add(new GridColView() { Field = CommonFunctionMapping.Fields.Function_Name, Header = "Function_Name", WidthWeight = 10, });
            defView.GridColsView.Add(new GridColView() { Field = CommonFunctionMapping.Fields.Action_Description, Header = "Action_Description", WidthWeight = 10, BindingMode = BindingMode.OneWay });
            defView.GridColsView.Add(new GridColView() { Field = CommonFunctionMapping.Fields.LocateBy, Header = "Param to Locate By", WidthWeight = 10, });
            defView.GridColsView.Add(new GridColView() { Field = CommonFunctionMapping.Fields.Value, Header = "Param to set Value", WidthWeight = 10, });
            defView.GridColsView.Add(new GridColView() { Field = CommonFunctionMapping.Fields.NoOfParameters, Header = "No Of Params", WidthWeight = 10, });

            CommonFunctionMappingUCGrid.SetAllColumnsDefaultView(defView);
            CommonFunctionMappingUCGrid.InitViewItems();
        }

        private void EditAction(object sender, RoutedEventArgs e)
        {
            Act a = ((CommonFunctionMapping)CommonFunctionMappingUCGrid.CurrentItem).TargetAction;
            ActionEditPage actedit = new ActionEditPage(a);            
            actedit.ShowAsWindow();
        }

        public void LoadCommonFunctionMapping(object sender, RoutedEventArgs e)
        {
            using (XmlReader reader = XmlReader.Create(@"c:\temp\CommonFunctionConvertor.xml"))
            {
                Convertor = new CommonFunctionConvertor();
                while (reader.Read())
                {
                    // Only detect start elements.
                    if (reader.Name == "Ginger.Imports.UFT.CommonFunctionMapping")
                    {
                        CommonFunctionMapping CFM1 = new CommonFunctionMapping();
                        ActGenElement a = new ActGenElement();
                        CFM1.Function_Name = reader["Function_Name"]; ;
                        CFM1.TargetAction = a;
                        CFM1.LocateBy = reader["LocateBy"];
                        CFM1.Value = reader["Value"];
                        CFM1.NoOfParameters = reader["NoOfParameters"];

                        if (CFM1.Function_Name!=null)
                        {
                            Convertor.CommonFunctionMappingList.Add(CFM1);
                        }
                    }
                }
                
                if (Convertor.CommonFunctionMappingList.Count>0)
                {
                    //Show the common function in Grid
                    CommonFunctionMappingUCGrid.DataSourceList = Convertor.CommonFunctionMappingList;
                }
            }
        }

        private void SaveCommonFunctionMapping(object sender, RoutedEventArgs e)
        {
            //temp TODO: fix me to select file
            mCommonFunctionConvertor.SaveToFile(@"c:\temp\CommonFunctionConvertor.xml");
        }

        private void AddAction(object sender, RoutedEventArgs e)
        {
            ObservableList<Act> ActionsList = new ObservableList<Act>();

            // We create one dummy activity in case we convert code without function
            mBusinessFlow.Activities = new ObservableList<Activity>();
            Activity at = new Activity();
            at.ActivityName = GingerDicser.GetTermResValue(eTermResKey.Activity) + "1";
            mBusinessFlow.Activities.Add(at);
            mBusinessFlow.CurrentActivity = at;
            mBusinessFlow.CurrentActivity.TargetApplication = App.UserProfile.Solution.MainApplication.ToString(); //"Google"; //TargetApplication.SelectedItem.ToString();
            App.BusinessFlow = mBusinessFlow;
           
            AddActionPage addAction = new AddActionPage();
            addAction.ShowAsWindow(ActionsList);

            // We will get only one action currently
            Act a = ActionsList[0];
            CommonFunctionMapping CFM = new CommonFunctionMapping();                        
            CFM.TargetAction = a;
            mCommonFunctionConvertor.CommonFunctionMappingList.Add(CFM);
        }

        private void SetActivitiesGridView()
        {            
            //# Default View for Import from UFT Dialog
            GridViewDef defView = new GridViewDef(GridViewDef.DefaultViewName);
            defView.GridColsView = new ObservableList<GridColView>();            
            defView.GridColsView.Add(new GridColView() { Field = ConvertedCodeLine.Fields.Checked, WidthWeight = 5, StyleType = Ginger.UserControls.GridColView.eGridColStyleType.CheckBox, ReadOnly = true });
            defView.GridColsView.Add(new GridColView() { Field = ConvertedCodeLine.Fields.CodeLine, Header = "Code Line", WidthWeight = 10 });
            defView.GridColsView.Add(new GridColView() { Field = ConvertedCodeLine.Fields.Converted, WidthWeight = 10 });
            defView.GridColsView.Add(new GridColView() { Field = ConvertedCodeLine.Fields.Status, WidthWeight = 2 });
            
            ResultsDataGrid.SetAllColumnsDefaultView(defView);
            ResultsDataGrid.InitViewItems();
        }

        // Handles browsing of Script File from user desktop
        private void ScriptFileBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            string path = "";
            var dlg = new System.Windows.Forms.OpenFileDialog();
            dlg.Title = "Select GUI Script File";
            System.Windows.Forms.DialogResult result = dlg.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                ScriptFileTextBox.Text = dlg.FileName;                
                path = ScriptFileTextBox.Text;
            }
        }

        //Handle browsing of BUS file script
        private void ScriptBUSFileBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new System.Windows.Forms.OpenFileDialog();
            dlg.Title = "Select BUS Script File";
            System.Windows.Forms.DialogResult result = dlg.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                ScriptBUSFileTextBox.Text = dlg.FileName;
            }
        }

        // Handles browsing of Repository File (xml) from user desktop
        private void UFTObjectRepositoryBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new System.Windows.Forms.OpenFileDialog();
            dlg.Title = "Select UFT Object Repository File";

            System.Windows.Forms.DialogResult result = dlg.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                UFTObjectRepositoryTextBox.Text = dlg.FileName;
            }
        }

        //Handles browsing of Calendar
        private void CalendarBrowse_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new System.Windows.Forms.OpenFileDialog();
            dlg.Title = "Select Calendar Excel";
            System.Windows.Forms.DialogResult result = dlg.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                CalendarTextBox.Text = dlg.FileName;
            }
        }

        private void CalendarTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (CalendarTextBox.Text != "")
            {
                //Populate Combo box on change of text in Calendar Text Box
                string sExcelFileName = CalendarTextBox.Text;
                List<string> BusFunction = new List<string>();

                UFT_ExcelProcessing Excel = new UFT_ExcelProcessing();
                dt_BizFlow = Excel.ProcessExcel(sExcelFileName);

                foreach (DataRow row in dt_BizFlow.Rows)
                {
                    if (row["Param0"].ToString() != "" && row["Param0"].ToString() != "ACTION")
                    {
                        BusFunction.Add(row["Param0"].ToString());
                        CalendarBusFunction.Items.Add(row["Param0"].ToString());
                    }
                }
                CalendarBusFunction.SelectedIndex = 0;
            }
            else
            {                
                Reporter.ToUser(eUserMsgKeys.StaticWarnMessage, "Please Enter a Valid Calendar path !!");
            }
        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {
            // for "Convert" Button in dialog
            Button ConvertButton = new Button();
            ConvertButton.Content = "Convert";
            ConvertButton.Click += new RoutedEventHandler(ConvertButton_Click);

            // for "Save To Business Flow" Button in dialog
            Button SaveBusinessFlowButton = new Button();
            SaveBusinessFlowButton.Content = "Save to " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow);
            SaveBusinessFlowButton.Click += new RoutedEventHandler(SaveBusinessFlowButton_Button_Click);

            // for "Fetch GUI" Button in dialog
            Button FetchGui = new Button();
            FetchGui.Content = "Fetch GUI functions";
            FetchGui.Click += new RoutedEventHandler(FetchGUI_Button_Click);

            // for "Clear" Button in dialog
            Button ClearGrid = new Button();
            ClearGrid.Content = "Clear Grid";
            ClearGrid.Click += new RoutedEventHandler(ClearGrid_Button_Click);

            // Create button list
            ObservableList<Button> Buttons = new ObservableList<Button>();
            Buttons.Add(ClearGrid);
            Buttons.Add(SaveBusinessFlowButton);
            Buttons.Add(ConvertButton);
            Buttons.Add(FetchGui);
            GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, windowStyle, this.Title, this, Buttons);
        }

        //Clears the "Code Line Display" grid
        private void ClearGrid_Button_Click(object sender, RoutedEventArgs e)
        {
            if (mCCL.Count == 0)
            {
                Reporter.ToUser(eUserMsgKeys.NoItemToDelete);
                return;
            }
            else
            {
                mCCL.Clear();
                ResultsDataGrid.DataSourceList = mCCL;
            }
        }

        //On Click on Fetch GUI button
        private void FetchGUI_Button_Click(object sender, RoutedEventArgs e)
        {
            string flowName="";
            mCCL.Clear();
            
            //Create new biz flow
            mBusinessFlow = new BusinessFlow();

            if (BusinessFlowNameTextBox.Text == "")
            {
                flowName = CalendarBusFunction.SelectedValue.ToString();
                flowName = flowName.Replace("fBus", "");
                flowName = Regex.Replace(flowName, "([a-z](?=[A-Z])|[A-Z](?=[A-Z][a-z]))", "$1 ");

                BusinessFlowNameTextBox.Text = flowName;
                mBusinessFlow.Name = flowName;
            }
            else mBusinessFlow.Name = BusinessFlowNameTextBox.Text;

            if (mBusinessFlow.Name != "")
            {
                mBusinessFlow.Activities = new ObservableList<Activity>();

                // We create one dummy activity in case we convert code without function
                mBusinessFlow.Activities.Add(new Activity() { ActivityName = GingerDicser.GetTermResValue(eTermResKey.Activity) + "1" });

                //Process BUS File (fetch BUS function and their respective GUI )
                BusFunctionHandler BusHandler = new BusFunctionHandler();
                BusList = BusHandler.ProcessBusScript(ScriptBUSFileTextBox.Text);

                // Now depending on Drop down selection , Show only the GUI which are part of that BUS function
                ShowGuiAsPerBus();

                //Read the Entire GUI file
                string[] CodeLines = System.IO.File.ReadAllLines(ScriptFileTextBox.Text);

                foreach (string CodeLine in CodeLines)
                {
                    ConvertedCodeLine CCL = new ConvertedCodeLine();
                    CCL.CodeLine = CodeLine;
                    CCL.Status = ConvertedCodeLine.eStatus.ConvertedToScript;
                    CCL.Converted = "Converted to " + GingerDicser.GetTermResValue(eTermResKey.Activity);
                    string CodeLineUpper = CodeLine.ToUpper();

                    if (CodeLineUpper.StartsWith("PUBLIC FUNCTION") || CodeLineUpper.StartsWith("FUNCTION") || CodeLineUpper.StartsWith("PUBLIC SUB") || CodeLineUpper.StartsWith("SUB"))
                    {
                        string ProcessedString="";

                        if (CCL.CodeLine.Contains("("))
                        {
                            int posCode = CCL.CodeLine.IndexOf("(");
                            CCL.CodeLine = CCL.CodeLine.Remove(posCode);
                        }

                        string funcName = ProceesNewFunction(CCL.CodeLine);

                        if (funcName.Contains("("))
                        {
                            int posFun = funcName.IndexOf("(");
                            ProcessedString = funcName.Remove(posFun).Trim();
                        }
                        else ProcessedString = funcName;
                        
                        if (ListOfSelectedGuis.Contains(ProcessedString))
                        {
                            mCCL.Add(CCL);
                        }
                    }
                }
    
                //Auto Check all the cgk box for Guis
                if (mCCL != null && mCCL.Count!=0)
                {
                    for (int i = 0; i < mCCL.Count; i++)
                    {
                        mCCL[i].Checked = true;
                    }
                    ResultsDataGrid.Title = "GUI functions from GUI file for selected BUS function";
                    ResultsDataGrid.DataSourceList = mCCL;
                }
                else
                {                    
                    Reporter.ToUser(eUserMsgKeys.StaticWarnMessage, "No GUI functions fetched, Click CONVERT, to convert other actions in BUS function");
                }
            }
            else
            {                
                Reporter.ToUser(eUserMsgKeys.EnterValidBusinessflow);
            }
        }

        //On Click of Save Button
        private void SaveBusinessFlowButton_Button_Click(object sender, RoutedEventArgs e)
        {
            mBusinessFlow.Name = BusinessFlowNameTextBox.Text;
            WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(mBusinessFlow);            

            //TODO: open the new BF in Automate tab + make sure it is added to the tree
            Reporter.ToGingerHelper(eGingerHelperMsgKey.ScriptImported_RefreshSolution);
            _pageGenericWin.Close();
        }

        private int FetchBusPosition(string[] BusCodeLines)
        {
          int pos=0;
          string BusLineUpper;
          foreach (string BusLine in BusCodeLines)
          {
            pos++;
            BusLineUpper = BusLine.ToUpper();
            if ((BusLineUpper.Contains("FUNCTION") || BusLineUpper.Contains("SUB")) && BusLine.Contains(CalendarBusFunction.SelectedValue.ToString()) && !BusLineUpper.Contains("'"))
            {
                return pos;
            }
          } 
          return pos;     
        }

        public void ConvertButton_Click(object sender, RoutedEventArgs e)
        {
                //If List contains elements for common function, then assign it to main list as well 
                if (Convertor.CommonFunctionMappingList.Count>0)    
                {
                    mCommonFunctionConvertor.CommonFunctionMappingList = Convertor.CommonFunctionMappingList;
                }

                //Extract Objects from XML repository
                ProcessUFTObjectRepository();

                if (ListOfSelectedGuis.Count != 0)
                {
                     //Identify actions from Script
                    ProcessScript();
                }
                else //if BUS function does not contain any GUI functions, process BUS function to see if any Actions can be retireved
                {
                    //Create an Activity with the BUS function Name
                    ConvertedCodeLine Bus = new ConvertedCodeLine();
                    CreateActivity(BusinessFlowNameTextBox.Text, Bus);

                    int Pos = 0;
                    string BusLineUpper="";

                    //Fetch the Entire BUS script
                    string[] BusCodeLines = System.IO.File.ReadAllLines(ScriptBUSFileTextBox.Text);

                    //Fetch the position of Bus function the BUS script
                    Pos = FetchBusPosition(BusCodeLines);
                    if (Pos!=0)
                    {
                        //Loop through the Specific bus Function
                         for(int i = Pos ; i<BusCodeLines.Count() ; i++)
                         {
                             BusLineUpper = BusCodeLines[i].ToUpper();
                             if (BusLineUpper.Contains("END FUNCTION") ||BusLineUpper.Contains("END SUB"))
                             {
                                 break;
                             }

                             ConvertedCodeLine BusCCL = new ConvertedCodeLine();
                             BusCCL.CodeLine = BusCodeLines[i];
                             ProceesActions(BusCCL);
                             mCCL.Add(BusCCL);
                         }
                    }
                 }

                //Auto Check all the cgk box for Guis
                if (mCCL != null)
                {
                    for (int i = 0; i < mCCL.Count; i++)
                    {
                        mCCL[i].Checked = true;
                    }

                    ResultsDataGrid.Title = "Converted Code Line Status";

                    //to Show only Converted Script as default
                    FilterComboBox.SelectedValue = "ConvertedtoScript";
                    IEnumerable<ConvertedCodeLine> FilterdCCLs;
                    ObservableList<ConvertedCodeLine> CCLs = new ObservableList<ConvertedCodeLine>();
                    FilterdCCLs = from x in mCCL where x.Status == ConvertedCodeLine.eStatus.ConvertedToScript select x;

                    foreach (ConvertedCodeLine CCL in FilterdCCLs)
                    {
                        CCLs.Add(CCL);
                    }

                    ResultsDataGrid.DataSourceList = CCLs;
                    ResultsDataGrid.Refresh();
                }

                //Show script conversion status
                ShowStats();

                //Create Variables for each of the Parameter in the selected flow
                CreateVaribales();
            }

        public List<string> ShowGuiAsPerBus()
        {
            string BusFunctionSelected = CalendarBusFunction.SelectedValue.ToString();
            foreach (BusFunction x in BusList)
            {
                if( x.BusFunctionName.Replace("(","").Replace(")","").Contains(BusFunctionSelected))
                {
                    ListOfSelectedGuis = x.ListOfGuiFunctions;
                }
            }
            return ListOfSelectedGuis;
        }

        private void CreateVaribales()
        {
            foreach (KeyValuePair<string, string> entry in Dictionary_Variables)
            {
                VariableString v = new VariableString();
                v.Description = GingerDicser.GetTermResValue(eTermResKey.Variable) + " added from Calendar";
                v.Name = entry.Key;
                v.Value = entry.Value;
                v.CreateCopy();
                mBusinessFlow.AddVariable(v);
            }
        }
   
        private void ProcessUFTObjectRepository()
        {
             //initializing variables
            string sXMLPath;

            //Fetch the XML Object Repository path
            sXMLPath = UFTObjectRepositoryTextBox.Text;

            //Calling function to Process XML
            Objectlist_ORI = ObjectRepositoryConverter.ProcessXML(sXMLPath);
        }
    
        private void ProcessScript()
        {
            string line;
            System.IO.StreamReader file = new System.IO.StreamReader(ScriptFileTextBox.Text);

            while ((line = file.ReadLine()) != null)
            {
                if ((line.StartsWith("Function") || line.StartsWith("Sub")) || (line.StartsWith("Public") || line.StartsWith("Private")))
                {
                    string fName = ProceesNewFunction(line);

                    //if Gui Function is Like fGuiLogin()
                    if (fName.Contains("()"))
                    {
                        fName = fName.Replace("()", "");
                    }
                    //If Gui function has input params like fGuiLogin(UserName,Pwd)
                    if (fName.Contains("("))
                    {
                        int pos = fName.IndexOf("(");
                        int len = fName.Length;
                        string midString = fName.Substring(pos);
                        fName = fName.Replace(midString, "");
                    }
            
                    if (ListOfSelectedGuis.Contains(fName))
                    {
                        ConvertedCodeLine CL = new ConvertedCodeLine();
                        CL.CodeLine = line;
                        CreateActivity(fName, CL);
                        while (!line.Contains("End Function") || line.Contains("End Sub"))
                        {
                            ProcessCodeLine(line);
                            line = file.ReadLine().ToString();
                        }
                    }
                }
            }
            ResultsDataGrid.DataSourceList = mCCL;
        }

        public void ProcessCodeLine(string CodeLine)
        {            
            ConvertedCodeLine CCL = new ConvertedCodeLine();
            CCL.CodeLine = CodeLine;
            CCL.Status = ConvertedCodeLine.eStatus.Unknown;
            CodeLine = CodeLine.Trim();
            string CodeLineUpper = CodeLine.ToUpper();

            if (CodeLine.StartsWith("'"))
            {
                // do nothing this is comment
                CCL.Converted = "' Comment";
                CCL.Status = ConvertedCodeLine.eStatus.Ignored;
            }
            else if (CodeLine.Length == 0)
            {
                // do nothing this is empty line
                CCL.Converted = "Empty Line";
                CCL.Status = ConvertedCodeLine.eStatus.Ignored;
            }
            else if (CodeLineUpper == "END IF" || CodeLineUpper == "ELSE" || CodeLineUpper == "END FUNCTION" || CodeLineUpper == "EXIT FUNCTION" || CodeLineUpper == "END SUB")
            {
                // do nothing 
                CCL.Converted = "Ignored (End if/Else/End Function/Sub)";
                CCL.Status = ConvertedCodeLine.eStatus.Ignored;
            }
            else 
            {
                ProceesActions(CCL);
                mCCL.Add(CCL);
            }
        }

        private string ProceesNewFunction(string b)
        {
            string FuncName;

            //Fetch the Function/Sub name from Script File
            if (b.Contains("Public") && b.Contains("Function"))
            {
                b = b.Replace("Public", "").Replace("Function", "");
            }
            else if (b.Contains("Public") && b.Contains("Sub"))
            {
                  b = b.Replace("Public", "").Replace("Sub", "");
            }
            else if (b.Contains("Private") && b.Contains("Function"))
            {
                b = b.Replace("Private", "").Replace("Function", "");
            }
            else if (b.Contains("Private") && b.Contains("Sub"))
            {
                b = b.Replace("Private", "").Replace("Sub", "");
            }
            else if (b.Contains("Function"))
            {
                 b = b.Replace("Function", "");
            }
            else if (b.Contains("Sub"))
            {
                b = b.Replace("Sub", "");
            }
           
            FuncName = b.Trim();
            return FuncName;
        }

        //Create Activity for each function/Sub identified in script
        void CreateActivity(string ActivityName, ConvertedCodeLine CCL)
        { 
            //Create a New Activity with the Function/Sub Name
            Activity a = new Activity();
            a.Description = GingerDicser.GetTermResValue(eTermResKey.Activity) + " Created for " + ActivityName + " function ";
            if (ActivityName.Contains("fGui")) ActivityName = ActivityName.Replace("fGui", "");
            if (ActivityName.Contains("_")) ActivityName = ActivityName.Replace("_", " ");

            ActivityName = Regex.Replace(ActivityName, "([a-z](?=[A-Z])|[A-Z](?=[A-Z][a-z]))", "$1 ");

            a.ActivityName = ActivityName;
            mBusinessFlow.Activities.Add(a);
            mBusinessFlow.CurrentActivity = a;
            CCL.Converted = "New " + GingerDicser.GetTermResValue(eTermResKey.Activity) + " - " + ActivityName;
            CCL.Status = ConvertedCodeLine.eStatus.ConvertedToScript;
        }

        //Function to identify Locate By and Locate Value
        public string ProcessLocateBy_Value(string ObjectName)
        {
            string xpath = "";
            string MultipleProperties = "";
            string sProp="";
            string sValue="";

            foreach (ObjectRepositoryItem Item in Objectlist_ORI)
            {
                if (Item.Name == ObjectName)
                {
                    foreach (ObjectRepositortyItemProperty Prop in Item.Properties)
                    {
                        if (Prop.Name == "_xpath")
                        {
                            xpath = Prop.Value;
                            xpath = xpath.Replace("\"", "'");
                            return xpath;
                        }
                    }
                }
            }

            //try Multiple Descriptive Properties 
            if (xpath != "") { Locator = "ByXPath"; return xpath; }
            else
            {
                char[] delimiterChars = { ',' };
                string TempParam = "";
                string[] paramArr = ObjectName.Split(delimiterChars);
  
                foreach (string param in paramArr)
                {
                    TempParam = param.Replace("\\", "").Replace("\\", "").Trim();
                    char[] delChr = { '=' };
                    string[] prmArr = TempParam.Split(delChr);

                    if (prmArr.Count()==2)
                    {
                         sProp = prmArr[0].Replace(":", "").Replace("\"", "").Trim();
                         sValue = prmArr[1].Replace("\"","").Trim();

                         if (sProp != "")
                         {
                             if (sProp.Contains("/"))
                             {
                                 sProp = sProp.Replace("/", "").Trim();
                             }

                             if (sValue.Contains("&"))
                             {
                                 sValue = sValue.Replace("&", "").Trim();
                             }

                             //If sValue is a Global Dictionary Parameter
                             if (sValue.ToUpper().Contains("GLOBALDICTIONARY") )
                             {
                                 string varName = sValue.ToUpper().Replace("GLOBALDICTIONARY(", "").Trim();
                                 sValue = "{Var Name=" + varName + "}";
                             }

                             switch (sProp)
                             {
                                 case "ID": MultipleProperties = MultipleProperties + "ByID:" + sValue + "|"; break;
                                 case "Text": MultipleProperties = MultipleProperties + "ByLinkText:" + sValue + "|"; break;
                                 case "innertext": MultipleProperties = MultipleProperties + "ByLinkText:" + sValue + "|"; break;
                                 case "Title": MultipleProperties = MultipleProperties + "ByTitle:" + sValue + "|"; break;
                                 case "name": MultipleProperties = MultipleProperties + "ByName:" + sValue + "|"; break;
                                 case "index": MultipleProperties = MultipleProperties + "ByIndex:" + sValue + "|"; break;
                                 case "class": MultipleProperties = MultipleProperties + "ByClassName:" + sValue + "|"; break;
                             }
                         }
                    }
                }

                if (MultipleProperties.EndsWith("|")) { MultipleProperties = MultipleProperties.Remove(MultipleProperties.Length - 1, 1); }
                Locator = "ByMulitpleProperties";
                return MultipleProperties.Trim();
            }
        }

        private void ProceesActions(ConvertedCodeLine CCL) 
        {
            string CodeLine = CCL.CodeLine;
            string value="";
            string SetValueinObject = "";
            string varName="";
            string xpath = "";
            string type = "";
            
            // Extract the WebEdit/WebCheckBox
            if (CodeLine.Contains(".Set") || CodeLine.Contains(".set"))
            {
                if (CodeLine.Contains("WebEdit"))
                {
                    type = "Edit Box";
                    SetValueinObject = GetStringBetween(CodeLine, ".WebEdit(\"", "\")");

                    //With Space in End 
                    if (SetValueinObject=="") SetValueinObject = GetStringBetween(CodeLine, ".WebEdit(\"", "\" )");

                    //Calling function to identify Locate By and Locate Value
                    xpath = ProcessLocateBy_Value(SetValueinObject);
                }
                else if (CodeLine.Contains("WebCheckBox"))
                {
                    type = "Check Box";
                    SetValueinObject = GetStringBetween(CodeLine, ".WebCheckBox(\"", "\")");

                    //With Space in End 
                    if (SetValueinObject == "") SetValueinObject = GetStringBetween(CodeLine, ".WebCheckBox(\"", "\" )");

                    //Calling function to identify Locate By and Locate Value
                    xpath = ProcessLocateBy_Value(SetValueinObject);
                }

                // for Values to Set
                if (CodeLine.Contains("Set \""))  // For hard coded values
                {
                    value = GetStringBetween(CodeLine, "Set \"", CodeLine[CodeLine.Length-1].ToString());
                }
                else if (CodeLine.Contains("GlobalDictionary"))
                {
                    varName = CodeLine.Substring(CodeLine.IndexOf("GlobalDictionary")).Replace("GlobalDictionary", "").Trim();
                    varName = varName.Replace("(", "").Replace(")", "");
                    value = "{Var Name=" + varName + "}";
                }
                else if (CodeLine.Contains("Globaldictionary"))
                {
                    varName = CodeLine.Substring(CodeLine.IndexOf("Globaldictionary")).Replace("Globaldictionary", "").Trim();
                    varName = varName.Replace("(", "").Replace(")", "");
                    value = "{Var Name=" + varName + "}";
                }
                else if (!CodeLine.Contains("GlobalDictionary") && !CodeLine.Contains("Globaldictionary"))  // for variables declared using Dim
                {
                    varName = CodeLine.Substring(CodeLine.IndexOf("Set")).Replace("Set", "").Trim();
                    value = "{Var Name=" + varName + "}";
                    VariableString v = new VariableString();
                    v.Description = GingerDicser.GetTermResValue(eTermResKey.Variable) + " added by UFT Script";
                    v.Name = varName;
                    v.Value = value;
                    mBusinessFlow.AddVariable(v);
                }

                // Add Action to biz flow
                ActGenElement act = new ActGenElement();
                act.Value = value.Replace("\"", "").Replace("\"", "");
                act.GenElementAction = ActGenElement.eGenElementAction.SetValue;
              
                if (Locator == "ByXPath"){ act.LocateBy = eLocateBy.ByXPath; }
                else if (Locator == "ByMulitpleProperties") { act.LocateBy = eLocateBy.ByXPath; }

                act.LocateValue = xpath;
                act.Description = "Set value in " + SetValueinObject + " " + type;
                mBusinessFlow.AddAct(act);
                CCL.Converted = "New Action - ActGenElement.SetValue : LocateValue=" + act.LocateValue + ", Value=" + act.Value;
                CCL.Status = ConvertedCodeLine.eStatus.ConvertedToScript;
            }
            // Extract the WebButton/Link/Image/WebELemnt
            else if (CodeLine.Contains(".Click") || CodeLine.Contains(".click"))
                {

                if (CodeLine.Contains("WebButton"))
                {
                    type = "Button";
                    SetValueinObject = GetStringBetween(CodeLine, ".WebButton(\"", "\")");

                    //With Space in End 
                    if (SetValueinObject == "") SetValueinObject = GetStringBetween(CodeLine, ".WebButton(\"", "\" )");

                    //Calling function to identify Locate By and Locate Value
                    xpath = ProcessLocateBy_Value(SetValueinObject);
                }
                else if (CodeLine.Contains("Link"))
                {
                    type = "Link";
                    SetValueinObject = GetStringBetween(CodeLine, ".Link(\"", "\")");

                    //With Space in End 
                    if (SetValueinObject == "") SetValueinObject = GetStringBetween(CodeLine, ".Link(\"", "\" )");

                    //Calling function to identify Locate By and Locate Value
                    xpath = ProcessLocateBy_Value(SetValueinObject);
                }
                else if (CodeLine.Contains("WebElement"))
                {
                    type = "Web Element";
                    SetValueinObject = GetStringBetween(CodeLine, ".WebElement(\"", "\")");

                    //With Space in End 
                    if (SetValueinObject == "") SetValueinObject = GetStringBetween(CodeLine, ".WebElement(\"", "\" )");

                    //Calling function to identify Locate By and Locate Value
                    xpath = ProcessLocateBy_Value(SetValueinObject);
                }
                else if (CodeLine.Contains("Image"))
                {
                    type = "Image";
                    SetValueinObject = GetStringBetween(CodeLine, ".Image(\"", "\")");

                    //With Space in End 
                    if (SetValueinObject == "") SetValueinObject = GetStringBetween(CodeLine, ".Image(\"", "\" )");

                    //Calling function to identify Locate By and Locate Value
                    xpath = ProcessLocateBy_Value(SetValueinObject);
                }

                // Add Action to biz flow
                ActGenElement act = new ActGenElement();
                act.Value = "";

                if (Locator == "ByXPath") { act.LocateBy = eLocateBy.ByXPath; }
                else if (Locator == "ByMulitpleProperties") { act.LocateBy = eLocateBy.ByMulitpleProperties; }

                act.LocateValue = xpath;
                act.GenElementAction = ActGenElement.eGenElementAction.Click;
                act.Description = "Click on " + SetValueinObject + " " + type;
                mBusinessFlow.AddAct(act);

                CCL.Converted = "New Action - ActGenElement.Click : LocateValue=" + act.LocateValue; 
                CCL.Status = ConvertedCodeLine.eStatus.ConvertedToScript;
            }
            // Extract the WeBlist/WebRadiogroup
            else if (CodeLine.Contains(".Select") || CodeLine.Contains(".select"))
            {
                if (CodeLine.Contains("WebList"))
                {
                    type = "List";
                    SetValueinObject = GetStringBetween(CodeLine, ".WebList(\"", "\")");

                    //With Space in End 
                    if (SetValueinObject == "") SetValueinObject = GetStringBetween(CodeLine, ".WebList(\"", "\" )");
                    
                    //Calling function to identify Locate By and Locate Value
                    xpath = ProcessLocateBy_Value(SetValueinObject);
                }
                else if (CodeLine.Contains("WebRadiogroup"))
                {
                    type = "Radio Group";
                    SetValueinObject = GetStringBetween(CodeLine, ".WebRadiogroup(\"", "\")");

                    //With Space in End 
                    if (SetValueinObject == "") SetValueinObject = GetStringBetween(CodeLine, ".WebRadiogroup(\"", "\" )");

                    //Calling function to identify Locate By and Locate Value
                    xpath = ProcessLocateBy_Value(SetValueinObject);
                }

                //For Values to Set
                if (CodeLine.Contains("Select \""))  // For hard coded values
                {
                    value = GetStringBetween(CodeLine, "Select \"", CodeLine[CodeLine.Length - 1].ToString());
                }
                else if (CodeLine.Contains("GlobalDictionary"))
                {
                    varName = CodeLine.Substring(CodeLine.IndexOf("GlobalDictionary")).Replace("GlobalDictionary", "").Trim();
                    varName = varName.Replace("(", "").Replace(")", "");
                    value = "{Var Name=" + varName + "}";
                }
                else if (CodeLine.Contains("Globaldictionary"))
                {
                    varName = CodeLine.Substring(CodeLine.IndexOf("Globaldictionary")).Replace("Globaldictionary", "").Trim();
                    varName = varName.Replace("(", "").Replace(")", "");
                    value = "{Var Name=" + varName + "}";
                }
                else 
                {
                    varName = CodeLine.Substring(CodeLine.IndexOf("Select ")).Replace("Select ", "").Trim();
                    value = "{Var Name=" + varName + "}";
                    VariableString v = new VariableString();
                    v.Description = GingerDicser.GetTermResValue(eTermResKey.Variable) + " added by UFT Script";
                    v.Name = varName;
                    v.Value = value;
                    mBusinessFlow.AddVariable(v);
                }

                // Add Action to biz flow
                ActGenElement act = new ActGenElement();
                act.Value = value;

                if (Locator == "ByXPath") { act.LocateBy = eLocateBy.ByXPath; }
                else if (Locator == "ByMulitpleProperties") { act.LocateBy = eLocateBy.ByXPath; }

                act.LocateValue = xpath;
                act.GenElementAction = ActGenElement.eGenElementAction.SetValue;
                act.Description = "Select value from " + SetValueinObject + " " + type;
                mBusinessFlow.AddAct(act);
                CCL.Converted = "New Action - ActGenElement.SetValue : LocateValue=" + act.LocateValue + ", Value=" + act.Value;
                CCL.Status = ConvertedCodeLine.eStatus.ConvertedToScript;
            }
            // Extract the URL to Navigate to
            else if (CodeLine.Contains("Navigate"))
            {
                // Extract the URL
                string URL = GetStringBetween(CodeLine, ".Navigate(\"", "\")");

                // Add Action to biz flow
                ActGotoURL act = new ActGotoURL();
                act.Value = URL;
                act.Description = "Navigate to URL";
                mBusinessFlow.AddAct(act);
                CCL.Converted = "New Action - ActGotoURL : " + URL;
                CCL.Status = ConvertedCodeLine.eStatus.ConvertedToScript;
            }
             // Extract the URL launched using SystemUtil.Run
            else if (CodeLine.Contains("SystemUtil.Run") && CodeLine.Contains("iexplore.exe"))
            {
                // Extract the URL
                string URL = GetStringBetween(CodeLine, "iexplore.exe\",\"", "\"");

                // Add Action to biz flow
                ActGotoURL act = new ActGotoURL();
                act.Value = URL;
                act.Description = "Navigate to URL";
                mBusinessFlow.AddAct(act);
                CCL.Converted = "New Action - ActGotoURL : " + URL;
                CCL.Status = ConvertedCodeLine.eStatus.ConvertedToScript;
            }
            else if (CodeLine.Contains("fDBCheck") || CodeLine.Contains("fDBActivities")) //ASAP function
            {
                // Temp until we read the SQL from common.xls
                ActDBValidation act = new ActDBValidation();
                act.Description = "DB Action-Configure Manually";
                mBusinessFlow.AddAct(act);
                CCL.Converted = "New Action - DB Action ";
                CCL.Status = ConvertedCodeLine.eStatus.ConvertedToScript;
            }
         
            Dictionary<string, string> actionCommon = mCommonFunctionConvertor.ConvertCodeLine(CodeLine);
            if (actionCommon != null)
            {
                //Fetch the OR object from Code Line
                if (actionCommon["LocateBy"].Contains("WebEdit")) { SetValueinObject = GetStringBetween(actionCommon["LocateBy"], ".WebEdit(", ")"); }
                else if (actionCommon["LocateBy"].Contains("WebCheckBox")) { SetValueinObject = GetStringBetween(actionCommon["LocateBy"], ".WebCheckBox(", ")"); }
                else if (actionCommon["LocateBy"].Contains("WebButton")) { SetValueinObject = GetStringBetween(actionCommon["LocateBy"], ".WebButton(", ")"); }
                else if (actionCommon["LocateBy"].Contains("Link")) { SetValueinObject = GetStringBetween(actionCommon["LocateBy"], ".Link(", ")"); }
                else if (actionCommon["LocateBy"].Contains("Image")) { SetValueinObject = GetStringBetween(actionCommon["LocateBy"], ".Image(", ")"); }
                else if (actionCommon["LocateBy"].Contains("WebElement")) { SetValueinObject = GetStringBetween(actionCommon["LocateBy"], ".WebElement(", ")"); }
                else if (actionCommon["LocateBy"].Contains("WebList")) { SetValueinObject = GetStringBetween(actionCommon["LocateBy"], ".WebList(", ")"); }
                else if (actionCommon["LocateBy"].Contains("WebRadiogroup")) { SetValueinObject = GetStringBetween(actionCommon["LocateBy"], ".WebRadiogroup(", ")"); }

                //to Handle stand alone object names as parameters in COmmon function
                if (SetValueinObject == "")
                {
                    SetValueinObject = actionCommon["LocateBy"];
                }

                xpath = ProcessLocateBy_Value(SetValueinObject);

                // Add Action to biz flow
                ActGenElement act = new ActGenElement();

                if (actionCommon["LocateValue"] != null) 
                {
                    if (actionCommon["LocateValue"].ToUpper().Contains("GLOBALDICTIONARY"))
                    {
                        act.Value = "{Var Name=" + actionCommon["LocateValue"] + "}";
                    }
                }
                else { act.Value = ""; }
                act.LocateValue = xpath;

                if (Locator == "ByXPath") { act.LocateBy = eLocateBy.ByXPath; }
                else if (Locator == "ByMulitpleProperties") { act.LocateBy = eLocateBy.ByMulitpleProperties; }

                if (actionCommon["ActionType"] == "Click") { act.Description = "Click on " + SetValueinObject; act.GenElementAction = ActGenElement.eGenElementAction.Click; }
                if (actionCommon["ActionType"] == "Enter") { act.Description = "Enter in " + SetValueinObject; act.GenElementAction = ActGenElement.eGenElementAction.SetValue; }
                if (actionCommon["ActionType"] == "Select") { act.Description = "Select " + SetValueinObject; act.GenElementAction = ActGenElement.eGenElementAction.SelectFromDropDown; }

                mBusinessFlow.AddAct(act);

                CCL.Converted = "New Action - " + act.GenElementAction + " : LocateValue=" + act.LocateValue;
                CCL.Status = ConvertedCodeLine.eStatus.ConvertedToScript;
            }
        }

        //Fetches string between FIRst and Last strings passed as parameter (Helper Function)
        public string GetStringBetween(string STR, string FirstString, string LastString = null)
        {
            string str = "";
            int Pos1 = STR.IndexOf(FirstString) + FirstString.Length;
            int Pos2;
            if (LastString != null)
            {
                Pos2 = STR.IndexOf(LastString, Pos1);
            }
            else
            {
                Pos2 = STR.Length;
            }

            if ((Pos2 - Pos1)>0)
            {
                 str = STR.Substring(Pos1, Pos2 - Pos1);
                 return str;
            }
            else
            {
                return "";
            }
        }

        //Handles the Combo box status and Lines Converted section in bottom of the dialog
        private void FilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            IEnumerable<ConvertedCodeLine> FilterdCCLs;
            ObservableList<ConvertedCodeLine> CCLs = new ObservableList<ConvertedCodeLine>();
            eFilter mFilter = (eFilter)FilterComboBox.SelectedValue;
            switch (mFilter)
            {
                case eFilter.AllLines:
                    FilterdCCLs = from x in mCCL select x;
                    break;
                case eFilter.ConvertedtoScript:
                    FilterdCCLs = from x in mCCL where x.Status == ConvertedCodeLine.eStatus.ConvertedToScript select x;
                    break;
                case eFilter.Ignored:
                    FilterdCCLs = from x in mCCL where x.Status == ConvertedCodeLine.eStatus.Ignored select x;
                    break;
                case eFilter.NotConverted:
                    FilterdCCLs = from x in mCCL where x.Status == ConvertedCodeLine.eStatus.Unknown select x;
                    break;
                default:
                    FilterdCCLs = null;
                    break;
            }

            foreach (ConvertedCodeLine CCL in FilterdCCLs)
            {
                CCLs.Add(CCL);
            }

            ResultsDataGrid.DataSourceList= CCLs;
            ResultsDataGrid.IsReadOnly = true;
            RecordsCountLabel.Content = "Records: " + CCLs.Count();
        }

        //Handle the Script Conversion Status
        private void ShowStats()
        {
            double count = (from x in mCCL where x.Status != ConvertedCodeLine.eStatus.Unknown select x).Count();
            string perc = (int)(count / mCCL.Count() * 100) + "%";
            ConvertedCountLabel.Content = "Converted Lines: " + count + " out of " + mCCL.Count() + " " + perc;
        }
        
        private void CalendarBusFunction_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Set the Business Flow name
            string flowName = CalendarBusFunction.SelectedValue.ToString();
            flowName = flowName.Replace("fBus", "");
            flowName = Regex.Replace(flowName, "([a-z](?=[A-Z])|[A-Z](?=[A-Z][a-z]))", "$1 ");

            BusinessFlowNameTextBox.Text = flowName;
            
            //Name of Bus Function selected form Combo Box
            string sSelectedBusFunction = CalendarBusFunction.SelectedValue.ToString();
            string sExcelFileName = CalendarTextBox.Text;

            //Fetch all the Variables and their values form KEEP_REFER
            UFT_ExcelProcessing Excel = new UFT_ExcelProcessing();
            Dictionary_Variables = Excel.FetchVariableValuesfromCalendar(sExcelFileName, sSelectedBusFunction, dt_BizFlow);
        }

        private void Image_Loaded(object sender, RoutedEventArgs e)
        {
            //Add info image for help

            BitmapImage b = new BitmapImage();
            b.BeginInit();
            b.UriSource = new Uri("pack://application:,,,/Ginger;component/Images/" + "HELP_UFT.png");   //new Uri(Directory.GetCurrentDirectory() + @"\Help\HELP_UFT.png"); // @"C:\\Users\\Preetigu\\Desktop\\UFT_GINGER\\help_browser.png");
            b.EndInit();
            var image = sender as Image;
            image.Source = b;
        }

        private void Image_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Process wordProcess = new Process();
            wordProcess.StartInfo.FileName = Directory.GetCurrentDirectory() + @"\Help\Import_From_ASAP.pdf";
            wordProcess.StartInfo.UseShellExecute = true;
            wordProcess.Start();
        }
        
        private void TargetApplication_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            if (mCommonFunctionConvertor.CommonFunctionMappingList.Count == 0)
            {
                Reporter.ToUser(eUserMsgKeys.NoItemToDelete);
                return;
            }
            else
            {
                Convertor.CommonFunctionMappingList.Clear();
                CommonFunctionMappingUCGrid.DataSourceList = Convertor.CommonFunctionMappingList;
            }
        }
    }
}