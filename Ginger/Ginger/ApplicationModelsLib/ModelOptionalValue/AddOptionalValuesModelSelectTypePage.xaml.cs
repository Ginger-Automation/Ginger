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
using Amdocs.Ginger.Repository;
using Ginger.UserControls;
using GingerWPF.BindingLib;
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using static Ginger.ApplicationModelsLib.ModelOptionalValue.AddModelOptionalValuesWizard;
using System.Collections.Specialized;
using System.Data;
using GingerCore;
using System.Linq;
using static GingerCore.Environments.Database;
using Amdocs.Ginger.Common.Repository.ApplicationModelLib;
using Amdocs.Ginger.UserControls;
using Amdocs.Ginger.Common.Enums;
using static Ginger.ExtensionMethods;
using System.Diagnostics;
using System.Reflection;
using Amdocs.Ginger.ValidationRules;
using System.IO;

namespace Ginger.ApplicationModelsLib.ModelOptionalValue
{
    /// <summary>
    /// Interaction logic for AddOptionalValuesModelSelectTypePage.xaml
    /// </summary>
    public partial class AddOptionalValuesModelSelectTypePage : Page, IWizardPage
    {
        public AddModelOptionalValuesWizard mAddModelOptionalValuesWizard;
        eOptionalValuesTargetType mOptionalValuesTargetType;

        public string FilePath { get; set; }
        public string SheetName { get; set; }

        public AddOptionalValuesModelSelectTypePage(eOptionalValuesTargetType OptionalValuesTargetType)
        {
            InitializeComponent();
            mOptionalValuesTargetType = OptionalValuesTargetType;
            switch (mOptionalValuesTargetType)
            {
                case eOptionalValuesTargetType.ModelLocalParams:
                    ControlsBinding.FillComboFromEnumType(xSourceTypeComboBox, typeof(eSourceType), null);
                    break;
                case eOptionalValuesTargetType.GlobalParams:
                    ControlsBinding.FillComboFromEnumType(xSourceTypeComboBox, typeof(eSourceType), new List<object>() { eSourceType.Excel, eSourceType.DB });
                    break;
            }
            
            xSourceTypeComboBox.Style = this.FindResource("$FlatInputComboBoxStyle") as Style;
            xSheetNameComboBox.Style = this.FindResource("$FlatInputComboBoxStyle") as Style;
            SetFieldsGrid(); //XML & JSON
            SetDefaultPresentation();
        }
        
        private void SetFieldsGrid()
        {
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();
            view.GridColsView.Add(new GridColView() { Field = nameof(TemplateFile.FilePath), Header = "File Path" });
            xImportOptionalValuesGrid.SetAllColumnsDefaultView(view);
            xImportOptionalValuesGrid.InitViewItems();
            xImportOptionalValuesGrid.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddOptioanlValueFile));
            xImportOptionalValuesGrid.btnClearAll.AddHandler(Button.ClickEvent, new RoutedEventHandler(ClearAllOptionalValuesFiles));
            xImportOptionalValuesGrid.btnDelete.AddHandler(Button.ClickEvent, new RoutedEventHandler(DeleteOptionalValueFile));
            
