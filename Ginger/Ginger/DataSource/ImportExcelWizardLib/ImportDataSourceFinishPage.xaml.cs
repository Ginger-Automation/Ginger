#region License
/*
Copyright © 2014-2026 European Support Limited

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

using GingerCore.DataSource;
using GingerWPF.WizardLib;
using System.Windows.Controls;
using System.Windows.Input;


namespace Ginger.DataSource.ImportExcelWizardLib
{
    /// <summary>
    /// Interaction logic for ImportDataSourceFinishPage.xaml
    /// </summary>
    public partial class ImportDataSourceFinishPage : Page, IWizardPage
    {
        public DataSourceBase DSDetails { get; set; }
        public WizardEventArgs mWizardEventArgs;

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
                    mWizardEventArgs = WizardEventArgs;
                    xLable.Content = "Proceed for Data Import, Click Finish!";
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Constructor for ImportDataSourceFinishPage class
        /// </summary>
        public ImportDataSourceFinishPage(DataSourceBase mDSDetails)
        {
            InitializeComponent();
            DSDetails = mDSDetails;
        }

        /// <summary>
        /// This method is the final FinishImport method
        /// </summary>
        public void FinishImport()
        {
            xLable.Content = "Data Importing...";
            Mouse.OverrideCursor = Cursors.Wait;
            mWizardEventArgs.Wizard.ProcessStarted();

            ((ImportDataSourceFromExcelWizard)mWizardEventArgs.Wizard).Finish();

            mWizardEventArgs.Wizard.ProcessEnded();
            Mouse.OverrideCursor = null;
            xLable.Content = "Data Imported Successfully!";

        }
    }
}
