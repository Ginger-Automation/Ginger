using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.CoreNET.RunLib;
using Amdocs.Ginger.CoreNET.RunLib.CLILib;
using GingerWPF.WizardLib;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.RunSetLib.CreateCLIWizardLib
{
    /// <summary>
    /// Interaction logic for CreateCLIChooseTypePage.xaml
    /// </summary>
    public partial class CreateCLIChooseTypePage : Page, IWizardPage
    {
        CreateCLIWizard mCreateCLIWizard;
        public CreateCLIChooseTypePage()
        {
            InitializeComponent();            
        }

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {            
            switch (WizardEventArgs.EventType)
            {
                case EventType.Init:
                    mCreateCLIWizard = (CreateCLIWizard)WizardEventArgs.Wizard;                    
                    break;
                case EventType.Active:
                    //
                    break;
            }

        }

        CLIConfigFile mCLIConfigFile;
        private void xConfigRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (mCLIConfigFile == null)
            {
                mCLIConfigFile = new CLIConfigFile();
            }
            mCreateCLIWizard.iCLI = mCLIConfigFile;        
            ShowContent();            
        }

        private void ShowContent()
        {
            string content = mCreateCLIWizard.iCLI.CreateContent(WorkSpace.Instance.RunsetExecutor);
            mCreateCLIWizard.FileContent = content;
            xCLIContentTextBox.Text = content;
        }

        CLIDynamicXML mCLIDynamicXML;
        private void XDynamicRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (mCLIDynamicXML == null)
            {
                mCLIDynamicXML = new CLIDynamicXML();
            }
            mCreateCLIWizard.iCLI = mCLIDynamicXML;
            ShowContent();
        }

        CLIScriptFile mCLIScriptFile;
        private void XScriptRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (mCLIScriptFile == null)
            {
                mCLIScriptFile = new CLIScriptFile();
            }
            mCreateCLIWizard.iCLI = mCLIScriptFile;
            ShowContent();
        }

        private void XParametersRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            mCreateCLIWizard.FileContent = "";
            ShowContent();
        }

        private void XExcelRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            mCreateCLIWizard.FileContent = "Excel view ";
            ShowContent();
        }
    }
}
