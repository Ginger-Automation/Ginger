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
using GingerWPF.WizardLib;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.External.Katalon
{
    /// <summary>
    /// Interaction logic for ImportPOMSummaryWizardPage.xaml
    /// </summary>
    public partial class ImportPOMSummaryWizardPage : Page, IWizardPage
    {
        private readonly ImportKatalonObjectRepositoryWizard _wizard;

        public ImportPOMSummaryWizardPage(ImportKatalonObjectRepositoryWizard wizard)
        {
            InitializeComponent();
            _wizard = wizard;
            _wizard.PropertyChanged += Wizard_PropertyChanged;
        }

        private void Wizard_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (!string.Equals(e.PropertyName, nameof(ImportKatalonObjectRepositoryWizard.ImportedPOMCount)))
            {
                return;
            }

            UpdateSummary(_wizard.ImportedPOMCount);
        }

        public void WizardEvent(WizardEventArgs e)
        {
            if (e.EventType == EventType.Active)
            {
                _wizard.mWizardWindow?.SetFinishButtonEnabled(true);
            }
            else if (e.EventType == EventType.Prev && _wizard.GetCurrentPage().Page == this)
            {
                e.CancelEvent = true;
            }
        }

        private void UpdateSummary(int importedPOMCount)
        {
            if (!string.Equals(ImportCountTextBlock.Text, importedPOMCount.ToString()))
            {
                ImportCountTextBlock.Text = importedPOMCount.ToString();
            }

            if (importedPOMCount <= 0 && ImportLocationStackPanel.Visibility != Visibility.Collapsed)
            {
                ImportLocationStackPanel.Visibility = Visibility.Collapsed;
            }
            else if (ImportLocationStackPanel.Visibility != Visibility.Visible)
            {
                ImportLocationStackPanel.Visibility = Visibility.Visible;
                ImportLocationTextBlock.Text = _wizard.ImportTargetDirectoryPath;
            }
        }
    }
}
