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
