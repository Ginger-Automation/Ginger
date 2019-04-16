using amdocs.ginger.GingerCoreNET;
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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

        private void XDriverRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            CalculateCLIContent();
        }

        private void CalculateCLIContent()
        {
            // TOOD: switch case...

            string sConfig = "Solution=" + WorkSpace.Instance.Solution.Folder + Environment.NewLine;
            sConfig += "Env=Env 1"  + Environment.NewLine;
            sConfig += "RunSet=" + "Deafulat Run Set" + Environment.NewLine;
            //sConfig += "Env=" + mCreateCLIWizard.ProjEnvironment.Name + Environment.NewLine;
            //sConfig += "RunSet=" + mCreateCLIWizard.RunSetConfig.Name + Environment.NewLine;

            xCLIContentTextBox.Text = sConfig;
        }
    }
}
