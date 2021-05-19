#region License
/*
Copyright © 2014-2021 European Support Limited

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
using Amdocs.Ginger.CoreNET.ActionsLib;
using GingerCore;
using GingerCore.Actions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.Actions
{
    /// <summary>
    /// Interaction logic for XLSReadDataToVariablesPage.xaml
    /// </summary>
    public partial class ActExcelEditPage 
    {       
        public ActionEditPage actp;
        private ActExcel mAct;

        public ActExcelEditPage(ActExcel act)
        {
            InitializeComponent();
            mAct = act;
            Bind();
            mAct.SolutionFolder =  WorkSpace.Instance.Solution.Folder.ToUpper();
        }
        
        public void Bind()
        {            
            ExcelActionComboBox.BindControl(mAct, nameof(ActExcel.ExcelActionType));
            ExcelFileNameTextBox.BindControl(Context.GetAsContext(mAct.Context), mAct, nameof(ActExcel.ExcelFileName));
            SheetNamComboBox.BindControl(mAct, nameof(ActExcel.SheetName));
            SelectRowsWhereTextBox.BindControl(Context.GetAsContext(mAct.Context), mAct, nameof(ActExcel.SelectRowsWhere));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(SelectAllRows,CheckBox.IsCheckedProperty,mAct, nameof(ActExcel.SelectAllRows));
            PrimaryKeyColumnTextBox.BindControl(Context.GetAsContext(mAct.Context), mAct, nameof(ActExcel.PrimaryKeyColumn));
            SetDataUsedTextBox.BindControl(Context.GetAsContext(mAct.Context), mAct, nameof(ActExcel.SetDataUsed));
            ColMappingRulesTextBox.BindControl(Context.GetAsContext(mAct.Context), mAct, nameof(ActExcel.ColMappingRules));
            
            if (mAct.ExcelActionType == ActExcel.eExcelActionType.ReadData)
            {                
                this.ColMappingRulesSection.Visibility = Visibility.Collapsed;
                SetDataUsedSection.Visibility = Visibility.Visible;
            }
            if (mAct.ExcelActionType == ActExcel.eExcelActionType.WriteData)
            {
                this.ColMappingRulesSection.Visibility = Visibility.Visible;
            }

            // populate Sheet dropdown
            if (!string.IsNullOrEmpty(mAct.ExcelFileName))
            {
                FillSheetCombo();
                if (mAct.SheetName != null)
                {                        
                    SheetNamComboBox.Items.Add(mAct.SheetName);
                    SheetNamComboBox.SelectedValue = mAct.SheetName;
                }
            }
        }

        private void BrowseExcelButton_Click(object sender, RoutedEventArgs e)
        {
            if (General.SetupBrowseFile(new System.Windows.Forms.OpenFileDialog()
            {
                DefaultExt = "*.xlsx or .xls or .xlsm",
                Filter = "Excel Files (*.xlsx, *.xls, *.xlsm)|*.xlsx;*.xls;*.xlsm"
            }) is string fileName)
            {
                ExcelFileNameTextBox.ValueTextBox.Text = fileName;
                FillSheetCombo();
            }
        }

        private void FillSheetCombo()
        {
            ContextProcessInputValueForDriver();
            //Move code to ExcelFunction no in Act...
            List<string> SheetsList = mAct.GetSheets();
            GingerCore.General.FillComboFromList(SheetNamComboBox, SheetsList);
        }

        private void ContextProcessInputValueForDriver()
        {
            var context = Context.GetAsContext(mAct.Context);
            if (context != null)
            {
                context.Runner.ProcessInputValueForDriver(mAct);
            }
        }

        private void ViewDataButton_Click(object sender, RoutedEventArgs e)
        {
            ContextProcessInputValueForDriver();

            DataTable dt = mAct.GetExcelSheetData(null);
            if (dt != null)
                ExcelDataGrid.ItemsSource = dt.AsDataView();
        }

        private void ViewWhereButton_Click(object sender, RoutedEventArgs e)
        {
            ContextProcessInputValueForDriver();

            DataTable dt = mAct.GetExcelSheetDataWithWhere();
            if(dt!=null)
                ExcelDataGrid.ItemsSource = dt.AsDataView();
        }

        private void ExcelActionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ContextProcessInputValueForDriver();

            if (ExcelActionComboBox.SelectedValue.ToString() == "ReadData")
            {
                SetDataUsedSection.Visibility = Visibility.Visible;
                ColMappingRulesSection.Visibility = Visibility.Collapsed;
                //fixing for #3811 : to clear Data from Write Data column
                mAct.ColMappingRules = string.Empty;
            }

            if (ExcelActionComboBox.SelectedValue.ToString() == "WriteData")
            {
                ColMappingRulesSection.Visibility = Visibility.Visible;
            }
        }

        private void SheetNamComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            mAct.SheetName = SheetNamComboBox.Text;
            ContextProcessInputValueForDriver();
        }

        private void SheetNamComboBox_DropDownOpened(object sender, EventArgs e)
        {
            FillSheetCombo();
        }

        private void SheetNamVEButton_Click(object sender, RoutedEventArgs e)
        {
            ValueExpressionEditorPage w = new ValueExpressionEditorPage(mAct, nameof(ActExcel.SheetName), Context.GetAsContext(mAct.Context));
            w.ShowAsWindow(eWindowShowStyle.Dialog);
            SheetNamComboBox.Text = mAct.SheetName;
        }

        private void xOpenExcelButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(mAct.GetExcelFileNameForDriver());
            }
            catch(Exception ex)
            {
                Reporter.ToUser(eUserMsgKey.MissingExcelDetails);
            }
        }
    }
}
