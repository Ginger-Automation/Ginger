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
    public partial class AutoRunWizardOptionsPage : Page, IWizardPage
    {
        AutoRunWizard mAutoRunWizard;
        
        public AutoRunWizardOptionsPage()
        {
            InitializeComponent();
        }

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            switch (WizardEventArgs.EventType)
            {
                case EventType.Init:
                    mAutoRunWizard = (AutoRunWizard)WizardEventArgs.Wizard;
                                       
                    if (WorkSpace.Instance.Solution.SourceControl == null)
                    {
                        xDownloadsolutionCheckBox.IsChecked = false;
                        xDownloadsolutionCheckBox.IsEnabled = true;
                    }
                    else
                    {
                        xDownloadsolutionCheckBox.IsEnabled = true;
                    }
                    break;
            }

        }
        
        private void xDownloadsolutionCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            mAutoRunWizard.mCLIHelper.DownloadUpgradeSolutionFromSourceControl = true;
        }

        private void xDownloadsolutionCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            mAutoRunWizard.mCLIHelper.DownloadUpgradeSolutionFromSourceControl = false;
        }

        private void xGingerRunEXEWindowShow_Checked(object sender, RoutedEventArgs e)
        {
            mAutoRunWizard.mCLIHelper.ShowAutoRunWindow = true;            
        }

        private void xGingerRunEXEWindowShow_Unchecked(object sender, RoutedEventArgs e)
        {
            mAutoRunWizard.mCLIHelper.ShowAutoRunWindow = false;            
        }

        private void xRunAnalyzerCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            mAutoRunWizard.mCLIHelper.RunAnalyzer = true;
        }

        private void xRunAnalyzerCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            mAutoRunWizard.mCLIHelper.RunAnalyzer = false;
        }
    }
}
