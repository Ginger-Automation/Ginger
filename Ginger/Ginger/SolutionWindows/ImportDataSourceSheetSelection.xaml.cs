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
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.IO;
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
using System.Windows.Media;
using System.Windows.Data;
using GingerWPF;
using GingerWPF.WizardLib;

namespace Ginger.DataSource
{
    /// <summary>
    /// Interaction logic for ImportDataSourceSheetSelection.xaml
    /// </summary>
    public partial class ImportDataSourceSheetSelection : Page, IWizardPage
    {
        ImportOptionalValuesForParameters impParams;

        /// <summary>
        /// Gets sets the File path
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Gets sets the SheetName
        /// </summary>
        public string SheetName { get; set; }

        /// <summary>
        /// This method is default wizard action event
        /// </summary>
        /// <param name="WizardEventArgs"></param>
        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            switch (WizardEventArgs.EventType)
            {
                case EventType.Init:                    
                    break;
                case EventType.Active:
                    Path = ((ImportDataSourceBrowseFile)(WizardEventArgs.Wizard.Pages[0].Page)).Path;
                    if (!string.IsNullOrEmpty(Path))
                    {
                        impParams.ExcelFileName = Path;
                        List<string> SheetsList = impParams.GetSheets(false);
                        SheetsList.Insert(0, "-- All --");
                        GingerCore.General.FillComboFromList(xSheetNameComboBox, SheetsList);
                    }
                    
                    break;
            }
        }

        /// <summary>
        /// Constrtuctor for ImportDataSourceSheetSelection class
        /// </summary>
        public ImportDataSourceSheetSelection()
        {           
            InitializeComponent();
            impParams = new ImportOptionalValuesForParameters();
            ShowRelevantPanel();

            xSheetNameComboBox.Style = this.FindResource("$FlatInputComboBoxStyle") as Style;
            xSheetNameComboBox.BindControl(this, nameof(SheetName));
            xSheetNameComboBox.AddValidationRule(new EmptyValidationRule());
            xSheetNameComboBox.Focus();
        }

        /// <summary>
        /// This method is used to ShowRelevantPanel
        /// </summary>
        /// <param name="FileType"></param>
        private void ShowRelevantPanel()
        {
            try
            {
                xExcelFileStackPanel.Visibility = Visibility.Visible;
                xSheetNameComboBox.Visibility = Visibility.Visible;
                xSheetLable.Visibility = Visibility.Visible;
            }
            catch (System.Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}");
            }
        }
        
        /// <summary>
        /// This event is used for selection changed on the SheetName ComboBox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void xSheetNameComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (xSheetNameComboBox.SelectedValue != null)
                {
                    SheetName = Convert.ToString(xSheetNameComboBox.SelectedValue);
                    impParams.ExcelSheetName = SheetName;
                }
            }
            catch (System.Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}");
            }
        }
        
        /// <summary>
        /// This method is used to cehck whether alternate page is required to load
        /// </summary>
        /// <returns></returns>
        public bool IsAlternatePageToLoad()
        {
            return false;
        }
    }
}
