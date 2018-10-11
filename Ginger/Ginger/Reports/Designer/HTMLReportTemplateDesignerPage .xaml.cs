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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using Ginger.Run;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Environments;
using GingerCore.Variables;
using Amdocs.Ginger.Repository;

namespace Ginger.Reports.Designer
{
    /// <summary>
    /// Interaction logic for ReportDesignerPage.xaml
    /// </summary>
    public partial class HTMLReportTemplateDesignerPage : Page
    {
        HTMLReportTemplate mReportTemplate;
        HTMLReportPage mReportPage;

        public HTMLReportTemplateDesignerPage(HTMLReportTemplate HReportTemplate)
        {
            InitializeComponent();
            mReportTemplate = HReportTemplate;
            App.ObjFieldBinding(NameTextBox, TextBox.TextProperty, mReportTemplate, HTMLReportTemplate.Fields.Name);
            App.ObjFieldBinding(ReportHTMLTextBox, TextBox.TextProperty, mReportTemplate, HTMLReportTemplate.Fields.HTML);
            LoadReportTemplatePage();
            LoadReportInfoTreeView();
        }

        private void LoadReportTemplatePage()
        {
            mReportPage = GetSampleReportPage(mReportTemplate.HTML);
            BodyWebBrowser.NavigateToString(mReportPage.HTML);
        }

        public static HTMLReportPage GetSampleReportPage(string Xaml)
        {
            BusinessFlow BF1 = new BusinessFlow() { Name = "BF1 - Create Customer", Description = "Create any type of customer: Business/Residential..." };
            BF1.Active = true;
            BF1.RunStatus = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed;
            BF1.Activities = new ObservableList<Activity>();
            BF1.Elapsed = 2364;

            //Activity 1
            Activity a1 = new Activity() { ActivityName = "Launch Application & Login", Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed };
            BF1.Activities.Add(a1);

            ActGotoURL act1 = new ActGotoURL() { Description = "Goto URL www.abcd.com", Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed, Elapsed = 3124 };
            a1.Acts.Add(act1);

            ActReturnValue ARV1 = new ActReturnValue();
            ARV1.Param = "RC";
            ARV1.Expected = "123";
            ARV1.Actual = "123";
            ARV1.Status = ActReturnValue.eStatus.Passed;
            act1.ReturnValues.Add(ARV1);
            ActTextBox act2 = new ActTextBox() { Description = "Enter User ID", Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed, Elapsed = 230 };
            // Add sample screen shot
            Bitmap tempBmp = new Bitmap(Ginger.Properties.Resources.ScreenShot1);
            act2.ScreenShots.Add(GingerCore.General.BitmapImageToFile(tempBmp));
            
            a1.Acts.Add(act2);

            ActTextBox act3 = new ActTextBox() { Description = "Enter Password", Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed, Elapsed = 112 };
            a1.Acts.Add(act3);

            ActSubmit act4 = new ActSubmit() { Description = "Click Submit Button", Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed, Elapsed = 1282 };
            a1.Acts.Add(act4);

            //Activity 2
            Activity a2 = new Activity() { ActivityName = "Create New customer", Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed };
            BF1.Activities.Add(a2);

            ActTextBox acta21 = new ActTextBox() { Description = "Enter First Name", Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed, Elapsed = 325 };
            a2.Acts.Add(acta21);

            ActTextBox acta22 = new ActTextBox() { Description = "Enter Last Name", Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed, Elapsed = 302 };
            a2.Acts.Add(acta22);

            ActTextBox acta23 = new ActTextBox() { Description = "Enter City", Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed, Elapsed = 820, Error = "Error: Element not found by ID 'City'", ExInfo = "Cannot find element" };
            a2.Acts.Add(acta23);

            ActSubmit acta24 = new ActSubmit() { Description = "Click Create Button", Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Pending };
            a2.Acts.Add(acta24);

            //Add Variables
            BF1.Variables = new ObservableList<VariableBase>();

            VariableString v1 = new VariableString() { Name = "FirstName", Value = "David Smith" };
            BF1.Variables.Add(v1);

            VariableRandomNumber v2 = new VariableRandomNumber() { Name = "Random 1", Min = 1, Max = 100, Value = "55" };
            BF1.Variables.Add(v2);

            //Add a few simple BFs            
            BusinessFlow BF2 = new BusinessFlow() { Name = "BF2 - Customer Order Product", Description = "", Active = true };            
            BF2.Activities = new ObservableList<Activity>();
            BF2.RunStatus = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
            BF2.Elapsed = 1249;
            
            ProjEnvironment env = new ProjEnvironment() { Name = "Env1" };
            //TODO: add more env info

            //cretae dummy GR, GMR
            RunsetExecutor GMR = new RunsetExecutor();

            GingerRunner GR = new GingerRunner();
            GR.BusinessFlows.Add(BF1);
            GR.BusinessFlows.Add(BF2);
            GR.CurrentSolution = App.UserProfile.Solution;
            GMR.Runners.Add(GR);

            ReportInfo RI = new ReportInfo(env, GMR);
            HTMLReportPage RP = new HTMLReportPage(RI, Xaml);
            return RP;
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadReportTemplatePage();
        }
        
