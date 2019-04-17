using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.CoreNET.RunLib;
using Amdocs.Ginger.CoreNET.RunLib.DynamicRunSetLib;
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
                    
                    break;

            }

        }

        private void xConfigRadioButton_Checked(object sender, RoutedEventArgs e)
        {            
            SetContent(CLIConfigFile.CreateConfig(WorkSpace.Instance.RunsetExecutor));            
        }

        private void SetContent(string v)
        {
            mCreateCLIWizard.FileContent = v;
            xCLIContentTextBox.Text = v;
        }

        private void XDynamicRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            SetContent(DynamicRunSetManager.CreateRunSet(WorkSpace.Instance.RunsetExecutor));
        }

        private void XScriptRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            SetContent("Script aaaa");
        }

        private void XParametersRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            mCreateCLIWizard.FileContent = "";
            SetContent("/Sol=111 / / / ");
        }

        private void XExcelRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            mCreateCLIWizard.FileContent = "Excel view ";
            SetContent("Excel view");
        }
    }
}
