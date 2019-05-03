using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.CoreNET.RunLib;
using Amdocs.Ginger.CoreNET.RunLib.CLILib;
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
        CLIHelper mCLIHelper = new CLIHelper();
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
                    xAppLoggingLevelComboBox.BindControl(mCreateCLIWizard , nameof(CreateCLIWizard.AppLoggingLevel));
                    if (!(WorkSpace.Instance.Solution.SourceControl == null))
                    {
                        xDownloadsolutionFromSourceControlcheckBox.IsEnabled = true;
                    }
                    else
                    {
                        xDownloadsolutionFromSourceControlcheckBox.IsEnabled = false;
                    }
                    break;
                case EventType.Active:
                    //
                    break;
            }

        }
        

        private void XDownloadsolutionFromSourceControlcheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CLIHelper.DownloadSolutionFromSourceControlBool = true;
        }

        private void XDownloadsolutionFromSourceControlcheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            CLIHelper.DownloadSolutionFromSourceControlBool = false;
        }

        private void XGingerEXERadioButton_Checked(object sender, RoutedEventArgs e)
        {
            mCreateCLIWizard.SetGingerExecutor();
        }
        
        private void xGingerRunEXEWindowShow_Checked(object sender, RoutedEventArgs e)
        {
            CLIHelper.ShowAutoRunWindow = true;
            
        }
        private void xGingerRunEXEWindowShow_Unchecked(object sender, RoutedEventArgs e)
        {
            CLIHelper.ShowAutoRunWindow = false;
            
        }
        private void XGingerConsoleRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            mCreateCLIWizard.SetGingerConsoleExecutor();
        }

        private void RunAnalyzerCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CLIHelper.RunAnalyzer = true;
        }

        private void RunAnalyzerCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            CLIHelper.RunAnalyzer = false;
        }
    }
}
