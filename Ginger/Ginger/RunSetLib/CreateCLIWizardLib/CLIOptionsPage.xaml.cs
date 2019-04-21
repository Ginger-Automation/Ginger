using GingerWPF.WizardLib;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.RunSetLib.CreateCLIWizardLib
{
    /// <summary>
    /// Interaction logic for CLISourceControlPage.xaml
    /// </summary>
    public partial class CLIOptionsPage : Page, IWizardPage
    {
        CreateCLIWizard mCreateCLIWizard;
        public CLIOptionsPage()
        {
            InitializeComponent();
        }

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            switch (WizardEventArgs.EventType)
            {
                case EventType.Init:
                    mCreateCLIWizard = (CreateCLIWizard)WizardEventArgs.Wizard;
                    xGingerEXERadioButton.IsChecked = true;
                    break;
                case EventType.Active:
                    //
                    break;
            }

        }
        

        private void XDownloadsolutionFromSourceControlcheckBox_Checked(object sender, RoutedEventArgs e)
        {
            mCreateCLIWizard.DownloadSolutionFromSourceControl = true;
        }

        private void XDownloadsolutionFromSourceControlcheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            mCreateCLIWizard.DownloadSolutionFromSourceControl = false;
        }

        private void XGingerEXERadioButton_Checked(object sender, RoutedEventArgs e)
        {
            mCreateCLIWizard.SetGingerExecutor();
        }

        private void XGingerConsoleRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            mCreateCLIWizard.SetGingerConsoleExecutor();
        }

        private void RunAnalyzerCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            mCreateCLIWizard.RunAnalyzer = true;
        }

        private void RunAnalyzerCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            mCreateCLIWizard.RunAnalyzer = false;
        }
    }
}
