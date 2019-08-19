#region License
/*
Copyright © 2014-2019 European Support Limited

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

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using GingerCore.Actions;
using GingerCore;
using System.Xml;
using System.Diagnostics;
using Amdocs.Ginger.Repository;
using GingerCore.Actions.WebServices;
using Ginger.UserControls;
using System.Linq;
using Amdocs.Ginger.Common;
using amdocs.ginger.GingerCoreNET;

namespace Ginger.Actions.WebServices
{
    /// <summary>
    /// Interaction logic for ActSoapUIEditPage.xaml
    /// </summary>
    public partial class ActSoapUIEditPage : Page
    {
        public ActionEditPage actp;
        private ActSoapUI mAct;
        Context mContext;

        public static string ReportPath { get; set; }

        public ActSoapUIEditPage(GingerCore.Actions.ActSoapUI act)
        {
            InitializeComponent();
            mAct = act;
            mContext = Context.GetAsContext(act.Context);

            Bind();
            mAct.SolutionFolder =  WorkSpace.Instance.Solution.Folder.ToUpper();
        }

        public void Bind()
        {
            XMLFilePathTextBox.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(ActSoapUI.Fields.XMLFile),true, true, UCValueExpression.eBrowserType.File, "xml", new RoutedEventHandler(BrowseButtonXML_Click));

            GingerCore.GeneralLib.BindingHandler.ActInputValueBinding(DoNotImportFile, CheckBox.IsCheckedProperty, mAct.GetOrCreateInputParam(ActSoapUI.Fields.ImportFile));

            GingerCore.GeneralLib.BindingHandler.ActInputValueBinding(IgnoreReportXMLValidation, CheckBox.IsCheckedProperty, mAct.GetOrCreateInputParam(ActSoapUI.Fields.IgnoreValidation));

            GingerCore.GeneralLib.BindingHandler.ActInputValueBinding(TestSuiteComboBox, ComboBox.TextProperty, mAct.GetOrCreateInputParam(ActSoapUI.Fields.TestSuite));
            GingerCore.GeneralLib.BindingHandler.ActInputValueBinding(TestCaseComboBox, ComboBox.TextProperty, mAct.GetOrCreateInputParam(ActSoapUI.Fields.TestCase));
            GingerCore.GeneralLib.BindingHandler.ActInputValueBinding(UIrelatedCheckBox, CheckBox.IsCheckedProperty, mAct.GetOrCreateInputParam(ActSoapUI.Fields.UIrelated));
            GingerCore.GeneralLib.BindingHandler.ActInputValueBinding(TestCasePropertiesRequieredCheckBox, CheckBox.IsCheckedProperty, mAct.GetOrCreateInputParam(ActSoapUI.Fields.TestCasePropertiesRequiered));

            PropertiesOrPlaceHoldersInit();
            MergeAndClearList();
            InitPropertiesGrid(mAct.AllProperties, "Properties", "Property Type", "Property Name", "Property Value", "Property Calculated Value");

            //place holder property list
            TestSuitePlaceHolderGrid.Init(Context.GetAsContext(mAct.Context), mAct.TestSuitePlaceHolder, "PlaceHolder Properties", "PlaceHolder Name", "PlaceHolder Value", "PlaceHolder Calculated Value");

            EndPointTextBox.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(ActSoapUI.Fields.EndPoint));
            HostPortTextBox.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(ActSoapUI.Fields.HostPort));
            UsernameTextBox.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(ActSoapUI.Fields.Username));
            PasswordTextBox.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(ActSoapUI.Fields.Password));
            DomainTextBox.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(ActSoapUI.Fields.Domain));



            PasswordWSSUCComboBox.Init(mAct.GetOrCreateInputParam(ActSoapUI.Fields.PasswordWSSType), typeof(ActSoapUI.ePasswordWSSType),false, new SelectionChangedEventHandler(PasswordWSSComboBox_SelectionChanged));
            PasswordWSSUCComboBox.ComboBox.IsEditable = true;

            if (!string.IsNullOrEmpty(XMLFilePathTextBox.ValueTextBox.Text))
            {
                TestSuiteComboBox.SelectedValue = mAct.GetInputParamCalculatedValue(ActSoapUI.Fields.TestSuite);
                TestCaseComboBox.SelectedValue = mAct.GetInputParamCalculatedValue(ActSoapUI.Fields.TestCase);
            }

            SystemPropertiesVEGrid.Init(Context.GetAsContext(mAct.Context), mAct.SystemProperties,"System Properties","Property Name","Property Value","Property Calculated Value");
            GlobalPropertiesVEGrid.Init(Context.GetAsContext(mAct.Context), mAct.GlobalProperties, "Global Properties", "Property Name", "Property Value", "Property Calculated Value");
            ExpendPopulatedExpenders();

            GingerCore.GeneralLib.BindingHandler.ActInputValueBinding(AddXMLTagsToOutput, CheckBox.IsCheckedProperty, mAct.GetOrCreateInputParam(ActSoapUI.Fields.AddXMLResponse));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(OpenExecutionDirectoryButton, Button.IsEnabledProperty, mAct, ActSoapUI.Fields.isActionExecuted, BindingMode.OneWay);
            ProjectPropertiesGrid.VEGrid.AddToolbarTool("@Reset_16x16.png", "Reset Properties to default", new RoutedEventHandler(ResetProjectButton_Click));
            
        }

        private void MergeAndClearList()
        {
            if (mAct.ProjectInnerProperties.Count > 0)
            {
                foreach (var innerProperty in mAct.ProjectInnerProperties)
                {
                    var item = mAct.ProjectProperties.Where(l => l.Param == innerProperty.Param).FirstOrDefault();
                    if (item == null)
                        mAct.ProjectProperties.Add(innerProperty);
                }
                mAct.ProjectInnerProperties.Clear();
            }
            if (mAct.ProjectProperties.Count > 0)
            {
                foreach (var item in mAct.ProjectProperties)
                {
                    ActSoapUiInputValue actUiInput = new ActSoapUiInputValue(ActSoapUiInputValue.ePropertyType.Project, item);
                    mAct.AllProperties.Add(actUiInput);
                }
                mAct.ProjectProperties.Clear();
            }
            if (mAct.TestSuiteProperties.Count > 0)
            {
                foreach (var item in mAct.TestSuiteProperties)
                {
                    ActSoapUiInputValue actUiInput = new ActSoapUiInputValue(ActSoapUiInputValue.ePropertyType.TestSuite, item);
                    mAct.AllProperties.Add(actUiInput);
                }
                mAct.TestSuiteProperties.Clear();
            }
            if (mAct.TestCaseProperties.Count > 0)
            {
                foreach (var item in mAct.TestCaseProperties)
                {
                    ActSoapUiInputValue actUiInput = new ActSoapUiInputValue(ActSoapUiInputValue.ePropertyType.TestCase, item);
                    mAct.AllProperties.Add(actUiInput);
                }
                mAct.TestCaseProperties.Clear();
            }
            if (mAct.TestStepProperties.Count > 0)
            {
                foreach (var item in mAct.TestStepProperties)
                {
                    ActSoapUiInputValue actUiInput = new ActSoapUiInputValue(ActSoapUiInputValue.ePropertyType.TestStep, item);
                    mAct.AllProperties.Add(actUiInput);
                }
                mAct.TestStepProperties.Clear();
            }
            if (!string.IsNullOrEmpty(mAct.GetInputParamCalculatedValue(ActSoapUI.Fields.TestCase)) && !string.IsNullOrEmpty(mAct.GetInputParamCalculatedValue(ActSoapUI.Fields.TestSuite)))
            {
                TestSuiteComboBox.SelectedValue = mAct.GetInputParamCalculatedValue(ActSoapUI.Fields.TestSuite);
                TestCaseComboBox.SelectedValue = mAct.GetInputParamCalculatedValue(ActSoapUI.Fields.TestCase);
                PopulateTestCasePropertiesList();
            }
        }
        public void InitPropertiesGrid(Amdocs.Ginger.Common.ObservableList<ActSoapUiInputValue> datasource, string gridTitle = "Input Values", string type = "Parameter Type", string paramTitle = "Parameter Name", string valueTitle = "Parameter Value", string valueForDriverTitle = "Calculated Parameter Value")
        {
            ProjectPropertiesGrid.VEGrid.Title = gridTitle;
            ProjectPropertiesGrid.VEGrid.SetTitleStyle((Style)TryFindResource("@ucGridTitleLightStyle"));
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new Amdocs.Ginger.Common.ObservableList<GridColView>();
            view.GridColsView.Add(new GridColView() { Field = nameof(ActSoapUiInputValue.Type), Header = type, WidthWeight = 100 });
            view.GridColsView.Add(new GridColView() { Field = nameof(ActInputValue.Param), Header = paramTitle, WidthWeight = 100 });
            view.GridColsView.Add(new GridColView() { Field = nameof(ActInputValue.Value), Header = valueTitle, WidthWeight = 100 });
            view.GridColsView.Add(new GridColView() { Field = "...", WidthWeight = 30, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)ProjectPropertiesGrid.controlGrid.Resources["VEGridValueExpressionButton"] });
            view.GridColsView.Add(new GridColView() { Field = nameof(ActInputValue.ValueForDriver), Header = valueForDriverTitle, WidthWeight = 100 });

            ProjectPropertiesGrid.VEGrid.SetAllColumnsDefaultView(view);
            ProjectPropertiesGrid.VEGrid.InitViewItems();
            ProjectPropertiesGrid.VEGrid.DataSourceList = datasource;

            ProjectPropertiesGrid.VEGrid.ShowRefresh = Visibility.Collapsed;
            ProjectPropertiesGrid.VEGrid.ShowUpDown = Visibility.Collapsed;
            ProjectPropertiesGrid.VEGrid.ShowEdit = Visibility.Collapsed;
            ProjectPropertiesGrid.VEGrid.ShowAdd = Visibility.Collapsed;
            ProjectPropertiesGrid.VEGrid.ShowDelete = Visibility.Collapsed;
            ProjectPropertiesGrid.VEGrid.ShowClearAll = Visibility.Collapsed;
            ProjectPropertiesGrid.VEGrid.ShowTagsFilter = Visibility.Collapsed;
        }

        private void RefreshAllPropertiesGridButton_Click(object sender, RoutedEventArgs e)
        {
            RefreshAllPropertiesGrid();
        }

        private void RefreshAllPropertiesGrid()
        {
            ProcessInputForDriver();

            PopulateProjectPropertiesList();
            PopulateTestCasePropertiesList();
            PopulateTestSuitePropertiesList();
            PopulateTestStepPropertiesList();
        }

        private void ResetProjectButton_Click(object sender, RoutedEventArgs e)
        {
            ProcessInputForDriver();

            mAct.AllProperties.Clear();
            mAct.TempProperties.Clear();
            RefreshAllPropertiesGrid();
        }

        public void PropertiesOrPlaceHoldersInit()
        {
            string currentValue = mAct.GetInputParamValue(ActSoapUI.Fields.PropertiesOrPlaceHolders);

            if (currentValue == null)
                return;

            if (currentValue == "PlaceHolders")
            {
                // In older version we were storing placeHolders in TestCaseProperties itself and now moved it to new TestSuitePlaceHolder list
                //  for backward compatibility  here we moving older placeholders items into TestSuitePlaceHolder

                foreach (var item in mAct.TestCaseProperties)
                {
                    mAct.TestSuitePlaceHolder.Add(item);
                }

                mAct.TestCaseProperties.Clear();
                mAct.RemoveInputParam(ActSoapUI.Fields.PropertiesOrPlaceHolders);
                PopulateTestCasePropertiesList();
            }
            else if (currentValue == "Properties")
            {
                mAct.RemoveInputParam(ActSoapUI.Fields.PropertiesOrPlaceHolders);
            }

        }



        private void ExpendPopulatedExpenders()
        {
            if (!string.IsNullOrEmpty(TestCaseComboBox.Text) || !string.IsNullOrEmpty(TestSuiteComboBox.Text) || (bool)UIrelatedCheckBox.IsChecked)
            {
                BasicExpander.IsExpanded = true;
                SuitePropertiesExpander.IsExpanded = true;
            }

            if (!string.IsNullOrEmpty(EndPointTextBox.ValueTextBox.Text) || !string.IsNullOrEmpty(HostPortTextBox.ValueTextBox.Text) || !string.IsNullOrEmpty(UsernameTextBox.ValueTextBox.Text) || !string.IsNullOrEmpty(PasswordTextBox.ValueTextBox.Text) || !string.IsNullOrEmpty(DomainTextBox.ValueTextBox.Text) || !PasswordWSSUCComboBox.ComboBox.Text.Equals(""))
            {
                OverridesExpander.IsExpanded = true;
            }

            if (SystemPropertiesVEGrid.DataSource.Count != 0 || GlobalPropertiesVEGrid.DataSource.Count != 0 )
            {
                PropertiesExpander.IsExpanded = true;
            }
        }

        private void BrowseButtonXML_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(XMLFilePathTextBox.ValueTextBox.Text) || XMLFilePathTextBox.ValueTextBox.Text.Substring(0, 1) == "~")
            {
                return;
            }

            ProcessInputForDriver();

            if (!Boolean.Parse((mAct.GetInputParamCalculatedValue(ActSoapUI.Fields.ImportFile))))
            {
                mAct.TempProperties.ClearAll();
                mAct.AllProperties.ClearAll();
                RefreshAllPropertiesGrid();
                FillSuiteComboBox();
                return;
            }

            string SolutionFolder =  WorkSpace.Instance.Solution.Folder;
            string targetPath = System.IO.Path.Combine(SolutionFolder, @"Documents\WebServices\SoapUI\ProjectXMLs");
            if (!System.IO.Directory.Exists(targetPath))
            {
                System.IO.Directory.CreateDirectory(targetPath);
            }

            string fileName = System.IO.Path.GetFileName(XMLFilePathTextBox.ValueTextBox.Text);
            string destFile = System.IO.Path.Combine(targetPath, fileName);

            int fileNum = 1;
            string copySufix = "_Copy";
            while (System.IO.File.Exists(destFile))
            {
                fileNum++;
                string newFileName = System.IO.Path.GetFileNameWithoutExtension(destFile);
                if (newFileName.IndexOf(copySufix) != -1)
                    newFileName = newFileName.Substring(0, newFileName.IndexOf(copySufix));
                newFileName = newFileName + copySufix + fileNum.ToString() + System.IO.Path.GetExtension(destFile);
                destFile = System.IO.Path.Combine(targetPath, newFileName);
            }

            System.IO.File.Copy(mAct.GetInputParamCalculatedValue(ActSoapUI.Fields.XMLFile), destFile, true);

            XMLFilePathTextBox.ValueTextBox.Text = @"~\Documents\WebServices\SoapUI\ProjectXMLs\" + System.IO.Path.GetFileName(destFile);

            FillSuiteComboBox();
        }

        private void FillSuiteComboBox()
        {
            TestSuiteComboBox.Items.Clear();
            TestSuiteComboBox.Items.Add(string.Empty);
            XmlDocument doc = new XmlDocument();

            ProcessInputForDriver();

            string XMLFiledValue = mAct.GetInputParamCalculatedValue(ActSoapUI.Fields.XMLFile);

            if (!XMLFiledValue.Equals(string.Empty))
            {
                if (XMLFiledValue.ToUpper().Substring(XMLFiledValue.Length - 4).Equals(".XML"))
                {
                    if (XMLFiledValue.Substring(0, 1).Equals("~"))
                    {
                        string SolutionFolder =  WorkSpace.Instance.Solution.Folder;
                        XMLFiledValue = System.IO.Path.Combine(SolutionFolder, XMLFiledValue.Substring(2));
                    }
                    if (!System.IO.File.Exists(XMLFiledValue))
                    {
                        Reporter.ToUser(eUserMsgKey.FileNotExist);
                        return;
                    }
                    doc.Load(XMLFiledValue);
                    XmlNamespaceManager manager = XMLDocExtended.GetAllNamespaces(doc);
                    XmlNodeList testSuits = doc.DocumentElement.SelectNodes("(//*[local-name()='soapui-project']/*[local-name()='testSuite'])", manager);
                    foreach (XmlNode testSuit in testSuits)
                    {
                        string testSuitName = testSuit.Attributes["name"].Value;
                        TestSuiteComboBox.Items.Add(testSuitName);
                    }
                }
            }
        }

        private void FillCaseComboBox()
        {
            TestCaseComboBox.SelectionChanged -= TestCaseComboBox_SelectionChanged;

            TestCaseComboBox.Items.Clear();
            mAct.TestCaseProperties.Clear();

            TestCaseComboBox.Items.Add(string.Empty);
            TestCaseComboBox.SelectedIndex = 0;

            XmlDocument doc = new XmlDocument();

            ProcessInputForDriver();

            string XMLFiledValue = mAct.GetInputParamCalculatedValue(ActSoapUI.Fields.XMLFile);

            if (XMLFiledValue.ToUpper().Substring(XMLFiledValue.Length - 4).Equals(".XML"))
            {
                if (XMLFiledValue.Substring(0, 1).Equals("~"))
                {
                    string SolutionFolder =  WorkSpace.Instance.Solution.Folder;
                    XMLFiledValue = System.IO.Path.Combine(SolutionFolder, XMLFiledValue.Substring(2));
                }
                if (!System.IO.File.Exists(XMLFiledValue))
                {
                    Reporter.ToUser(eUserMsgKey.FileNotExist);
                    return;
                }
                doc.Load(XMLFiledValue);
                XmlNamespaceManager manager = XMLDocExtended.GetAllNamespaces(doc); 
                int SuiteSelectedItemID = TestSuiteComboBox.SelectedIndex;
                XmlNodeList testCases;
                if (SuiteSelectedItemID > 0)
                {
                    testCases = doc.DocumentElement.SelectNodes("(//*[local-name()='soapui-project']/*[local-name()='testSuite'][" + SuiteSelectedItemID + "]/*[local-name()='testCase'])", manager);
                    foreach (XmlNode testCase in testCases)
                    {
                        string testCaseName = testCase.Attributes["name"].Value;
                        TestCaseComboBox.Items.Add(testCaseName);
                    }
                }
            }

            TestCaseComboBox.SelectionChanged += TestCaseComboBox_SelectionChanged;
        }

        public void RefreshComboBoxButton(object sender, RoutedEventArgs e)
        {
            FillSuiteComboBox();

        }

        private void TestSuiteVEButton_Click(object sender, RoutedEventArgs e)
        {
            ValueExpressionEditorPage w = new ValueExpressionEditorPage(mAct, ActSoapUI.Fields.TestSuite, Context.GetAsContext(mAct.Context));
            w.ShowAsWindow(eWindowShowStyle.Dialog);
            TestSuiteComboBox.Text = w.ValueUCTextEditor.textEditor.Text;
        }

        private void TestCaseVEButton_Click(object sender, RoutedEventArgs e)
        {
            ValueExpressionEditorPage w = new ValueExpressionEditorPage(mAct, ActSoapUI.Fields.TestCase, Context.GetAsContext(mAct.Context));
            w.ShowAsWindow(eWindowShowStyle.Dialog);
            TestCaseComboBox.Text = w.ValueUCTextEditor.textEditor.Text;
        }


        private void PasswordWSSComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ProcessInputForDriver();
        }

        private void TestSuiteComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ProcessInputForDriver();

            FillCaseComboBox();
            RefreshAllPropertiesGrid();
        }

        private void TestCaseComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ProcessInputForDriver();

            if (!(TestCaseComboBox.SelectedItem == null) && TestCasePropertiesRequieredCheckBox.IsChecked == true)
            {
                PopulateProjectPropertiesList();
                PopulateTestSuitePropertiesList();
                PopulateTestCasePropertiesList();
                PopulateTestStepPropertiesList();
            }
        }

        private void HideGridIfPropetiesCountIsZero()
        {
            if (mAct.AllProperties.Count == 0)
            {
                SuitePropertiesExpander.Visibility = Visibility.Collapsed;
            }
            else
            {
                SuitePropertiesExpander.Visibility = Visibility.Visible;
            }
        }
        private void ExecutionDirectoryButton_Click(object sender, RoutedEventArgs e)
        {
            ReportPath = mAct.LastExecutionFolderPath;
            Process.Start(ReportPath);
        }

        private void TextCasePropertiesRequieredTextBoxChecked(object sender, RoutedEventArgs e)
        {
            SuitePropertiesExpander.Visibility = Visibility.Visible;
            SuitePlaceHoldderExpander.Visibility = Visibility.Visible;

            ProcessInputForDriver();

            if (!string.IsNullOrEmpty(XMLFilePathTextBox.ValueTextBox.Text) && TestCaseComboBox.SelectedValue != null)
            {
                if (mAct.AllProperties.Count == 0)
                {
                    PopulateProjectPropertiesList();
                    PopulateTestSuitePropertiesList();
                    PopulateTestCasePropertiesList();
                    PopulateTestStepPropertiesList();
                }
            }
            HideGridIfPropetiesCountIsZero();
        }

        private void ProcessInputForDriver()
        {
            if (mContext != null)
            {
                mContext.Runner.ProcessInputValueForDriver(mAct);
            }
        }

        private void TextCasePropertiesRequieredTextBoxUnChecked(object sender, RoutedEventArgs e)
        {
            SuitePropertiesExpander.Visibility = Visibility.Collapsed;
            SuitePlaceHoldderExpander.Visibility = Visibility.Collapsed;
        }

        private void PopulateProjectPropertiesList()
        {
            ClearPropertyFromList(ActSoapUiInputValue.ePropertyType.Project);
            XmlDocument doc = new XmlDocument();
            string XMLFiledValue = mAct.GetInputParamCalculatedValue(ActSoapUI.Fields.XMLFile);

            if (!XMLFiledValue.Equals(string.Empty))
            {
                {
                    if (XMLFiledValue.Substring(0, 1).Equals("~"))
                    {
                        string SolutionFolder = mAct.SolutionFolder;
                        XMLFiledValue = System.IO.Path.Combine(SolutionFolder, XMLFiledValue.Substring(2));
                    }
                    if (!System.IO.File.Exists(XMLFiledValue))
                    {
                        Reporter.ToUser(eUserMsgKey.FileNotExist);
                        return;
                    }
                    doc.Load(XMLFiledValue);
                    XmlNamespaceManager manager = XMLDocExtended.GetAllNamespaces(doc);

                    XmlNodeList properties = doc.SelectNodes("//*[local-name()='soapui-project']/*[local-name()='properties']/*[local-name()='property']", manager);

                    PopulatePropertiesGrid(properties, ActSoapUiInputValue.ePropertyType.Project);
                }
            }
        }

        public void PopulateTestCasePropertiesList()
        {
            ClearPropertyFromList(ActSoapUiInputValue.ePropertyType.TestCase);

            if (TestSuiteComboBox.SelectedValue == null)
                return;
            string testSuite = TestSuiteComboBox.SelectedValue.ToString();

            string testCase = TestCaseComboBox.SelectedValue.ToString();
            if (string.IsNullOrEmpty(testCase))
                return;

            XmlDocument doc = new XmlDocument();
            string XMLFiledValue = mAct.GetInputParamCalculatedValue(ActSoapUI.Fields.XMLFile);

            if (!XMLFiledValue.Equals(string.Empty))
            {
                {
                    if (XMLFiledValue.Substring(0, 1).Equals("~"))
                    {
                        string SolutionFolder = mAct.SolutionFolder;
                        XMLFiledValue = System.IO.Path.Combine(SolutionFolder, XMLFiledValue.Substring(2));
                    }
                    if (!System.IO.File.Exists(XMLFiledValue))
                    {
                        Reporter.ToUser(eUserMsgKey.FileNotExist);
                        return;
                    }
                    doc.Load(XMLFiledValue);
                    XmlNamespaceManager manager = XMLDocExtended.GetAllNamespaces(doc);

                    XmlNodeList properties = doc.SelectNodes("//*[local-name()='soapui-project']/*[local-name()='testSuite'][@name='" + testSuite + "']/*[local-name()='testCase'][@name='" + testCase + "']/*[local-name()='properties']/*[local-name()='property']", manager);

                    PopulatePropertiesGrid(properties, ActSoapUiInputValue.ePropertyType.TestCase);
                }
            }
        }

        public void PopulateTestSuitePropertiesList()
        {
            ClearPropertyFromList(ActSoapUiInputValue.ePropertyType.TestSuite);

            if (TestSuiteComboBox.SelectedValue == null)
                return;
            string testSuite = TestSuiteComboBox.SelectedValue.ToString();

            XmlDocument doc = new XmlDocument();
            string XMLFiledValue = mAct.GetInputParamCalculatedValue(ActSoapUI.Fields.XMLFile);

            if (!XMLFiledValue.Equals(string.Empty))
            {

                if (XMLFiledValue.Substring(0, 1).Equals("~"))
                {
                    string SolutionFolder = mAct.SolutionFolder;
                    XMLFiledValue = System.IO.Path.Combine(SolutionFolder, XMLFiledValue.Substring(2));
                }
                if (!System.IO.File.Exists(XMLFiledValue))
                {
                    Reporter.ToUser(eUserMsgKey.FileNotExist);
                    return;
                }
                doc.Load(XMLFiledValue);
                XmlNamespaceManager manager = XMLDocExtended.GetAllNamespaces(doc);
                XmlNodeList properties = doc.SelectNodes("//*[local-name()='soapui-project']/*[local-name()='testSuite'][@name='" + testSuite + "']/*[local-name()='properties']/*[local-name()='property']", manager);

                PopulatePropertiesGrid(properties, ActSoapUiInputValue.ePropertyType.TestSuite);

            }
        }

        public void PopulateTestStepPropertiesList()
        {
            ClearPropertyFromList(ActSoapUiInputValue.ePropertyType.TestStep);

            if (TestSuiteComboBox.SelectedValue == null)
                return;
            string testSuite = TestSuiteComboBox.SelectedValue.ToString();
            string testCase = TestCaseComboBox.SelectedValue.ToString();
            if (string.IsNullOrEmpty(testCase))
                return;

            XmlDocument doc = new XmlDocument();
            string XMLFiledValue = mAct.GetInputParamCalculatedValue(ActSoapUI.Fields.XMLFile);

            if (!XMLFiledValue.Equals(string.Empty))
            {

                if (XMLFiledValue.Substring(0, 1).Equals("~"))
                {
                    string SolutionFolder = mAct.SolutionFolder;
                    XMLFiledValue = System.IO.Path.Combine(SolutionFolder, XMLFiledValue.Substring(2));
                }
                if (!System.IO.File.Exists(XMLFiledValue))
                {
                    Reporter.ToUser(eUserMsgKey.FileNotExist);
                    return;
                }
                doc.Load(XMLFiledValue);
                XmlNamespaceManager manager = XMLDocExtended.GetAllNamespaces(doc); 
                XmlNodeList properties = doc.SelectNodes("//*[local-name()='soapui-project']/*[local-name()='testSuite'][@name='" + testSuite + "']/*[local-name()='testCase'][@name='" + testCase + "']/*[local-name()='testStep'][@type='properties']/*[local-name()='config']/*[local-name()='properties']/*[local-name()='property']", manager);

                PopulatePropertiesGrid(properties, ActSoapUiInputValue.ePropertyType.TestStep);

            }
        }

        private void PopulatePropertiesGrid(XmlNodeList properties, ActSoapUiInputValue.ePropertyType propertiesType)
        {
            foreach (XmlNode property in properties)
            {
                ActSoapUiInputValue testProperty = new ActSoapUiInputValue();
                XmlNodeList propertyTags = property.ChildNodes;
                foreach (XmlNode tag in propertyTags)
                {

                    if (tag.Name == "con:name")
                    {
                        testProperty.Param = tag.InnerText;
                    }
                    if (tag.Name == "con:value")
                    {
                        testProperty.Value = tag.InnerText;
                    }

                }
                //update Property type Field
                testProperty.Type = propertiesType.ToString();

                if (mAct.TempProperties.Count > 0)
                {
                    var savedProperty = mAct.TempProperties.Where(x => x.Param == testProperty.Param && x.Type == testProperty.Type.ToString()).FirstOrDefault();
                    if (savedProperty != null)
                    {
                        var savedPropertyValue = savedProperty.Value;
                        if (!string.IsNullOrEmpty(savedPropertyValue))
                            testProperty.Value = savedPropertyValue;
                    }
                }

                mAct.AllProperties.Add(testProperty);
            }
        }

        private void ClearPropertyFromList(ActSoapUiInputValue.ePropertyType propertyType)
        {
            if (mAct.AllProperties.Any(x => x.Type.ToString() == propertyType.ToString()))
            {
                foreach (var item in mAct.AllProperties)
                {
                    if (mAct.TempProperties.Count() > 0)
                    {
                        var result = mAct.TempProperties.Where(x => x.Type.ToString() == item.Type.ToString() && x.Param == item.Param && x.Value == item.Value).FirstOrDefault();
                        if (result == null)
                        {
                            var clearItem = mAct.TempProperties.Where(x => x.Type.ToString() == item.Type.ToString() && x.Param == item.Param).FirstOrDefault();
                            mAct.TempProperties.Remove(clearItem);
                            mAct.TempProperties.Add(item);
                        }
                    }
                    else
                        mAct.TempProperties.Add(item);
                }
                mAct.AllProperties.Where(l => l.Type == propertyType.ToString()).ToList().All(i => mAct.AllProperties.Remove(i));
            }

        }
    }
}