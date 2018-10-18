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
using System.Windows;
using System.Windows.Controls;
using GingerCore;
using GingerCore.DataSource;
using System.Reflection;
using Ginger.ApplicationModelsLib.ModelOptionalValue;
using System.Data;
using System.Collections.Generic;
using System;
using System.Text;
using Ginger.SolutionWindows.TreeViewItems;
using Amdocs.Ginger.ValidationRules;
using GingerWPF;
using GingerWPF.WizardLib;

namespace Ginger.DataSource.ImportExcelWizardLib
{
    /// <summary>
    /// Interaction logic for ImportDataSourceBrowseFile.xaml
    /// </summary>
    public partial class ImportDataSourceBrowseFile : Page, IWizardPage
    {
        ImportOptionalValuesForParameters impParams;

        ImportDataSourceFromExcelWizard mWizard;

        /// <summary>
        /// This method is default wizard action event
        /// </summary>
        /// <param name="WizardEventArgs"></param>
        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {            
            switch (WizardEventArgs.EventType)
            {
                case EventType.Init:
                    mWizard = (ImportDataSourceFromExcelWizard)WizardEventArgs.Wizard;
                    xPathTextBox.BindControl(mWizard, nameof(ImportDataSourceFromExcelWizard.Path));
                    //xPathTextBox.TextChanged += XPathTextBox_TextChanged;
                    xPathTextBox.AddValidationRule(new EmptyValidationRule());
                    break;
                case EventType.Active:
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Constrtuctor for ImportDataSourceBrowseFile class
        /// </summary>
        public ImportDataSourceBrowseFile()
        {           
            InitializeComponent();
            impParams = new ImportOptionalValuesForParameters();
            ShowRelevantPanel();
            
            xPathTextBox.Focus();
        }

        //private void XPathTextBox_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    mWizard.Path = xPathTextBox.Text;
        //}

        /// <summary>
        /// This method is used to ShowRelevantPanel
        /// </summary>
        /// <param name="FileType"></param>
        private void ShowRelevantPanel()
        {
            try
            {
                xExcelFileStackPanel.Visibility = Visibility.Visible;
                xSaveExcelLable.Visibility = Visibility.Visible;
            }
            catch (System.Exception ex)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}");
            }
        }
        
        /// <summary>
        /// This event handles browsing of Script File from user desktop
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void xBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog();
                dlg.Multiselect = false;
                dlg.Filter = "Excel Files|*.xls;*.xlsx;*.xlsm";
                System.Windows.Forms.DialogResult result = dlg.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    xPathTextBox.Text = dlg.FileName;
                    impParams.ExcelFileName = dlg.FileName;
                    List<string> SheetsList = impParams.GetSheets(false);
                }
            }
            catch (System.Exception ex)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}");
            }
        }
    }
}
