using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.CoreNET.RunLib;
using Amdocs.Ginger.CoreNET.RunLib.CLILib;
using GingerWPF.WizardLib;
using System;
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
        string CLITXT;
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
                    xConfigRadioButton.IsChecked = true;
                    break;
                case EventType.Active:
                    xCLICommandTextBox.Text = mCreateCLIWizard.CLIExecutor;
                    break;
            }

        }

        

        private void ShowContent()
        {
            string content = mCreateCLIWizard.SelectedCLI.CreateContent(WorkSpace.Instance.RunsetExecutor);
            mCreateCLIWizard.FileContent = content;
            xCLIContentTextBox.Text = content;
            xCLITypeHelpTextBlock.Text = CLITXT;
        }

        CLIConfigFile mCLIConfigFile;
        private void xConfigRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (mCLIConfigFile == null)
            {
                mCLIConfigFile = new CLIConfigFile();
            }
            mCreateCLIWizard.SelectedCLI = mCLIConfigFile;
            CLITXT = "Simple text file which contain the configuration, executed using Ginger ConfigFile=%filename%" + Environment.NewLine;
            CLITXT += @"Example: Ginger.exe ConfigFile = C:\Ginger\Regression1.txt" + Environment.NewLine;
            CLITXT += "When you need to execute fixed predefined run set which include Solution, RunSetName and Env" + Environment.NewLine;            
            ShowContent();            
        }

        

        CLIDynamicXML mCLIDynamicXML;
        private void XDynamicRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (mCLIDynamicXML == null)
            {
                mCLIDynamicXML = new CLIDynamicXML();
            }
            mCreateCLIWizard.SelectedCLI = mCLIDynamicXML;
            CLITXT = "Dynamic xml file which contain the runset details, executed using Ginger DynamicFile=%filename%" + Environment.NewLine;
            CLITXT += "Enable to create dynamic run set by changing the xml content" + Environment.NewLine;
            ShowContent();
        }

        CLIScriptFile mCLIScriptFile;
        private void XScriptRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (mCLIScriptFile == null)
            {
                mCLIScriptFile = new CLIScriptFile();
            }
            mCreateCLIWizard.SelectedCLI = mCLIScriptFile;
            CLITXT = "Script file(C#) which contain the code, executed using Ginger ConfigFile=%filename%" + Environment.NewLine;
            CLITXT += "enable to create run set with loops and much more complex execution..." + Environment.NewLine;
            ShowContent();
        }

        CLIArgs mCLIArgs;
        private void XParametersRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (mCLIArgs == null)
            {
                mCLIArgs = new CLIArgs();
            }
            mCreateCLIWizard.SelectedCLI = mCLIArgs;
            CLITXT = @"Using CLI only without file, contains all run set arguments in the command line itself using switches like --solution executed using Ginger --solution c:\ginger\solution\sol1" + Environment.NewLine;
            CLITXT += "Enable to run different runset on different env using only command line arguments" + Environment.NewLine;
            ShowContent();
        }

        private void XExcelRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            mCreateCLIWizard.FileContent = "Excel view ";
            ShowContent();
        }
    }
}
