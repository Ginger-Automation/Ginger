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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
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
            ExcelActionComboBox.BindControl(mAct, ActExcel.Fields.ExcelActionType);
            ExcelFileNameTextBox.BindControl(Context.GetAsContext(mAct.Context), mAct, ActExcel.Fields.ExcelFileName);
            SheetNamComboBox.BindControl(mAct, ActExcel.Fields.SheetName);
            SelectRowsWhereTextBox.BindControl(Context.GetAsContext(mAct.Context), mAct, ActExcel.Fields.SelectRowsWhere);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(SelectAllRows,CheckBox.IsCheckedProperty,mAct, ActExcel.Fields.SelectAllRows);
            PrimaryKeyColumnTextBox.BindControl(Context.GetAsContext(mAct.Context), mAct, ActExcel.Fields.PrimaryKeyColumn);
            SetDataUsedTextBox.BindControl(Context.GetAsContext(mAct.Context), mAct, ActExcel.Fields.SetDataUsed);
            ColMappingRulesTextBox.BindControl(Context.GetAsContext(mAct.Context), mAct, ActExcel.Fields.ColMappingRules);
            
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
            System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog();

            dlg.DefaultExt = "*.xlsx or .xls or .xlsm";
            dlg.Filter = "Excel Files (*.xlsx, *.xls, *.xlsm)|*.xlsx;*.xls;*.xlsm";
            string SolutionFolder = WorkSpace.Instance.Solution.Folder.ToUpper(); 
            
            if(dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                // replace Absolute file name with relative to solution
                string FileName = dlg.FileName.ToUpper();
                if (FileName.Contains(SolutionFolder))
                {
                    FileName = FileName.Replace(SolutionFolder, @"~\");
                }
                
                ExcelFileNameTextBox.ValueTextBox.Text = FileName;
                FillSheetCombo();
            }
        }

        private void FillSheetCombo()
        {
            Context.GetAsContext(mAct.Context).Runner.ProcessInputValueForDriver(mAct);
            //Move code to ExcelFunction no in Act...
            List<string> SheetsList = mAct.GetSheets();
            GingerCore.General.FillComboFromList(SheetNamComboBox, SheetsList);
        }


        private void ViewDataButton_Click(object sender, RoutedEventArgs e)
        {
            Context.GetAsContext(mAct.Context).Runner.ProcessInputValueForDriver(mAct);

            DataTable dt = mAct.GetExcelSheetData(null);
            if (dt != null)
                ExcelDataGrid.ItemsSource = dt.AsDataView();
        }

        private void ViewWhereButton_Click(object sender, RoutedEventArgs e)
        {
            Context.GetAsContext(mAct.Context).Runner.ProcessInputValueForDriver(mAct);

            DataTable dt = mAct.GetExcelSheetDataWithWhere();
            if(dt!=null)
                ExcelDataGrid.ItemsSource = dt.AsDataView();
        }

        private void ExcelActionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Context.GetAsContext(mAct.Context).Runner.ProcessInputValueForDriver(mAct);

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
            Context.GetAsContext(mAct.Context).Runner.ProcessInputValueForDriver(mAct);
        }

        private void SheetNamComboBox_DropDownOpened(object sender, EventArgs e)
        {
            FillSheetCombo();
        }

        private void SheetNamVEButton_Click(object sender, RoutedEventArgs e)
        {
            ValueExpressionEditorPage w = new ValueExpressionEditorPage(mAct, ActExcel.Fields.SheetName, Context.GetAsContext(mAct.Context));
            w.ShowAsWindow(eWindowShowStyle.Dialog);
            SheetNamComboBox.Text = mAct.SheetName;
        }

        private void xOpenExcelButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(mAct.GetExcelFileNameForDriver());
        }
    }
}
