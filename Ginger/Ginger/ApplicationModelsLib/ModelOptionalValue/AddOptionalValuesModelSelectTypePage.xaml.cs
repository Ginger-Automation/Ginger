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

namespace Ginger.ApplicationModelsLib.ModelOptionalValue
{
    /// <summary>
    /// Interaction logic for AddOptionalValuesModelSelectTypePage.xaml
    /// </summary>
    public partial class AddOptionalValuesModelSelectTypePage : Page, IWizardPage
    {
        public AddModelOptionalValuesWizard mAddModelOptionalValuesWizard;
        eOptionalValuesTargetType mOptionalValuesTargetType;

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
                mAddModelOptionalValuesWizard = ((AddModelOptionalValuesWizard)WizardEventArgs.Wizard);              
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
                    mAddModelOptionalValuesWizard.mAAMB.OptionalValuesTemplates.Clear();//XML & JSON
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
                if ((xSourceTypeComboBox.SelectedValue.ToString() == eSourceType.Excel.ToString()))
                {
                    xPathTextBox.Text = dlg.FileName;
                    FillSheetCombo();
                    xExcelDataGridDockPanel.Visibility = Visibility.Collapsed;
                    // AddModelOptionalValuesWizard.FinishEnabled = false;
                    xExcelViewButtonsDoclPanel.Visibility = Visibility.Collapsed;
                    xExcelGridSplitter.Visibility = Visibility.Collapsed;
                }
                else
                {
                    foreach (String file in dlg.FileNames)
                    {
                        mAddModelOptionalValuesWizard.OVFList.Add(new TemplateFile() { FilePath = file });
                        mAddModelOptionalValuesWizard.mAAMB.OptionalValuesTemplates.Add(new TemplateFile() { FilePath = file });
                    }
                    xImportOptionalValuesGrid.DataSourceList = mAddModelOptionalValuesWizard.OVFList;
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
                xSaveExcelLable.Visibility = Visibility.Visible;
                xExcelDataGridDockPanel.Visibility = Visibility.Collapsed;
                xExcelViewButtonsDoclPanel.Visibility = Visibility.Collapsed;
                xExcelGridSplitter.Visibility = Visibility.Collapsed;
                xSheetNameComboBox.Visibility = Visibility.Visible;
                xSheetLable.Visibility = Visibility.Visible;
                xDBStackPanel.Visibility = Visibility.Collapsed;
                xImportOptionalValuesGrid.Visibility = Visibility.Collapsed;
            }
            else if (FileType == eSourceType.XML.ToString() || FileType == eSourceType.Json.ToString())
            {
                xExcelFileStackPanel.Visibility = Visibility.Collapsed;
                xSaveExcelLable.Visibility = Visibility.Collapsed;
                xExcelDataGridDockPanel.Visibility = Visibility.Collapsed;
                xExcelViewButtonsDoclPanel.Visibility = Visibility.Collapsed;
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
                xExcelFileStackPanel.Visibility = Visibility.Collapsed;
                xSaveExcelLable.Visibility = Visibility.Collapsed;
                xExcelDataGridDockPanel.Visibility = Visibility.Collapsed;
                xExcelViewButtonsDoclPanel.Visibility = Visibility.Collapsed;
                xExcelGridSplitter.Visibility = Visibility.Collapsed;
                xSheetNameComboBox.Visibility = Visibility.Collapsed;
                xSheetLable.Visibility = Visibility.Collapsed;
                xDBStackPanel.Visibility = Visibility.Visible;
                xImportOptionalValuesGrid.Visibility = Visibility.Collapsed;
                FillDBTypeComboBox();
                
            }
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
            if (!string.IsNullOrEmpty(xSelectRowTextBox.Text))
            {
                mAddModelOptionalValuesWizard.ProcessStarted();
                mAddModelOptionalValuesWizard.ImportOptionalValues.ExcelWhereCondition = xSelectRowTextBox.Text;
                DataTable dt = mAddModelOptionalValuesWizard.ImportOptionalValues.GetExceSheetlData(true);
                if (dt != null)
                {
                    xExcelDataGrid.ItemsSource = dt.AsDataView();
                    xExcelDataGridDockPanel.Visibility = Visibility.Visible;
                    //AddModelOptionalValuesWizard.NextEnabled = true;
                }
                mAddModelOptionalValuesWizard.ProcessEnded(); 
            }
        }
        private void xExcelViewDataButton_Click(object sender, RoutedEventArgs e)
        {
            mAddModelOptionalValuesWizard.ProcessStarted();
            DataTable dt = mAddModelOptionalValuesWizard.ImportOptionalValues.GetExceSheetlData(false);
            if (dt != null)
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
                    xExcelViewButtonsDoclPanel.Visibility = Visibility.Visible;
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
            mAddModelOptionalValuesWizard.ImportOptionalValues.ExcelFileName = xPathTextBox.Text;
            List<string> SheetsList = mAddModelOptionalValuesWizard.ImportOptionalValues.GetSheets();
            GingerCore.General.FillComboFromList(xSheetNameComboBox, SheetsList);
            mAddModelOptionalValuesWizard.ProcessEnded();
        }
        private void xPathTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
        }
        private void xCreateTemplateExcelButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(xPathTextBox.Text.Trim())) { Reporter.ToUser(eUserMsgKeys.MissingExcelDetails); return; }
            if (!xPathTextBox.Text.Trim().ToLower().EndsWith(".xlsx")) { Reporter.ToUser(eUserMsgKeys.InvalidExcelDetails); return; }
            mAddModelOptionalValuesWizard.ProcessStarted();
            bool exportSuccess = false;
            if (mAddModelOptionalValuesWizard.ImportOptionalValues.ParameterType == ImportOptionalValuesForParameters.eParameterType.Local)
            {
                exportSuccess = mAddModelOptionalValuesWizard.ImportOptionalValues.ExportTemplateExcelFileForImportOptionalValues(mAddModelOptionalValuesWizard.mAAMB.AppModelParameters.ToList(), xPathTextBox.Text.Trim());
            }
            else if (mAddModelOptionalValuesWizard.ImportOptionalValues.ParameterType == ImportOptionalValuesForParameters.eParameterType.Global)
            {
                List<AppModelParameter> GlobalParamList = mAddModelOptionalValuesWizard.mGlobalParamterList.ToList().ConvertAll(x => (AppModelParameter)x);
                exportSuccess = mAddModelOptionalValuesWizard.ImportOptionalValues.ExportTemplateExcelFileForImportOptionalValues(GlobalParamList, xPathTextBox.Text.Trim());
            }
            if (exportSuccess)
            {
                System.Diagnostics.Process.Start(xPathTextBox.Text.Trim());
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
    }
}