        private void LoadReportInfoTreeView()
        {
            TreeViewItem tvi0 = new TreeViewItem() { Header = "Report Data" };
            LoadReportInfoObjects(mReportPage.ReportInfo, tvi0, "");

            TreeViewItem tviNative = new TreeViewItem() { Header = "Basic Elements" };

            TreeViewItem tviN1 = new TreeViewItem() { Header = "Paragraph", DataContext = "<Paragraph>" + Environment.NewLine + "</Paragraph>" };
            tviN1.MouseDoubleClick += ReportObjectsTreeView_MouseDoubleClick;
            tviNative.Items.Add(tviN1);

            TreeViewItem tviN2 = new TreeViewItem() { Header = "Run", DataContext = "<Run FontSize=\"24\" Foreground=\"Blue\" Text=\"Enter Your Text\"/>" };
            tviN2.MouseDoubleClick += ReportObjectsTreeView_MouseDoubleClick;
            tviNative.Items.Add(tviN2);

            ReportObjectsTreeView.Items.Add(tviNative);
            ReportObjectsTreeView.Items.Add(tvi0);
        }

        private void LoadReportInfoObjects(Object obj, TreeViewItem TVI, string Path)
        {
            Type T = obj.GetType();
            PropertyInfo[] properties = T.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            
            foreach (PropertyInfo pi in properties)
            {
                // First handle generic types
                string TypeFullName = pi.PropertyType.FullName;
                //TODO: add all generic types
                if (TypeFullName == "System.Int32" || TypeFullName == "System.String" || TypeFullName == "System.Boolean" || TypeFullName == "System.TimeSpan")
                {
                    TreeViewItem tviN2 = new TreeViewItem() { Header = pi.Name, DataContext = pi.Name };
                    tviN2.MouseDoubleClick += ReportObjectsTreeView_MouseDoubleClick;
                    TVI.Items.Add(tviN2);
                    continue;
                }


                // Single?
                if (pi.PropertyType.IsGenericType && pi.PropertyType.GetGenericArguments()[0].FullName == "System.Single")
                {
                    TreeViewItem tviN2 = new TreeViewItem() { Header = pi.Name, DataContext = pi.Name };
                    tviN2.MouseDoubleClick += ReportObjectsTreeView_MouseDoubleClick;
                    TVI.Items.Add(tviN2);
                    continue;
                }

                //Check if List
                if (!pi.PropertyType.IsGenericTypeDefinition)
                {
                    // Is kind of object?
                    Object CurrObj = null;
                    try
                    {
                        CurrObj = pi.GetValue(obj);
                    }
                    catch(Exception ex)
                    {
                        Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}");
                    }
                    if (CurrObj is object)
                    {
                        Type oType = CurrObj.GetType();
                        if (oType.IsGenericType && (oType.GetGenericTypeDefinition() == typeof(List<>)))
                        {
                            TreeViewItem tviObj = new TreeViewItem() { Header = pi.Name, DataContext = pi.Name };
                            TVI.Items.Add(tviObj);

                            // Get the first item from the list for sample
                            IEnumerable listObject = (IEnumerable)CurrObj;
                            object obj1 = null;
                            foreach (object o in listObject)
                            {
                                    obj1 = o;
                                    break;
                            }
                            if (obj1 == null)
                            {
                                break;
                            }
                            LoadReportInfoObjects(obj1, tviObj, pi.Name + "[i].");
                            continue;
                        }
                    }
                }
                //else
                //TODO: err...
            }
        }

        private void ReportObjectsTreeView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Loop all the way to the root.
            
            TreeViewItem tvi=(TreeViewItem)sender;

            string path = (string)(tvi).DataContext;

            while (tvi.Parent is TreeViewItem) 
            {               
                tvi = (TreeViewItem)tvi.Parent;                                
                // path = "." + path;
                
                if ((string)(tvi).DataContext != null)
                {
                    path = "[i]." + path;
                }

                string s = (string)(tvi).DataContext;                
                path = s + path;                
            }
            
            if (path != null)
            {
                ReportHTMLTextBox.Focus();
                int selectionIndex = ReportHTMLTextBox.SelectionStart;
                ReportHTMLTextBox.Text = ReportHTMLTextBox.Text.Insert(selectionIndex, path);
                ReportHTMLTextBox.SelectionStart = selectionIndex;
                ReportHTMLTextBox.SelectionLength = path.Length;
            }
        }


        private IEnumerable<DependencyObject> GetRunsAndParagraphs(FlowDocument doc)
        {
            //TO be used for the designer - DO not delete!!!

            // use the GetNextContextPosition method to iterate through the   
            // FlowDocument   

            for (TextPointer position = doc.ContentStart;
                position != null && position.CompareTo(doc.ContentEnd) <= 0;
                position = position.GetNextContextPosition(LogicalDirection.Forward))
            {
                if (position.GetPointerContext(LogicalDirection.Forward) ==
                    TextPointerContext.ElementEnd)
                {
                    yield return position.Parent;
                }
            }
        }  
    }
}
