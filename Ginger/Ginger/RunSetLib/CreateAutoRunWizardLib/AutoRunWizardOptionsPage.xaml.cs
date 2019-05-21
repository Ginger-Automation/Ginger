using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.CoreNET.RunLib;
using Amdocs.Ginger.CoreNET.RunLib.CLILib;
using GingerCore.GeneralLib;
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
                        xDownloadsolutionCheckBox.IsEnabled = false;
                        mAutoRunWizard.CliHelper.DownloadUpgradeSolutionFromSourceControl = false;
                    }
                    else
                    {
                        xDownloadsolutionCheckBox.IsEnabled = true;
                        mAutoRunWizard.CliHelper.DownloadUpgradeSolutionFromSourceControl = true;
                    }
                    mAutoRunWizard.CliHelper.ShowAutoRunWindow = false;
                    mAutoRunWizard.CliHelper.RunAnalyzer = true;
                    BindingHandler.ObjFieldBinding(xDownloadsolutionCheckBox, CheckBox.IsCheckedProperty, mAutoRunWizard.CliHelper, nameof(CLIHelper.DownloadUpgradeSolutionFromSourceControl));
                    BindingHandler.ObjFieldBinding(xGingerRunEXEWindowShow, CheckBox.IsCheckedProperty, mAutoRunWizard.CliHelper, nameof(CLIHelper.ShowAutoRunWindow));
                    BindingHandler.ObjFieldBinding(xRunAnalyzerCheckBox, CheckBox.IsCheckedProperty, mAutoRunWizard.CliHelper, nameof(CLIHelper.RunAnalyzer));
                    break;
            }
        }
    }
}