            xImportOptionalValuesGrid.SetTitleLightStyle = true;
            xImportOptionalValuesGrid.Title = "Sample Request Files";
            xImportOptionalValuesGrid.ShowRefresh = Visibility.Collapsed ;
            xImportOptionalValuesGrid.ShowEdit = Visibility.Collapsed;
            xImportOptionalValuesGrid.ShowAdd = Visibility.Visible;
            xImportOptionalValuesGrid.ShowDelete = Visibility.Visible;
            xImportOptionalValuesGrid.ShowClearAll = Visibility.Visible;
            xImportOptionalValuesGrid.ShowUndo = Visibility.Collapsed;
            xImportOptionalValuesGrid.EnableTagsPanel = false;
            xImportOptionalValuesGrid.ShowUpDown = Visibility.Collapsed;
        }
        private void SetDefaultPresentation()
        {
            xSourceTypeComboBox.Text = eSourceType.Excel.ToString();
            ShowRelevantPanel(eSourceType.Excel.ToString());
        }
        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            if (WizardEventArgs.EventType == EventType.Init)
            {
                mAddModelOptionalValuesWizard = (AddModelOptionalValuesWizard)WizardEventArgs.Wizard;
                xPathTextBox.BindControl(this, nameof(FilePath));
                xPathTextBox.AddValidationRule(new EmptyValidationRule());

                xSheetNameComboBox.BindControl(this, nameof(SheetName));
                xSheetNameComboBox.AddValidationRule(new EmptyValidationRule());
                xPathTextBox.Focus();
            }
            else if (WizardEventArgs.EventType == EventType.LeavingForNextPage)
            {
                if (xSourceTypeComboBox.SelectedValue.ToString() == eSourceType.Excel.ToString())
                {
                    LoadFile();
                    mAddModelOptionalValuesWizard.SourceType = eSourceType.Excel;
                }
                else if (xSourceTypeComboBox.SelectedValue.ToString() == eSourceType.XML.ToString())
                {
                    mAddModelOptionalValuesWizard.SourceType = eSourceType.XML;
                }
                else if (xSourceTypeComboBox.SelectedValue.ToString() == eSourceType.Json.ToString())
                {
                    mAddModelOptionalValuesWizard.SourceType = eSourceType.Json;
                }
                else if (xSourceTypeComboBox.SelectedValue.ToString() == eSourceType.DB.ToString())
                {
                    LoadFile();
                    mAddModelOptionalValuesWizard.SourceType = eSourceType.DB;
                } 
            }
            else if (WizardEventArgs.EventType == EventType.Cancel)
            {
                if (mAddModelOptionalValuesWizard.ImportOptionalValues.ParameterType == ImportOptionalValuesForParameters.eParameterType.Local)
                {
                    if(mAddModelOptionalValuesWizard.mAAMB is ApplicationAPIModel)
                        ((ApplicationAPIModel)mAddModelOptionalValuesWizard.mAAMB).OptionalValuesTemplates.Clear();//XML & JSON
                    mAddModelOptionalValuesWizard.ParameterValuesByNameDic.Clear();//Excel & DB
                } 
                else if(mAddModelOptionalValuesWizard.ImportOptionalValues.ParameterType == ImportOptionalValuesForParameters.eParameterType.Global)
                {
                    mAddModelOptionalValuesWizard.ParameterValuesByNameDic.Clear();//Excel & DB
                }
            }
        }
        private void xSourceTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ShowRelevantPanel(xSourceTypeComboBox.SelectedValue.ToString());
        }
        private void BrowseFiles(bool MultiSelect)
        {
            string oldPathFile = string.Empty;
            System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog();
            System.Windows.Forms.DialogResult result;
            dlg.Multiselect = MultiSelect;
            if (xSourceTypeComboBox.SelectedValue.ToString() == eSourceType.Excel.ToString())
            {
                dlg.Filter = "Excel Files|*.xls;*.xlsx;*.xlsm";
            }
            else if (xSourceTypeComboBox.SelectedValue.ToString() == eSourceType.XML.ToString())
            {
                dlg.Filter = "XML Files (*.xml)|*.xml|Text files (*.txt)|*.txt";
            }
            else if (xSourceTypeComboBox.SelectedValue.ToString() == eSourceType.Json.ToString())
            {
                dlg.Filter = "Json files (*.json)|*.json|Text files (*.txt)|*.txt";
            }

            if (!string.IsNullOrEmpty(xPathTextBox.Text))
            {
                oldPathFile = xPathTextBox.Text;
                xPathTextBox.Text = string.Empty;
            }
            result = dlg.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    if ((xSourceTypeComboBox.SelectedValue.ToString() == eSourceType.Excel.ToString()))
                    {
                        xPathTextBox.Text = dlg.FileName;
                        FillSheetCombo();
                        xExcelDataGridDockPanel.Visibility = Visibility.Collapsed;
                        // AddModelOptionalValuesWizard.FinishEnabled = false;
                        xExcelViewDataButton.Visibility = Visibility.Collapsed;
                        xExcelViewWhereButton.Visibility = Visibility.Collapsed;
                        xExcelGridSplitter.Visibility = Visibility.Collapsed;
                        if (xSheetNameComboBox.Items.Count >= 1)
                        {
                            xSheetNameComboBox.SelectedIndex = 0;
                        }                        
                    }
                    else
                    {
                        foreach (String file in dlg.FileNames)
                        {
                            mAddModelOptionalValuesWizard.OVFList.Add(new TemplateFile() { FilePath = file });
                            if (mAddModelOptionalValuesWizard.mAAMB is ApplicationAPIModel)
                                ((ApplicationAPIModel)mAddModelOptionalValuesWizard.mAAMB).OptionalValuesTemplates.Add(new TemplateFile() { FilePath = file });
                        }
                        xImportOptionalValuesGrid.DataSourceList = mAddModelOptionalValuesWizard.OVFList;
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}");
                }
            }
            else
            {
                xPathTextBox.Text = oldPathFile;
            }
        }
        private void LoadFile()
        {
            mAddModelOptionalValuesWizard.ProcessStarted();
            if (xSourceTypeComboBox.SelectedValue.ToString() == eSourceType.Excel.ToString())
            {
                mAddModelOptionalValuesWizard.ParameterValuesByNameDic = mAddModelOptionalValuesWizard.ImportOptionalValues.UpdateParametersOptionalValuesFromCurrentExcelTable();
                //AddModelOptionalValuesWizard.NextEnabled = true;
            }
            else if (xSourceTypeComboBox.SelectedValue.ToString() == eSourceType.DB.ToString())
            {
                mAddModelOptionalValuesWizard.ImportOptionalValues.ExecuteFreeSQL(xSQLTextBox.Text.Trim());
                mAddModelOptionalValuesWizard.ParameterValuesByNameDic = mAddModelOptionalValuesWizard.ImportOptionalValues.UpdateParametersOptionalValuesFromDB();
            }
            mAddModelOptionalValuesWizard.ProcessEnded();
        }
        private void ShowRelevantPanel(string FileType)
        {
            if (FileType == eSourceType.Excel.ToString())
            {
                xExcelFileStackPanel.Visibility = Visibility.Visible;
                xExcelDataGridDockPanel.Visibility = Visibility.Collapsed;
                xExcelViewDataButton.Visibility = Visibility.Collapsed;
                xExcelViewWhereButton.Visibility = Visibility.Collapsed;
                xExcelGridSplitter.Visibility = Visibility.Collapsed;
                xSheetNameComboBox.Visibility = Visibility.Visible;
                xSheetLable.Visibility = Visibility.Visible;
                xDBStackPanel.Visibility = Visibility.Collapsed;
                xImportOptionalValuesGrid.Visibility = Visibility.Collapsed;

                xPathTextBox.BindControl(this, nameof(Path));
                xPathTextBox.RemoveValidations(TextBox.TextProperty);
                xPathTextBox.AddValidationRule(new EmptyValidationRule());

                xSheetNameComboBox.BindControl(this, nameof(SheetName));
                xSheetNameComboBox.RemoveValidations(ComboBox.TextProperty);
                xSheetNameComboBox.AddValidationRule(new EmptyValidationRule());
                xPathTextBox.Focus();
            }
            else if (FileType == eSourceType.XML.ToString() || FileType == eSourceType.Json.ToString())
            {
                xPathTextBox.RemoveValidations(TextBox.TextProperty);
                xSheetNameComboBox.RemoveValidations(ComboBox.TextProperty);

                xExcelFileStackPanel.Visibility = Visibility.Collapsed;
                xExcelDataGridDockPanel.Visibility = Visibility.Collapsed;
                xExcelViewDataButton.Visibility = Visibility.Collapsed;
                xExcelViewWhereButton.Visibility = Visibility.Collapsed;
                xExcelGridSplitter.Visibility = Visibility.Collapsed;
                xSheetNameComboBox.Visibility = Visibility.Collapsed;
                xSheetLable.Visibility = Visibility.Collapsed;
                xDBStackPanel.Visibility = Visibility.Collapsed;
                xImportOptionalValuesGrid.Visibility = Visibility.Visible;
                xImportOptionalValuesGrid.DataSourceList = mAddModelOptionalValuesWizard.OVFList;
                xImportOptionalValuesGrid.DataSourceList.CollectionChanged += xImportOptionalValuesGrid_CollectionChanged;
            }
            else if (FileType == eSourceType.DB.ToString())
            {
                xPathTextBox.RemoveValidations(TextBox.TextProperty);
                xSheetNameComboBox.RemoveValidations(ComboBox.TextProperty);

                xExcelFileStackPanel.Visibility = Visibility.Collapsed;
                xExcelDataGridDockPanel.Visibility = Visibility.Collapsed;
                xExcelViewDataButton.Visibility = Visibility.Collapsed;
                xExcelViewWhereButton.Visibility = Visibility.Collapsed;
                xExcelGridSplitter.Visibility = Visibility.Collapsed;
                xSheetNameComboBox.Visibility = Visibility.Collapsed;
                xSheetLable.Visibility = Visibility.Collapsed;
                xDBStackPanel.Visibility = Visibility.Visible;
                xImportOptionalValuesGrid.Visibility = Visibility.Collapsed;
                FillDBTypeComboBox();                
            }
            xSaveExcelLable.Visibility = Visibility.Collapsed;
        }
        #region XML&JSON
        private void xImportOptionalValuesGrid_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            //if (xImportOptionalValuesGrid.DataSourceList.Count == 0)
            //    AddModelOptionalValuesWizard.NextEnabled = false;
            //else
            //    AddModelOptionalValuesWizard.NextEnabled = true;
        }
        private void DeleteOptionalValueFile(object sender, RoutedEventArgs e)
        {

        }
        private void ClearAllOptionalValuesFiles(object sender, RoutedEventArgs e)
        {
            mAddModelOptionalValuesWizard.OVFList.Clear();
        }
        private void AddOptioanlValueFile(object sender, RoutedEventArgs e)
        {
            BrowseFiles(true);
        }
        #endregion

        #region Excel
        private void xExcelViewWhereButton_Click(object sender, RoutedEventArgs e)
        {
            mAddModelOptionalValuesWizard.ProcessStarted();
            mAddModelOptionalValuesWizard.ImportOptionalValues.ExcelWhereCondition = Convert.ToString(xSelectRowTextBox.Text);
            DataTable dt = mAddModelOptionalValuesWizard.ImportOptionalValues.GetExceSheetlData(true);
            if (dt != null)
            {
                xExcelDataGrid.ItemsSource = dt.AsDataView();
                xExcelDataGridDockPanel.Visibility = Visibility.Visible;
                //AddModelOptionalValuesWizard.NextEnabled = true;
            }
            mAddModelOptionalValuesWizard.ProcessEnded();
        }
        private void xExcelViewDataButton_Click(object sender, RoutedEventArgs e)
        {
            mAddModelOptionalValuesWizard.ProcessStarted();
            DataTable dt = mAddModelOptionalValuesWizard.ImportOptionalValues.GetExceSheetlData(false);
            if (dt != null && dt.Columns.Count > 0 && dt.Rows.Count > 0)
            {
                xExcelDataGrid.ItemsSource = dt.AsDataView();
                xExcelDataGridDockPanel.Visibility = Visibility.Visible;
                //AddModelOptionalValuesWizard.NextEnabled = true;
            }
            mAddModelOptionalValuesWizard.ProcessEnded();
        }
        private void xSheetNameComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (xSheetNameComboBox.SelectedValue != null)
            {
                mAddModelOptionalValuesWizard.ImportOptionalValues.ExcelSheetName = xSheetNameComboBox.SelectedValue.ToString();
                if (!string.IsNullOrEmpty(xSheetNameComboBox.SelectedValue.ToString()))
                {
                    xExcelDataGridDockPanel.Visibility = Visibility.Collapsed;
                    xExcelViewDataButton.Visibility = Visibility.Visible;
                    xExcelViewWhereButton.Visibility = Visibility.Visible;
                    xExcelGridSplitter.Visibility = Visibility.Visible;
                    // AddModelOptionalValuesWizard.NextEnabled = false;
                }
            }
        }
        private void xBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            BrowseFiles(false);
        }
        private void FillSheetCombo()
        {
            mAddModelOptionalValuesWizard.ProcessStarted();
            List<string> SheetsList = new List<string>();
            try
            {
                xSaveExcelLable.Visibility = Visibility.Collapsed;
                mAddModelOptionalValuesWizard.ImportOptionalValues.ExcelFileName = xPathTextBox.Text;
                SheetsList = mAddModelOptionalValuesWizard.ImportOptionalValues.GetSheets(true);
            }
            catch (Exception ex)
            {
                xSaveExcelLable.Visibility = Visibility.Visible;
                Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}");
            }

            GingerCore.General.FillComboFromList(xSheetNameComboBox, SheetsList);
            mAddModelOptionalValuesWizard.ProcessEnded();
        }

        private void xPathTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(xPathTextBox.Text))
            {
                xSaveExcelLable.Visibility = Visibility.Collapsed;
            }
        }

        private void xCreateTemplateExcelButton_Click(object sender, RoutedEventArgs e)
        {
            mAddModelOptionalValuesWizard.ProcessStarted();

            string fileName = string.Empty;
            if (mAddModelOptionalValuesWizard.ImportOptionalValues.ParameterType == ImportOptionalValuesForParameters.eParameterType.Local)
            {
                fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), string.Format("{0}_Parameters", mAddModelOptionalValuesWizard.mAAMB.Name) + ".xlsx");
            }
            else
            {
                fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "GlobalParameters.xlsx");
            }

            bool overrideFile = true;
            if (File.Exists(fileName))
            {
                if(MessageBox.Show("File already exists, do you want to override?", "File Exists", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                {
                    overrideFile = false;
                }
            }

            if (overrideFile)
            {
                ImportOptionalValuesForParameters im = new ImportOptionalValuesForParameters();
                List<AppParameters> parameters = new List<AppParameters>();
                if (mAddModelOptionalValuesWizard.ImportOptionalValues.ParameterType == ImportOptionalValuesForParameters.eParameterType.Local)
                {
                    foreach (var prms in mAddModelOptionalValuesWizard.mAAMB.AppModelParameters)
                    {
                        im.AddNewParameterToList(parameters, prms);
                    }
                    xPathTextBox.Text = im.ExportParametersToExcelFile(parameters, string.Format("{0}_Parameters", mAddModelOptionalValuesWizard.mAAMB.Name), Convert.ToString(xPathTextBox.Text.Trim()));
                }
                else if (mAddModelOptionalValuesWizard.ImportOptionalValues.ParameterType == ImportOptionalValuesForParameters.eParameterType.Global)
                {
                    foreach (var prms in mAddModelOptionalValuesWizard.mGlobalParamterList)
                    {
                        im.AddNewParameterToList(parameters, prms);
                    }
                    xPathTextBox.Text = im.ExportParametersToExcelFile(parameters, "GlobalParameters", Convert.ToString(xPathTextBox.Text.Trim()));
                }

                mAddModelOptionalValuesWizard.ImportOptionalValues.ExcelFileName = xPathTextBox.Text;
                List<string> SheetsList = mAddModelOptionalValuesWizard.ImportOptionalValues.GetSheets(true);
                GingerCore.General.FillComboFromList(xSheetNameComboBox, SheetsList);
                if (xSheetNameComboBox.Items.Count >= 1)
                {
                    xSheetNameComboBox.SelectedIndex = 0;
                }
            }
            mAddModelOptionalValuesWizard.ProcessEnded();
        }       

        #endregion

        #region DB

        private void FillDBTypeComboBox()
        {
            mAddModelOptionalValuesWizard.ProcessStarted();
            List<string> DBTypeList = mAddModelOptionalValuesWizard.ImportOptionalValues.GetDBTypeList().Where(x => x == eDBTypes.Oracle.ToString() || x == eDBTypes.MSAccess.ToString()).ToList();
            GingerCore.General.FillComboFromList(xDBTypeComboBox, DBTypeList);
           
            mAddModelOptionalValuesWizard.ProcessEnded();
        }
        private void xSQLTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            //if (string.IsNullOrEmpty(xSQLTextBox.Text))
            //{
            //    AddModelOptionalValuesWizard.NextEnabled = false;
            //}
            //else
            //{
            //    AddModelOptionalValuesWizard.NextEnabled = true;
            //}
        }
        private void xDBTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }
        private void xConnectDBButton_Click(object sender, RoutedEventArgs e)
        {
            if (xDBTypeComboBox.SelectedValue == null || string.IsNullOrEmpty(xBDHostTextBox.Text.Trim()))
            {
                Reporter.ToUser(eUserMsgKeys.ErrorConnectingToDataBase, "Please Fill all connection details.");
            }
            else
            {
                try
                {
                    mAddModelOptionalValuesWizard.ImportOptionalValues.SetDBDetails(xDBTypeComboBox.SelectedValue.ToString(), xBDHostTextBox.Text.Trim(), xDBUserTextBox.Text.Trim(), xDBPasswordTextBox.Text.Trim());
                    if (mAddModelOptionalValuesWizard.ImportOptionalValues.Connect())
                    {
                        xSQLLable.Visibility = Visibility.Visible;
                        xSQLTextBox.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        xSQLLable.Visibility = Visibility.Collapsed;
                        xSQLTextBox.Visibility = Visibility.Collapsed;
                    }

                }
                catch (Exception ex)
                {
                    if (ex.Message.ToUpper().Contains("COULD NOT LOAD FILE OR ASSEMBLY 'ORACLE.MANAGEDDATAACCESS"))
                    {
                        if (Reporter.ToUser(eUserMsgKeys.OracleDllIsMissing, AppDomain.CurrentDomain.BaseDirectory) == MessageBoxResult.Yes)
                        {
                            System.Diagnostics.Process.Start("https://docs.oracle.com/database/121/ODPNT/installODPmd.htm#ODPNT8149");
                            System.Diagnostics.Process.Start("http://www.oracle.com/technetwork/topics/dotnet/downloads/odacdeploy-4242173.html");

                        }
                    }
                    else
                        Reporter.ToUser(eUserMsgKeys.ErrorConnectingToDataBase, ex.Message);
                }
            }
        }
        #endregion

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            List<string> SheetsList = mAddModelOptionalValuesWizard.ImportOptionalValues.GetSheets(true);
            GingerCore.General.FillComboFromList(xSheetNameComboBox, SheetsList);
        }
    }
}
